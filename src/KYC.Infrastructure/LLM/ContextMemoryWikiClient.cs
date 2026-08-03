using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using Microsoft.Extensions.Options;

namespace KYC.Infrastructure.LLM;

public sealed class ContextMemoryWikiClient : IContextMemoryWikiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly ContextMemoryOptions _options;

    public ContextMemoryWikiClient(HttpClient http, IOptions<ContextMemoryOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<WikiUpsertResult> IngestDocumentAsync(
        string documentId,
        WikiUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var encodedId = Uri.EscapeDataString(documentId);
        var path = $"/apps/{Uri.EscapeDataString(_options.AppId)}/wiki/documents/{encodedId}";
        using var httpRequest = CreateRequest(HttpMethod.Put, path);
        httpRequest.Content = JsonContent.Create(request, options: JsonOptions);

        using var response = await _http.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessOrThrowAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content.ReadFromJsonAsync<WikiUpsertResult>(JsonOptions, cancellationToken)
                   .ConfigureAwait(false)
               ?? new WikiUpsertResult { DocumentId = documentId };
    }

    public async Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        var encodedId = Uri.EscapeDataString(documentId);
        var path = $"/apps/{Uri.EscapeDataString(_options.AppId)}/wiki/documents/{encodedId}";
        using var httpRequest = CreateRequest(HttpMethod.Delete, path);
        using var response = await _http.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return;
        await EnsureSuccessOrThrowAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<WikiQueryResult> QueryAsync(
        WikiQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = $"/apps/{Uri.EscapeDataString(_options.AppId)}/wiki/query";
        using var httpRequest = CreateRequest(HttpMethod.Post, path);
        httpRequest.Content = JsonContent.Create(request, options: JsonOptions);

        using var response = await _http.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessOrThrowAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content.ReadFromJsonAsync<WikiQueryResult>(JsonOptions, cancellationToken)
                   .ConfigureAwait(false)
               ?? new WikiQueryResult();
    }

    public async Task<LlmModelsListResult> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Get, "/v1/models");
        using var response = await _http.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessOrThrowAsync(response, cancellationToken).ConfigureAwait(false);

        var result = await response.Content.ReadFromJsonAsync<LlmModelsListResult>(JsonOptions, cancellationToken)
                         .ConfigureAwait(false)
                     ?? new LlmModelsListResult();
        result.Provider = "contextmemory";

        if (result.Data.All(m => !m.IsActive)
            && response.Headers.TryGetValues("X-Context-Memory-Active-Model", out var values))
        {
            var headerModel = values.FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(headerModel))
            {
                var existing = result.Data.FirstOrDefault(m =>
                    string.Equals(m.Id, headerModel, StringComparison.OrdinalIgnoreCase));
                if (existing is null)
                {
                    result.Data.Insert(0, new LlmModelInfo { Id = headerModel, OwnedBy = "active" });
                }
            }
        }

        return result;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation("X-App-Id", _options.AppId);
        if (!string.IsNullOrWhiteSpace(_options.UserId))
            request.Headers.TryAddWithoutValidation("X-User-Id", _options.UserId.Trim());
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey.Trim());
        return request;
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new HttpRequestException(
            $"ContextMemory wiki API returned {(int)response.StatusCode}: {body}",
            null,
            response.StatusCode);
    }
}
