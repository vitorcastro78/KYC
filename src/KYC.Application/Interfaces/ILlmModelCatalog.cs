using KYC.Application.Models;

namespace KYC.Application.Interfaces;

/// <summary>Lists LLM models from ContextMemory (preferred) or direct Ollama fallback.</summary>
public interface ILlmModelCatalog
{
    Task<LlmModelsListResult> ListAsync(CancellationToken cancellationToken = default);
}
