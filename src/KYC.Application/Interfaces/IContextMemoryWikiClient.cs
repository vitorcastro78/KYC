using KYC.Application.Models;

namespace KYC.Application.Interfaces;

/// <summary>ContextMemory Global Wiki + models client (CompanyBrain pattern).</summary>
public interface IContextMemoryWikiClient
{
    Task<WikiUpsertResult> IngestDocumentAsync(
        string documentId,
        WikiUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);

    Task<WikiQueryResult> QueryAsync(
        WikiQueryRequest request,
        CancellationToken cancellationToken = default);

    /// <summary><c>GET /v1/models</c> — active model has <c>owned_by=active</c>.</summary>
    Task<LlmModelsListResult> ListModelsAsync(CancellationToken cancellationToken = default);
}
