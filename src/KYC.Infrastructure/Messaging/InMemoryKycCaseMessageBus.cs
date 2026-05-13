using KYC.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Messaging;

public sealed class InMemoryKycCaseMessageBus(
    InMemoryCaseStartedQueue queue,
    ILogger<InMemoryKycCaseMessageBus> log) : IKycCaseMessageBus
{
    public Task PublishCaseStartedAsync(Guid caseId, string nif, CancellationToken ct = default)
    {
        if (!queue.Writer.TryWrite(new CaseStartedWork(caseId, nif)))
            log.LogWarning("Fila in-memory de casos iniciados fechada ou cheia; mensagem ignorada para {CaseId}", caseId);
        return Task.CompletedTask;
    }

    public Task PublishEntityScanAsync(Guid caseId, Guid partyId, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task PublishLlmSynthesisAsync(Guid caseId, CancellationToken ct = default) =>
        Task.CompletedTask;
}
