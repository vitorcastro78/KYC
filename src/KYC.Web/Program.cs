using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using KYC.Application;
using KYC.Application.Interfaces;
using KYC.Infrastructure;
using KYC.Web.Hubs;
using KYC.Web.Security;
using KYC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped<KYC.Web.Services.KycHubConnectionFactory>();

var azureAd = builder.Configuration.GetSection("AzureAd");
var azureAdEnabled = azureAd.GetValue<bool?>("Enabled");
var useEntra = azureAdEnabled ?? (
    !string.IsNullOrWhiteSpace(azureAd["TenantId"]) &&
    !string.IsNullOrWhiteSpace(azureAd["ClientId"])
);
if (useEntra)
{
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(azureAd);
}
else
{
    var authConnectionString = builder.Configuration.GetConnectionString("KycDatabase")
        ?? builder.Configuration["KYC_DB_CONNECTION"]
        ?? throw new InvalidOperationException("Connection string KycDatabase or KYC_DB_CONNECTION required.");

    builder.Services.AddDbContext<AuthDbContext>(options =>
        options.UseNpgsql(authConnectionString));

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddDefaultTokenProviders()
        .AddDefaultUI();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.LogoutPath = "/Identity/Account/Logout";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.Events.OnRedirectToLogin = context =>
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/Identity/Account/Login");
                return Task.CompletedTask;
            }

            var returnUrl = ReturnUrlNormalizer.Normalize(
                context.Request.Path + context.Request.QueryString);
            context.Response.Redirect(
                $"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            return Task.CompletedTask;
        };
    });
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
var hasHttpsEndpointConfigured =
    !string.IsNullOrWhiteSpace(builder.Configuration["Kestrel:Endpoints:Https:Url"]) ||
    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT"));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (hasHttpsEndpointConfigured)
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (!useEntra)
    await SeedIdentityAsync(app.Services, app.Configuration);

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

static async Task SeedIdentityAsync(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = ["KYC.Admin", "KYC.Supervisor", "KYC.Analyst", "KYC.Auditor"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = configuration["Auth:AdminEmail"] ?? "admin@kyc.local";
    var adminPassword = configuration["Auth:AdminPassword"] ?? "Admin@1234";
    var adminName = configuration["Auth:AdminFullName"] ?? "Administrador KYC";

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin is null)
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = adminName,
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(admin, adminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed admin user: {errors}");
        }
    }

    foreach (var role in roles)
    {
        if (!await userManager.IsInRoleAsync(admin, role))
            await userManager.AddToRoleAsync(admin, role);
    }
}
