using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KYC.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources;

public interface IOpenSanctionsClient
{
    /// <summary>
    /// Triagem via POST /match/{dataset}. Devolve lista vazia se desactivado, sem API key, ou em erro.
    /// </summary>
    Task<IReadOnlyList<SanctionsListHit>> MatchAsync(
        string name,
        EntityType? entityType = null,
        string? nationality = null,
        string? registrationNumber = null,
        CancellationToken ct = default);
}

/// <summary>Cliente da API OpenSanctions (yente). Ver https://api.opensanctions.org/openapi.json</summary>
public sealed class OpenSanctionsClient(
    HttpClient http,
    IConfiguration configuration,
    ILogger<OpenSanctionsClient> log) : IOpenSanctionsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<SanctionsListHit>> MatchAsync(
        string name,
        EntityType? entityType = null,
        string? nationality = null,
        string? registrationNumber = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return [];

        if (!IsEnabled())
        {
            log.LogDebug("OpenSanctions desactivado ou sem API key — ignorado.");
            return [];
        }

        var dataset = configuration["ExternalSources:OpenSanctions:Dataset"] ?? "sanctions";
        var limit = Math.Clamp(configuration.GetValue("ExternalSources:OpenSanctions:Limit", 5), 1, 50);
        var threshold = configuration.GetValue("ExternalSources:OpenSanctions:Threshold", 0.7);
        var algorithm = configuration["ExternalSources:OpenSanctions:Algorithm"] ?? "best";

        var schema = MapSchema(entityType);
        var properties = BuildProperties(name, nationality, registrationNumber, schema);

        var body = new Dictionary<string, object>
        {
            ["queries"] = new Dictionary<string, object>
            {
                ["q"] = new Dictionary<string, object>
                {
                    ["schema"] = schema,
                    ["properties"] = properties
                }
            }
        };

        var query = $"match/{Uri.EscapeDataString(dataset)}?limit={limit}&threshold={threshold.ToString(System.Globalization.CultureInfo.InvariantCulture)}&algorithm={Uri.EscapeDataString(algorithm)}";

        try
        {
            using var content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await http.PostAsync(query, content, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                log.LogWarning(
                    "OpenSanctions match falhou ({Status}): {Detail}",
                    (int)response.StatusCode,
                    Truncate(err, 400));
                return [];
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
            return ParseMatchResponse(doc, threshold);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "OpenSanctions match falhou para {Name}.", name.Trim());
            return [];
        }
    }

    private bool IsEnabled()
    {
        if (!configuration.GetValue("ExternalSources:OpenSanctions:Enabled", true))
            return false;

        var apiKey = configuration["ExternalSources:OpenSanctions:ApiKey"]
                     ?? configuration["OPENSANCTIONS_API_KEY"];
        return !string.IsNullOrWhiteSpace(apiKey);
    }

    private static string MapSchema(EntityType? entityType) =>
        entityType switch
        {
            EntityType.Individual => "Person",
            EntityType.Trust or EntityType.Foundation => "LegalEntity",
            _ => "Company"
        };

    private static Dictionary<string, string[]> BuildProperties(
        string name,
        string? nationality,
        string? registrationNumber,
        string schema)
    {
        var props = new Dictionary<string, string[]>
        {
            ["name"] = [name.Trim()]
        };

        if (!string.IsNullOrWhiteSpace(registrationNumber))
            props["registrationNumber"] = [registrationNumber.Trim()];

        if (!string.IsNullOrWhiteSpace(nationality))
        {
            var country = nationality.Trim().ToLowerInvariant();
            if (schema == "Person")
                props["nationality"] = [country];
            else
                props["country"] = [country];
        }

        return props;
    }

    private static IReadOnlyList<SanctionsListHit> ParseMatchResponse(JsonDocument doc, double threshold)
    {
        if (!doc.RootElement.TryGetProperty("responses", out var responses))
            return [];

        if (!responses.TryGetProperty("q", out var queryBlock))
            return [];

        if (!queryBlock.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
            return [];

        var hits = new List<SanctionsListHit>();
        foreach (var item in results.EnumerateArray())
        {
            var score = item.TryGetProperty("score", out var scoreEl) && scoreEl.ValueKind == JsonValueKind.Number
                ? scoreEl.GetDouble()
                : 0;

            var isMatch = item.TryGetProperty("match", out var matchEl) &&
                          matchEl.ValueKind == JsonValueKind.True;
            if (!isMatch && score < threshold)
                continue;

            var caption = item.TryGetProperty("caption", out var cap) && cap.ValueKind == JsonValueKind.String
                ? cap.GetString()
                : null;
            if (string.IsNullOrWhiteSpace(caption))
                continue;

            var id = item.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String
                ? idEl.GetString()
                : null;

            var datasets = ReadStringArray(item, "datasets");
            var listLabel = datasets.Count > 0
                ? $"OpenSanctions ({string.Join(", ", datasets)})"
                : "OpenSanctions";

            var details = id is null
                ? null
                : datasets.Count > 0
                    ? $"{id}; datasets={string.Join(',', datasets)}"
                    : id;

            hits.Add(new SanctionsListHit(listLabel, caption, score, details));
        }

        return hits;
    }

    private static List<string> ReadStringArray(JsonElement item, string property)
    {
        var list = new List<string>();
        if (!item.TryGetProperty(property, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return list;

        foreach (var el in arr.EnumerateArray())
        {
            if (el.ValueKind == JsonValueKind.String)
            {
                var s = el.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                    list.Add(s);
            }
        }

        return list;
    }

    private static string Truncate(string? text, int max) =>
        string.IsNullOrEmpty(text) || text.Length <= max ? text ?? "" : text[..max] + "…";
}

/// <summary>Configura o header Authorization no HttpClient quando a API key está disponível.</summary>
internal sealed class OpenSanctionsApiKeyHandler(IConfiguration configuration) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            var apiKey = configuration["ExternalSources:OpenSanctions:ApiKey"]
                         ?? configuration["OPENSANCTIONS_API_KEY"];
            if (!string.IsNullOrWhiteSpace(apiKey))
                request.Headers.Authorization = new AuthenticationHeaderValue("ApiKey", apiKey.Trim());
        }

        return base.SendAsync(request, cancellationToken);
    }
}
