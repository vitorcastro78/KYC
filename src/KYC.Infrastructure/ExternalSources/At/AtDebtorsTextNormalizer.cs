using System.Net;
using System.Text.RegularExpressions;

namespace KYC.Infrastructure.ExternalSources.At;

public static partial class AtDebtorsTextNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var text = WebUtility.HtmlDecode(value);
        text = text.Replace('\u00A0', ' ');
        text = MultiSpace().Replace(text, " ").Trim();

        if (text.StartsWith(". ", StringComparison.Ordinal))
            text = text[2..].Trim();

        return text;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultiSpace();
}
