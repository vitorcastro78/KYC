using System.Security.Cryptography;
using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace KYC.Infrastructure.Documents;

public sealed class LocalDpiaDocumentStorage(
    IConfiguration configuration,
    IHostEnvironment environment) : IDpiaDocumentStorage
{
    public async Task<StoredDocumentRef> SaveAsync(
        string version,
        Stream content,
        string fileName,
        CancellationToken ct = default)
    {
        var safeVersion = string.Concat(version.Where(c => char.IsLetterOrDigit(c) || c is '.' or '-' or '_'));
        if (string.IsNullOrWhiteSpace(safeVersion))
            safeVersion = "unknown";

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".pdf";

        var relativePath = Path.Combine("dpia", safeVersion, $"dpia{ext}");
        var fullPath = Path.Combine(ResolveRoot(), relativePath);
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
