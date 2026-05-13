using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources;

/// <summary>Pesquisa de media adversa via NewsAPI.org (v2). Requer chave em NewsApi:ApiKey ou NEWSAPI_KEY.</summary>
public sealed class AdverseMediaService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<AdverseMediaService> log) : IAdverseMediaService
{
    private static readonly string[] NegativeHints =
    [
        "fraude", "crime", "corrup", "insolv", "falência", "falencia", "prisão", "prisao",
        "multa", "conden", "investig", "escândalo", "escandalo", "processo", "judicial",
        "lavagem", "sanction", "embargo", "ilegal"
    ];

    private static readonly HashSet<string> WeakNameTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "lda", "sa", "s.a", "unipessoal", "limitada", "sociedade", "empresa", "grupo", "holding",
        "holdings", "servicos", "serviços", "consulting", "consultoria", "international", "global",
        "portugal", "portuguesa", "português", "portugues", "solutions", "services", "group",
        "the", "and", "de", "da", "do", "das", "dos", "e", "ltd", "ltda", "inc", "corp", "llc"
    };

    private static readonly Regex TokenSplit = new(@"[\s\-.,/&]+", RegexOptions.Compiled);

    public async Task<AdverseMediaResult> ScanAsync(string entityName, string? nif = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            return new AdverseMediaResult([]);

        var apiKey = configuration["NewsApi:ApiKey"] ?? configuration["NEWSAPI_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            log.LogInformation("NewsApi:ApiKey / NEWSAPI_KEY não definidos — pesquisa de notícias desactivada.");
            return new AdverseMediaResult([]);
        }

        var lang = configuration["NewsApi:Language"] ?? "pt";
        var pageSize = Math.Clamp(configuration.GetValue("NewsApi:PageSize", 15), 1, 30);
        var country = configuration["NewsApi:TopHeadlinesCountry"];

        var cleanedName = CleanCompanyName(entityName);
        var query = BuildNewsApiQuery(cleanedName);
        if (string.IsNullOrWhiteSpace(query))
            return new AdverseMediaResult([]);

        var client = httpClientFactory.CreateClient("newsapi");

        var hits = await TryEverythingAsync(client, apiKey, query, lang, pageSize, cleanedName, nif, ct)
            .ConfigureAwait(false);
        if (hits.Count == 0 && !string.IsNullOrWhiteSpace(country))
        {
            hits = await TryTopHeadlinesAsync(client, apiKey, query, country.Trim(), pageSize, cleanedName, nif, ct)
                .ConfigureAwait(false);
        }

        return new AdverseMediaResult(hits);
    }

    /// <summary>
    /// Frase entre aspas para nomes compostos (NewsAPI); sem OR NIF na query — o NIF no corpo dos artigos
    /// gera muitos falsos positivos; o NIF só entra no filtro local.
    /// </summary>
    private static string BuildNewsApiQuery(string cleanedName)
    {
        if (string.IsNullOrWhiteSpace(cleanedName))
            return "";
        var inner = cleanedName.Replace("\"", "\\\"", StringComparison.Ordinal);
        return inner.Contains(' ', StringComparison.Ordinal) ? $"\"{inner}\"" : inner;
    }

    internal static string CleanCompanyName(string entityName)
    {
        var s = entityName.Trim();
        while (s.Contains("  ", StringComparison.Ordinal))
            s = s.Replace("  ", " ", StringComparison.Ordinal);

        var lower = s.ToLowerInvariant();
        var suffixes = new[]
        {
            "unipessoal, lda.", "unipessoal lda.", "unipessoal, lda", "unipessoal lda",
            ", unipessoal lda.", ", lda.", ", lda", " lda.", " lda",
            " s.a.", " s.a", " s.l.", " s.l", " ltd.", " ltd", "ltd."
        };
        foreach (var suf in suffixes)
        {
            if (lower.EndsWith(suf, StringComparison.Ordinal))
            {
                s = s[..^suf.Length].TrimEnd(' ', ',', '.');
                lower = s.ToLowerInvariant();
            }
        }

        return s.Trim();
    }

    private async Task<List<AdverseMediaHit>> TryEverythingAsync(
        HttpClient client,
        string apiKey,
        string query,
        string language,
        int pageSize,
        string cleanedEntityName,
        string? nif,
        CancellationToken ct)
    {
        var toDate = DateTime.UtcNow.Date;
        var fromDate = toDate.AddDays(-28);

        try
        {
            for (var includeLanguage = true; ; includeLanguage = false)
            {
                var langPart = includeLanguage ? $"&language={Uri.EscapeDataString(language)}" : "";
                var path =
                    $"/v2/everything?q={Uri.EscapeDataString(query)}&from={fromDate:yyyy-MM-dd}&to={toDate:yyyy-MM-dd}" +
                    $"&sortBy=publishedAt&pageSize={pageSize}{langPart}";
                using var req = new HttpRequestMessage(HttpMethod.Get, path);
                req.Headers.TryAddWithoutValidation("X-Api-Key", apiKey);
                using var res = await client.SendAsync(req, ct).ConfigureAwait(false);
                var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                if (res.IsSuccessStatusCode)
                    return ParseArticles(body, cleanedEntityName, nif);

                log.LogWarning(
                    "NewsAPI /everything falhou: {Status} {Body}",
                    (int)res.StatusCode,
                    TruncateForLog(body));

                if (includeLanguage && res.StatusCode == HttpStatusCode.BadRequest)
                    continue;

                return [];
            }
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "NewsAPI /everything excepção.");
            return [];
        }
    }

    private async Task<List<AdverseMediaHit>> TryTopHeadlinesAsync(
        HttpClient client,
        string apiKey,
        string query,
        string countryIso2,
        int pageSize,
        string cleanedEntityName,
        string? nif,
        CancellationToken ct)
    {
        try
        {
            var path =
                $"/v2/top-headlines?country={Uri.EscapeDataString(countryIso2.ToLowerInvariant())}" +
                $"&q={Uri.EscapeDataString(query)}&pageSize={pageSize}";
            using var req = new HttpRequestMessage(HttpMethod.Get, path);
            req.Headers.TryAddWithoutValidation("X-Api-Key", apiKey);
            using var res = await client.SendAsync(req, ct).ConfigureAwait(false);
            var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
            {
                log.LogWarning(
                    "NewsAPI /top-headlines falhou: {Status} {Body}",
                    (int)res.StatusCode,
                    TruncateForLog(body));
                return [];
            }

            return ParseArticles(body, cleanedEntityName, nif);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "NewsAPI /top-headlines excepção.");
            return [];
        }
    }

    private static string TruncateForLog(string? body, int max = 600) =>
        string.IsNullOrEmpty(body) ? "" : body.Length <= max ? body : body[..max] + "…";

    private List<AdverseMediaHit> ParseArticles(string json, string cleanedEntityName, string? nif)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.TryGetProperty("status", out var st) && st.GetString() is not "ok")
        {
            var msg = root.TryGetProperty("message", out var m) ? m.GetString() : "unknown";
            log.LogWarning("NewsAPI resposta status!=ok: {Message}", msg);
            return [];
        }

        if (!root.TryGetProperty("articles", out var articles) || articles.ValueKind != JsonValueKind.Array)
            return [];

        var list = new List<AdverseMediaHit>();
        foreach (var a in articles.EnumerateArray())
        {
            var title = a.TryGetProperty("title", out var t) ? t.GetString() : null;
            if (string.IsNullOrWhiteSpace(title))
                continue;
            var url = a.TryGetProperty("url", out var u) ? u.GetString() : null;
            if (string.IsNullOrWhiteSpace(url))
                continue;

            DateTime? published = null;
            if (a.TryGetProperty("publishedAt", out var p) && p.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(p.GetString(), CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                published = dt;

            var desc = a.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String
                ? d.GetString()
                : null;
            var content = a.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String
                ? c.GetString()
                : null;

            if (!ArticleMentionsEntity(title, desc, content, cleanedEntityName, nif))
                continue;

            var sentiment = ClassifySentiment(title, desc);
            list.Add(new AdverseMediaHit(title, url, published, sentiment));
        }

        return list;
    }

    /// <summary>
    /// Mantém só artigos em que o título/descrição/conteúdo referem claramente a entidade (nome ou NIF).
    /// </summary>
    internal static bool ArticleMentionsEntity(
        string title,
        string? description,
        string? content,
        string cleanedEntityName,
        string? nif)
    {
        var blob = FoldForMatch($"{title} {description} {content}");

        if (!string.IsNullOrWhiteSpace(nif))
        {
            var digits = ExtractDigits(nif);
            if (digits.Length >= 5 && blob.Contains(digits, StringComparison.Ordinal))
                return true;
        }

        if (string.IsNullOrWhiteSpace(cleanedEntityName))
            return false;

        var nameFolded = FoldForMatch(cleanedEntityName);
        if (nameFolded.Length >= 6 && blob.Contains(nameFolded, StringComparison.Ordinal))
            return true;

        var tokens = TokenizeSignificant(cleanedEntityName);
        if (tokens.Count == 0)
            return false;

        if (tokens.Count >= 2)
            return tokens.TrueForAll(t => blob.Contains(FoldForMatch(t), StringComparison.Ordinal));

        var only = FoldForMatch(tokens[0]);
        if (only.Length >= 8)
            return blob.Contains(only, StringComparison.Ordinal);

        if (only.Length >= 4)
            return blob.Contains(only, StringComparison.Ordinal) && BlobHasNegativeContext(blob);

        return false;
    }

    private static List<string> TokenizeSignificant(string cleanedName)
    {
        var parts = TokenSplit.Split(cleanedName);
        var list = new List<string>();
        foreach (var part in parts)
        {
            var p = part.Trim();
            if (p.Length < 4)
                continue;
            if (WeakNameTokens.Contains(p))
                continue;
            list.Add(p);
        }

        return list;
    }

    private static string ExtractDigits(string nif) =>
        string.Create(nif.Count(char.IsDigit), nif, static (span, s) =>
        {
            var i = 0;
            foreach (var ch in s)
            {
                if (char.IsDigit(ch))
                    span[i++] = ch;
            }
        });

    private static string FoldForMatch(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
        var lower = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(lower.Length);
        foreach (var ch in lower)
        {
            if (char.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool BlobHasNegativeContext(string blobFolded) =>
        NegativeHints.Any(h => blobFolded.Contains(h, StringComparison.Ordinal));

    private static string ClassifySentiment(string title, string? description)
    {
        var blob = $"{title} {description}".ToLowerInvariant();
        return NegativeHints.Any(h => blob.Contains(h, StringComparison.Ordinal)) ? "Negativo" : "Neutro";
    }
}
