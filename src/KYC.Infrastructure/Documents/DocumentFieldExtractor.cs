using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace KYC.Infrastructure.Documents;

public sealed partial class DocumentFieldExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public sealed class DocumentExtractionPayload
    {
        public string? CompanyName { get; set; }
        public string? Nif { get; set; }
        public string? Address { get; set; }
        public string? Cae { get; set; }
        public string? Revenue { get; set; }
        public string? Equity { get; set; }
        public string? Iban { get; set; }
        public string? DocumentDate { get; set; }
        public string? Summary { get; set; }
        public List<DocumentPartyPayload>? Shareholders { get; set; }
        public List<DocumentPartyPayload>? Ubos { get; set; }
        public List<DocumentPartyPayload>? Directors { get; set; }
    }

    public sealed class DocumentPartyPayload
    {
        public string? Name { get; set; }
        public string? Nif { get; set; }
        public decimal? Percentage { get; set; }
        public string? Nationality { get; set; }
    }

    public DocumentExtractionPayload ExtractFromText(string text)
    {
        var payload = new DocumentExtractionPayload
        {
            Shareholders = [],
            Ubos = [],
            Directors = []
        };

        if (string.IsNullOrWhiteSpace(text))
            return payload;

        var nifMatch = PortugueseNif().Match(text);
        if (nifMatch.Success)
            payload.Nif = nifMatch.Value;

        var ibanMatch = PortugueseIban().Match(text);
        if (ibanMatch.Success)
            payload.Iban = ibanMatch.Value.Replace(" ", string.Empty);

        var caeMatch = CaeCode().Match(text);
        if (caeMatch.Success)
            payload.Cae = caeMatch.Groups[1].Value;

        var dateMatch = DocumentDatePattern().Match(text);
        if (dateMatch.Success)
            payload.DocumentDate = dateMatch.Value;

        payload.Summary = text.Length > 500 ? text[..500] + "…" : text;
        return payload;
    }

    public DocumentExtractionPayload? TryParseLlmJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        var cleaned = json.Trim();
        var start = cleaned.IndexOf('{');
        var end = cleaned.LastIndexOf('}');
        if (start >= 0 && end > start)
            cleaned = cleaned[start..(end + 1)];

        try
        {
            return JsonSerializer.Deserialize<DocumentExtractionPayload>(cleaned, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(DocumentExtractionPayload Payload, string RawJson, string PromptHash)> ExtractStructuredAsync(
        string text,
        Func<string, string, CancellationToken, Task<string?>> llmJsonFunc,
        CancellationToken ct)
    {
        const string system = """
            Extrai dados KYC do texto do documento. Responde APENAS JSON válido:
            {"companyName":null,"nif":null,"address":null,"cae":null,"revenue":null,"equity":null,"iban":null,"documentDate":null,
            "shareholders":[{"name":"","nif":"","percentage":null}],
            "ubos":[{"name":"","nif":"","percentage":null}],
            "directors":[{"name":"","nif":""}],"summary":""}
            """;
        var user = text.Length > 12000 ? text[..12000] : text;
        var promptHash = DocumentExtractionMapper.ComputePromptHash(system + user);

        var llmResponse = await llmJsonFunc(system, user, ct);
        var parsed = TryParseLlmJson(llmResponse);
        var fallback = ExtractFromText(text);
        var merged = Merge(fallback, parsed);
        var rawJson = DocumentExtractionMapper.SerializePayload(merged);
        return (merged, rawJson, promptHash);
    }

    private static DocumentExtractionPayload Merge(DocumentExtractionPayload fallback, DocumentExtractionPayload? llm)
    {
        if (llm is null)
            return fallback;

        return new DocumentExtractionPayload
        {
            CompanyName = Coalesce(llm.CompanyName, fallback.CompanyName),
            Nif = NormalizeNif(Coalesce(llm.Nif, fallback.Nif)),
            Address = Coalesce(llm.Address, fallback.Address),
            Cae = Coalesce(llm.Cae, fallback.Cae),
            Revenue = Coalesce(llm.Revenue, fallback.Revenue),
            Equity = Coalesce(llm.Equity, fallback.Equity),
            Iban = Coalesce(llm.Iban, fallback.Iban),
            DocumentDate = Coalesce(llm.DocumentDate, fallback.DocumentDate),
            Summary = Coalesce(llm.Summary, fallback.Summary),
            Shareholders = MergeParties(llm.Shareholders, fallback.Shareholders),
            Ubos = MergeParties(llm.Ubos, fallback.Ubos),
            Directors = MergeParties(llm.Directors, fallback.Directors)
        };
    }

    private static List<DocumentPartyPayload> MergeParties(
        List<DocumentPartyPayload>? primary,
        List<DocumentPartyPayload>? secondary) =>
        primary?.Count > 0 ? primary : secondary ?? [];

    private static string? Coalesce(string? a, string? b) =>
        string.IsNullOrWhiteSpace(a) ? b : a.Trim();

    private static string? NormalizeNif(string? nif)
    {
        if (string.IsNullOrWhiteSpace(nif))
            return null;
        var digits = new string(nif.Where(char.IsDigit).ToArray());
        return digits.Length == 9 ? digits : nif.Trim();
    }

    [GeneratedRegex(@"\b[1-9]\d{8}\b")]
    private static partial Regex PortugueseNif();

    [GeneratedRegex(@"PT\s?\d{2}(?:\s?\d{4}){5}", RegexOptions.IgnoreCase)]
    private static partial Regex PortugueseIban();

    [GeneratedRegex(@"\bCAE[:\s]*(\d{5})\b", RegexOptions.IgnoreCase)]
    private static partial Regex CaeCode();

    [GeneratedRegex(@"\b\d{1,2}[/-]\d{1,2}[/-]\d{2,4}\b")]
    private static partial Regex DocumentDatePattern();
}
