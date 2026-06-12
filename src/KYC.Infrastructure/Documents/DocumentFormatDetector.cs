namespace KYC.Infrastructure.Documents;

public enum DocumentFormat
{
    Pdf,
    Docx,
    Jpeg,
    Png,
    Tiff,
    Unknown
}

public static class DocumentFormatDetector
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".jpg", ".jpeg", ".png", ".tif", ".tiff"
    };

    public static bool IsAllowedExtension(string fileName) =>
        AllowedExtensions.Contains(Path.GetExtension(fileName));

    public static DocumentFormat Detect(string fileName, Stream? content = null)
    {
        var ext = Path.GetExtension(fileName);
        if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            return DocumentFormat.Pdf;
        if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
            return DocumentFormat.Docx;
        if (ext is ".jpg" or ".jpeg")
            return DocumentFormat.Jpeg;
        if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
            return DocumentFormat.Png;
        if (ext is ".tif" or ".tiff")
            return DocumentFormat.Tiff;

        if (content is { CanRead: true, CanSeek: true })
        {
            var header = new byte[8];
            var read = content.Read(header, 0, header.Length);
            content.Seek(0, SeekOrigin.Begin);
            if (read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46)
                return DocumentFormat.Pdf;
            if (read >= 2 && header[0] == 0xFF && header[1] == 0xD8)
                return DocumentFormat.Jpeg;
            if (read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                return DocumentFormat.Png;
        }

        return DocumentFormat.Unknown;
    }
}
