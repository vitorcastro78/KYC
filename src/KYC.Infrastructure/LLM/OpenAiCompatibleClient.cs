using System.Net.Http.Json;
using System.Text.Json;

namespace KYC.Infrastructure.LLM;

/// <summary>OpenAI-compatible client for ContextMemory (<c>/v1/chat/completions</c>, <c>/v1/models</c>).</summary>
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

            using var cmHealth = await healthClient.GetAsync("health", ct).ConfigureAwait(false);
            return cmHealth.IsSuccessStatusCode;
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

        return string.Empty;
    }
}
