using KYC.Application.Compliance;
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
    SarSubmissionProcessor processor,
    ISarSubmissionQueue sarQueue) : IRequestHandler<SubmitSarCommand, UifSubmissionResult>
{
    public async Task<UifSubmissionResult> Handle(SubmitSarCommand request, CancellationToken cancellationToken)
    {
        if (request.IsUrgent)
            return await processor.SubmitAsync(
                request.CaseId,
                request.SuspicionDescription,
                request.AnalystId,
                isUrgent: true,
                cancellationToken);

        var result = await processor.SubmitAsync(
            request.CaseId,
            request.SuspicionDescription,
            request.AnalystId,
            isUrgent: false,
            cancellationToken);

        if (result.IsQueued)
        {
            await sarQueue.EnqueueAsync(
                new SarSubmissionWork(request.CaseId, request.SuspicionDescription, request.AnalystId),
                cancellationToken);
        }

        return result;
    }
}

public class RegisterManualUifReferenceCommandHandler(
    IKycCaseRepository repository,
    IMediator mediator) : IRequestHandler<RegisterManualUifReferenceCommand, Unit>
{
    public async Task<Unit> Handle(RegisterManualUifReferenceCommand request, CancellationToken cancellationToken)
    {
        if (request.ReferenceNumber.Length < 5)
            throw new ArgumentException("Referência UIF inválida.");

        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");

        if (kyc.SarStatus is not SarStatus.Pending and not SarStatus.None)
            throw new InvalidOperationException("SAR já foi finalizado para este caso.");

        kyc.RecordSarSubmitted(request.ReferenceNumber.Trim(), request.AnalystId);
        kyc.AppendAudit(AuditEntry.Create(
            kyc.Id,
            "SarManualRegistered",
            request.AnalystId,
            "User",
            request.ReferenceNumber.Trim()));

        await repository.UpdateAsync(kyc, cancellationToken);
        await mediator.Publish(
            new Compliance.SarSubmittedNotification(kyc.Id, kyc.CompanyName, request.ReferenceNumber.Trim(), false),
            cancellationToken);
        return Unit.Value;
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

public class RecordVerificationResultCommandHandler(
    IKycCaseRepository repository,
    IMediator mediator) : IRequestHandler<RecordVerificationResultCommand, Unit>
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

        if (request.IsVerified)
        {
            await mediator.Publish(
                new EntityIdentityVerifiedNotification(kyc.Id, party.Id, party.Name),
                cancellationToken);
        }
        else
        {
            await mediator.Publish(
                new EntityIdentityVerificationFailedNotification(
                    kyc.Id,
                    party.Id,
                    party.Name,
                    request.FailureReason),
                cancellationToken);
        }

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
        party.StartVerification(request.Method, session.SessionId, session.VerificationUrl);
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
