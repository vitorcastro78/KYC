namespace KYC.Application.Interfaces;

public interface IKycCasePipelineRunner
{
    Task RunCaseStartedAsync(Guid caseId, CancellationToken ct = default);
}
