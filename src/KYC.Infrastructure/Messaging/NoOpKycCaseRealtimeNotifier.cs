using KYC.Application.Interfaces;
using KYC.Domain.Enums;

namespace KYC.Infrastructure.Messaging;

public class NoOpKycCaseRealtimeNotifier : IKycCaseRealtimeNotifier
{
    public Task NotifyScanProgressAsync(Guid caseId, string module, int percentComplete, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task NotifySignalDetectedAsync(Guid caseId, string signalSummary, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task NotifyReportReadyAsync(Guid caseId, RiskLevel level, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task NotifyStatusChangedAsync(Guid caseId, KycStatus newStatus, CancellationToken ct = default) =>
        Task.CompletedTask;
}
