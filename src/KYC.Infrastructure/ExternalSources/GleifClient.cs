using System.Net.Http.Json;
using System.Text.Json;

namespace KYC.Infrastructure.ExternalSources;

public interface IGleifClient
{
    /// <summary>Pesquisa por número de registo comercial ou LEI (20 caracteres).</summary>
    Task<GleifEntityMatch?> FindByCommercialIdentifierAsync(string commercialId, CancellationToken ct = default);
}

public sealed record GleifEntityMatch(string LegalName, string? CountryIso2, string Lei, string? RegisteredAs);

public sealed class GleifClient(HttpClient http, ILogger<GleifClient> log) : IGleifClient
{
    public async Task<GleifEntityMatch?> FindByCommercialIdentifierAsync(string commercialId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(commercialId))
            return null;

        var trimmed = commercialId.Trim();
        try
        {
            var leiCandidate = trimmed.ToUpperInvariant();
            if (IsLeiFormat(leiCandidate))
            {
                using var response = await http.GetAsync($"lei-records/{Uri.EscapeDataString(leiCandidate)}", ct);
                if (!response.IsSuccessStatusCode)
                    return null;
                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
                    return null;
                return MapDataElement(data);
            }

            var query =
                $"lei-records?page%5Bsize%5D=5&filter%5Bentity.registeredAs%5D={Uri.EscapeDataString(trimmed)}";
            using var listResponse = await http.GetAsync(query, ct);
            if (!listResponse.IsSuccessStatusCode)
                return null;
            await using var listStream = await listResponse.Content.ReadAsStreamAsync(ct);
            using var listDoc = await JsonDocument.ParseAsync(listStream, cancellationToken: ct);
            if (!listDoc.RootElement.TryGetProperty("data", out var dataEl) || dataEl.ValueKind != JsonValueKind.Array ||
                dataEl.GetArrayLength() == 0)
                return null;
            return MapDataElement(dataEl[0]);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "GLEIF lookup failed for {Id}.", commercialId);
            return null;
        }
    }

    private static bool IsLeiFormat(string id) =>
        id.Length == 20 && id.All(c => char.IsLetterOrDigit(c));

    private static GleifEntityMatch? MapDataElement(JsonElement dataItem)
    {
        if (!dataItem.TryGetProperty("attributes", out var attrs))
            return null;
        var lei = attrs.TryGetProperty("lei", out var leiEl) ? leiEl.GetString() : null;
        if (string.IsNullOrEmpty(lei) || !attrs.TryGetProperty("entity", out var entity))
            return null;
        if (!entity.TryGetProperty("legalName", out var ln) || !ln.TryGetProperty("name", out var nameEl))
            return null;
        var legalName = nameEl.GetString();
        if (string.IsNullOrWhiteSpace(legalName))
            return null;

        string? country = null;
        if (entity.TryGetProperty("legalAddress", out var addr) &&
            addr.TryGetProperty("country", out var cEl))
            country = cEl.GetString();

        var registeredAs = entity.TryGetProperty("registeredAs", out var reg) ? reg.GetString() : null;
        return new GleifEntityMatch(legalName, country, lei, registeredAs);
    }
}
