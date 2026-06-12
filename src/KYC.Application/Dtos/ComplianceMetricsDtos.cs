namespace KYC.Application.Dtos;

public record ScreeningMetricsDto(
    int TotalSignals,
    int SanctionSignals,
    int SanctionConfirmed,
    int SanctionDismissed,
    int HighSeverityUnconfirmed,
    decimal FalsePositiveRatePct,
    decimal FalseNegativeRateEstimatePct,
    string Methodology);

public record BiometricMetricsDto(
    int TotalVerificationAttempts,
    int Verified,
    int Failed,
    int WithLivenessScore,
    decimal? AverageLivenessScore,
    decimal FalseAcceptRatePct,
    decimal FalseRejectRatePct,
    string? Iso30107ComplianceNote,
    string Methodology);

public record ComplianceMetricsBundleDto(
    ScreeningMetricsDto Screening,
    BiometricMetricsDto Biometric,
    DateTime GeneratedAtUtc);
