using KYC.Application.Interfaces;
using KYC.Domain.Enums;
using KYC.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace KYC.Web.Services;

public class HubKycCaseRealtimeNotifier(IHubContext<KycCaseHub> hub) : IKycCaseRealtimeNotifier
{
    public Task NotifyScanProgressAsync(Guid caseId, string module, int percentComplete, CancellationToken ct = default) =>
        hub.Clients.Group(KycCaseHub.CaseGroup(caseId))
            .SendAsync("ScanProgressUpdated", caseId, module, percentComplete, ct);

    public Task NotifySignalDetectedAsync(Guid caseId, string signalSummary, CancellationToken ct = default) =>
        hub.Clients.Group(KycCaseHub.CaseGroup(caseId))
            .SendAsync("SignalDetected", caseId, signalSummary, ct);

    public Task NotifyReportReadyAsync(Guid caseId, RiskLevel level, CancellationToken ct = default) =>
        hub.Clients.Group(KycCaseHub.CaseGroup(caseId))
            .SendAsync("ReportReady", caseId, level.ToString(), ct);

    public Task NotifyStatusChangedAsync(Guid caseId, KycStatus newStatus, CancellationToken ct = default) =>
        hub.Clients.Group(KycCaseHub.CaseGroup(caseId))
            .SendAsync("StatusChanged", caseId, newStatus.ToString(), ct);

    public Task NotifyDocumentIngestionUpdatedAsync(
        Guid caseId,
        Guid documentId,
        DocumentIngestionStatus status,
        CancellationToken ct = default) =>
        hub.Clients.Group(KycCaseHub.CaseGroup(caseId))
            .SendAsync("DocumentIngestionUpdated", caseId, documentId, status.ToString(), ct);

    public Task NotifyComplianceAlertAsync(
        Guid caseId,
        string alertType,
        string message,
        CancellationToken ct = default) =>
        hub.Clients.Group(KycCaseHub.CaseGroup(caseId))
            .SendAsync("ComplianceAlert", caseId, alertType, message, ct);

    public Task NotifySupervisorsAsync(
        string alertType,
        string message,
        CancellationToken ct = default) =>
        hub.Clients.Group(KycCaseHub.SupervisorsGroup)
            .SendAsync("SupervisorAlert", alertType, message, ct);
}
