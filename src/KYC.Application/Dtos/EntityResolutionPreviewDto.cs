namespace KYC.Application.Dtos;

public record EntityResolutionPreviewDto(
    string NormalizedKey,
    string? LegalName,
    bool UsedFallback,
    bool Success,
    string? RegistrySource,
    string Message);
