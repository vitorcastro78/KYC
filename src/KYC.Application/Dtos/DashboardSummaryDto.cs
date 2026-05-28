namespace KYC.Application.Dtos;

public record DashboardSummaryDto(
    int TotalCases,
    int OpenCases,
    int ApprovedToday,
    int UnderReview,
    double ApprovalRatePercent);
