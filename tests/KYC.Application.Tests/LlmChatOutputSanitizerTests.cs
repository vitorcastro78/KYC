using KYC.Application.Common;

namespace KYC.Application.Tests;

public class LlmChatOutputSanitizerTests
{
    private const string LeakedPrompt =
        "<|endoftext|><|im_start|>user ### Prompt: # Role You are a Senior Web Developer and AI Prompt " +
        "Engineer. Your task is to create high-quality prompts for LLMs. # Input: { \"company_name\": \"Identity " +
        "UI E2E\" } <|im_start|>user";

    [Fact]
    public void LooksLikePromptLeak_detects_chat_template_garbage()
    {
        Assert.True(LlmChatOutputSanitizer.LooksLikePromptLeak(LeakedPrompt));
    }

    [Fact]
    public void IsAcceptableReportHtml_rejects_leaked_prompt()
    {
        Assert.False(LlmChatOutputSanitizer.IsAcceptableReportHtml(LeakedPrompt));
    }

    [Fact]
    public void IsAcceptableReportHtml_accepts_valid_section()
    {
        const string html =
            "<section class=\"ai-summary\">" +
            "<h2>Síntese assistida por IA</h2>" +
            "<p>O caso apresenta risco moderado. A decisão final permanece com o analista (Art. 22 RGPD).</p>" +
            "</section>";

        Assert.True(LlmChatOutputSanitizer.IsAcceptableReportHtml(html));
    }

    [Fact]
    public void CleanStoredReportHtml_removes_poisoned_ai_summary()
    {
        var html =
            "<main><p>Relatório base</p></main>" +
            "<section class=\"ai-summary\">" + LeakedPrompt + "</section>";

        var cleaned = LlmChatOutputSanitizer.CleanStoredReportHtml(html);

        Assert.DoesNotContain("<|im_start|>", cleaned, StringComparison.Ordinal);
        Assert.DoesNotContain("Senior Web Developer", cleaned, StringComparison.Ordinal);
        Assert.Contains("Relatório base", cleaned, StringComparison.Ordinal);
    }
}
