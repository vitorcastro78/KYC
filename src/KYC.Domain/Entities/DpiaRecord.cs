namespace KYC.Domain.Entities;

public class DpiaRecord
{
    public Guid Id { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public DateTime ApprovedAt { get; private set; }
    public string ApprovedBy { get; private set; } = string.Empty;
    public DateTime NextReviewDue { get; private set; }
    public string DocumentStoragePath { get; private set; } = string.Empty;
    public string ProcessingActivitiesJson { get; private set; } = "[]";
    public string MitigationMeasuresJson { get; private set; } = "[]";
    public bool IsActive { get; private set; }

    private DpiaRecord()
    {
    }

    public static DpiaRecord Create(string version, string approvedBy, string documentPath)
    {
        return new DpiaRecord
        {
            Id = Guid.NewGuid(),
            Version = version,
            ApprovedAt = DateTime.UtcNow,
            ApprovedBy = approvedBy,
            NextReviewDue = DateTime.UtcNow.AddYears(1),
            DocumentStoragePath = documentPath,
            ProcessingActivitiesJson = "[\"KYC_screening\",\"UBO_graph_analysis\",\"LLM_risk_scoring\",\"document_ingestion\"]",
            MitigationMeasuresJson = "[\"Human_review_required\",\"Prompt_hash_audit\",\"Data_minimization\"]",
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;
}
