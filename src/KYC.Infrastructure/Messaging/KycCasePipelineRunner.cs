using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Application.Services;
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
    IReportEmbeddingWriter embeddingWriter,
    IDocumentConsistencyChecker documentConsistency,
    ICustomerAcceptancePolicyRepository policyRepo,
    IScoringEngineConfigRepository scoringRepo,
    DueDiligenceLevelEvaluator ddEvaluator,
    PolicyComplianceValidator policyValidator,
    SarEligibilityEvaluator sarEvaluator,
    ILogger<KycCasePipelineRunner> log) : IKycCasePipelineRunner
{
    public Task RunCaseStartedAsync(Guid caseId, CancellationToken ct = default) =>
        ExecuteAsync(caseId, "System", isRescreen: false, ct);

    public Task RunRescreenAsync(Guid caseId, string actorId, CancellationToken ct = default) =>
        ExecuteAsync(caseId, actorId, isRescreen: true, ct);

    private async Task ExecuteAsync(Guid caseId, string actorId, bool isRescreen, CancellationToken ct)
    {
        var kyc = await cases.GetByIdAsync(caseId, ct) ?? throw new KeyNotFoundException("Case not found");

        if (isRescreen)
        {
            kyc.PrepareForAutomaticRescreen(actorId);
            EnsureTargetPartyExists(kyc);
            await cases.UpdateAsync(kyc, ct);
            await progress.UpsertAsync(new KycCaseScanProgressState(caseId, 1, 0, 0), ct);
            await notifier.NotifyScanProgressAsync(caseId, "A iniciar", 0, ct);
        }

        await notifier.NotifyScanProgressAsync(caseId, "EntityResolution", 5, ct);

        await SyncGleifPartiesAsync(kyc, ct);

        var policy = await policyRepo.GetActiveAsync(ct)
                     ?? CustomerAcceptancePolicy.CreateV1("System");
        var policyResult = policyValidator.Validate(kyc.Parties.ToList(), caeCode: null, policy);
        if (policyResult.AutoRejected)
        {
            kyc.RejectByPolicy(string.Join("; ", policyResult.Violations));
            await cases.UpdateAsync(kyc, ct);
            await notifier.NotifyStatusChangedAsync(kyc.Id, kyc.Status, ct);
            log.LogWarning("Caso {CaseId} auto-rejeitado pela PAC: {Violations}", caseId, string.Join("; ", policyResult.Violations));
            return;
        }

        var dd = ddEvaluator.Evaluate(
            kyc.RequestedCreditAmount,
            kyc.RelationshipType,
            kyc.Parties.ToList(),
            policy);
        kyc.SetDueDiligenceLevel(dd.Level, dd.Justification);

        var scoringConfig = await scoringRepo.GetActiveAsync(ct);
        if (scoringConfig is not null)
        {
            kyc.SetScoringEngineSnapshot(
                scoringConfig.Version,
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    scoringConfig.LocalModelName,
                    scoringConfig.CloudModelName,
                    scoringConfig.SystemPromptHash,
                    scoringConfig.WeightsJson
                }));
        }

        var parties = kyc.Parties.ToList();
        var total = Math.Max(1, parties.Count);
        await progress.UpsertAsync(new KycCaseScanProgressState(caseId, total, 0, 0), ct);

        var signals = new List<RiskSignal>();
        var pct = 10;
        foreach (var party in parties)
        {
            await notifier.NotifyScanProgressAsync(caseId, "Sanctions", pct, ct);
            var batch = await CasePartyScanOperations.CollectSignalsForPartyAsync(
                kyc, party, sanctions, adverse, financial, judicial, icij, ct);
            signals.AddRange(batch);

            pct = Math.Min(90, pct + Math.Max(1, 80 / Math.Max(1, parties.Count)));
            await progress.IncrementCompletedAsync(caseId, ct);
        }

        foreach (var sig in signals)
            kyc.AddRiskSignal(sig);

        foreach (var docSignal in documentConsistency.Check(kyc))
            kyc.AddRiskSignal(docSignal);

        await notifier.NotifyScanProgressAsync(caseId, "Documents", 91, ct);

        await notifier.NotifyScanProgressAsync(caseId, "LLM", 92, ct);

        var ctx = BuildContext(kyc);
        var score = await llm.ComputeRiskScoreAsync(ctx, ct);
        kyc.SetScore(score);

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

        ctx = BuildContext(kyc);
        var composeRequest = BuildReportRequest(kyc, ctx, score);
        var report = await llm.GenerateNarrativeReportAsync(ctx, score, composeRequest, ct);
        kyc.SetFinalReport(report);

        if (score.Level == RiskLevel.Low && !kyc.RiskSignals.Any(x => x.Severity >= SignalSeverity.High))
            kyc.AutoApproveLowRisk(actorId);
        else
            kyc.MarkHumanReviewAfterScan(actorId);

        if (sarEvaluator.ShouldSuggestSar(kyc) && kyc.SarStatus == SarStatus.None)
            kyc.AppendAudit(AuditEntry.Create(kyc.Id, "SarSuggested", "System", "Agent", "Condições SAR detectadas"));

        if (isRescreen)
            kyc.RecordAutomaticRescreenCompleted(actorId, signals.Count);

        await embeddingWriter.EmbedReportTextAsync(caseId, report.NarrativeHtml, ct);
        await cases.UpdateAsync(kyc, ct);
        await progress.UpsertAsync(new KycCaseScanProgressState(caseId, total, total, 0), ct);
        await notifier.NotifyScanProgressAsync(caseId, "Concluído", 100, ct);
        await notifier.NotifyReportReadyAsync(caseId, score.Level, ct);
        await notifier.NotifyStatusChangedAsync(caseId, kyc.Status, ct);

        log.LogInformation(
            "Pipeline {Mode} concluído para o caso {CaseId}: {SignalCount} sinais novos, estado {Status}.",
            isRescreen ? "re-triagem" : "inicial",
            caseId,
            signals.Count,
            kyc.Status);
    }

    private async Task SyncGleifPartiesAsync(KycCase kyc, CancellationToken ct)
    {
        EnsureTargetPartyExists(kyc);
        var graph = await resolution.BuildUboGraphAsync(kyc.Nif, 5, ct);
        var targetPartyId = kyc.Parties.FirstOrDefault(p => p.Role == EntityRole.Target)?.Id;
        var existingKeys = BuildPartyMatchKeys(kyc);

        foreach (var node in graph.Nodes.Where(n => n.Depth > 0))
        {
            var key = PartyMatchKey(node.Nif, node.Name);
            if (!existingKeys.Add(key))
                continue;

            var role = MapGleifPartyRole(node.GleifRelationshipKind);
            var party = CaseParty.Create(
                kyc.Id,
                node.Type.Equals("Individual", StringComparison.OrdinalIgnoreCase)
                    ? EntityType.Individual
                    : EntityType.Company,
                node.Name,
                node.Nif,
                role,
                node.OwnershipPct ?? 0,
                node.Depth,
                targetPartyId,
                node.CountryIso2);
            kyc.AddParty(party);
        }
    }

    private static HashSet<string> BuildPartyMatchKeys(KycCase kyc)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var party in kyc.Parties)
        {
            keys.Add(PartyMatchKey(party.Nif, party.Name));
        }

        return keys;
    }

    private static void EnsureTargetPartyExists(KycCase kyc)
    {
        if (kyc.Parties.Any(p => p.Role == EntityRole.Target))
            return;

        var target = CaseParty.Create(
            kyc.Id,
            EntityType.Company,
            kyc.CompanyName,
            kyc.Nif,
            EntityRole.Target,
            ownershipPercentage: 100,
            uboDepthLevel: 0,
            parentPartyId: null);
        kyc.AddParty(target);
    }

    private static string PartyMatchKey(string? nif, string name)
    {
        if (!string.IsNullOrWhiteSpace(nif))
            return nif.Trim().ToUpperInvariant();
        return name.Trim().ToUpperInvariant();
    }

    private static EntityRole MapGleifPartyRole(string? gleifRelationshipKind) =>
        string.IsNullOrEmpty(gleifRelationshipKind) ||
        gleifRelationshipKind.Equals("Self", StringComparison.OrdinalIgnoreCase)
            ? EntityRole.Ubo
            : EntityRole.Shareholder;

    private static KycReportComposeRequest BuildReportRequest(KycCase kyc, KycScanContext ctx, RiskScore score) =>
        new(
            kyc.Id,
            kyc.Nif,
            kyc.CompanyName,
            kyc.Status,
            kyc.RequestedCreditAmount,
            kyc.RequestedCreditCurrency,
            kyc.CreatedAt,
            ctx.Parties,
            ctx.Signals,
            score,
            DateTime.UtcNow);

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

        var declaredFacts = kyc.Documents
            .Where(d => d.IngestionStatus == DocumentIngestionStatus.Completed)
            .SelectMany(d => d.ExtractedFacts.Select(f => new DocumentFactScanDto(
                d.Id,
                f.FactKey.ToString(),
                f.FactValue)))
            .ToList();

        var declaredParties = kyc.Documents
            .Where(d => d.IngestionStatus == DocumentIngestionStatus.Completed)
            .SelectMany(d => d.ExtractedParties.Select(p => new DocumentPartyScanDto(
                d.Id,
                p.Name,
                p.Nif,
                p.Role.ToString(),
                p.OwnershipPercentage)))
            .ToList();

        var uboSummary = declaredParties.Count == 0
            ? null
            : string.Join("; ", declaredParties.Select(p =>
                $"{p.Name} ({p.Role}){(p.OwnershipPercentage is { } pct ? $" {pct}%" : string.Empty)}"));

        return new KycScanContext(
            kyc.Id,
            kyc.Nif,
            kyc.CompanyName,
            parties,
            sigs,
            declaredFacts,
            declaredParties,
            uboSummary);
    }
}
