namespace KYC.Application.Dtos;

public record RiskTimelineEntryDto(DateTime At, string Title, string Description, string Severity);

public record RiskTimelineDto(IReadOnlyList<RiskTimelineEntryDto> Entries);
