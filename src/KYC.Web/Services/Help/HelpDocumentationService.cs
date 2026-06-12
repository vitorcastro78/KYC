namespace KYC.Web.Services.Help;

public interface IHelpDocumentationService
{
    Task<HelpDocContent?> LoadAsync(string docId, bool includeTechnical, CancellationToken ct = default);
}

public sealed record HelpDocContent(
    HelpDocEntry Entry,
    string Html,
    IReadOnlyList<HelpHeading> Headings);

public sealed class HelpDocumentationService(
    IWebHostEnvironment environment,
    HelpMarkdownRenderer markdown) : IHelpDocumentationService
{
    public async Task<HelpDocContent?> LoadAsync(string docId, bool includeTechnical, CancellationToken ct = default)
    {
        var entry = HelpDocManifest.FindById(docId, includeTechnical);
        if (entry is null)
            return null;

        var path = ResolveHelpFilePath(entry);
        if (path is null)
            return null;

        var markdownText = await File.ReadAllTextAsync(path, ct);
        var headings = markdown.ExtractHeadings(markdownText);
        var html = markdown.ToHtml(markdownText, HelpDocManifest.FileNameToIdMap(includeTechnical));
        return new HelpDocContent(entry, html, headings);
    }

    private string? ResolveHelpFilePath(HelpDocEntry entry)
    {
        var subfolder = entry.Technical ? "help-technical" : "help-online";
        var candidates = new[]
        {
            Path.Combine(environment.WebRootPath, subfolder, entry.FileName),
            Path.Combine(environment.WebRootPath, "help", entry.FileName),
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "docs",
                entry.Technical ? "" : "help-online", entry.FileName))
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        if (entry.Technical)
        {
            var docsRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "docs", entry.FileName));
            if (File.Exists(docsRoot))
                return docsRoot;
        }

        return null;
    }
}
