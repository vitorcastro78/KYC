using KYC.Domain.Entities;
using KYC.Domain.Enums;

namespace KYC.Infrastructure.Compliance;

/// <summary>Agregação de métricas RPB a partir de casos em memória (testável sem BD).</summary>
public static class AmlComplianceMetricsBuilder
{
    public static void Apply(
        AmlComplianceReport report,
        IReadOnlyList<KycCase> cases,
        int reviewsCompleted,
        ScoringEngineConfig? scoring,
        string platformVersion = "1.0.0")
    {
        report.PopulateMetrics(
            totalCases: cases.Count,
            approved: cases.Count(c => c.Status == KycStatus.Approved),
            rejected: cases.Count(c => c.Status == KycStatus.Rejected),
            underReview: cases.Count(c => c.Status == KycStatus.UnderReview),
            low: cases.Count(c => c.Score?.Level == RiskLevel.Low),
            medium: cases.Count(c => c.Score?.Level == RiskLevel.Medium),
            high: cases.Count(c => c.Score?.Level == RiskLevel.High),
            critical: cases.Count(c => c.Score?.Level == RiskLevel.Critical),
            signals: cases.Sum(c => c.RiskSignals.Count),
            sanctions: cases.Sum(c => c.RiskSignals.Count(s => s.Type == SignalType.Sanction)),
            peps: cases.Count(c => c.Parties.Any(p => p.IsPep)),
            sars: cases.Count(c => c.SarStatus == SarStatus.Submitted),
            freezes: cases.Count(c => c.AssetFreezeNotified),
            simplified: cases.Count(c => c.DueDiligenceLevel == DueDiligenceLevel.Simplified),
            standard: cases.Count(c => c.DueDiligenceLevel == DueDiligenceLevel.Standard),
            enhanced: cases.Count(c => c.DueDiligenceLevel == DueDiligenceLevel.Enhanced),
            reviewsCompleted: reviewsCompleted,
            reviewsOverdue: cases.Count(c => c.NextReviewDue < DateTime.UtcNow && c.Status == KycStatus.Approved),
            platformVersion: platformVersion,
            aiModelsJson: AmlComplianceReportService.BuildOllamaOnlyModelsJson(scoring));
    }
}
