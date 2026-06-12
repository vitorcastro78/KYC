using System.Net.Http.Json;
using System.Text.Json;
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
            logger.LogWarning("Ollama indisponível para OCR de imagem.");
            return string.Empty;
        }

        var model = configuration["LLM:LocalModel"] ?? "qwen3.5:9b";
        var timeoutSeconds = Math.Clamp(configuration.GetValue("LLM:DocumentExtractionTimeoutSeconds", 120), 30, 600);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var base64 = Convert.ToBase64String(imageBytes);
        var payload = new
        {
            model,
            stream = false,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = "Extrai todo o texto desta página de documento KYC, preservando números (NIF, IBAN). Responde só com o texto.",
                    images = new[] { base64 }
                }
            }
        };

        try
        {
            var client = httpClientFactory.CreateClient("ollama");
            using var response = await client.PostAsJsonAsync("/api/chat", payload, cts.Token);
            response.EnsureSuccessStatusCode();
            var doc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cts.Token);
            return doc.GetProperty("message").GetProperty("content").GetString()?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha OCR visão via Ollama.");
            return string.Empty;
        }
    }

    private async Task<bool> IsOllamaReachableAsync(CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ollama-health");
            using var response = await client.GetAsync("/api/tags", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
