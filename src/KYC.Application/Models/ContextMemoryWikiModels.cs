using System.Text.Json.Serialization;

namespace KYC.Application.Models;

public sealed class WikiUpsertRequest
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    [JsonPropertyName("sourceId")]
    public string? SourceId { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; init; }
}

public sealed class WikiUpsertResult
{
    [JsonPropertyName("documentId")]
    public string? DocumentId { get; init; }

    [JsonPropertyName("created")]
    public bool Created { get; init; }

    [JsonPropertyName("unchanged")]
    public bool Unchanged { get; init; }
}

public sealed class WikiQueryRequest
{
    [JsonPropertyName("query")]
    public required string Query { get; init; }

    [JsonPropertyName("sourceId")]
    public string? SourceId { get; init; }

    [JsonPropertyName("topK")]
    public int TopK { get; init; } = 8;

    [JsonPropertyName("budgetChars")]
    public int BudgetChars { get; init; }

    [JsonPropertyName("includeIndex")]
    public bool IncludeIndex { get; init; } = true;
}

public sealed class WikiQueryResult
{
    [JsonPropertyName("compiledMarkdown")]
    public string CompiledMarkdown { get; init; } = string.Empty;

    [JsonPropertyName("charCount")]
    public int CharCount { get; init; }

    [JsonPropertyName("includedDocuments")]
    public int IncludedDocuments { get; init; }

    [JsonPropertyName("matches")]
    public List<WikiMatch> Matches { get; init; } = [];
}

public sealed class WikiMatch
{
    [JsonPropertyName("documentId")]
    public string? DocumentId { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("score")]
    public double Score { get; init; }

    [JsonPropertyName("sourceId")]
    public string SourceId { get; init; } = string.Empty;
}

public sealed class LlmModelsListResult
{
    [JsonPropertyName("object")]
    public string Object { get; init; } = "list";

    [JsonPropertyName("data")]
    public List<LlmModelInfo> Data { get; init; } = [];

    public string? ActiveModelId =>
        Data.FirstOrDefault(m => m.IsActive)?.Id
        ?? Data.FirstOrDefault()?.Id;

    public string Provider { get; set; } = "contextmemory";
}

public sealed class LlmModelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; init; } = "model";

    [JsonPropertyName("created")]
    public long Created { get; init; }

    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; init; } = string.Empty;

    public bool IsActive =>
        string.Equals(OwnedBy, "active", StringComparison.OrdinalIgnoreCase);
}
