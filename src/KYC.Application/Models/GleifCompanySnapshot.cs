namespace KYC.Application.Models;

/// <summary>Resumo dos dados LEI obtidos na API GLEIF (https://api.gleif.org/api/v1/).</summary>
public record GleifCompanySnapshot(
    string Lei,
    string LegalName,
    string? Jurisdiction,
    string? LegalAddressCountry,
    string? RegisteredAs,
    string? RegisteredAtId,
    string? LegalFormId,
    string? EntityStatus,
    string? RegistrationStatus,
    string? LegalAddressSummary,
    string? HeadquartersAddressSummary,
    string? Ocid,
    string? Bic,
    string? ConformityFlag,
    DateTime? EntityCreationDate,
    DateTime? InitialRegistrationDate,
    DateTime? LastUpdateDate,
    IReadOnlyList<string> PreviousNames);
