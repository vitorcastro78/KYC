using KYC.Domain.Enums;

namespace KYC.Application.Dtos;

public record AmlComplianceReportListItemDto(
    Guid Id,
    int ReportingYear,
    AmlReportStatus Status,
    DateTime GeneratedAt,
    string? BdpReferenceNumber);
