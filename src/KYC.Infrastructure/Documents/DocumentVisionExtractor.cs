using KYC.Infrastructure.LLM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Documents;

public sealed class DocumentVisionExtractor(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<DocumentVisionExtractor> logger)
{
    public async Task<string> ExtractTextFromImageAsync(byte[] imageBytes, string mimeType, CancellationToken ct = default)
    {
        if (!await IsOllamaReachableAsync(ct))
        {
            logger.LogWarning("LLM indisponível para OCR de imagem.");
            return string.Empty;
        }

        var model = configuration["LLM:LocalModel"] ?? "qwen3.5:9b";
        var timeoutSeconds = Math.Clamp(configuration.GetValue("LLM:DocumentExtractionTimeoutSeconds", 120), 30, 600);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var base64 = Convert.ToBase64String(imageBytes);
        var messages = new object[]
        {
            OpenAiCompatibleClient.VisionUserMessage(
                "Extrai todo o texto desta página de documento KYC, preservando números (NIF, IBAN). Responde só com o texto.",
                base64,
                string.IsNullOrWhiteSpace(mimeType) ? "image/png" : mimeType)
        };

        try
        {
            var client = httpClientFactory.CreateClient("ollama");
            return (await OpenAiCompatibleClient
                .ChatAsync(client, model, messages, ct: cts.Token)
                .ConfigureAwait(false)).Trim();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha OCR visão via LLM OpenAI-compatible.");
            return string.Empty;
        }
    }

    private async Task<bool> IsOllamaReachableAsync(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("ollama-health");
        return await OpenAiCompatibleClient.IsReachableAsync(client, ct).ConfigureAwait(false);
    }
}
