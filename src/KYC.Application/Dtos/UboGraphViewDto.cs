namespace KYC.Application.Dtos;

public record UboGraphViewDto(
    string CompanyName,
    string CompanyNif,
    bool HasGleifLevel2,
    IReadOnlyList<UboGraphNodeDto> Nodes,
    IReadOnlyList<UboGraphEdgeDto> Edges);

public record UboGraphNodeDto(
    Guid Id,
    Guid? CasePartyId,
    string Name,
    string? Nif,
    string EntityType,
    int Depth,
    decimal? OwnershipPct,
    string? RelationshipKind,
    string? CountryIso2,
    bool IsPep,
    bool IsSanctioned,
    bool IsOffshore,
    string? VerificationStatus,
    string? CaseRoleLabel,
    bool IsSynthetic);

public record UboGraphEdgeDto(
    Guid FromId,
    Guid ToId,
    decimal OwnershipPct,
    string? Label);
