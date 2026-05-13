using Microsoft.Extensions.Options;

namespace KYC.Workers;

/// <summary>Descarrega periodicamente o XML consolidado de sanções financeiras da UE (FSF / webgate).</summary>
public sealed class EuFsfXmlDailyDownloadHostedService(
    IHttpClientFactory httpClientFactory,
    IHostEnvironment hostEnvironment,
    IOptionsMonitor<EuFsfXmlDailyDownloadOptions> options,
    ILogger<EuFsfXmlDailyDownloadHostedService> log) : BackgroundService
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
                "Sincronização XML FSF UE desactivada (ExternalSources:EuFsfDailyDownload:Enabled=false). " +
                "O serviço reavalia a configuração a cada 5 minutos.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var opts = options.CurrentValue;
            if (!opts.Enabled)
            {
                log.LogDebug(
                    "Sincronização XML FSF UE desactivada (ExternalSources:EuFsfDailyDownload:Enabled=false). Nova verificação em 5 min.");
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
            try
            {
                await DownloadOnceAsync(opts.ExportUrl, dest, stoppingToken).ConfigureAwait(false);
                var len = new FileInfo(dest).Length;
                log.LogInformation("Lista consolidada FSF UE actualizada: {Path} ({Size} bytes)", dest, len);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                log.LogWarning(ex, "Falha ao descarregar XML FSF UE de {Url}", opts.ExportUrl);
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
            var http = httpClientFactory.CreateClient("eu-fsf-export");
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

public sealed class EuFsfXmlDailyDownloadOptions
{
    public const string SectionKey = "ExternalSources:EuFsfDailyDownload";

    public bool Enabled { get; set; }

    public string ExportUrl { get; set; } =
        "https://webgate.ec.europa.eu/fsd/fsf/public/files/xmlFullSanctionsList_1_1/content?token=dG9rZW4tMjAxNw";

    public string LocalPath { get; set; } = "Data/eu-fsf/xmlFullSanctionsList_1_1.xml";

    public int IntervalHours { get; set; } = 24;
}
