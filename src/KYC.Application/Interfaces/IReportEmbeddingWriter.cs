namespace KYC.Application.Interfaces;

public interface IReportEmbeddingWriter
{
    /// <summary>Adiciona linhas a <c>ReportEmbeddings</c> sem gravar — o chamador deve fazer SaveChanges.</summary>
    Task StoreChunksAsync(Guid kycCaseId, IReadOnlyList<(string Chunk, float[] Vector)> chunks, CancellationToken ct = default);

    Task EmbedReportTextAsync(Guid kycCaseId, string markdown, CancellationToken ct = default);

    Task ClearEmbeddingsAsync(Guid kycCaseId, CancellationToken ct = default);
}