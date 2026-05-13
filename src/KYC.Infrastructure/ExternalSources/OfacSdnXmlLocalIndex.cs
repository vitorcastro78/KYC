using System.Collections.Concurrent;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources;

/// <summary>
/// Índice em memória dos textos em &lt;NamePartValue&gt; do SDN_ADVANCED.XML.
/// Quando <see cref="ResolvePath"/> aponta para um ficheiro existente, a triagem OFAC usa só este ficheiro (sem HTTP).
/// </summary>
public sealed class OfacSdnXmlLocalIndex(
    IConfiguration configuration,
    IHostEnvironment hostEnvironment,
    ILogger<OfacSdnXmlLocalIndex> log)
{
    private const int MaxHits = 40;

    private readonly ConcurrentDictionary<string, byte> _names = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _loadGate = new(1, 1);
    private string? _loadedPath;
    private DateTime _loadedWriteTimeUtc;

    /// <summary>
    /// Caminho absoluto do XML, ou null se não configurado / ficheiro inexistente.
    /// </summary>
    public string? ResolvePath()
    {
        var raw = configuration["ExternalSources:OfacSdnXmlPath"]
                  ?? configuration["ExternalSources:OfacSdnDailyDownload:LocalPath"];
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var path = Path.IsPathRooted(raw)
            ? raw
            : Path.GetFullPath(Path.Combine(hostEnvironment.ContentRootPath, raw));

        return File.Exists(path) ? path : null;
    }

    /// <summary>
    /// Se o XML local existir, pesquisa apenas nele e devolve a lista (pode ser vazia).
    /// Se não existir fonte local, devolve null (o chamador usa HTTP).
    /// </summary>
    public async Task<IReadOnlyList<SanctionsListHit>?> TrySearchWhenLocalFileExistsAsync(
        string name,
        CancellationToken ct = default)
    {
        var path = ResolvePath();
        if (path is null)
            return null;

        await EnsureIndexAsync(path, ct).ConfigureAwait(false);

        var needle = name.Trim();
        if (needle.Length == 0)
            return [];

        var hits = new List<SanctionsListHit>();
        foreach (var entry in _names.Keys)
        {
            if (entry.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                hits.Add(new SanctionsListHit("OFAC SDN (XML local)", entry, 1.0, null));
                if (hits.Count >= MaxHits)
                    break;
            }
        }

        return hits;
    }

    private async Task EnsureIndexAsync(string path, CancellationToken ct)
    {
        DateTime writeUtc;
        try
        {
            writeUtc = File.GetLastWriteTimeUtc(path);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Não foi possível ler metadados do ficheiro SDN {Path}", path);
            return;
        }

        await _loadGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_names.Count > 0 && string.Equals(_loadedPath, path, StringComparison.Ordinal)
                                  && writeUtc == _loadedWriteTimeUtc)
                return;

            log.LogInformation("A carregar índice OFAC SDN a partir de {Path}…", path);
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                bufferSize: 262_144,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var settings = new XmlReaderSettings { Async = true, IgnoreWhitespace = true };
            using var reader = XmlReader.Create(stream, settings);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                ct.ThrowIfCancellationRequested();
                if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "NamePartValue")
                    continue;

                var text = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(text))
                    set.Add(text.Trim());
            }

            _names.Clear();
            foreach (var s in set)
                _names.TryAdd(s, 0);
            _loadedPath = path;
            _loadedWriteTimeUtc = writeUtc;
            log.LogInformation("Índice OFAC SDN: {Count} nomes distintos.", _names.Count);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Falha ao indexar SDN OFAC em {Path}", path);
        }
        finally
        {
            _loadGate.Release();
        }
    }
}
