using Microsoft.AspNetCore.Http;

namespace KYC.Web.Security;

/// <summary>
/// Cookies compatíveis com iframe FinSight (shell e app em origens diferentes).
/// </summary>
public static class FinSightEmbedCookies
{
    public static bool IsEmbedMode(IConfiguration configuration) =>
        configuration.GetSection("FinSight:EmbedAncestors").Get<string[]>() is { Length: > 0 };

    public static void ApplyTo(CookieBuilder cookie, IConfiguration configuration)
    {
        var ancestors = configuration.GetSection("FinSight:EmbedAncestors").Get<string[]>() ?? [];
        var hasHttpAncestor = ancestors.Any(a =>
            a.StartsWith("http://", StringComparison.OrdinalIgnoreCase));
        var hasHttpsAncestor = ancestors.Any(a =>
            a.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

        cookie.IsEssential = true;

        // Dev Aspire: shell HTTP + apps HTTP — priorizar cookies compatíveis com HTTP.
        if (hasHttpAncestor || !hasHttpsAncestor)
        {
            cookie.SameSite = SameSiteMode.Lax;
            cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            return;
        }

        cookie.SameSite = SameSiteMode.None;
        cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
}
