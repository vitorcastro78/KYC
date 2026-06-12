namespace KYC.Application.Interfaces;

public interface IDocumentIngestionService
{
    Task ProcessDocumentAsync(Guid documentId, CancellationToken ct = default);
}
