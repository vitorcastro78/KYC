namespace KYC.Application.Interfaces;

public interface IKycCasePipelineRunner
{
    Task RunCaseStartedAsync(Guid caseId, CancellationToken ct = default);

    Task RunRescreenAsync(Guid caseId, string actorId, CancellationToken ct = default);
}
