using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using MediatR;

namespace KYC.Application.Cases;

public class OverrideSignalCommandHandler(
    IKycCaseRepository repository,
    IAssetFreezeNotificationService assetFreeze,
    IKycCaseRealtimeNotifier notifier) : IRequestHandler<OverrideSignalCommand, Unit>
{
    public async Task<Unit> Handle(OverrideSignalCommand request, CancellationToken cancellationToken)
    {
        var result = await repository.GetCaseWithSignalAsync(request.SignalId, cancellationToken);
        if (result is null)
            throw new KeyNotFoundException("Sinal não encontrado.");

        var (kyc, signal) = result.Value;
        signal.OverrideConfirmation(request.Confirm, request.Notes);
        kyc.AppendAudit(AuditEntry.Create(
            kyc.Id,
            "AnalystOverride",
            request.AnalystId,
            "User",
            $"Signal {request.SignalId} confirm={request.Confirm}"));

        if (request.Confirm && signal.Type == SignalType.Sanction)
        {
            var partyId = signal.CasePartyId ?? kyc.Parties.FirstOrDefault()?.Id ?? Guid.Empty;
            var notify = await assetFreeze.NotifyAsync(
                kyc.Id,
                partyId,
                signal.Source,
                signal.Id.ToString(),
                request.AnalystId,
                cancellationToken);
            if (notify.IsSuccess)
                kyc.RecordAssetFreezeNotification(notify.ConfirmationNumber ?? "OK");

            kyc.RequireSupervisorReviewAfterSanction(request.AnalystId, signal.Description);
            await notifier.NotifyComplianceAlertAsync(
                kyc.Id,
                "AssetFreeze",
                $"Sanção confirmada — congelamento notificado. Ref: {notify.ConfirmationNumber}",
                cancellationToken);
        }

        await repository.UpdateAsync(kyc, cancellationToken);
        if (request.Confirm && signal.Type == SignalType.Sanction)
            await notifier.NotifyStatusChangedAsync(kyc.Id, kyc.Status, cancellationToken);
        return Unit.Value;
    }
}

public class SubmitSarCommandHandler(
    IKycCaseRepository repository,
    IUifReportingService uif) : IRequestHandler<SubmitSarCommand, UifSubmissionResult>
{
    public async Task<UifSubmissionResult> Handle(SubmitSarCommand request, CancellationToken cancellationToken)
    {
        if (request.SuspicionDescription.Length < 200)
            throw new ArgumentException("Narrativa de suspeita deve ter pelo menos 200 caracteres.");

        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");

        var hasCritical = kyc.RiskSignals.Any(s => s.Severity == SignalSeverity.Critical && !s.IsConfirmed);
        var riskOk = kyc.Score?.Level >= RiskLevel.High || hasCritical
                     || kyc.Parties.Any(p => p.IsSanctioned);
        if (!riskOk)
            throw new InvalidOperationException(
                "SAR apenas permitido para risco Alto/Crítico, sinal Critical ou correspondência em sanções.");

        var report = new SuspiciousActivityReport(
            kyc.Id,
            kyc.Nif,
            kyc.CompanyName,
            request.SuspicionDescription,
            kyc.RiskSignals.Select(s => $"{s.Type}:{s.Source}").ToList(),
            kyc.RequestedCreditAmount,
            request.AnalystId,
            request.AnalystId,
            DateTime.UtcNow);

        var result = await uif.SubmitSuspiciousActivityReportAsync(report, cancellationToken);

        if (result.IsSuccess && result.ReferenceNumber is not null)
        {
            kyc.RecordSarSubmitted(result.ReferenceNumber, request.AnalystId);
            if (request.IsUrgent)
            {
                kyc.AppendAudit(AuditEntry.Create(
                    kyc.Id,
                    "SarUrgentSubmitted",
                    request.AnalystId,
                    "User",
                    $"UIF síncrono: {result.ReferenceNumber}"));
            }
        }

        await repository.UpdateAsync(kyc, cancellationToken);
        return result;
    }
}

public class MarkSarNotRequiredCommandHandler(IKycCaseRepository repository)
    : IRequestHandler<MarkSarNotRequiredCommand, Unit>
{
    public async Task<Unit> Handle(MarkSarNotRequiredCommand request, CancellationToken cancellationToken)
    {
        if (request.Justification.Length < 50)
            throw new ArgumentException("Justificação deve ter pelo menos 50 caracteres.");

        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");
        kyc.MarkSarNotRequired(request.AnalystId, request.Justification);
        await repository.UpdateAsync(kyc, cancellationToken);
        return Unit.Value;
    }
}

public class RecordVerificationResultCommandHandler(IKycCaseRepository repository)
    : IRequestHandler<RecordVerificationResultCommand, Unit>
{
    public async Task<Unit> Handle(RecordVerificationResultCommand request, CancellationToken cancellationToken)
    {
        var match = await repository.GetCaseWithPartyBySessionIdAsync(request.SessionId, cancellationToken)
                    ?? await repository.GetCaseWithPartyAsync(request.PartyId, cancellationToken);
        if (match is null)
            throw new KeyNotFoundException("Sessão ou parte não encontrada.");

        var kyc = match.Value.Case;
        var party = match.Value.Party;
        var method = party.VerificationMethod;
        party.RecordVerificationResult(request.IsVerified, method);
        kyc.AppendAudit(AuditEntry.Create(
            kyc.Id,
            request.IsVerified ? "IdentityVerified" : "IdentityVerificationFailed",
            "System",
            "Agent",
            request.FailureReason ?? request.EidasLevel));
        await repository.UpdateAsync(kyc, cancellationToken);
        return Unit.Value;
    }
}

public class ReportRcbeDiscrepancyCommandHandler(IKycCaseRepository repository)
    : IRequestHandler<ReportRcbeDiscrepancyCommand, Unit>
{
    public async Task<Unit> Handle(ReportRcbeDiscrepancyCommand request, CancellationToken cancellationToken)
    {
        var match = await repository.GetCaseWithPartyAsync(request.PartyId, cancellationToken)
                    ?? throw new KeyNotFoundException("Parte não encontrada.");
        var kyc = match.Case;
        var party = match.Party;
        if (kyc.Id != request.CaseId)
            throw new InvalidOperationException("Parte não pertence ao caso.");

        party.ReportRcbeDiscrepancy();
        kyc.AppendAudit(AuditEntry.Create(kyc.Id, "RcbeDiscrepancyReported", request.AnalystId, "User", party.Name));
        await repository.UpdateAsync(kyc, cancellationToken);
        return Unit.Value;
    }
}

public class InitiateEntityVerificationCommandHandler(
    IKycCaseRepository repository,
    IIdentityVerificationService identity) : IRequestHandler<InitiateEntityVerificationCommand, IdentityVerificationSession>
{
    public async Task<IdentityVerificationSession> Handle(
        InitiateEntityVerificationCommand request,
        CancellationToken cancellationToken)
    {
        var (kyc, party) = await repository.GetCaseWithPartyAsync(request.PartyId, cancellationToken)
                           ?? throw new KeyNotFoundException("Parte não encontrada.");
        if (kyc.Id != request.CaseId)
            throw new InvalidOperationException("Parte não pertence ao caso.");

        var session = await identity.InitiateVerificationAsync(
            party.Id, request.Method, party.Name, null, cancellationToken);
        party.StartVerification(request.Method, session.SessionId);
        await repository.UpdateAsync(kyc, cancellationToken);
        return session;
    }
}

public class RecordPresentialVerificationCommandHandler(
    IKycCaseRepository repository,
    IIdentityVerificationService identity) : IRequestHandler<RecordPresentialVerificationCommand, Unit>
{
    public async Task<Unit> Handle(RecordPresentialVerificationCommand request, CancellationToken cancellationToken)
    {
        var (kyc, party) = await repository.GetCaseWithPartyAsync(request.PartyId, cancellationToken)
                           ?? throw new KeyNotFoundException("Parte não encontrada");

        await identity.RecordPresentialVerificationAsync(
            party.Id, request.AnalystId, request.DocumentReference, cancellationToken);
        party.RecordPresentialVerification(request.DocumentReference);
        await repository.UpdateAsync(kyc, cancellationToken);
        return Unit.Value;
    }
}
