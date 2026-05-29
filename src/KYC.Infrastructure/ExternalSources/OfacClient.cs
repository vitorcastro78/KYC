using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources;

public interface IOfacClient
{
    Task<IReadOnlyList<SanctionsListHit>> SearchAsync(string name, CancellationToken ct = default);
}

public record SanctionsListHit(string ListName, string MatchedName, double Score, string? Details);

/// <summary>
/// Triagem OFAC SDN via índice local do ficheiro SDN_ADVANCED.XML (SLS /api/download).
/// A API /entities?name= devolve o dataset completo (~100MB) — não usar para screening por nome.
/// </summary>
public class OfacClient(
    OfacSdnXmlLocalIndex localXml,
    ILogger<OfacClient> log) : IOfacClient
{
    public async Task<IReadOnlyList<SanctionsListHit>> SearchAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return [];

        var local = await localXml.TrySearchWhenLocalFileExistsAsync(name, ct).ConfigureAwait(false);
        if (local is not null)
            return local;

        if (localXml.ResolvePath() is null)
        {
            log.LogWarning(
                "OFAC SDN: ficheiro local indisponível. Active ExternalSources:OfacSdnDailyDownload nos Workers " +
                "ou descarregue SDN_ADVANCED.XML de /api/download/SDN_ADVANCED.XML (SLS).");
        }

        return [];
    }
}
