namespace KYC.Application.Interfaces;

public record KycCaseScanProgressState(
    Guid KycCaseId,
    int TotalScans,
    int CompletedScans,
    int FailedScans);

public interface IKycCaseScanProgressRepository
{
    Task<KycCaseScanProgressState?> GetAsync(Guid caseId, CancellationToken ct = default);
    Task UpsertAsync(KycCaseScanProgressState state, CancellationToken ct = default);
    Task IncrementCompletedAsync(Guid caseId, CancellationToken ct = default);
    Task IncrementFailedAsync(Guid caseId, CancellationToken ct = default);
}
