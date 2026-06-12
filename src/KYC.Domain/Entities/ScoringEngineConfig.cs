namespace KYC.Domain.Entities;

public class ScoringEngineConfig
{
    public Guid Id { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public DateTime ActiveFrom { get; private set; }
    public DateTime? ActiveTo { get; private set; }
    public bool IsActive { get; private set; }
    public string LocalModelName { get; private set; } = string.Empty;
    public string LocalModelVersion { get; private set; } = string.Empty;
    public string CloudModelName { get; private set; } = string.Empty;
    public string SystemPromptHash { get; private set; } = string.Empty;
    public string WeightsJson { get; private set; } = "{}";
    public string ApprovedBy { get; private set; } = string.Empty;
    public DateTime ApprovedAt { get; private set; }

    private ScoringEngineConfig()
    {
    }

    public static ScoringEngineConfig CreateDefault(string approvedBy, string promptHash) =>
        CreateVersion("1.0.0", "qwen3.5:9b", promptHash, approvedBy);

    public static ScoringEngineConfig CreateVersion(
        string version,
        string localModelName,
        string promptHash,
        string approvedBy)
    {
        return new ScoringEngineConfig
        {
            Id = Guid.NewGuid(),
            Version = version,
            ActiveFrom = DateTime.UtcNow,
            IsActive = true,
            LocalModelName = localModelName,
            LocalModelVersion = "latest",
            CloudModelName = "(Ollama local)",
            SystemPromptHash = promptHash,
            WeightsJson = "{\"sanctions\":0.25,\"pep\":0.15,\"adverse\":0.15,\"financial\":0.15,\"judicial\":0.15,\"ubo\":0.15}",
            ApprovedBy = approvedBy,
            ApprovedAt = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        ActiveTo = DateTime.UtcNow;
    }
}
