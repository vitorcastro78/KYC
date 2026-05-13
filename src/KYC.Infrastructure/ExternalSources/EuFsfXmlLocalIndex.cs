using System.Collections.Concurrent;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources;

/// <summary>
/// Índice em memória de nomes do XML consolidado FSF da UE (lista financeira).
/// Prioriza &lt;nameAlias&gt; (atributos wholeName / firstName / …); inclui elementos de nome legados.
/// </summary>
public sealed class EuFsfXmlLocalIndex(
    IConfiguration configuration,
    IHostEnvironment hostEnvironment,
    ILogger<EuFsfXmlLocalIndex> log)
{
    private const int MaxHits = 40;

    private readonly ConcurrentDictionary<string, byte> _names = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _loadGate = new(1, 1);
    private string? _loadedPath;
    private DateTime _loadedWriteTimeUtc;

    public string? ResolvePath()
    {
        var raw = configuration["ExternalSources:EuFsfXmlPath"]
                  ?? configuration["ExternalSources:EuFsfDailyDownload:LocalPath"];
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var path = Path.IsPathRooted(raw)
            ? raw
            : Path.GetFullPath(Path.Combine(hostEnvironment.ContentRootPath, raw));

        return File.Exists(path) ? path : null;
    }

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
                hits.Add(new SanctionsListHit("UE FSF (XML local)", entry, 1.0, null));
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
            log.LogWarning(ex, "Não foi possível ler metadados do ficheiro FSF UE {Path}", path);
            return;
        }

        await _loadGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_names.Count > 0 && string.Equals(_loadedPath, path, StringComparison.Ordinal)
                                  && writeUtc == _loadedWriteTimeUtc)
                return;

            log.LogInformation("A carregar índice FSF UE a partir de {Path}…", path);
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
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                var ln = reader.LocalName;
                if (ln.Equals("nameAlias", StringComparison.OrdinalIgnoreCase))
                {
                    await ConsumeNameAliasAsync(reader, set, ct).ConfigureAwait(false);
                    continue;
                }

                if (IsLegacyNameElement(ln))
                {
                    var text = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    Add(set, text);
                }
            }

            _names.Clear();
            foreach (var s in set)
                _names.TryAdd(s, 0);
            _loadedPath = path;
            _loadedWriteTimeUtc = writeUtc;
            log.LogInformation("Índice FSF UE: {Count} nomes distintos.", _names.Count);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Falha ao indexar FSF UE em {Path}", path);
        }
        finally
        {
            _loadGate.Release();
        }
    }

    private static bool IsLegacyNameElement(string localName) =>
        localName.Equals("WholeName", StringComparison.OrdinalIgnoreCase)
        || localName.Equals("LastName", StringComparison.OrdinalIgnoreCase)
        || localName.Equals("FirstName", StringComparison.OrdinalIgnoreCase)
        || localName.Equals("MiddleName", StringComparison.OrdinalIgnoreCase);

    private static async Task ConsumeNameAliasAsync(XmlReader reader, HashSet<string> set, CancellationToken ct)
    {
        AddFromNameAliasAttributes(reader, set);
        if (reader.IsEmptyElement)
            return;

        var depth = reader.Depth;
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (reader.NodeType == XmlNodeType.EndElement
                && reader.Depth == depth
                && reader.LocalName.Equals("nameAlias", StringComparison.OrdinalIgnoreCase))
                return;

            if (reader.NodeType != XmlNodeType.Element || !IsLegacyNameElement(reader.LocalName))
                continue;

            var text = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
            Add(set, text);
        }
    }

    private static void AddFromNameAliasAttributes(XmlReader reader, HashSet<string> set)
    {
        Add(set, reader.GetAttribute("wholeName"));
        Add(set, reader.GetAttribute("WholeName"));
        var fn = reader.GetAttribute("firstName") ?? reader.GetAttribute("FirstName");
        var mn = reader.GetAttribute("middleName") ?? reader.GetAttribute("MiddleName");
        var ln = reader.GetAttribute("lastName") ?? reader.GetAttribute("LastName");
        var joined = string.Join(' ', new[] { fn, mn, ln }.Where(s => !string.IsNullOrWhiteSpace(s)));
        Add(set, joined);
    }

    private static void Add(HashSet<string> set, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        var t = value.Trim();
        if (t.Length > 0)
            set.Add(t);
    }
}
