using KYC.Application.Compliance;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using MediatR;

namespace KYC.Application.Cases;

/// <summary>Submissão SAR à UIF (síncrona ou via fila).</summary>
public sealed class SarSubmissionProcessor(
    IKycCaseRepository repository,
    IUifReportingService uif,
    IMediator mediator)
{
    public async Task<UifSubmissionResult> SubmitAsync(
        Guid caseId,
        string suspicionDescription,
        string analystId,
        bool isUrgent,
        CancellationToken ct)
    {
        if (suspicionDescription.Length < 200)
            throw new ArgumentException("Narrativa de suspeita deve ter pelo menos 200 caracteres.");

        var kyc = await repository.GetByIdAsync(caseId, ct)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");

        ValidateEligibility(kyc);

        if (!isUrgent)
        {
            var queueRef = $"SAR-QUEUE-{Guid.NewGuid():N}";
            kyc.RecordSarQueued(queueRef, analystId);
            await repository.UpdateAsync(kyc, ct);
            return new UifSubmissionResult(true, queueRef, null, DateTime.UtcNow, IsQueued: true);
        }

        return await SubmitToUifAsync(kyc, suspicionDescription, analystId, isUrgent: true, ct);
    }

    public async Task ProcessQueuedAsync(SarSubmissionWork work, CancellationToken ct)
    {
        var kyc = await repository.GetByIdAsync(work.CaseId, ct)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");

        await SubmitToUifAsync(kyc, work.SuspicionDescription, work.AnalystId, isUrgent: false, ct);
    }

    private async Task<UifSubmissionResult> SubmitToUifAsync(
        KycCase kyc,
        string suspicionDescription,
        string analystId,
        bool isUrgent,
        CancellationToken ct)
    {
        var report = new SuspiciousActivityReport(
            kyc.Id,
            kyc.Nif,
            kyc.CompanyName,
            suspicionDescription,
            kyc.RiskSignals.Select(s => $"{s.Type}:{s.Source}").ToList(),
            kyc.RequestedCreditAmount,
            analystId,
            analystId,
            DateTime.UtcNow);

        var result = await uif.SubmitSuspiciousActivityReportAsync(report, ct);

        if (result.IsSuccess && result.ReferenceNumber is not null)
        {
            kyc.RecordSarSubmitted(result.ReferenceNumber, analystId);
            if (isUrgent)
            {
                kyc.AppendAudit(AuditEntry.Create(
                    kyc.Id,
                    "SarUrgentSubmitted",
                    analystId,
                    "User",
                    $"UIF síncrono: {result.ReferenceNumber}"));
            }

            await repository.UpdateAsync(kyc, ct);
            await mediator.Publish(
                new SarSubmittedNotification(kyc.Id, kyc.CompanyName, result.ReferenceNumber, isUrgent),
                ct);
        }
        else
        {
            if (kyc.SarStatus != SarStatus.Submitted)
            {
                if (isUrgent)
                    kyc.RecordSarPendingAfterApiFailure(analystId, result.ErrorMessage);
                else
                    kyc.AppendAudit(AuditEntry.Create(
                        kyc.Id,
                        "SarApiFailedPendingManual",
                        analystId,
                        "User",
                        result.ErrorMessage ?? "Submissão UIF em fila falhou — registo manual necessário."));
            }

            await repository.UpdateAsync(kyc, ct);
        }

        return result;
    }

    private static void ValidateEligibility(KycCase kyc)
    {
        var hasCritical = kyc.RiskSignals.Any(s => s.Severity == SignalSeverity.Critical && !s.IsConfirmed);
        var riskOk = kyc.Score?.Level >= RiskLevel.High || hasCritical
                     || kyc.Parties.Any(p => p.IsSanctioned);
        if (!riskOk)
            throw new InvalidOperationException(
                "SAR apenas permitido para risco Alto/Crítico, sinal Critical ou correspondência em sanções.");
    }
}
