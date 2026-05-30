using KYC.Domain.Enums;

namespace KYC.Application.Dtos;

public record AmlComplianceReportDetailDto(
    Guid Id,
    int ReportingYear,
    AmlReportStatus Status,
    DateTime GeneratedAt,
    string GeneratedBy,
    string? BdpReferenceNumber,
    int TotalCasesProcessed,
    int TotalCasesApproved,
    int TotalCasesRejected,
    int TotalCasesUnderReview,
    int CasesLowRisk,
    int CasesMediumRisk,
    int CasesHighRisk,
    int CasesCriticalRisk,
    int TotalRiskSignalsDetected,
    int SanctionMatches,
    int PepMatches,
    int SarsSubmitted,
    int AssetFreezeNotifications,
    int CasesSimplifiedDd,
    int CasesStandardDd,
    int CasesEnhancedDd,
    int PeriodicReviewsCompleted,
    int PeriodicReviewsOverdue,
    string PlatformVersion,
    string AiModelsUsed);
