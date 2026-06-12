using KYC.Domain.Enums;

namespace KYC.Domain.Entities;

public class DocumentExtractedFact
{
    public Guid Id { get; private set; }
    public Guid CaseDocumentId { get; private set; }
    public Guid KycCaseId { get; private set; }
    public DocumentFactKey FactKey { get; private set; }
    public string FactValue { get; private set; } = string.Empty;
    public decimal? Confidence { get; private set; }
    public int? SourcePage { get; private set; }
    public DateTime ExtractedAt { get; private set; }

    private DocumentExtractedFact()
    {
    }

    public static DocumentExtractedFact Create(
        Guid caseDocumentId,
        Guid kycCaseId,
        DocumentFactKey factKey,
        string factValue,
        decimal? confidence = null,
        int? sourcePage = null)
    {
        return new DocumentExtractedFact
        {
            Id = Guid.NewGuid(),
            CaseDocumentId = caseDocumentId,
            KycCaseId = kycCaseId,
            FactKey = factKey,
            FactValue = factValue.Trim(),
            Confidence = confidence,
            SourcePage = sourcePage,
            ExtractedAt = DateTime.UtcNow
        };
    }
}
