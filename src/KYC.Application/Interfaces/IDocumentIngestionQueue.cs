namespace KYC.Application.Interfaces;

public interface IDocumentIngestionQueue
{
    ValueTask EnqueueAsync(Guid documentId, CancellationToken ct = default);
}
