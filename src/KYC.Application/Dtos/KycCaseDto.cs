using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Application.Dtos;

public record KycCaseDto(
    Guid Id,
    string Nif,
    string CompanyName,
    KycStatus Status,
    RiskScore? Score,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? AssignedAnalystId,
    int PartyCount,
    int SignalCount,
    DueDiligenceLevel DueDiligenceLevel = DueDiligenceLevel.Standard,
    SarStatus SarStatus = SarStatus.None);
