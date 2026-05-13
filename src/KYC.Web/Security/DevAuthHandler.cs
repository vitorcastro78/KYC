using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace KYC.Web.Security;

/// <summary>Autenticação local quando AzureAd:TenantId não está configurado.</summary>
public sealed class DevAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "dev-user"),
            new Claim(ClaimTypes.Name, "Developer"),
            new Claim(ClaimTypes.Role, "KYC.Analyst"),
            new Claim(ClaimTypes.Role, "KYC.Supervisor"),
            new Claim(ClaimTypes.Role, "KYC.Admin"),
            new Claim(ClaimTypes.Role, "KYC.Auditor")
        };
        var id = new ClaimsIdentity(claims, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(id), Scheme.Name)));
    }
}
