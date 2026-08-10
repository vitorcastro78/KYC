using Microsoft.Extensions.Configuration;

namespace KYC.Infrastructure.LLM;

/// <summary>Reads LLM runtime settings (model id resolved via ContextMemory gateway).</summary>
internal static class LlmOptions
{
    public static string GetModel(IConfiguration configuration) =>
        configuration["LLM:Model"] ?? "qwen3.5:9b";
}
