using KYC.Application.Models;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Application.Dtos;

public record CasePartyDto(
    Guid Id,
    string Name,
    string? Nif,
    EntityRole Role,
    int UboDepthLevel,
    decimal OwnershipPercentage,
    bool IsPep,
    bool IsSanctioned,
    bool IsOffshore,
    IdentityVerificationStatus VerificationStatus = IdentityVerificationStatus.Pending,
    IdentityVerificationMethod VerificationMethod = IdentityVerificationMethod.NotYetVerified,
    bool RcbeDiscrepancyDetected = false,
    bool RcbeDiscrepancyReported = false);

public record RiskSignalDetailDto(
    Guid Id,
    SignalType Type,
    SignalSeverity Severity,
    string Description,
    string Source,
    DateTime DetectedAt,
    bool IsConfirmed,
    Guid? CasePartyId = null,
    string? PartyName = null);

public record AuditEntryDto(string Action, string ActorId, string ActorType, DateTime Timestamp, string? Details);

public record KycCaseDetailDto(
    Guid Id,
    string Nif,
    string CompanyName,
    KycStatus Status,
    RiskScore? Score,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? AssignedAnalystId,
    IReadOnlyList<CasePartyDto> Parties,
    IReadOnlyList<RiskSignalDetailDto> Signals,
    IReadOnlyList<AuditEntryDto> AuditTrail,
    KycReportDto? Report,
    IReadOnlyList<CaseDocumentDto> Documents,
    GleifCompanySnapshot? Gleif = null,
    IReadOnlyList<GleifRelatedParty>? GleifRelatedParties = null,
    DueDiligenceLevel DueDiligenceLevel = DueDiligenceLevel.Standard,
    string? DueDiligenceJustification = null,
    SarStatus SarStatus = SarStatus.None,
    string? SarReferenceNumber = null,
    DateTime? NextReviewDue = null,
    string? FundsOriginDescription = null,
    bool SuggestSar = false,
    string? CanApproveMessage = null,
    RelationshipType RelationshipType = RelationshipType.Ongoing,
    string? LegalBasisRef = null,
    bool AssetFreezeNotified = false,
    DateTime? AssetFreezeNotifiedAt = null,
    string? AssetFreezeConfirmationRef = null);
