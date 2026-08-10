namespace KYC.Application.Interfaces;

/// <summary>
/// Persists KYC report knowledge into ContextMemory Global Wiki (CompanyBrain pattern).
/// </summary>
public interface IReportWikiWriter
{
    /// <summary>Upsert the case report into ContextMemory Global Wiki.</summary>
    Task UpsertReportAsync(Guid kycCaseId, string markdownOrHtml, CancellationToken ct = default);

    /// <summary>Remove the case report document from ContextMemory Global Wiki.</summary>
    Task ClearReportAsync(Guid kycCaseId, CancellationToken ct = default);
}
