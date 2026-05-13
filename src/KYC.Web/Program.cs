using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using KYC.Application;
using KYC.Application.Interfaces;
using KYC.Infrastructure;
using KYC.Web.Hubs;
using KYC.Web.Security;
using KYC.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

var kvName = builder.Configuration["KeyVaultName"] ?? Environment.GetEnvironmentVariable("KYC_KEYVAULT_NAME");
if (!string.IsNullOrWhiteSpace(kvName))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{kvName}.vault.azure.net/"),
        new DefaultAzureCredential());
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IKycCaseRealtimeNotifier, HubKycCaseRealtimeNotifier>();

var azureAd = builder.Configuration.GetSection("AzureAd");
var useEntra = !string.IsNullOrWhiteSpace(azureAd["TenantId"]) && !string.IsNullOrWhiteSpace(azureAd["ClientId"]);
if (useEntra)
{
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(azureAd);
}
else
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Dev";
            options.DefaultAuthenticateScheme = "Dev";
            options.DefaultChallengeScheme = "Dev";
        })
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>("Dev", _ => { });
}

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("Analyst", p => p.RequireRole("KYC.Analyst", "KYC.Supervisor", "KYC.Admin"));
    options.AddPolicy("Supervisor", p => p.RequireRole("KYC.Supervisor", "KYC.Admin"));
    options.AddPolicy("Admin", p => p.RequireRole("KYC.Admin"));
    options.AddPolicy("Auditor", p => p.RequireRole("KYC.Auditor", "KYC.Admin"));
});

var razor = builder.Services.AddRazorPages();
if (useEntra)
    razor.AddMicrosoftIdentityUI();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationStateProvider, KYC.Web.Security.HttpContextAuthenticationStateProvider>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment() || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT")))
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapHub<KycCaseHub>("/hubs/kyc-case");
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToPage("/_Host");

app.MapGet("/api/cases/{caseId:guid}/report.pdf", async (
    Guid caseId,
    IKycReportPdfGenerator pdf,
    CancellationToken ct) =>
{
    var bytes = await pdf.GenerateAsync(caseId, ct);
    return Results.File(bytes, "application/pdf", $"kyc-report-{caseId}.pdf");
}).RequireAuthorization(policy => policy.RequireRole("KYC.Analyst", "KYC.Supervisor", "KYC.Admin"));

app.Run();
