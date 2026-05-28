namespace KYC.Application.Models;

/// <summary>Entidade ligada na rede corporativa GLEIF (Level 2) — candidata a triagem no caso.</summary>
public record GleifRelatedParty(
    string Lei,
    string LegalName,
    string? RegisteredAs,
    string? CountryIso2,
    string RelationshipKind);

public record GleifEnrichment(
    GleifCompanySnapshot? Snapshot,
    IReadOnlyList<GleifRelatedParty> RelatedParties);
