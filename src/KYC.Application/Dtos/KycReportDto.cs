namespace KYC.Application.Dtos;

public record KycReportDto(Guid CaseId, string Markdown, string? ModelUsed, DateTime GeneratedAt);
