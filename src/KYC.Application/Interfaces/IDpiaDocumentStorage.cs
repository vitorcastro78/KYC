namespace KYC.Application.Interfaces;

public interface IDpiaDocumentStorage
{
    Task<StoredDocumentRef> SaveAsync(string version, Stream content, string fileName, CancellationToken ct = default);
}
