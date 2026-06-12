using System.Security.Cryptography;
using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace KYC.Infrastructure.Documents;

public sealed class LocalCaseDocumentStorage(
    IConfiguration configuration,
    IHostEnvironment environment) : ICaseDocumentStorage
{
    public async Task<StoredDocumentRef> SaveAsync(
        Guid caseId,
        Guid documentId,
        Stream content,
        string fileName,
        CancellationToken ct = default)
    {
        var root = ResolveRoot();
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".bin";

        var relativePath = Path.Combine(caseId.ToString(), "documents", documentId.ToString(), $"original{ext}");
        var fullPath = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        var tempPath = fullPath + ".tmp";
        string hash;
        long sizeBytes;
        await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[81920];
            sizeBytes = 0;
            int read;
            while ((read = await content.ReadAsync(buffer, ct)) > 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, read), ct);
                hasher.AppendData(buffer.AsSpan(0, read));
                sizeBytes += read;
            }

            hash = Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
        }

        if (File.Exists(fullPath))
            File.Delete(fullPath);
        File.Move(tempPath, fullPath);
        return new StoredDocumentRef(NormalizeRelativePath(relativePath), hash, sizeBytes);
    }

    public Task<Stream> OpenReadAsync(string storageRelativePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(ResolveRoot(), storageRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Documento não encontrado.", fullPath);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageRelativePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(ResolveRoot(), storageRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    private string ResolveRoot()
    {
        var configured = configuration["Documents:StorageRoot"] ?? "Data/cases";
        return Path.IsPathRooted(configured)
            ? configured
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, configured));
    }

    private static string NormalizeRelativePath(string path) =>
        path.Replace('\\', '/');
}
