using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using MediatR;

namespace KYC.Application.Cases;

public class OverrideSignalCommandHandler(
    IKycCaseRepository repository,
    IAssetFreezeNotificationService assetFreeze) : IRequestHandler<OverrideSignalCommand, Unit>
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
        }

        await repository.UpdateAsync(kyc, cancellationToken);
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

        var result = request.IsUrgent
            ? await uif.SubmitSuspiciousActivityReportAsync(report, cancellationToken)
            : await uif.SubmitSuspiciousActivityReportAsync(report, cancellationToken);

        if (result.IsSuccess && result.ReferenceNumber is not null)
            kyc.RecordSarSubmitted(result.ReferenceNumber, request.AnalystId);

        await repository.UpdateAsync(kyc, cancellationToken);
        return result;
    }
}

public class MarkSarNotRequiredCommandHandler(IKycCaseRepository repository)
    : IRequestHandler<MarkSarNotRequiredCommand, Unit>
{
    public async Task<Unit> Handle(MarkSarNotRequiredCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");
        kyc.MarkSarNotRequired(request.AnalystId, request.Justification);
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
