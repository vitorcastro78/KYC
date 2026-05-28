using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace KYC.Infrastructure.Documents;

public static class DocumentTextExtractor
{
    private const double LineGroupingTolerance = 4.0;
    private const int MinCharsPerPageForNativePdf = 50;

    public sealed record ExtractionResult(string Text, int PageCount, bool UsedVisionFallback);

    public static ExtractionResult ExtractPdf(Stream stream)
    {
        using var document = PdfDocument.Open(stream);
        var pageCount = document.NumberOfPages;
        var lines = new List<string>();
        foreach (var page in document.GetPages())
            lines.AddRange(ExtractLinesFromPage(page));

        var text = string.Join('\n', lines.Where(l => !string.IsNullOrWhiteSpace(l)));
        var avgChars = pageCount > 0 ? text.Length / pageCount : text.Length;
        return new ExtractionResult(text, pageCount, avgChars < MinCharsPerPageForNativePdf);
    }

    public static ExtractionResult ExtractDocx(Stream stream)
    {
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document.Body;
        if (body is null)
            return new ExtractionResult(string.Empty, 1, false);

        var text = string.Join('\n',
            body.Descendants<Text>()
                .Select(t => t.Text)
                .Where(t => !string.IsNullOrWhiteSpace(t)));
        return new ExtractionResult(text, 1, false);
    }

    public static ExtractionResult ExtractFromFormat(DocumentFormat format, Stream stream) =>
        format switch
        {
            DocumentFormat.Pdf => ExtractPdf(stream),
            DocumentFormat.Docx => ExtractDocx(stream),
            _ => new ExtractionResult(string.Empty, 0, true)
        };

    private static IEnumerable<string> ExtractLinesFromPage(Page page) =>
        page.GetWords()
            .GroupBy(w => Math.Round(w.BoundingBox.Bottom / LineGroupingTolerance) * LineGroupingTolerance)
            .OrderByDescending(g => g.Key)
            .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));
}
