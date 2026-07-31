using System.Net.Http.Json;
using System.Text.Json;

namespace KYC.Infrastructure.LLM;

/// <summary>Helpers for OpenAI-compatible chat/embeddings/models (Ollama /v1, vLLM, ContextMemory).</summary>
public static class OpenAiCompatibleClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task<bool> IsReachableAsync(HttpClient healthClient, CancellationToken ct = default)
    {
        try
        {
            using var response = await healthClient.GetAsync("v1/models", ct).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return true;

            // ContextMemory liveness (no LLM probe required for process up)
            using var cmHealth = await healthClient.GetAsync("health", ct).ConfigureAwait(false);
            if (cmHealth.IsSuccessStatusCode)
                return true;

            // Fallback for older Ollama without /v1
            using var legacy = await healthClient.GetAsync("api/tags", ct).ConfigureAwait(false);
            return legacy.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<string> ChatAsync(
        HttpClient client,
        string model,
        IReadOnlyList<object> messages,
        float? temperature = null,
        int? maxTokens = null,
        CancellationToken ct = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["stream"] = false,
            ["messages"] = messages
        };
        if (temperature is not null)
            payload["temperature"] = temperature;
        if (maxTokens is not null)
            payload["max_tokens"] = maxTokens;

        using var response = await client
            .PostAsJsonAsync("v1/chat/completions", payload, JsonOptions, ct)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct)
            .ConfigureAwait(false);
        return ExtractAssistantContent(doc);
    }

    public static object TextMessage(string role, string content) =>
        new { role, content };

    public static object VisionUserMessage(string text, string base64Image, string mimeType = "image/png")
    {
        var dataUrl = $"data:{mimeType};base64,{base64Image}";
        return new
        {
            role = "user",
            content = new object[]
            {
                new { type = "text", text },
                new { type = "image_url", image_url = new { url = dataUrl } }
            }
        };
    }

    public static async Task<float[]?> EmbedAsync(
        HttpClient client,
        string model,
        string text,
        CancellationToken ct = default)
    {
        using var response = await client
            .PostAsJsonAsync(
                "v1/embeddings",
                new { model, input = text },
                JsonOptions,
                ct)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;

        var doc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct)
            .ConfigureAwait(false);
        if (!doc.TryGetProperty("data", out var data) || data.GetArrayLength() == 0)
            return null;

        var embedding = data[0].GetProperty("embedding");
        var result = new float[embedding.GetArrayLength()];
        var i = 0;
        foreach (var el in embedding.EnumerateArray())
            result[i++] = (float)el.GetDouble();
        return result;
    }

    public static string ExtractAssistantContent(JsonElement doc)
    {
        if (doc.TryGetProperty("choices", out var choices)
            && choices.ValueKind == JsonValueKind.Array
            && choices.GetArrayLength() > 0)
        {
            var choice = choices[0];
            if (choice.TryGetProperty("message", out var message)
                && message.TryGetProperty("content", out var content))
            {
                return content.ValueKind == JsonValueKind.String
                    ? content.GetString() ?? string.Empty
                    : content.ToString();
            }
        }

        // Legacy Ollama /api/chat
        if (doc.TryGetProperty("message", out var legacy)
            && legacy.TryGetProperty("content", out var legacyContent))
            return legacyContent.GetString() ?? string.Empty;

        return string.Empty;
    }
}
