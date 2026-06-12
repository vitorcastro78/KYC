using KYC.Domain.Enums;

namespace KYC.Domain.Entities;

public class CaseDocument
{
    public Guid Id { get; private set; }
    public Guid KycCaseId { get; private set; }
    public Guid? CasePartyId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string Sha256 { get; private set; } = string.Empty;
    public string StorageRelativePath { get; private set; } = string.Empty;
    public CaseDocumentKind DocumentKind { get; private set; }
    public DocumentIngestionStatus IngestionStatus { get; private set; }
    public string? ExtractedText { get; private set; }
    public string? RawExtractionJson { get; private set; }
    public string? ExtractionModel { get; private set; }
    public string? ExtractionPromptHash { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public string UploadedBy { get; private set; } = string.Empty;
    public DateTime? ProcessedAt { get; private set; }

    public ICollection<DocumentExtractedFact> ExtractedFacts { get; } = new List<DocumentExtractedFact>();
    public ICollection<DocumentExtractedParty> ExtractedParties { get; } = new List<DocumentExtractedParty>();

    private CaseDocument()
    {
    }

    public static CaseDocument Create(
        Guid kycCaseId,
        string fileName,
        string contentType,
        long sizeBytes,
        string sha256,
        string storageRelativePath,
        CaseDocumentKind documentKind,
        string uploadedBy,
        Guid? casePartyId = null)
    {
        return new CaseDocument
        {
            Id = Guid.NewGuid(),
            KycCaseId = kycCaseId,
            CasePartyId = casePartyId,
            FileName = fileName.Trim(),
            ContentType = contentType.Trim(),
            SizeBytes = sizeBytes,
            Sha256 = sha256,
            StorageRelativePath = storageRelativePath,
            DocumentKind = documentKind,
            IngestionStatus = DocumentIngestionStatus.Pending,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy
        };
    }

    public void MarkProcessing()
    {
        if (IngestionStatus is DocumentIngestionStatus.Processing)
            return;
        IngestionStatus = DocumentIngestionStatus.Processing;
        FailureReason = null;
    }

    public void MarkCompleted(
        string extractedText,
        string? rawExtractionJson,
        string? extractionModel,
        string? extractionPromptHash)
    {
        IngestionStatus = DocumentIngestionStatus.Completed;
        ExtractedText = extractedText;
        RawExtractionJson = rawExtractionJson;
        ExtractionModel = extractionModel;
        ExtractionPromptHash = extractionPromptHash;
        FailureReason = null;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        IngestionStatus = DocumentIngestionStatus.Failed;
        FailureReason = reason.Length > 2000 ? reason[..2000] : reason;
        ProcessedAt = DateTime.UtcNow;
    }

    public void ReplaceExtractedData(
        IEnumerable<DocumentExtractedFact> facts,
        IEnumerable<DocumentExtractedParty> parties)
    {
        ExtractedFacts.Clear();
        ExtractedParties.Clear();
        foreach (var fact in facts)
        {
            if (fact.CaseDocumentId != Id || fact.KycCaseId != KycCaseId)
                throw new InvalidOperationException("Fact belongs to another document.");
            ExtractedFacts.Add(fact);
        }

        foreach (var party in parties)
        {
            if (party.CaseDocumentId != Id || party.KycCaseId != KycCaseId)
                throw new InvalidOperationException("Party belongs to another document.");
            ExtractedParties.Add(party);
        }
    }
}
