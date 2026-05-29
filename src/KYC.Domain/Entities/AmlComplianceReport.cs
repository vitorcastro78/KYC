using KYC.Domain.Enums;

namespace KYC.Domain.Entities;

public class AmlComplianceReport
{
    public Guid Id { get; private set; }
    public int ReportingYear { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public string GeneratedBy { get; private set; } = string.Empty;
    public AmlReportStatus Status { get; private set; }
    public string? BdpReferenceNumber { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    public int TotalAmlAnalysts { get; private set; }
    public int TotalCasesProcessed { get; private set; }
    public int TotalCasesApproved { get; private set; }
    public int TotalCasesRejected { get; private set; }
    public int TotalCasesUnderReview { get; private set; }
    public int CasesLowRisk { get; private set; }
    public int CasesMediumRisk { get; private set; }
    public int CasesHighRisk { get; private set; }
    public int CasesCriticalRisk { get; private set; }
    public int TotalRiskSignalsDetected { get; private set; }
    public int SanctionMatches { get; private set; }
    public int PepMatches { get; private set; }
    public int SarsSubmitted { get; private set; }
    public int AssetFreezeNotifications { get; private set; }
    public int CasesSimplifiedDd { get; private set; }
    public int CasesStandardDd { get; private set; }
    public int CasesEnhancedDd { get; private set; }
    public int PeriodicReviewsCompleted { get; private set; }
    public int PeriodicReviewsOverdue { get; private set; }
    public string PlatformVersion { get; private set; } = "1.0.0";
    public string AiModelsUsed { get; private set; } = "{}";

    private AmlComplianceReport()
    {
    }

    public static AmlComplianceReport CreateDraft(int year, string generatedBy)
    {
        return new AmlComplianceReport
        {
            Id = Guid.NewGuid(),
            ReportingYear = year,
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = generatedBy,
            Status = AmlReportStatus.Draft
        };
    }

    public void PopulateMetrics(
        int totalCases, int approved, int rejected, int underReview,
        int low, int medium, int high, int critical,
        int signals, int sanctions, int peps, int sars, int freezes,
        int simplified, int standard, int enhanced,
        int reviewsCompleted, int reviewsOverdue,
        string platformVersion, string aiModelsJson)
    {
        TotalCasesProcessed = totalCases;
        TotalCasesApproved = approved;
        TotalCasesRejected = rejected;
        TotalCasesUnderReview = underReview;
        CasesLowRisk = low;
        CasesMediumRisk = medium;
        CasesHighRisk = high;
        CasesCriticalRisk = critical;
        TotalRiskSignalsDetected = signals;
        SanctionMatches = sanctions;
        PepMatches = peps;
        SarsSubmitted = sars;
        AssetFreezeNotifications = freezes;
        CasesSimplifiedDd = simplified;
        CasesStandardDd = standard;
        CasesEnhancedDd = enhanced;
        PeriodicReviewsCompleted = reviewsCompleted;
        PeriodicReviewsOverdue = reviewsOverdue;
        PlatformVersion = platformVersion;
        AiModelsUsed = aiModelsJson;
    }

    public void MarkSubmitted(string referenceNumber)
    {
        Status = AmlReportStatus.Submitted;
        BdpReferenceNumber = referenceNumber;
        SubmittedAt = DateTime.UtcNow;
    }
}
