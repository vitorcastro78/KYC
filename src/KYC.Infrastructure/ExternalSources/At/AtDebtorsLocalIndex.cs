using System.Text.Json;
using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources.At;

public sealed class AtDebtorsLocalIndex(
    IConfiguration configuration,
    IHostEnvironment hostEnvironment,
    ILogger<AtDebtorsLocalIndex> log) : IAtDebtorsLocalIndex
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly object _sync = new();
    private Dictionary<string, AtDebtorsIndexMatch>? _index;
    private DateTime _manifestWriteUtc;

    public AtDebtorsIndexMatch? FindByNif(string nif)
    {
        if (string.IsNullOrWhiteSpace(nif))
            return null;

        var normalized = nif.Trim();
        EnsureLoaded();
        return _index!.TryGetValue(normalized, out var match) ? match : null;
    }

    private void EnsureLoaded()
    {
        var manifestPath = ResolveManifestPath();
        if (!File.Exists(manifestPath))
        {
            lock (_sync)
            {
                _index ??= new Dictionary<string, AtDebtorsIndexMatch>(StringComparer.Ordinal);
            }

            return;
        }

        var writeUtc = File.GetLastWriteTimeUtc(manifestPath);
        lock (_sync)
        {
            if (_index is not null && writeUtc == _manifestWriteUtc)
                return;

            _index = LoadIndex(manifestPath);
            _manifestWriteUtc = writeUtc;
        }
    }

    private Dictionary<string, AtDebtorsIndexMatch> LoadIndex(string manifestPath)
    {
        var dataRoot = Path.GetDirectoryName(manifestPath)
                       ?? ResolveDataRoot();

        AtDebtorsManifest? manifest;
        try
        {
            using var stream = File.OpenRead(manifestPath);
            manifest = JsonSerializer.Deserialize<AtDebtorsManifest>(stream, JsonOptions);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Falha ao ler manifest AT devedores em {Path}.", manifestPath);
            return new Dictionary<string, AtDebtorsIndexMatch>(StringComparer.Ordinal);
        }

        if (manifest?.Tiers is not { Count: > 0 })
            return new Dictionary<string, AtDebtorsIndexMatch>(StringComparer.Ordinal);

        var index = new Dictionary<string, AtDebtorsIndexMatch>(StringComparer.Ordinal);
        foreach (var tier in manifest.Tiers)
        {
            var jsonPath = Path.Combine(dataRoot, tier.JsonRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(jsonPath))
            {
                log.LogDebug("JSON AT devedores em falta: {Path}", jsonPath);
                continue;
            }

            try
            {
                using var stream = File.OpenRead(jsonPath);
                var document = JsonSerializer.Deserialize<AtDebtorsTierDocument>(stream, JsonOptions);
                if (document?.Entries is null)
                    continue;

                foreach (var entry in document.Entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.Nif))
                        continue;

                    index[entry.Nif.Trim()] = new AtDebtorsIndexMatch(
                        entry.Nif.Trim(),
                        entry.Name,
                        document.TierCode,
                        document.TaxpayerType,
                        document.DebtRangeLabel,
                        document.SourceUpdatedAt);
                }
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Falha ao indexar JSON AT devedores {Path}.", jsonPath);
            }
        }

        log.LogInformation("Índice AT devedores carregado: {Count} NIF(s) a partir de {Manifest}.",
            index.Count,
            manifestPath);

        return index;
    }

    private string ResolveManifestPath() =>
        Path.Combine(ResolveDataRoot(), "manifest.json");

    private string ResolveDataRoot()
    {
        var configured = configuration["ExternalSources:AtDebtorsDailyDownload:DataRootPath"]
                         ?? "Data/AT/Devedores";
        if (Path.IsPathRooted(configured))
            return configured;

        return Path.Combine(hostEnvironment.ContentRootPath, configured);
    }
}
