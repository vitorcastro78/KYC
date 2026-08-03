using KYC.Application.Models;

namespace KYC.Application.Interfaces;

/// <summary>Lists LLM models from ContextMemory gateway.</summary>
public interface ILlmModelCatalog
{
    Task<LlmModelsListResult> ListAsync(CancellationToken cancellationToken = default);
}
