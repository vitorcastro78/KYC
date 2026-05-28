namespace KYC.Application.Interfaces;

public interface IKycCaseMessageBus
{
    Task PublishCaseStartedAsync(Guid caseId, string nif, CancellationToken ct = default);

    Task PublishCaseRescreenAsync(Guid caseId, string actorId, CancellationToken ct = default);

    Task PublishEntityScanAsync(Guid caseId, Guid partyId, CancellationToken ct = default);

    Task PublishLlmSynthesisAsync(Guid caseId, CancellationToken ct = default);
}
