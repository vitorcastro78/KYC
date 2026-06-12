using KYC.Infrastructure.ExternalSources;
using Microsoft.Extensions.Options;

namespace KYC.Workers;

/// <summary>Descarrega periodicamente o SDN_ADVANCED.XML via OFAC SLS (/api/download).</summary>
public sealed class OfacSdnDailyDownloadHostedService(
    IHttpClientFactory httpClientFactory,
    IHostEnvironment hostEnvironment,
    IConfiguration configuration,
    IOptionsMonitor<OfacSdnDailyDownloadOptions> options,
    ILogger<OfacSdnDailyDownloadHostedService> log) : BackgroundService
{
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
                "Sincronização SDN OFAC desactivada (ExternalSources:OfacSdnDailyDownload:Enabled=false). " +
                "O serviço reavalia a configuração a cada 5 minutos.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var opts = options.CurrentValue;
            if (!opts.Enabled)
            {
                log.LogDebug(
                    "Sincronização SDN OFAC desactivada (ExternalSources:OfacSdnDailyDownload:Enabled=false). Nova verificação em 5 min.");
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

            var dest = ResolveLocalPath(opts.LocalPath);
            var url = OfacSlsOptions.ResolveDownloadUrl(configuration);
            try
            {
                await DownloadOnceAsync(url, dest, stoppingToken).ConfigureAwait(false);
                var len = new FileInfo(dest).Length;
                log.LogInformation("Lista SDN OFAC actualizada via SLS: {Path} ({Size} bytes)", dest, len);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                log.LogWarning(ex, "Falha ao descarregar SDN OFAC de {Url}", url);
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

    private string ResolveLocalPath(string relativeOrAbsolute)
    {
        if (Path.IsPathRooted(relativeOrAbsolute))
            return relativeOrAbsolute;
        return Path.Combine(hostEnvironment.ContentRootPath, relativeOrAbsolute);
    }

    private async Task DownloadOnceAsync(string url, string destPath, CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var tmp = destPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var http = httpClientFactory.CreateClient("ofac-sdn-export");
            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
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
}

public sealed class OfacSdnDailyDownloadOptions
{
    public const string SectionKey = "ExternalSources:OfacSdnDailyDownload";

    /// <summary>Quando false, o worker não descarrega (reavalia a cada 5 min).</summary>
    public bool Enabled { get; set; }

    /// <summary>Override opcional; por defeito usa SLS /api/download/SDN_ADVANCED.XML.</summary>
    public string? ExportUrl { get; set; }

    /// <summary>Caminho absoluto ou relativo ao ContentRoot do worker.</summary>
    public string LocalPath { get; set; } = "Data/ofac/SDN_ADVANCED.xml";

    /// <summary>Intervalo entre descargas (1–168 horas). Por defeito 24.</summary>
    public int IntervalHours { get; set; } = 24;
}
