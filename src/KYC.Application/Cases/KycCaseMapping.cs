using KYC.Application.Dtos;
using KYC.Application.Services;
using KYC.Domain.Entities;

namespace KYC.Application.Cases;

internal static class KycCaseMapping
{
    public static KycCaseDto ToListDto(KycCase c) =>
        new(
            c.Id,
            c.Nif,
            c.CompanyName,
            c.Status,
            c.Score,
            c.CreatedAt,
            c.CompletedAt,
            c.AssignedAnalystId,
            c.Parties.Count,
            c.RiskSignals.Count,
            c.DueDiligenceLevel,
            c.SarStatus);

    public static KycCaseDetailDto? ToDetailDto(KycCase? c)
    {
        if (c is null) return null;
        var parties = c.Parties.Select(p => new CasePartyDto(
            p.Id,
            p.Name,
            p.Nif,
            p.Role,
            p.UboDepthLevel,
            p.OwnershipPercentage,
            p.IsPep,
            p.IsSanctioned,
            p.IsOffshore,
            p.VerificationStatus,
            p.VerificationMethod,
            p.VerificationSessionId,
            p.VerificationUrl,
            p.RcbeDiscrepancyDetected,
            p.RcbeDiscrepancyReported)).ToList();

        var partyNames = c.Parties.ToDictionary(p => p.Id, p => p.Name);
        var signals = c.RiskSignals.Select(s => new RiskSignalDetailDto(
            s.Id,
            s.Type,
            s.Severity,
            s.Description,
            s.Source,
            s.DetectedAt,
            s.IsConfirmed,
            s.CasePartyId,
            s.CasePartyId is { } pid && partyNames.TryGetValue(pid, out var name) ? name : null)).ToList();

        var audit = c.AuditTrail.Select(a => new AuditEntryDto(
            a.Action,
            a.ActorId,
            a.ActorType,
            a.Timestamp,
            a.Details)).ToList();

        var report = c.FinalReport is null
            ? null
            : new KycReportDto(c.Id, c.FinalReport.NarrativeHtml, c.FinalReport.ModelUsed, c.FinalReport.GeneratedAt);

        var documents = c.Documents
            .OrderByDescending(d => d.UploadedAt)
            .Select(ToDocumentDto)
            .ToList();

        var suggestSar = new SarEligibilityEvaluator().ShouldSuggestSar(c);
        var canApprove = c.CanApprove();
        var freezeRef = c.AuditTrail
            .Where(a => a.Action == "AssetFreezeNotificationSent")
            .OrderByDescending(a => a.Timestamp)
            .Select(a => a.Details)
            .FirstOrDefault();

        string[] sarActions =
        [
            "SarSubmitted",
            "SarUrgentSubmitted",
            "SarQueued",
            "SarNotRequired",
            "SarSuggested"
        ];
        var sarHistory = c.AuditTrail
            .Where(a => sarActions.Contains(a.Action))
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new SarAuditEntryDto(a.Action, a.ActorId, a.Timestamp, a.Details))
            .ToList();

        return new KycCaseDetailDto(
            c.Id,
            c.Nif,
            c.CompanyName,
            c.Status,
            c.Score,
            c.CreatedAt,
            c.CompletedAt,
            c.AssignedAnalystId,
            parties,
            signals,
            audit,
            report,
            documents,
            Gleif: null,
            GleifRelatedParties: null,
            c.DueDiligenceLevel,
            c.DueDiligenceJustification,
            c.SarStatus,
            c.SarReferenceNumber,
            c.NextReviewDue,
            c.FundsOriginDescription,
            suggestSar,
            canApprove.IsSuccess ? null : canApprove.Error,
            c.RelationshipType,
            c.LegalBasisRef,
            c.AssetFreezeNotified,
            c.AssetFreezeNotifiedAt,
            freezeRef,
            sarHistory);
    }

    public static CaseDocumentDto ToDocumentDto(CaseDocument d)
    {
        var facts = d.ExtractedFacts
            .Select(f => new DocumentExtractedFactDto(f.FactKey, f.FactValue, f.Confidence, f.SourcePage))
            .ToList();
        var parties = d.ExtractedParties
            .Select(p => new DocumentExtractedPartyDto(
                p.Name, p.Nif, p.Role, p.OwnershipPercentage, p.Nationality))
            .ToList();
        return new CaseDocumentDto(
            d.Id,
            d.KycCaseId,
            d.CasePartyId,
            d.FileName,
            d.ContentType,
            d.SizeBytes,
            d.DocumentKind,
            d.IngestionStatus,
            d.FailureReason,
            d.UploadedAt,
            d.UploadedBy,
            d.ProcessedAt,
            facts,
            parties,
            !string.IsNullOrWhiteSpace(d.ExtractedText));
    }
}
