namespace KYC.Application.Interfaces;

/// <summary>
/// Persists KYC report knowledge for retrieval.
/// Implementation upserts Markdown into ContextMemory Global Wiki (CompanyBrain pattern);
/// no local pgvector / embeddings.
/// </summary>
public interface IReportEmbeddingWriter
{
    /// <summary>Upsert the case report into ContextMemory Global Wiki.</summary>
    Task EmbedReportTextAsync(Guid kycCaseId, string markdown, CancellationToken ct = default);

    /// <summary>Remove the case report document from ContextMemory Global Wiki.</summary>
    Task ClearEmbeddingsAsync(Guid kycCaseId, CancellationToken ct = default);
}
