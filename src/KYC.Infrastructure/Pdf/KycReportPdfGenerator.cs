using KYC.Application.Interfaces;
using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KYC.Infrastructure.Pdf;

public class KycReportPdfGenerator(KycDbContext db) : IKycReportPdfGenerator
{
    static KycReportPdfGenerator() => QuestPDF.Settings.License = LicenseType.Community;

    public async Task<byte[]> GenerateAsync(Guid caseId, CancellationToken ct = default)
    {
        var kyc = await db.KycCases.AsNoTracking().Include(c => c.FinalReport).FirstOrDefaultAsync(c => c.Id == caseId, ct);
        var markdown = kyc?.FinalReport?.NarrativeMarkdown ?? "Relatório indisponível.";

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Text($"Relatório KYC — {caseId}").SemiBold().FontSize(18);
                page.Content().Text(markdown).FontSize(10);
                page.Footer().AlignRight().Text(t => t.Span("KYC AI Platform").FontSize(8));
            });
        });

        return doc.GeneratePdf();
    }
}
