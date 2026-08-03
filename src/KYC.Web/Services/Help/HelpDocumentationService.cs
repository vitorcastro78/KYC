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
        var cultureFolder = ResolveCultureFolder();
        var candidates = new List<string>();

        void AddCandidates(string? culture)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                candidates.Add(Path.Combine(environment.WebRootPath, subfolder, culture, entry.FileName));
                candidates.Add(Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "docs",
                    entry.Technical ? culture : Path.Combine("help-online", culture), entry.FileName)));
            }
        }

        AddCandidates(cultureFolder);
        candidates.Add(Path.Combine(environment.WebRootPath, subfolder, entry.FileName));
        candidates.Add(Path.Combine(environment.WebRootPath, "help", entry.FileName));
        candidates.Add(Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "docs",
            entry.Technical ? "" : "help-online", entry.FileName)));

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

    private static string? ResolveCultureFolder()
    {
        var name = System.Globalization.CultureInfo.CurrentUICulture.Name;
        if (name.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            return "en";
        if (name.StartsWith("es", StringComparison.OrdinalIgnoreCase))
            return "es";
        if (name.StartsWith("pt", StringComparison.OrdinalIgnoreCase))
            return "pt";
        return null;
    }
}
