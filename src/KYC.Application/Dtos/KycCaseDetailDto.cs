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
    bool IsOffshore);

public record RiskSignalDetailDto(
    Guid Id,
    SignalType Type,
    SignalSeverity Severity,
    string Description,
    string Source,
    DateTime DetectedAt,
    bool IsConfirmed);

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
    KycReportDto? Report);
