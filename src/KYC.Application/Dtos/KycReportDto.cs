namespace KYC.Application.Dtos;

public record KycReportDto(Guid CaseId, string Html, string? ModelUsed, DateTime GeneratedAt);
