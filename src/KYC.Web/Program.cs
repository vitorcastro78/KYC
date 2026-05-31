using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using KYC.Application;
using KYC.Application.Cases;
using KYC.Application.Interfaces;
using KYC.Domain.Enums;
using KYC.Infrastructure;
using MediatR;
using KYC.Web.Endpoints;
using KYC.Web.Hubs;
using KYC.Web.OpenApi;
using KYC.Web.Security;
using KYC.Web.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
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

var behindProxy = builder.Configuration.IsBehindReverseProxy(builder.Environment);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddKycProductionHosting(builder.Configuration, builder.Environment, builder.Environment.ContentRootPath);
builder.Services.AddSingleton<IKycCaseRealtimeNotifier, HubKycCaseRealtimeNotifier>();
builder.Services.AddScoped<KYC.Web.Services.KycHubConnectionFactory>();
builder.Services.AddSingleton<ToastService>();

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
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        // Proxy TLS (nginx): Kestrel vê HTTP; forçar Secure para o browser em https://
        options.Cookie.SecurePolicy = behindProxy
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
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

builder.Services.AddScoped<ICurrentAnalystAccessor, HttpContextAnalystAccessor>();

if (useEntra)
{
    if (!string.IsNullOrWhiteSpace(builder.Configuration["Compliance:SupervisorGroupObjectId"])
        && (!string.IsNullOrWhiteSpace(builder.Configuration["AzureAd:ClientSecret"])
            || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AZURE_AD_CLIENT_SECRET"))))
        builder.Services.AddSingleton<ISupervisorUserDirectory, EntraGraphSupervisorUserDirectory>();
    else
        builder.Services.AddSingleton<ISupervisorUserDirectory, ConfigSupervisorUserDirectory>();
}
else
    builder.Services.AddScoped<ISupervisorUserDirectory, IdentitySupervisorUserDirectory>();

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
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
builder.Services.AddControllersWithViews();
builder.Services.AddKycOpenApiDocumentation();

var app = builder.Build();
var hasHttpsEndpointConfigured =
    !string.IsNullOrWhiteSpace(builder.Configuration["Kestrel:Endpoints:Https:Url"]) ||
    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT"));

app.UseKycProductionHosting();

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

if (!useEntra && !app.Environment.IsEnvironment("Testing"))
    await SeedIdentityAsync(app.Services, app.Configuration);

app.MapGet("/", (HttpContext ctx) =>
    ctx.User.Identity?.IsAuthenticated == true
        ? Results.Redirect("/dashboard")
        : Results.Redirect("/Identity/Account/Login?returnUrl=%2Fdashboard"))
    .AllowAnonymous();

app.MapBlazorHub();
app.MapHub<KycCaseHub>("/hubs/kyc-case");
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToPage("/_Host");

app.MapHealthChecks("/health");

app.MapIdentityWebhookEndpoints();

app.MapGet("/api/admin/compliance/metrics", async (IComplianceMetricsService metrics, CancellationToken ct) =>
{
    var bundle = await metrics.GetMetricsAsync(ct);
    return Results.Ok(bundle);
}).RequireAuthorization(policy => policy.RequireRole("KYC.Admin", "KYC.Auditor"))
.WithName("GetComplianceMetrics")
.WithTags("Compliance", "Admin")
.WithSummary("Métricas de triagem (FP/FN) e biometria (FRR, liveness)")
.Produces(StatusCodes.Status200OK);

app.MapGet("/api/openapi/info", () => Results.Ok(new
{
    title = "KYC AI Platform API",
    version = "v1",
    swagger = "/swagger",
    spec = "/swagger/v1/swagger.json",
    health = "/health"
})).AllowAnonymous()
.WithName("OpenApiInfo")
.WithTags("Meta");

app.MapGet("/api/admin/aml-reports/{reportId:guid}/export", async (
    Guid reportId,
    string? format,
    IAmlComplianceReportService svc,
    CancellationToken ct) =>
{
    var useBdp = string.Equals(format, "bdp", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(format, "xml", StringComparison.OrdinalIgnoreCase);
    var stream = useBdp
        ? await svc.ExportRpbBdpAsync(reportId, ct)
        : await svc.ExportRpbAsync(reportId, ct);
    var contentType = useBdp ? "application/xml" : "application/json";
    var ext = useBdp ? "xml" : "json";
    return Results.File(stream, contentType, $"rpb-{reportId}.{ext}");
}).RequireAuthorization(policy => policy.RequireRole("KYC.Admin"));

app.MapGet("/api/cases/{caseId:guid}/report.pdf", async (
    Guid caseId,
    IKycReportPdfGenerator pdf,
    CancellationToken ct) =>
{
    var bytes = await pdf.GenerateAsync(caseId, ct);
    return Results.File(bytes, "application/pdf", $"kyc-report-{caseId}.pdf");
}).RequireAuthorization(policy => policy.RequireRole("KYC.Analyst", "KYC.Supervisor", "KYC.Admin"));

app.MapPost("/api/cases/{caseId:guid}/documents", async (
    Guid caseId,
    HttpRequest request,
    IMediator mediator,
    CancellationToken ct) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("multipart/form-data required");

    var form = await request.ReadFormAsync(ct);
    var file = form.Files.GetFile("file");
    if (file is null || file.Length == 0)
        return Results.BadRequest("Ficheiro em falta.");

    if (!Enum.TryParse<CaseDocumentKind>(form["kind"], ignoreCase: true, out var kind))
        kind = CaseDocumentKind.Other;

    Guid? casePartyId = null;
    if (Guid.TryParse(form["casePartyId"], out var pid))
        casePartyId = pid;

    var actorId = request.HttpContext.User.Identity?.Name ?? "System";
    await using var stream = file.OpenReadStream();
    var documentId = await mediator.Send(new UploadCaseDocumentCommand(
        caseId,
        actorId,
        file.FileName,
        file.ContentType,
        stream,
        kind,
        casePartyId), ct);

    return Results.Accepted($"/api/cases/{caseId}/documents/{documentId}", new { documentId });
}).RequireAuthorization(policy => policy.RequireRole("KYC.Analyst", "KYC.Supervisor", "KYC.Admin"));

app.MapGet("/api/cases/{caseId:guid}/documents/{documentId:guid}/file", async (
    Guid caseId,
    Guid documentId,
    IKycCaseRepository cases,
    ICaseDocumentStorage storage,
    CancellationToken ct) =>
{
    var kyc = await cases.GetByIdAsync(caseId, ct);
    var doc = kyc?.Documents.FirstOrDefault(d => d.Id == documentId);
    if (doc is null)
        return Results.NotFound();

    await using var stream = await storage.OpenReadAsync(doc.StorageRelativePath, ct);
    using var ms = new MemoryStream();
    await stream.CopyToAsync(ms, ct);
    return Results.File(ms.ToArray(), doc.ContentType, doc.FileName);
}).RequireAuthorization(policy => policy.RequireRole("KYC.Analyst", "KYC.Supervisor", "KYC.Admin"));

app.MapGet("/api/cases/{caseId:guid}/documents/{documentId:guid}/text", async (
    Guid caseId,
    Guid documentId,
    IKycCaseRepository cases,
    CancellationToken ct) =>
{
    var kyc = await cases.GetByIdAsync(caseId, ct);
    var doc = kyc?.Documents.FirstOrDefault(d => d.Id == documentId);
    if (doc is null || string.IsNullOrWhiteSpace(doc.ExtractedText))
        return Results.NotFound();
    return Results.Text(doc.ExtractedText, "text/plain; charset=utf-8");
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

public partial class Program;
