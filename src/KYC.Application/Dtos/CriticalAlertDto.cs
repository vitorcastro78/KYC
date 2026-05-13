namespace KYC.Application.Dtos;

public record CriticalAlertDto(Guid CaseId, string CompanyName, string SignalSummary, DateTime DetectedAt);
