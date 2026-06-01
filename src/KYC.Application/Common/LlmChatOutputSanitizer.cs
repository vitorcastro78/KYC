using System.Text.RegularExpressions;

namespace KYC.Application.Common;

/// <summary>
/// Remove artefactos de modelos chat (Qwen, Llama, etc.) e rejeita eco de prompts na narrativa do relatório.
/// </summary>
public static partial class LlmChatOutputSanitizer
{
    private static readonly Regex ThinkBlockRegex = new(
        string.Concat("<", "think>", "[\\s\\S]*?", "<", "/think>"),
        RegexOptions.IgnoreCase | RegexOptions.Singleline,
        TimeSpan.FromSeconds(2));

    private static readonly string[] PromptLeakMarkers =
    [
        "<|im_start|>",
        string.Concat("<|", "im_end|>"),
        "<|endoftext|>",
        "### Prompt:",
        "# Role",
        "You are a Senior Web Developer",
        "You are a Senior Web Developer and AI Prompt Engineer",
        "\"company_name\"",
        "\"company_score\"",
        "create high-quality prompts for LLMs"
    ];

    public static string StripChatArtifacts(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var cleaned = text.Trim();
        cleaned = ChatRoleTokens().Replace(cleaned, string.Empty);
        cleaned = ThinkBlockRegex.Replace(cleaned, string.Empty);
        cleaned = MarkdownFence().Replace(cleaned, "$1");
        return cleaned.Trim();
    }

    public static bool LooksLikePromptLeak(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return PromptLeakMarkers.Any(m =>
            text.Contains(m, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsAcceptableReportHtml(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var candidate = ExtractReportHtmlFragment(text);
        if (candidate.Length < 80)
            return false;

        if (LooksLikePromptLeak(candidate))
            return false;

        if (!candidate.Contains('<') || !candidate.Contains('>'))
            return false;

        return HtmlTagPattern().IsMatch(candidate);
    }

    public static string ExtractReportHtmlFragment(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var cleaned = StripChatArtifacts(text);

        var sectionMatch = AiSummarySection().Match(cleaned);
        if (sectionMatch.Success)
            return sectionMatch.Value.Trim();

        var htmlFence = HtmlCodeFence().Match(cleaned);
        if (htmlFence.Success)
            return htmlFence.Groups[1].Value.Trim();

        var firstTag = cleaned.IndexOf('<');
        if (firstTag >= 0)
            return cleaned[firstTag..].Trim();

        return cleaned;
    }

    /// <summary>Limpa HTML já gravado (exportação PDF / relatórios antigos com lixo do LLM).</summary>
    public static string CleanStoredReportHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html ?? string.Empty;

        var result = AiSummarySection().Replace(
            html,
            match => LooksLikePromptLeak(match.Value) ? string.Empty : match.Value);

        if (LooksLikePromptLeak(result))
        {
            result = ChatRoleTokens().Replace(result, string.Empty);
            result = ThinkBlockRegex.Replace(result, string.Empty);
        }

        return result.Trim();
    }

    [GeneratedRegex(@"<\|im_start\|>|<\|im_end\|>|<\|endoftext\|>", RegexOptions.IgnoreCase)]
    private static partial Regex ChatRoleTokens();

    [GeneratedRegex(@"```(?:html)?\s*([\s\S]*?)```", RegexOptions.IgnoreCase)]
    private static partial Regex MarkdownFence();

    [GeneratedRegex(@"```(?:html)?\s*([\s\S]*?)```", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlCodeFence();

    [GeneratedRegex(@"<section\s+class=""ai-summary""[^>]*>[\s\S]*?</section>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AiSummarySection();

    [GeneratedRegex(@"</?(?:section|p|h[1-6]|div|ul|ol|li|table|tr|td|th|strong|em)\b", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlTagPattern();
}
