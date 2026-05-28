using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KYC.Infrastructure.ExternalSources.At;
using Microsoft.Extensions.Options;

namespace KYC.Workers;

/// <summary>
/// Descarrega as listas públicas de devedores da AT (PDF) e converte para JSON estruturado
/// em Data/AT/Devedores/Singulares e Data/AT/Devedores/Coletivos.
/// </summary>
public sealed class AtDebtorsDailyDownloadHostedService(
    IHttpClientFactory httpClientFactory,
    IHostEnvironment hostEnvironment,
    IOptionsMonitor<AtDebtorsDailyDownloadOptions> options,
    ILogger<AtDebtorsDailyDownloadHostedService> log) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = AtDebtorsJson.SerializerOptions;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunLoopAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // encerramento normal
        }
    }

    private async Task RunLoopAsync(CancellationToken stoppingToken)
    {
        if (!options.CurrentValue.Enabled)
        {
            log.LogInformation(
                "Sincronização listas de devedores AT desactivada ({Section}:Enabled=false). " +
                "O serviço reavalia a configuração a cada 5 minutos.",
                AtDebtorsDailyDownloadOptions.SectionKey);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var opts = options.CurrentValue;
            if (!opts.Enabled)
            {
                log.LogDebug(
                    "Sincronização listas de devedores AT desactivada. Nova verificação em 5 min.");
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                continue;
            }

            try
            {
                await SyncAllTiersAsync(opts, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                log.LogWarning(ex, "Falha na sincronização das listas de devedores AT.");
            }

            var hours = Math.Clamp(opts.IntervalHours, 1, 168);
            try
            {
                await Task.Delay(TimeSpan.FromHours(hours), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task SyncAllTiersAsync(AtDebtorsDailyDownloadOptions opts, CancellationToken ct)
    {
        var dataRoot = ResolveLocalPath(opts.DataRootPath);
        Directory.CreateDirectory(dataRoot);

        var manifestTiers = new List<AtDebtorsManifestTier>();
        var syncedAt = DateTimeOffset.UtcNow;

        foreach (var tier in AtDebtorsTierCatalog.All)
        {
            ct.ThrowIfCancellationRequested();

            var tierDir = Path.Combine(dataRoot, tier.FolderName);
            var pdfDir = Path.Combine(tierDir, "_pdf");
            Directory.CreateDirectory(pdfDir);

            var pdfPath = Path.Combine(pdfDir, tier.PdfFileName);
            var jsonPath = Path.Combine(tierDir, $"{tier.Code}.json");
            var sourceUrl = BuildPdfUrl(opts.BaseUrl, tier.PdfFileName);

            await DownloadOnceAsync(sourceUrl, pdfPath, ct).ConfigureAwait(false);

            var parsed = AtDebtorsPdfParser.ParseFile(pdfPath);
            var document = new AtDebtorsTierDocument
            {
                SourceUrl = sourceUrl,
                PdfFileName = tier.PdfFileName,
                TierCode = tier.Code,
                TaxpayerType = tier.TaxpayerTypeLabel,
                DebtRangeLabel = AtDebtorsTextNormalizer.Normalize(tier.DebtRangeLabel),
                SourceUpdatedAt = parsed.SourceUpdatedAt?.ToString("yyyy-MM-dd", null),
                DownloadedAt = syncedAt,
                EntryCount = parsed.Entries.Count,
                Entries = parsed.Entries
                    .Select(e => new AtDebtorsTierEntry { Nif = e.Nif, Name = e.Name })
                    .ToList()
            };

            await WriteJsonAtomicAsync(jsonPath, document, ct).ConfigureAwait(false);

            manifestTiers.Add(new AtDebtorsManifestTier
            {
                TierCode = tier.Code,
                TaxpayerType = tier.TaxpayerTypeLabel,
                DebtRangeLabel = document.DebtRangeLabel,
                PdfRelativePath = ToRelative(dataRoot, pdfPath),
                JsonRelativePath = ToRelative(dataRoot, jsonPath),
                SourceUpdatedAt = document.SourceUpdatedAt,
                EntryCount = document.EntryCount
            });

            log.LogInformation(
                "Lista de devedores AT {Tier} actualizada: {JsonPath} ({Count} entradas, PDF {PdfSize} bytes).",
                tier.Code,
                jsonPath,
                document.EntryCount,
                new FileInfo(pdfPath).Length);
        }

        var manifest = new AtDebtorsManifest
        {
            SyncedAt = syncedAt,
            SourceIndexUrl = NormalizeBaseUrl(opts.BaseUrl) + "de-devedores.html",
            TierCount = manifestTiers.Count,
            TotalEntries = manifestTiers.Sum(t => t.EntryCount),
            Tiers = manifestTiers
        };

        var manifestPath = Path.Combine(dataRoot, "manifest.json");
        await WriteJsonAtomicAsync(manifestPath, manifest, ct).ConfigureAwait(false);

        log.LogInformation(
            "Manifest AT devedores actualizado: {ManifestPath} ({TierCount} escalões, {TotalEntries} entradas).",
            manifestPath,
            manifest.TierCount,
            manifest.TotalEntries);
    }

    private string ResolveLocalPath(string relativeOrAbsolute)
    {
        if (Path.IsPathRooted(relativeOrAbsolute))
            return relativeOrAbsolute;

        return Path.Combine(hostEnvironment.ContentRootPath, relativeOrAbsolute);
    }

    private static string BuildPdfUrl(string baseUrl, string pdfFileName) =>
        NormalizeBaseUrl(baseUrl) + pdfFileName;

    private static string NormalizeBaseUrl(string baseUrl)
    {
        var trimmed = baseUrl.Trim();
        return trimmed.EndsWith('/') ? trimmed : trimmed + "/";
    }

    private static string ToRelative(string root, string fullPath) =>
        Path.GetRelativePath(root, fullPath).Replace('\\', '/');

    private async Task DownloadOnceAsync(string url, string destPath, CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var tmp = destPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var http = httpClientFactory.CreateClient("at-devedores-export");
            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            await using (var network = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false))
            await using (var fs = new FileStream(tmp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 262_144,
                         FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await network.CopyToAsync(fs, ct).ConfigureAwait(false);
            }

            File.Move(tmp, destPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tmp))
            {
                try
                {
                    File.Delete(tmp);
                }
                catch (Exception ex)
                {
                    log.LogDebug(ex, "Não foi possível apagar ficheiro temporário {Tmp}", tmp);
                }
            }
        }
    }

    private static async Task WriteJsonAtomicAsync<T>(string destPath, T payload, CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var tmp = destPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            await File.WriteAllTextAsync(tmp, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), ct)
                .ConfigureAwait(false);
            File.Move(tmp, destPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tmp))
                File.Delete(tmp);
        }
    }
}
