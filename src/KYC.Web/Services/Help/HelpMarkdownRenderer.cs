using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;

namespace KYC.Web.Services.Help;

public sealed record HelpHeading(string Anchor, string Text, int Level);

public sealed partial class HelpMarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseBootstrap()
        .Build();

    public string ToHtml(string markdown, IReadOnlyDictionary<string, string> fileNameToId)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var withAnchors = InjectHeadingAnchors(markdown, out _);
        var html = Markdown.ToHtml(withAnchors, Pipeline);
        return RewriteInternalLinks(html, fileNameToId);
    }

    public IReadOnlyList<HelpHeading> ExtractHeadings(string markdown)
    {
        InjectHeadingAnchors(markdown, out var headings);
        return headings;
    }

    private static string InjectHeadingAnchors(string markdown, out List<HelpHeading> headings)
    {
        headings = [];
        var sb = new StringBuilder();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in markdown.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            var match = HeadingLine().Match(trimmed);
            if (!match.Success)
            {
                sb.AppendLine(trimmed);
                continue;
            }

            var level = match.Groups["hashes"].Value.Length;
            if (level is < 2 or > 3)
            {
                sb.AppendLine(trimmed);
                continue;
            }

            var text = match.Groups["text"].Value.Trim();
            var anchor = UniqueAnchor(Slugify(text), used);
            headings.Add(new HelpHeading(anchor, text, level));
            sb.AppendLine($"<a id=\"help-anchor-{anchor}\"></a>");
            sb.AppendLine(trimmed);
        }

        return sb.ToString();
    }

    private static string UniqueAnchor(string baseSlug, HashSet<string> used)
    {
        var slug = string.IsNullOrEmpty(baseSlug) ? "secao" : baseSlug;
        if (used.Add(slug))
            return slug;

        var i = 2;
        while (!used.Add($"{slug}-{i}"))
            i++;

        return $"{slug}-{i}";
    }

    private static string Slugify(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
                sb.Append(char.ToLowerInvariant(c));
            else if (char.IsWhiteSpace(c) || c is '-' or '_')
                sb.Append('-');
        }

        return Regex.Replace(sb.ToString(), "-+", "-").Trim('-');
    }

    private static string RewriteInternalLinks(string html, IReadOnlyDictionary<string, string> fileNameToId)
    {
        return DocLinkPattern().Replace(html, match =>
        {
            var href = match.Groups["href"].Value;
            var fileName = Path.GetFileName(href.TrimStart('.', '/'));
            if (!fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                return match.Value;

            if (!fileNameToId.TryGetValue(fileName, out var docId))
                return match.Value;

            return $"href=\"/help/{docId}\"";
        });
    }

    [GeneratedRegex(@"^(?<hashes>#{2,3})\s+(?<text>.+)$")]
    private static partial Regex HeadingLine();

    [GeneratedRegex("href=\"(?<href>[^\"]+\\.md[^\"]*)\"", RegexOptions.IgnoreCase)]
    private static partial Regex DocLinkPattern();
}
