namespace KYC.Web.Security;

public static class ReturnUrlNormalizer
{
    private static readonly string[] BlockedPathSegments =
    [
        "/Identity/Account/Login",
        "/Identity/Account/Logout",
        "/Identity/Account/AccessDenied"
    ];

    public const string DefaultLandingPath = "/dashboard";

    public static string Normalize(string? returnUrl, string fallback = DefaultLandingPath)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return fallback;

        if (!returnUrl.StartsWith('/') || returnUrl.StartsWith("//", StringComparison.Ordinal))
            return fallback;

        var path = returnUrl.Split('?', '#')[0];
        if (path is "/" or "")
            return DefaultLandingPath;

        if (BlockedPathSegments.Any(blocked =>
                path.Contains(blocked, StringComparison.OrdinalIgnoreCase)))
            return fallback;

        return returnUrl;
    }
}
