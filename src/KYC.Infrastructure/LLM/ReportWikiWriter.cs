using System.Net;
using System.Text.RegularExpressions;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using Microsoft.Extensions.Options;

namespace KYC.Infrastructure.LLM;

/// <summary>Upserts KYC case reports into ContextMemory Global Wiki.</summary>
public sealed partial class ReportWikiWriter(
    IContextMemoryWikiClient wiki,
    IOptions<ContextMemoryOptions> options,
    ILogger<ReportWikiWriter> log) : IReportWikiWriter
{
    public const string SourceId = "kyc:reports";

    public static string DocumentIdForCase(Guid kycCaseId) =>
        $"kyc:report:{kycCaseId:N}";

    public async Task UpsertReportAsync(Guid kycCaseId, string markdownOrHtml, CancellationToken ct = default)
    {
        if (!options.Value.IsConfigured)
        {
            log.LogDebug(
                "ContextMemory not configured; skipping wiki upsert for case {CaseId}.",
                kycCaseId);
            return;
        }

        if (string.IsNullOrWhiteSpace(markdownOrHtml))
            return;

        var content = ToWikiContent(markdownOrHtml);
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

    public async Task ClearReportAsync(Guid kycCaseId, CancellationToken ct = default)
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
        var text = HtmlTagRegex().Replace(narrative, " ");
        text = WebUtility.HtmlDecode(text);
        return CollapseWhitespaceRegex().Replace(text, " ").Trim();
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex CollapseWhitespaceRegex();
}
