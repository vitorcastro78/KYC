namespace KYC.Infrastructure.LLM;

public sealed class ContextMemoryOptions
{
    public const string SectionName = "ContextMemory";

    /// <summary>ContextMemory gateway base URL (OpenAI-compatible <c>/v1</c>).</summary>
    public string BaseUrl { get; set; } = string.Empty;

    public string AppId { get; set; } = "kyc";

    public string ApiKey { get; set; } = string.Empty;

    public string UserId { get; set; } = "kyc-jobs";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl) && !string.IsNullOrWhiteSpace(ApiKey);
}
