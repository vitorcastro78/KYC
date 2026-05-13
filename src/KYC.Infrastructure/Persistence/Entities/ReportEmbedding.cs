using Pgvector;

namespace KYC.Infrastructure.Persistence.Entities;

public class ReportEmbedding
{
    public Guid Id { get; set; }
    public Guid KycCaseId { get; set; }
    public HalfVector Embedding { get; set; } = null!;
    public string ContentChunk { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
