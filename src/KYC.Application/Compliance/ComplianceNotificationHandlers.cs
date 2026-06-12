using KYC.Application.Interfaces;
using MediatR;

namespace KYC.Application.Compliance;

public sealed class EntityIdentityVerifiedNotificationHandler(IKycCaseRealtimeNotifier notifier)
    : INotificationHandler<EntityIdentityVerifiedNotification>
{
    public async Task Handle(EntityIdentityVerifiedNotification n, CancellationToken ct)
    {
        await notifier.NotifyComplianceAlertAsync(
            n.CaseId,
            "IdentityVerified",
            $"Identidade verificada: {n.PartyName}",
            ct);
    }
}

public sealed class EntityIdentityVerificationFailedNotificationHandler(IKycCaseRealtimeNotifier notifier)
    : INotificationHandler<EntityIdentityVerificationFailedNotification>
{
    public async Task Handle(EntityIdentityVerificationFailedNotification n, CancellationToken ct)
    {
        await notifier.NotifyComplianceAlertAsync(
            n.CaseId,
            "IdentityFailed",
            $"Verificação falhou ({n.PartyName}): {n.Reason ?? "sem motivo"}",
            ct);
    }
}

public sealed class SarSubmittedNotificationHandler(IKycCaseRealtimeNotifier notifier)
    : INotificationHandler<SarSubmittedNotification>
{
    public async Task Handle(SarSubmittedNotification n, CancellationToken ct)
    {
        var msg = $"SAR submetido à UIF — {n.CompanyName} — ref. {n.ReferenceNumber}" +
                  (n.IsUrgent ? " (URGENTE)" : "");
        await notifier.NotifyComplianceAlertAsync(n.CaseId, "SarSubmitted", msg, ct);
        await notifier.NotifySupervisorsAsync("SarSubmitted", msg, ct);
    }
}
