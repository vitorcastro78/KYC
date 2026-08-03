using System.Net;
using System.Text.RegularExpressions;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using Microsoft.Extensions.Options;

namespace KYC.Infrastructure.LLM;

/// <summary>
/// Upserts KYC case reports into ContextMemory Global Wiki (CompanyBrain pattern).
/// Retrieval is lexical/FTS on the gateway — no local pgvector.
/// </summary>
public sealed partial class ReportEmbeddingWriter(
    IContextMemoryWikiClient wiki,
    IOptions<ContextMemoryOptions> options,
    ILogger<ReportEmbeddingWriter> log) : IReportEmbeddingWriter
{
    public const string SourceId = "kyc:reports";

    public static string DocumentIdForCase(Guid kycCaseId) =>
        $"kyc:report:{kycCaseId:N}";

    public async Task EmbedReportTextAsync(Guid kycCaseId, string markdown, CancellationToken ct = default)
    {
        if (!options.Value.IsConfigured)
        {
            log.LogDebug(
                "ContextMemory not configured; skipping wiki upsert for case {CaseId}.",
                kycCaseId);
            return;
        }

        if (string.IsNullOrWhiteSpace(markdown))
            return;

        var content = ToWikiContent(markdown);
        var documentId = DocumentIdForCase(kycCaseId);

        await wiki.IngestDocumentAsync(
            documentId,
            new WikiUpsertRequest
            {
                Title = $"KYC report {kycCaseId:N}",
                Content = content,
                SourceId = SourceId,
                Metadata = new Dictionary<string, string>
                {
                    ["sourceType"] = "kyc-report",
                    ["kycCaseId"] = kycCaseId.ToString("N")
                }
            },
            ct).ConfigureAwait(false);

        log.LogInformation(
            "Upserted KYC report {CaseId} to ContextMemory wiki ({DocumentId}).",
            kycCaseId,
            documentId);
    }

    public async Task ClearEmbeddingsAsync(Guid kycCaseId, CancellationToken ct = default)
    {
        if (!options.Value.IsConfigured)
            return;

        var documentId = DocumentIdForCase(kycCaseId);
        try
        {
            await wiki.DeleteDocumentAsync(documentId, ct).ConfigureAwait(false);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // already gone
        }
    }

    private static string ToWikiContent(string narrative)
    {
        // Narrative may be HTML from the report composer — keep searchable plain-ish text.
        var text = HtmlTagRegex().Replace(narrative, " ");
        text = WebUtility.HtmlDecode(text);
        return CollapseWhitespaceRegex().Replace(text, " ").Trim();
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex CollapseWhitespaceRegex();
}
