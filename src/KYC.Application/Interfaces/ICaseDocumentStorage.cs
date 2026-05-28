namespace KYC.Application.Interfaces;

public record StoredDocumentRef(string StorageRelativePath, string Sha256, long SizeBytes);

public interface ICaseDocumentStorage
{
    Task<StoredDocumentRef> SaveAsync(
        Guid caseId,
        Guid documentId,
        Stream content,
        string fileName,
        CancellationToken ct = default);

    Task<Stream> OpenReadAsync(string storageRelativePath, CancellationToken ct = default);

    Task DeleteAsync(string storageRelativePath, CancellationToken ct = default);
}
