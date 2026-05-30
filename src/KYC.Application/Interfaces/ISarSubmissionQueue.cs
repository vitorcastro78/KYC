namespace KYC.Application.Interfaces;

public interface ISarSubmissionQueue
{
    ValueTask EnqueueAsync(SarSubmissionWork work, CancellationToken ct = default);
}

public readonly record struct SarSubmissionWork(
    Guid CaseId,
    string SuspicionDescription,
    string AnalystId);
