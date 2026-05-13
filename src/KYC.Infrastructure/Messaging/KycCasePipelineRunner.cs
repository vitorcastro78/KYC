using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Messaging;

public class KycCasePipelineRunner(
    IKycCaseRepository cases,
    IEntityResolutionService resolution,
    ISanctionsScreeningService sanctions,
    IAdverseMediaService adverse,
    IFinancialHealthService financial,
    IJudicialIntelligenceService judicial,
    IIcijOffshoreService icij,
    IKycLlmEngine llm,
    IKycCaseScanProgressRepository progress,
    IKycCaseRealtimeNotifier notifier,
    IReportEmbeddingWriter embeddingWriter) : IKycCasePipelineRunner
{
    public async Task RunCaseStartedAsync(Guid caseId, CancellationToken ct = default)
    {
        var kyc = await cases.GetByIdAsync(caseId, ct) ?? throw new KeyNotFoundException("Case not found");
        await notifier.NotifyScanProgressAsync(caseId, "EntityResolution", 5, ct);

        var graph = await resolution.BuildUboGraphAsync(kyc.Nif, 5, ct);
        var targetPartyId = kyc.Parties.FirstOrDefault(p => p.Role == EntityRole.Target)?.Id;
        var uboNodeCount = graph.Nodes.Count(n => n.Depth > 0);
        var total = Math.Max(1, kyc.Parties.Count + uboNodeCount) * 5;
        await progress.UpsertAsync(new KycCaseScanProgressState(caseId, total, 0, 0), ct);

        foreach (var node in graph.Nodes.Where(n => n.Depth > 0))
        {
            var party = CaseParty.Create(
                kyc.Id,
                node.Type.Equals("Individual", StringComparison.OrdinalIgnoreCase) ? EntityType.Individual : EntityType.Company,
                node.Name,
                node.Nif,
                EntityRole.Ubo,
                node.OwnershipPct ?? 0,
                node.Depth,
                targetPartyId,
                null);
            kyc.AddParty(party);
        }

        var parties = kyc.Parties.ToList();

        var signals = new List<RiskSignal>();
        var pct = 10;
        foreach (var party in parties)
        {
            await notifier.NotifyScanProgressAsync(caseId, "Sanctions", pct, ct);
            var batch = await CasePartyScanOperations.CollectSignalsForPartyAsync(
                kyc, party, sanctions, adverse, financial, judicial, icij, ct);
            signals.AddRange(batch);

            pct = Math.Min(90, pct + 10);
            await progress.IncrementCompletedAsync(caseId, ct);
        }

        foreach (var sig in signals)
            kyc.AddRiskSignal(sig);

        await notifier.NotifyScanProgressAsync(caseId, "LLM", 92, ct);

        var ctx = BuildContext(kyc);
        var score = await llm.ComputeRiskScoreAsync(ctx, ct);
        kyc.SetScore(score);
        var report = await llm.GenerateNarrativeReportAsync(ctx, score, ct);
        kyc.SetFinalReport(report);

        var consistency = await llm.CheckConsistencyAsync(ctx, ct);
        if (!consistency.IsConsistent)
        {
            kyc.AddRiskSignal(RiskSignal.Create(
                kyc.Id,
                null,
                SignalType.Inconsistency,
                SignalSeverity.Medium,
                string.Join("; ", consistency.Issues),
                "ConsistencyEngine"));
        }

        if (score.Level == RiskLevel.Low && !kyc.RiskSignals.Any(x => x.Severity >= SignalSeverity.High))
            kyc.AutoApproveLowRisk("System");
        else
            kyc.MarkHumanReviewAfterScan("System");

        await embeddingWriter.EmbedReportTextAsync(caseId, report.NarrativeMarkdown, ct);
        await cases.UpdateAsync(kyc, ct);
        await notifier.NotifyReportReadyAsync(caseId, score.Level, ct);
        await notifier.NotifyStatusChangedAsync(caseId, kyc.Status, ct);
    }

    private static KycScanContext BuildContext(KycCase kyc)
    {
        var parties = kyc.Parties.Select(p => new PartyScanDto(
            p.Id,
            p.Name,
            p.Nif,
            p.Role.ToString(),
            p.UboDepthLevel,
            p.IsPep,
            p.IsSanctioned)).ToList();
        var sigs = kyc.RiskSignals.Select(s => new RiskSignalScanDto(
            s.Type.ToString(),
            s.Severity.ToString(),
            s.Description,
            s.Source)).ToList();
        return new KycScanContext(kyc.Id, kyc.Nif, kyc.CompanyName, parties, sigs, null);
    }
}
