namespace KYC.Application.Models;

public record EntityResolutionResult(
    string Nif,
    string LegalName,
    string? Jurisdiction,
    string? RegistryId,
    bool Success,
    string? ErrorMessage,
    bool UsedFallback = false,
    GleifCompanySnapshot? Gleif = null);
