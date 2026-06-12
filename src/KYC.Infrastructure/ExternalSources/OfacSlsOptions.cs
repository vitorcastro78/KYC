using Microsoft.Extensions.Configuration;

namespace KYC.Infrastructure.ExternalSources;

/// <summary>
/// OFAC Sanctions List Service (SLS) — https://sanctionslistservice.ofac.treas.gov
/// Gratuito, sem autenticação; requer header User-Agent (403 sem ele).
/// </summary>
public static class OfacSlsOptions
{
    public const string SectionKey = "ExternalSources:Ofac";

    public const string DefaultBaseUrl = "https://sanctionslistservice.ofac.treas.gov";

    public const string DefaultUserAgent = "KYC/1.0 (+https://example.com; OFAC SLS)";

    public const string DefaultDownloadFilename = "SDN_ADVANCED.XML";

    public static string GetBaseUrl(IConfiguration configuration) =>
        configuration[$"{SectionKey}:BaseUrl"]
        ?? configuration["ExternalSources:OfacBaseUrl"]
        ?? DefaultBaseUrl;

    public static string GetUserAgent(IConfiguration configuration) =>
        configuration[$"{SectionKey}:UserAgent"] ?? DefaultUserAgent;

    public static string GetDownloadFilename(IConfiguration configuration) =>
        configuration[$"{SectionKey}:DownloadFilename"] ?? DefaultDownloadFilename;

    /// <summary>GET /api/download/{filename} — redirect S3 com lista SDN actual.</summary>
    public static string ResolveDownloadUrl(IConfiguration configuration)
    {
        var explicitUrl = configuration["ExternalSources:OfacSdnDailyDownload:ExportUrl"];
        if (!string.IsNullOrWhiteSpace(explicitUrl))
            return explicitUrl;

        var baseUrl = GetBaseUrl(configuration).TrimEnd('/');
        var file = GetDownloadFilename(configuration);
        return $"{baseUrl}/api/download/{file}";
    }

    public static Uri AliveUri(IConfiguration configuration) =>
        new($"{GetBaseUrl(configuration).TrimEnd('/')}/alive");
}
