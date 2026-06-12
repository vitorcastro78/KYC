namespace KYC.Application.Common;

public static class DocumentUploadRules
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".jpg", ".jpeg", ".png", ".tif", ".tiff"
    };

    public static bool IsAllowedExtension(string fileName) =>
        AllowedExtensions.Contains(Path.GetExtension(fileName));
}
