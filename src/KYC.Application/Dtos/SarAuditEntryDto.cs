namespace KYC.Application.Dtos;

public record SarAuditEntryDto(
    string Action,
    string ActorId,
    DateTime Timestamp,
    string? Details);
