namespace KYC.Domain.Entities;

public class KycReport
{
    public Guid Id { get; private set; }
    public Guid KycCaseId { get; private set; }
    public string NarrativeMarkdown { get; private set; } = string.Empty;
    public string? ModelUsed { get; private set; }
    public DateTime GeneratedAt { get; private set; }

    private KycReport()
    {
    }

    public static KycReport Create(Guid kycCaseId, string narrativeMarkdown, string? modelUsed)
    {
        return new KycReport
        {
            Id = Guid.NewGuid(),
            KycCaseId = kycCaseId,
            NarrativeMarkdown = narrativeMarkdown,
            ModelUsed = modelUsed,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>Actualiza o relatório existente (mesmo Id) — evita substituir a navegação 1:1 e estados incorrectos no EF.</summary>
    public void UpdateContent(string narrativeMarkdown, string? modelUsed)
    {
        NarrativeMarkdown = narrativeMarkdown;
        ModelUsed = modelUsed;
        GeneratedAt = DateTime.UtcNow;
    }
}
