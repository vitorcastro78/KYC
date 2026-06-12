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
public sealed class OfacClient : IOfacClient
{
    private readonly OfacSdnXmlLocalIndex _localXml;
    private readonly ILogger<OfacClient> _log;

    public OfacClient(OfacSdnXmlLocalIndex localXml, ILogger<OfacClient> log)
    {
        _localXml = localXml;
        _log = log;
    }

    public async Task<IReadOnlyList<SanctionsListHit>> SearchAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return [];

        var local = await _localXml.TrySearchWhenLocalFileExistsAsync(name, ct).ConfigureAwait(false);
        if (local is not null)
            return local;

        if (_localXml.ResolvePath() is null)
        {
            _log.LogWarning(
                "OFAC SDN: ficheiro local indisponível. Active ExternalSources:OfacSdnDailyDownload nos Workers " +
                "ou descarregue SDN_ADVANCED.XML de /api/download/SDN_ADVANCED.XML (SLS).");
        }

        return [];
    }
}
