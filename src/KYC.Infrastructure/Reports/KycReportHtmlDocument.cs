namespace KYC.Infrastructure.Reports;

internal static class KycReportHtmlDocument
{
    public static string Wrap(string bodyContent, string title) =>
        $"""
        <!DOCTYPE html>
        <html lang="pt">
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <title>{HtmlEncode(title)}</title>
          <style>{KycReportHtmlStyles.Css}</style>
        </head>
        <body>
        {bodyContent}
        </body>
        </html>
        """;

    public static string HtmlEncode(string? text) =>
        string.IsNullOrEmpty(text)
            ? string.Empty
            : System.Net.WebUtility.HtmlEncode(text);

    public static string ToPlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var text = System.Text.RegularExpressions.Regex.Replace(html, "<(script|style)[^>]*>.*?</\\1>", "",
            System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        text = System.Text.RegularExpressions.Regex.Replace(text, "<br\\s*/?>", "\n",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        text = System.Text.RegularExpressions.Regex.Replace(text, "</p>|</div>|</tr>|</li>", "\n",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        return System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ").Trim();
    }
}
