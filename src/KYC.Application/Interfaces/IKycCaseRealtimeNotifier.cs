using KYC.Domain.Enums;

namespace KYC.Application.Interfaces;

public interface IKycCaseRealtimeNotifier
{
    Task NotifyScanProgressAsync(Guid caseId, string module, int percentComplete, CancellationToken ct = default);
    Task NotifySignalDetectedAsync(Guid caseId, string signalSummary, CancellationToken ct = default);
    Task NotifyReportReadyAsync(Guid caseId, RiskLevel level, CancellationToken ct = default);
    Task NotifyStatusChangedAsync(Guid caseId, KycStatus newStatus, CancellationToken ct = default);
}
