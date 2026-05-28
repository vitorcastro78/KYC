namespace KYC.Application.Models;

public record PartyScanDto(
    Guid? PartyId,
    string Name,
    string? Nif,
    string Role,
    int Depth,
    bool IsPep,
    bool IsSanctioned);

public record RiskSignalScanDto(string Type, string Severity, string Description, string Source);

public record DocumentFactScanDto(
    Guid DocumentId,
    string Key,
    string Value);

public record DocumentPartyScanDto(
    Guid DocumentId,
    string Name,
    string? Nif,
    string Role,
    decimal? OwnershipPercentage);

public record KycScanContext(
    Guid CaseId,
    string Nif,
    string CompanyName,
    IReadOnlyList<PartyScanDto> Parties,
    IReadOnlyList<RiskSignalScanDto> Signals,
    IReadOnlyList<DocumentFactScanDto> DeclaredFacts,
    IReadOnlyList<DocumentPartyScanDto> DeclaredParties,
    string? DeclaredUboSummary);
