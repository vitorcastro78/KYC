using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace KYC.Infrastructure.Pdf;

/// <summary>Converte o documento HTML do relatÃ³rio em PDF via Chromium (Puppeteer).</summary>
public sealed class PuppeteerKycHtmlToPdfConverter(
    IConfiguration configuration,
    ILogger<PuppeteerKycHtmlToPdfConverter> log) : IKycHtmlToPdfConverter
{
    private static readonly SemaphoreSlim BrowserLock = new(1, 1);
    private static IBrowser? _browser;

    public async Task<byte[]> ConvertAsync(string htmlDocument, CancellationToken ct = default)
    {
        var browser = await GetBrowserAsync(ct).ConfigureAwait(false);
        await using var page = await browser.NewPageAsync().ConfigureAwait(false);
        await page.SetContentAsync(htmlDocument, new SetContentOptions
        {
            WaitUntil = [WaitUntilNavigation.Networkidle0]
        }).ConfigureAwait(false);

        var pdfOptions = new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "20mm",
                Bottom = "20mm",
                Left = "15mm",
                Right = "15mm"
            }
        };

        return await page.PdfDataAsync(pdfOptions).ConfigureAwait(false);
    }

    private async Task<IBrowser> GetBrowserAsync(CancellationToken ct)
    {
        if (_browser is { IsConnected: true })
            return _browser;

        await BrowserLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_browser is { IsConnected: true })
                return _browser;

            var fetcher = new BrowserFetcher();
            await fetcher.DownloadAsync().ConfigureAwait(false);

            var headless = configuration.GetValue("Reports:PdfHeadless", true);
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = headless,
                Args = ["--no-sandbox", "--disable-setuid-sandbox"]
            }).ConfigureAwait(false);

            log.LogInformation("Chromium iniciado para exportaÃ§Ã£o PDF de relatÃ³rios KYC.");
            return _browser;
        }
        finally
        {
            BrowserLock.Release();
        }
    }
}
