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

    public Task NotifyDocumentIngestionUpdatedAsync(
        Guid caseId,
        Guid documentId,
        DocumentIngestionStatus status,
        CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task NotifyComplianceAlertAsync(
        Guid caseId,
        string alertType,
        string message,
        CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task NotifySupervisorsAsync(string alertType, string message, CancellationToken ct = default) =>
        Task.CompletedTask;
}
