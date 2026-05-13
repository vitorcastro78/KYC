using System.Text.Json;

namespace KYC.Infrastructure.ExternalSources;

public interface IWikidataCompanyClient
{
    /// <summary>Pesquisa por identificador ou nome; filtra itens que parecem empresas (P31 / descrição).</summary>
    Task<WikidataCompanyMatch?> FindCompanyByCommercialIdentifierAsync(string commercialId, CancellationToken ct = default);
}

public sealed record WikidataCompanyMatch(string Label, string? Description, string? CountryIso2, string WikidataId);

public sealed class WikidataCompanyClient(HttpClient http, ILogger<WikidataCompanyClient> log) : IWikidataCompanyClient
{
    private static readonly HashSet<string> CompanyInstanceIds = new(StringComparer.Ordinal)
    {
        "Q43229", "Q4830453", "Q783794", "Q6881511", "Q891723", "Q319845", "Q658255", "Q167037", "Q219577",
        "Q463167", "Q134161", "Q22687", "Q167269", "Q64027599", "Q18394972", "Q167270", "Q3401692"
    };

    private static readonly HashSet<string> ExcludedInstanceIds = new(StringComparer.Ordinal)
    {
        "Q16521", "Q11424", "Q11173", "Q7187", "Q6256", "Q17476917", "Q6999", "Q523"
    };

    public async Task<WikidataCompanyMatch?> FindCompanyByCommercialIdentifierAsync(string commercialId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(commercialId))
            return null;

        var q = Uri.EscapeDataString(commercialId.Trim());
        try
        {
            var searchUrl =
                $"w/api.php?action=wbsearchentities&search={q}&language=en&uselang=en&type=item&format=json&limit=12";
            using var searchResp = await http.GetAsync(searchUrl, ct);
            if (!searchResp.IsSuccessStatusCode)
                return null;
            await using var searchStream = await searchResp.Content.ReadAsStreamAsync(ct);
            using var searchDoc = await JsonDocument.ParseAsync(searchStream, cancellationToken: ct);
            if (!searchDoc.RootElement.TryGetProperty("search", out var searchArr) ||
                searchArr.ValueKind != JsonValueKind.Array || searchArr.GetArrayLength() == 0)
                return null;

            var titles = new List<string>();
            foreach (var item in searchArr.EnumerateArray())
            {
                if (item.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                    titles.Add(t.GetString()!);
            }

            if (titles.Count == 0)
                return null;

            var ids = string.Join('|', titles.Distinct(StringComparer.Ordinal));
            var entitiesUrl =
                $"w/api.php?action=wbgetentities&ids={ids}&props=claims|labels|descriptions&languages=en&format=json";
            using var entResp = await http.GetAsync(entitiesUrl, ct);
            if (!entResp.IsSuccessStatusCode)
                return null;
            await using var entStream = await entResp.Content.ReadAsStreamAsync(ct);
            using var entDoc = await JsonDocument.ParseAsync(entStream, cancellationToken: ct);
            if (!entDoc.RootElement.TryGetProperty("entities", out var entities))
                return null;

            foreach (var title in titles)
            {
                if (!entities.TryGetProperty(title, out var entity) ||
                    entity.TryGetProperty("missing", out _))
                    continue;

                var label = ReadEnLabel(entity);
                var description = ReadEnDescription(entity);
                if (string.IsNullOrWhiteSpace(label))
                    continue;

                if (!PassesCompanyFilters(entity, description))
                    continue;

                var countryId = ReadP17CountryId(entity);
                string? iso = null;
                if (countryId is not null && WikidataCountryIso.TryGetIso2(countryId, out var c))
                    iso = c;

                return new WikidataCompanyMatch(label, description, iso, title);
            }

            return null;
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Wikidata lookup failed for {Id}.", commercialId);
            return null;
        }
    }

    private static string? ReadEnLabel(JsonElement entity)
    {
        if (!entity.TryGetProperty("labels", out var labels) ||
            !labels.TryGetProperty("en", out var en) ||
            !en.TryGetProperty("value", out var v))
            return null;
        return v.GetString();
    }

    private static string? ReadEnDescription(JsonElement entity)
    {
        if (!entity.TryGetProperty("descriptions", out var desc) ||
            !desc.TryGetProperty("en", out var en) ||
            !en.TryGetProperty("value", out var v))
            return null;
        return v.GetString();
    }

    private static string? ReadP17CountryId(JsonElement entity)
    {
        if (!entity.TryGetProperty("claims", out var claims) || !claims.TryGetProperty("P17", out var p17))
            return null;
        foreach (var stmt in p17.EnumerateArray())
        {
            if (!stmt.TryGetProperty("mainsnak", out var mainsnak))
                continue;
            if (mainsnak.TryGetProperty("snaktype", out var st) && st.GetString() != "value")
                continue;
            if (!mainsnak.TryGetProperty("datavalue", out var dv) ||
                !dv.TryGetProperty("value", out var val))
                continue;
            if (val.TryGetProperty("id", out var idEl))
                return idEl.GetString();
        }

        return null;
    }

    private static bool PassesCompanyFilters(JsonElement entity, string? descriptionEn)
    {
        var p31Ids = ReadP31Ids(entity);
        if (p31Ids.Any(id => ExcludedInstanceIds.Contains(id)))
            return false;
        if (p31Ids.Any(id => CompanyInstanceIds.Contains(id)))
            return true;

        if (string.IsNullOrEmpty(descriptionEn))
            return false;

        var d = descriptionEn.ToLowerInvariant();
        if (d.Contains("species of", StringComparison.Ordinal) ||
            d.Contains("taxon", StringComparison.Ordinal) ||
            d.Contains("genus of", StringComparison.Ordinal) ||
            d.Contains("album", StringComparison.Ordinal) ||
            d.Contains("film", StringComparison.Ordinal) ||
            d.Contains("television series", StringComparison.Ordinal) ||
            d.Contains("video game", StringComparison.Ordinal) ||
            d.Contains("asteroid", StringComparison.Ordinal) ||
            d.Contains("crater on", StringComparison.Ordinal))
            return false;

        return d.Contains("company", StringComparison.Ordinal) ||
               d.Contains("corporation", StringComparison.Ordinal) ||
               d.Contains("holding", StringComparison.Ordinal) ||
               d.Contains("multinational", StringComparison.Ordinal) ||
               d.Contains("enterprise", StringComparison.Ordinal) ||
               d.Contains("business", StringComparison.Ordinal) ||
               d.Contains(" s.a.", StringComparison.Ordinal) ||
               d.Contains(" ltd", StringComparison.Ordinal) ||
               d.Contains("limited", StringComparison.Ordinal);
    }

    private static List<string> ReadP31Ids(JsonElement entity)
    {
        var list = new List<string>();
        if (!entity.TryGetProperty("claims", out var claims) || !claims.TryGetProperty("P31", out var p31))
            return list;
        foreach (var stmt in p31.EnumerateArray())
        {
            if (!stmt.TryGetProperty("mainsnak", out var mainsnak))
                continue;
            if (mainsnak.TryGetProperty("snaktype", out var st) && st.GetString() != "value")
                continue;
            if (!mainsnak.TryGetProperty("datavalue", out var dv) ||
                !dv.TryGetProperty("value", out var val))
                continue;
            if (val.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String)
                list.Add(idEl.GetString()!);
        }

        return list;
    }
}
