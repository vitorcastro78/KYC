using System.Net.Http.Json;
using System.Text.Json;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace KYC.Infrastructure.LLM;

public sealed class LlmModelCatalog(
    IContextMemoryWikiClient wikiClient,
    IHttpClientFactory httpClientFactory,
    IOptions<ContextMemoryOptions> cmOptions,
    IConfiguration configuration,
    ILogger<LlmModelCatalog> log) : ILlmModelCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<LlmModelsListResult> ListAsync(CancellationToken cancellationToken = default)
    {
        if (cmOptions.Value.IsConfigured)
        {
            try
            {
                return await wikiClient.ListModelsAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "ContextMemory /v1/models failed; falling back to local LLM endpoint.");
            }
        }

        return await ListFromLocalAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<LlmModelsListResult> ListFromLocalAsync(CancellationToken cancellationToken)
    {
        var configured = configuration["LLM:LocalModel"] ?? "qwen3.5:9b";
        var client = httpClientFactory.CreateClient("ollama-health");
        var result = new LlmModelsListResult { Provider = "ollama" };

        try
        {
            using var response = await client.GetAsync("v1/models", cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content
                    .ReadFromJsonAsync<LlmModelsListResult>(JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
                if (payload?.Data is { Count: > 0 })
                {
                    MarkActive(payload.Data, configured);
                    payload.Provider = "ollama";
                    return payload;
                }
            }
        }
        catch (Exception ex)
        {
            log.LogDebug(ex, "Local /v1/models unavailable.");
        }

        try
        {
            using var tags = await client.GetAsync("api/tags", cancellationToken).ConfigureAwait(false);
            if (tags.IsSuccessStatusCode)
            {
                await using var stream = await tags.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                if (doc.RootElement.TryGetProperty("models", out var models)
                    && models.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in models.EnumerateArray())
                    {
                        var id = item.TryGetProperty("name", out var name) ? name.GetString() : null;
                        if (string.IsNullOrWhiteSpace(id))
                            continue;
                        result.Data.Add(new LlmModelInfo { Id = id.Trim(), OwnedBy = "backend" });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.LogDebug(ex, "Local /api/tags unavailable.");
        }

        if (result.Data.Count == 0)
            result.Data.Add(new LlmModelInfo { Id = configured, OwnedBy = "active" });
        else
            MarkActive(result.Data, configured);

        return result;
    }

    private static void MarkActive(List<LlmModelInfo> models, string configured)
    {
        var idx = models.FindIndex(m =>
            string.Equals(m.Id, configured, StringComparison.OrdinalIgnoreCase));
        if (idx < 0)
        {
            models.Insert(0, new LlmModelInfo { Id = configured, OwnedBy = "active" });
            return;
        }

        var current = models[idx];
        models[idx] = new LlmModelInfo
        {
            Id = current.Id,
            Created = current.Created,
            OwnedBy = "active"
        };
    }
}
