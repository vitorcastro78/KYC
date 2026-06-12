namespace KYC.Domain.Entities;

public class KycReport
{
    public Guid Id { get; private set; }
    public Guid KycCaseId { get; private set; }
    public string NarrativeHtml { get; private set; } = string.Empty;
    public string? ModelUsed { get; private set; }
    public DateTime GeneratedAt { get; private set; }

    private KycReport()
    {
    }

    public static KycReport Create(Guid kycCaseId, string narrativeHtml, string? modelUsed)
    {
        return new KycReport
        {
            Id = Guid.NewGuid(),
            KycCaseId = kycCaseId,
            NarrativeHtml = narrativeHtml,
            ModelUsed = modelUsed,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public void UpdateContent(string narrativeHtml, string? modelUsed)
    {
        NarrativeHtml = narrativeHtml;
        ModelUsed = modelUsed;
        GeneratedAt = DateTime.UtcNow;
    }
}
