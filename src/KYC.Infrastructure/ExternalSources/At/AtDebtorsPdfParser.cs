using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace KYC.Infrastructure.ExternalSources.At;

public static partial class AtDebtorsPdfParser
{
    private const double LineGroupingTolerance = 4.0;

    private static readonly Regex EntryStart = EntryStartRegex();
    private static readonly Regex UpdatedAt = UpdatedAtRegex();
    private static readonly Regex PageHeader = PageHeaderRegex();
    private static readonly Regex FlatTextNifMarker = FlatTextNifMarkerRegex();

    public sealed record ParsedAtDebtorsList(
        DateOnly? SourceUpdatedAt,
        IReadOnlyList<AtDebtorsParsedEntry> Entries);

    public sealed record AtDebtorsParsedEntry(string Nif, string Name);

    public static ParsedAtDebtorsList ParseFile(string pdfPath)
    {
        using var document = PdfDocument.Open(pdfPath);
        var lines = new List<string>();
        foreach (var page in document.GetPages())
            lines.AddRange(ExtractLinesFromPage(page));

        var parsed = ParseLines(lines);
        if (parsed.Entries.Count > 0)
            return parsed;

        var flatText = string.Concat(document.GetPages().Select(p => p.Text));
        return ParseFlatText(flatText);
    }

    public static ParsedAtDebtorsList ParseText(string text) =>
        ParseLines(text.Split('\n', StringSplitOptions.RemoveEmptyEntries));

    private static IEnumerable<string> ExtractLinesFromPage(Page page)
    {
        return page.GetWords()
            .GroupBy(w => Math.Round(w.BoundingBox.Bottom / LineGroupingTolerance) * LineGroupingTolerance)
            .OrderByDescending(g => g.Key)
            .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));
    }

    private static ParsedAtDebtorsList ParseLines(IEnumerable<string> rawLines)
    {
        DateOnly? sourceUpdatedAt = null;
        var entries = new List<AtDebtorsParsedEntry>();
        AtDebtorsParsedEntry? current = null;

        foreach (var rawLine in rawLines)
        {
            var line = NormalizeLine(rawLine);
            if (line.Length == 0)
                continue;

            if (sourceUpdatedAt is null)
            {
                var updatedMatch = UpdatedAt.Match(line);
                if (updatedMatch.Success
                    && DateOnly.TryParseExact(
                        updatedMatch.Groups[1].Value,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsedDate))
                {
                    sourceUpdatedAt = parsedDate;
                }
            }

            if (IsSkippableLine(line))
            {
                if (current is not null)
                {
                    entries.Add(current);
                    current = null;
                }

                continue;
            }

            var entryMatch = EntryStart.Match(line);
            if (entryMatch.Success)
            {
                if (current is not null)
                    entries.Add(current);

                current = new AtDebtorsParsedEntry(
                    entryMatch.Groups[1].Value,
                    AtDebtorsTextNormalizer.Normalize(entryMatch.Groups[2].Value));
                continue;
            }

            if (current is not null)
            {
                current = current with
                {
                    Name = AtDebtorsTextNormalizer.Normalize($"{current.Name} {line}")
                };
            }
        }

        if (current is not null)
            entries.Add(current);

        return new ParsedAtDebtorsList(sourceUpdatedAt, entries);
    }

    private static ParsedAtDebtorsList ParseFlatText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new ParsedAtDebtorsList(null, []);

        DateOnly? sourceUpdatedAt = null;
        var updatedMatch = UpdatedAt.Match(text);
        if (updatedMatch.Success
            && DateOnly.TryParseExact(
                updatedMatch.Groups[1].Value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
        {
            sourceUpdatedAt = parsedDate;
        }

        var matches = FlatTextNifMarker.Matches(text);
        if (matches.Count == 0)
            return new ParsedAtDebtorsList(sourceUpdatedAt, []);

        var entries = new List<AtDebtorsParsedEntry>(matches.Count);
        for (var i = 0; i < matches.Count; i++)
        {
            var nif = matches[i].Groups[1].Value;
            var start = matches[i].Index + matches[i].Length;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : text.Length;
            var name = NormalizeLine(text[start..end]);
            if (name.Length == 0)
                continue;

            entries.Add(new AtDebtorsParsedEntry(nif, AtDebtorsTextNormalizer.Normalize(name)));
        }

        return new ParsedAtDebtorsList(sourceUpdatedAt, entries);
    }

    private static string NormalizeLine(string line) =>
        line.Replace('\r', ' ').Trim();

    private static bool IsSkippableLine(string line)
    {
        if (PageHeader.IsMatch(line))
            return true;

        if (line.StartsWith("Contribuintes", StringComparison.OrdinalIgnoreCase))
            return true;

        if (line.StartsWith("Devedores de", StringComparison.OrdinalIgnoreCase))
            return true;

        if (line.StartsWith("Informa", StringComparison.OrdinalIgnoreCase)
            && line.Contains("actualizada", StringComparison.OrdinalIgnoreCase))
            return true;

        if (line.Contains("NIPC", StringComparison.OrdinalIgnoreCase)
            && line.Contains("DESIGN", StringComparison.OrdinalIgnoreCase))
            return true;

        if (line.Contains("NIF", StringComparison.OrdinalIgnoreCase)
            && line.Contains("DESIGN", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    [GeneratedRegex(@"^(\d{9})\.?\s+(.+)$")]
    private static partial Regex EntryStartRegex();

    [GeneratedRegex(@"Informa(?:ç|c)(?:ã|a)o actualizada em (\d{4}-\d{2}-\d{2})", RegexOptions.IgnoreCase)]
    private static partial Regex UpdatedAtRegex();

    [GeneratedRegex(@"^P[áa]gina:\s*\d+$", RegexOptions.IgnoreCase)]
    private static partial Regex PageHeaderRegex();

    [GeneratedRegex(@"(?<!\d)(\d{9})(?=\.?\s*)")]
    private static partial Regex FlatTextNifMarkerRegex();
}
