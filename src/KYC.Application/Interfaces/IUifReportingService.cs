namespace KYC.Application.Interfaces;

public interface IUifReportingService
{
    Task<UifSubmissionResult> SubmitSuspiciousActivityReportAsync(
        SuspiciousActivityReport report,
        CancellationToken ct = default);

    Task<UifSubmissionStatus> GetSubmissionStatusAsync(string referenceNumber, CancellationToken ct = default);
}

public record SuspiciousActivityReport(
    Guid KycCaseId,
    string Nif,
    string CompanyName,
    string SuspicionDescription,
    IReadOnlyList<string> SignalSources,
    decimal? AmountInvolved,
    string SubmittedByAnalystId,
    string SubmittedByAnalystName,
    DateTime DetectedAt);

public record UifSubmissionResult(
    bool IsSuccess,
    string? ReferenceNumber,
    string? ErrorMessage,
    DateTime SubmittedAt,
    bool IsQueued = false);

public record UifSubmissionStatus(string ReferenceNumber, string Status, DateTime? LastUpdated);
