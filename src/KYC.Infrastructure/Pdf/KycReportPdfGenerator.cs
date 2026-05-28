using KYC.Application.Interfaces;
using KYC.Infrastructure.Persistence;
using KYC.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Pdf;

public class KycReportPdfGenerator(
    KycDbContext db,
    IKycHtmlToPdfConverter htmlToPdf,
    ILogger<KycReportPdfGenerator> log) : IKycReportPdfGenerator
{
    public async Task<byte[]> GenerateAsync(Guid caseId, CancellationToken ct = default)
    {
        var kyc = await db.KycCases.AsNoTracking()
            .Include(c => c.FinalReport)
            .FirstOrDefaultAsync(c => c.Id == caseId, ct);

        var html = kyc?.FinalReport?.NarrativeHtml;
        if (string.IsNullOrWhiteSpace(html))
        {
            var title = kyc?.CompanyName ?? caseId.ToString();
            html = KycReportHtmlDocument.Wrap(
                "<main><p>Relatório indisponível. Execute a triagem automática do caso para gerar o documento.</p></main>",
                $"Relatório KYC — {title}");
        }

        try
        {
            return await htmlToPdf.ConvertAsync(html, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Falha ao converter HTML em PDF para o caso {CaseId}.", caseId);
            throw new InvalidOperationException(
                "Não foi possível exportar o PDF. Verifique se o Chromium (Puppeteer) está disponível no servidor.",
                ex);
        }
    }
}
