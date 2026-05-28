using KYC.Domain.Enums;

namespace KYC.Application.Dtos;

public record DocumentExtractedFactDto(
    DocumentFactKey FactKey,
    string FactValue,
    decimal? Confidence,
    int? SourcePage);

public record DocumentExtractedPartyDto(
    string Name,
    string? Nif,
    DocumentPartyRole Role,
    decimal? OwnershipPercentage,
    string? Nationality);

public record CaseDocumentDto(
    Guid Id,
    Guid KycCaseId,
    Guid? CasePartyId,
    string FileName,
    string ContentType,
    long SizeBytes,
    CaseDocumentKind DocumentKind,
    DocumentIngestionStatus IngestionStatus,
    string? FailureReason,
    DateTime UploadedAt,
    string UploadedBy,
    DateTime? ProcessedAt,
    IReadOnlyList<DocumentExtractedFactDto> Facts,
    IReadOnlyList<DocumentExtractedPartyDto> Parties,
    bool HasExtractedText);
