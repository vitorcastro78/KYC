using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Domain.Entities;

public class KycCase
{
    public Guid Id { get; private set; }
    public string Nif { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public KycStatus Status { get; private set; }
    public RiskScore? Score { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? AssignedAnalystId { get; private set; }
    public decimal RequestedCreditAmount { get; private set; }
    public string RequestedCreditCurrency { get; private set; } = "EUR";
    public string RequestedBy { get; private set; } = string.Empty;

    public ICollection<CaseParty> Parties { get; } = new List<CaseParty>();
    public ICollection<RiskSignal> RiskSignals { get; } = new List<RiskSignal>();
    public ICollection<AuditEntry> AuditTrail { get; } = new List<AuditEntry>();
    public KycReport? FinalReport { get; private set; }

    private KycCase()
    {
    }

    public static KycCase Start(string nif, string companyName, string requestedBy, CreditAmount requestedAmount)
    {
        var id = Guid.NewGuid();
        var kyc = new KycCase
        {
            Id = id,
            Nif = nif,
            CompanyName = companyName,
            Status = KycStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            RequestedBy = requestedBy,
            RequestedCreditAmount = requestedAmount.Amount,
            RequestedCreditCurrency = requestedAmount.Currency
        };
        kyc.AuditTrail.Add(AuditEntry.Create(id, "CaseStarted", requestedBy, "User", null));
        return kyc;
    }

    public void MarkInProgress()
    {
        EnsureStatus(KycStatus.Pending);
        Status = KycStatus.InProgress;
    }

    public void AssignAnalyst(string analystId) => AssignedAnalystId = analystId;

    public void AddParty(CaseParty party)
    {
        if (party.KycCaseId != Id)
            throw new InvalidOperationException("Party belongs to another case.");
        Parties.Add(party);
    }

    public void AddRiskSignal(RiskSignal signal)
    {
        if (signal.KycCaseId != Id)
            throw new InvalidOperationException("Signal belongs to another case.");
        RiskSignals.Add(signal);
    }

    public void AppendAudit(AuditEntry entry)
    {
        if (entry.KycCaseId != Id)
            throw new InvalidOperationException("Audit belongs to another case.");
        AuditTrail.Add(entry);
    }

    public void SetScore(RiskScore score) => Score = score;

    public void SetFinalReport(KycReport report)
    {
        if (report.KycCaseId != Id)
            throw new InvalidOperationException("Report belongs to another case.");
        if (FinalReport is null)
        {
            FinalReport = report;
            return;
        }

        FinalReport.UpdateContent(report.NarrativeMarkdown, report.ModelUsed);
    }

    public void Approve(string analystId)
    {
        EnsureStatus(KycStatus.InProgress, KycStatus.UnderReview);
        Status = KycStatus.Approved;
        CompletedAt = DateTime.UtcNow;
        AppendAudit(AuditEntry.Create(Id, "Approved", analystId, "User"));
    }

    public void Reject(string analystId, string reason)
    {
        EnsureStatus(KycStatus.InProgress, KycStatus.UnderReview);
        Status = KycStatus.Rejected;
        CompletedAt = DateTime.UtcNow;
        AppendAudit(AuditEntry.Create(Id, "Rejected", analystId, "User", reason));
    }

    public void RequestManualReview(string reason)
    {
        Status = KycStatus.UnderReview;
        AppendAudit(AuditEntry.Create(Id, "ManualReviewRequested", AssignedAnalystId ?? "System", "User", reason));
    }

    /// <summary>Anonimização pós-retenção (preserva audit trail e chaves técnicas).</summary>
    public void AnonymizeForRetention()
    {
        CompanyName = "ANON";
        Nif = "000000000";
    }

    public void AutoApproveLowRisk(string actorId)
    {
        EnsureStatus(KycStatus.InProgress);
        Status = KycStatus.Approved;
        CompletedAt = DateTime.UtcNow;
        AppendAudit(AuditEntry.Create(Id, "AutoApproved", actorId, "Agent", "Critérios automáticos"));
    }

    public void MarkHumanReviewAfterScan(string actorId)
    {
        EnsureStatus(KycStatus.InProgress);
        Status = KycStatus.UnderReview;
        AppendAudit(AuditEntry.Create(Id, "ScanAwaitingHumanReview", actorId, "Agent"));
    }

    /// <summary>Adiciona uma parte manualmente (quadro social, accionistas, etc.).</summary>
    public void AddManualParty(CaseParty party, string actorId, string? auditDetails = null)
    {
        EnsureStatus(KycStatus.InProgress, KycStatus.UnderReview, KycStatus.Approved);
        AddParty(party);
        var details = auditDetails ?? $"{party.Name} ({party.Role})";
        AppendAudit(AuditEntry.Create(Id, "ManualPartyAdded", actorId, "User", details));
    }

    private void EnsureStatus(params KycStatus[] allowed)
    {
        if (!allowed.Contains(Status))
            throw new InvalidOperationException($"Invalid transition from {Status}.");
    }
}
