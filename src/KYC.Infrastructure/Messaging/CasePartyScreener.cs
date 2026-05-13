using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Messaging;

public sealed class CasePartyScreener(
    IKycCaseRepository cases,
    ISanctionsScreeningService sanctions,
    IAdverseMediaService adverse,
    IFinancialHealthService financial,
    IJudicialIntelligenceService judicial,
    IIcijOffshoreService icij,
    ILogger<CasePartyScreener> log) : ICasePartyScreener
{
    public async Task AppendScreeningSignalsAsync(Guid caseId, Guid partyId, string actorId, CancellationToken ct = default)
    {
        var kyc = await cases.GetByIdAsync(caseId, ct) ?? throw new KeyNotFoundException("Caso não encontrado.");
        if (kyc.Status is not (KycStatus.InProgress or KycStatus.UnderReview or KycStatus.Approved))
            throw new InvalidOperationException("Triagem por parte só é permitida em casos em análise ou aprovados.");

        var party = kyc.Parties.FirstOrDefault(p => p.Id == partyId)
                    ?? throw new KeyNotFoundException("Parte não encontrada neste caso.");

        var newSignals = await CasePartyScanOperations.CollectSignalsForPartyAsync(
            kyc, party, sanctions, adverse, financial, judicial, icij, ct);

        foreach (var sig in newSignals)
            kyc.AddRiskSignal(sig);

        kyc.AppendAudit(AuditEntry.Create(
            caseId,
            "PartyScreened",
            actorId,
            "User",
            $"Triagens para parte {party.Name} ({party.Id}) — {newSignals.Count} sinais novos."));

        await cases.UpdateAsync(kyc, ct);
        log.LogInformation("Triagens manuais concluídas para parte {PartyId} no caso {CaseId}.", partyId, caseId);
    }
}
