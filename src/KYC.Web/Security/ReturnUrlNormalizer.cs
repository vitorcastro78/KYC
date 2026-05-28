namespace KYC.Web.Security;

public static class ReturnUrlNormalizer
{
    private static readonly string[] BlockedPathSegments =
    [
        "/Identity/Account/Login",
        "/Identity/Account/Logout",
        "/Identity/Account/AccessDenied"
    ];

    public static string Normalize(string? returnUrl, string fallback = "/")
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return fallback;

        if (!returnUrl.StartsWith('/') || returnUrl.StartsWith("//", StringComparison.Ordinal))
            return fallback;

        var path = returnUrl.Split('?', '#')[0];
        if (BlockedPathSegments.Any(blocked =>
                path.Contains(blocked, StringComparison.OrdinalIgnoreCase)))
            return fallback;

        return returnUrl;
    }
}
