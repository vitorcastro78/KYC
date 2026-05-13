namespace KYC.Domain.Entities;

public class AuditEntry
{
    public Guid Id { get; private set; }
    public Guid KycCaseId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string ActorId { get; private set; } = string.Empty;
    public string ActorType { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public string? LlmPromptHash { get; private set; }
    public DateTime Timestamp { get; private set; }

    private AuditEntry()
    {
    }

    public static AuditEntry Create(
        Guid kycCaseId,
        string action,
        string actorId,
        string actorType,
        string? details = null,
        string? llmPromptHash = null)
    {
        return new AuditEntry
        {
            Id = Guid.NewGuid(),
            KycCaseId = kycCaseId,
            Action = action,
            ActorId = actorId,
            ActorType = actorType,
            Details = details,
            LlmPromptHash = llmPromptHash,
            Timestamp = DateTime.UtcNow
        };
    }
}
