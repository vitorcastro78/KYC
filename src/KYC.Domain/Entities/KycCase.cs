using KYC.Domain.Common;
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

    public DueDiligenceLevel DueDiligenceLevel { get; private set; } = DueDiligenceLevel.Standard;
    public string? DueDiligenceJustification { get; private set; }
    public RelationshipType RelationshipType { get; private set; } = RelationshipType.Ongoing;
    public DateTime? NextReviewDue { get; private set; }
    public DateTime? LastReviewedAt { get; private set; }
    public SarStatus SarStatus { get; private set; } = SarStatus.None;
    public string? SarReferenceNumber { get; private set; }
    public DateTime? SarSubmittedAt { get; private set; }
    public bool AssetFreezeNotified { get; private set; }
    public DateTime? AssetFreezeNotifiedAt { get; private set; }
    public string? ScoringEngineVersion { get; private set; }
    public string? ScoringModelSnapshot { get; private set; }
    public string LegalBasisRef { get; private set; } = "Lei83/2017-Art24";
    public string? FundsOriginDescription { get; private set; }
    public bool FundsOriginVerified { get; private set; }
    public string? FundsOriginDocumentId { get; private set; }
    public string? ApprovedByAnalystId { get; private set; }
    public string? SecondApproverId { get; private set; }

    public ICollection<CaseParty> Parties { get; } = new List<CaseParty>();
    public ICollection<RiskSignal> RiskSignals { get; } = new List<RiskSignal>();
    public ICollection<AuditEntry> AuditTrail { get; } = new List<AuditEntry>();
    public ICollection<CaseDocument> Documents { get; } = new List<CaseDocument>();
    public KycReport? FinalReport { get; private set; }

    private KycCase()
    {
    }

    public static KycCase Start(
        string nif,
        string companyName,
        string requestedBy,
        CreditAmount requestedAmount,
        RelationshipType? relationshipType = null)
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
            RequestedCreditCurrency = requestedAmount.Currency,
            RelationshipType = relationshipType
                ?? (requestedAmount.Amount >= 12500m ? RelationshipType.Ongoing : RelationshipType.Occasional)
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

    public void SetLegalBasisRef(string legalBasisRef) => LegalBasisRef = legalBasisRef;

    public void RequireSupervisorReviewAfterSanction(string actorId, string reason)
    {
        Status = KycStatus.UnderReview;
        AppendAudit(AuditEntry.Create(Id, "SanctionConfirmedUnderReview", actorId, "User", reason));
    }

    public void SetDueDiligenceLevel(DueDiligenceLevel level, string justification)
    {
        DueDiligenceLevel = level;
        DueDiligenceJustification = justification;
        AppendAudit(AuditEntry.Create(Id, "DueDiligenceLevelSet", "System", "Agent", $"{level}: {justification}"));
    }

    public void SetScoringEngineSnapshot(string version, string snapshotJson)
    {
        ScoringEngineVersion = version;
        ScoringModelSnapshot = snapshotJson;
    }

    public void SetFundsOrigin(string description, bool verified, string? documentId = null)
    {
        FundsOriginDescription = description;
        FundsOriginVerified = verified;
        FundsOriginDocumentId = documentId;
    }

    public void RecordSarQueued(string queueReference, string analystId)
    {
        SarStatus = SarStatus.Pending;
        SarReferenceNumber = queueReference;
        AppendAudit(AuditEntry.Create(Id, "SarQueued", analystId, "User", queueReference));
    }

    /// <summary>SAR urgente falhou na API — pendente de registo manual UIF.</summary>
    public void RecordSarPendingAfterApiFailure(string analystId, string? apiError)
    {
        SarStatus = SarStatus.Pending;
        SarReferenceNumber = null;
        AppendAudit(AuditEntry.Create(
            Id,
            "SarApiFailedPendingManual",
            analystId,
            "User",
            apiError ?? "Submissão UIF falhou — registo manual necessário."));
    }

    public void RecordSarSubmitted(string referenceNumber, string analystId)
    {
        SarStatus = SarStatus.Submitted;
        SarReferenceNumber = referenceNumber;
        SarSubmittedAt = DateTime.UtcNow;
        AppendAudit(AuditEntry.Create(Id, "SarSubmitted", analystId, "User", referenceNumber));
    }

    public void MarkSarNotRequired(string analystId, string justification)
    {
        SarStatus = SarStatus.NotRequired;
        AppendAudit(AuditEntry.Create(Id, "SarNotRequired", analystId, "User", justification));
    }

    public void RecordAssetFreezeNotification(string confirmationNumber)
    {
        AssetFreezeNotified = true;
        AssetFreezeNotifiedAt = DateTime.UtcNow;
        Status = KycStatus.UnderReview;
        AppendAudit(AuditEntry.Create(Id, "AssetFreezeNotificationSent", "System", "Agent", confirmationNumber));
    }

    public void RecordManualAssetFreezeNotification(string confirmationReference, string analystId)
    {
        AssetFreezeNotified = true;
        AssetFreezeNotifiedAt = DateTime.UtcNow;
        Status = KycStatus.UnderReview;
        AppendAudit(AuditEntry.Create(
            Id,
            "AssetFreezeManualRegistered",
            analystId,
            "User",
            confirmationReference.Trim()));
    }

    public void ScheduleNextReview(CustomerAcceptancePolicy policy)
    {
        var days = Score?.Level switch
        {
            RiskLevel.Low => policy.ReviewDaysLowRisk,
            RiskLevel.Medium => policy.ReviewDaysMediumRisk,
            RiskLevel.High => policy.ReviewDaysHighRisk,
            RiskLevel.Critical => policy.ReviewDaysCriticalRisk,
            _ => policy.ReviewDaysLowRisk
        };
        if (DueDiligenceLevel == DueDiligenceLevel.Enhanced)
            days = Math.Min(days, 90);

        NextReviewDue = DateTime.UtcNow.AddDays(days);
        LastReviewedAt = DateTime.UtcNow;
    }

    public Result CanApprove()
    {
        var unverified = Parties
            .Where(e => e.Role is EntityRole.Ubo or EntityRole.BoardMember or EntityRole.Proxy)
            .Where(e => e.VerificationStatus != IdentityVerificationStatus.Verified)
            .ToList();

        if (unverified.Count > 0)
            return Result.Failure(
                $"Aprovação bloqueada: {unverified.Count} entidade(s) com identidade não verificada: " +
                string.Join(", ", unverified.Select(e => e.Name)));

        if (DueDiligenceLevel == DueDiligenceLevel.Enhanced &&
            string.IsNullOrWhiteSpace(FundsOriginDescription))
            return Result.Failure("EDD requer declaração de origem de fundos (Art. 37.º Lei 83/2017).");

        if (Parties.Any(p => p.IsSanctioned)
            || RiskSignals.Any(s => s.Type == SignalType.Sanction && s.IsConfirmed))
            return Result.Failure("Aprovação bloqueada: correspondência em lista de sanções confirmada.");

        if (DueDiligenceLevel == DueDiligenceLevel.Enhanced)
        {
            var weakId = Parties
                .Where(e => e.Role is EntityRole.Ubo or EntityRole.BoardMember or EntityRole.Proxy)
                .Where(e => e.VerificationStatus == IdentityVerificationStatus.Verified)
                .Where(e => e.VerificationMethod is not (
                    IdentityVerificationMethod.Presential
                    or IdentityVerificationMethod.QualifiedSignature
                    or IdentityVerificationMethod.CMD))
                .ToList();
            if (weakId.Count > 0)
                return Result.Failure(
                    "EDD requer verificação presencial, CMD ou assinatura qualificada eIDAS para: " +
                    string.Join(", ", weakId.Select(e => e.Name)));
        }

        return Result.Success();
    }

    public Result CanProceedWithEnhancedDd()
    {
        if (DueDiligenceLevel != DueDiligenceLevel.Enhanced)
            return Result.Success();

        if (string.IsNullOrWhiteSpace(FundsOriginDescription))
            return Result.Failure("EDD requer declaração de origem de fundos.");

        return Result.Success();
    }

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

        FinalReport.UpdateContent(report.NarrativeHtml, report.ModelUsed);
    }

    public void Approve(string analystId, string? secondApproverId = null)
    {
        var check = CanApprove();
        if (!check.IsSuccess)
            throw new InvalidOperationException(check.Error);

        if (DueDiligenceLevel == DueDiligenceLevel.Enhanced)
        {
            if (string.IsNullOrWhiteSpace(secondApproverId) || secondApproverId == analystId)
                throw new InvalidOperationException("EDD requer aprovação 4-eyes com segundo aprovador distinto.");
            SecondApproverId = secondApproverId;
        }

        EnsureStatus(KycStatus.InProgress, KycStatus.UnderReview);
        Status = KycStatus.Approved;
        CompletedAt = DateTime.UtcNow;
        ApprovedByAnalystId = analystId;
        AppendAudit(AuditEntry.Create(Id, "Approved", analystId, "User", secondApproverId));
    }

    public void Reject(string analystId, string reason)
    {
        EnsureStatus(KycStatus.InProgress, KycStatus.UnderReview);
        Status = KycStatus.Rejected;
        CompletedAt = DateTime.UtcNow;
        AppendAudit(AuditEntry.Create(Id, "Rejected", analystId, "User", reason));
    }

    public void RejectByPolicy(string reason)
    {
        Status = KycStatus.Rejected;
        CompletedAt = DateTime.UtcNow;
        AppendAudit(AuditEntry.Create(Id, "AutoRejectedByPolicy", "System", "Agent", reason));
    }

    public void RequestManualReview(string reason)
    {
        Status = KycStatus.UnderReview;
        AppendAudit(AuditEntry.Create(Id, "ManualReviewRequested", AssignedAnalystId ?? "System", "User", reason));
    }

    public void AnonymizeForRetention()
    {
        CompanyName = "ANON";
        Nif = "000000000";
    }

    public bool CanAutoApproveLowRisk() =>
        Score is { Level: RiskLevel.Low, Overall: <= 30 }
        && !RiskSignals.Any(s => s.Severity >= SignalSeverity.High)
        && !RiskSignals.Any(s => s.Type == SignalType.Sanction && s.IsConfirmed)
        && !Parties.Any(p => p.IsSanctioned);

    public void AutoApproveLowRisk(string actorId)
    {
        EnsureStatus(KycStatus.InProgress);
        if (!CanAutoApproveLowRisk())
            throw new InvalidOperationException(
                "Auto-approve apenas para risco Low (score ≤30), sem sinais High/Critical nem sanções.");

        var check = CanApprove();
        if (!check.IsSuccess)
        {
            MarkHumanReviewAfterScan(actorId);
            return;
        }

        Status = KycStatus.Approved;
        CompletedAt = DateTime.UtcNow;
        AppendAudit(AuditEntry.Create(Id, "AutoApproved", actorId, "Agent", "Critérios automáticos Low"));
    }

    public void MarkHumanReviewAfterScan(string actorId)
    {
        EnsureStatus(KycStatus.InProgress);
        Status = KycStatus.UnderReview;
        AppendAudit(AuditEntry.Create(Id, "ScanAwaitingHumanReview", actorId, "Agent"));
    }

    public void PrepareForAutomaticRescreen(string actorId)
    {
        EnsureStatus(KycStatus.InProgress, KycStatus.UnderReview, KycStatus.Approved);
        if (Status != KycStatus.InProgress)
        {
            Status = KycStatus.InProgress;
            CompletedAt = null;
        }

        Parties.Clear();
        RiskSignals.Clear();
        Score = null;
        AppendAudit(AuditEntry.Create(Id, "AutomaticRescreenRequested", actorId, "User"));
    }

    public void RecordAutomaticRescreenCompleted(string actorId, int newSignalCount)
    {
        AppendAudit(AuditEntry.Create(
            Id,
            "AutomaticRescreenCompleted",
            actorId,
            "Agent",
            $"{newSignalCount} sinais gerados na nova triagem."));
    }

    public void RecordPeriodicReviewCompleted(string actorId)
    {
        AppendAudit(AuditEntry.Create(Id, "PeriodicReviewCompleted", actorId, "User"));
    }

    public void AddDocument(CaseDocument document, string actorId)
    {
        if (document.KycCaseId != Id)
            throw new InvalidOperationException("Document belongs to another case.");
        Documents.Add(document);
        AppendAudit(AuditEntry.Create(
            Id,
            "DocumentUploaded",
            actorId,
            "User",
            $"{document.FileName} ({document.DocumentKind})"));
    }

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
