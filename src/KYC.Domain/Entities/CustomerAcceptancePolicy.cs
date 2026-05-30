namespace KYC.Domain.Entities;

public class CustomerAcceptancePolicy
{
    public Guid Id { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public string ApprovedBy { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public decimal OccasionalThreshold { get; private set; }
    public decimal EnhancedDdThreshold { get; private set; }
    public string HighRiskJurisdictionsJson { get; private set; } = "[]";
    public string ProhibitedJurisdictionsJson { get; private set; } = "[]";
    public string ProhibitedCaeActivitiesJson { get; private set; } = "[]";
    public bool BlockShellCompanies { get; private set; }
    public bool BlockOffshoreAboveThreshold { get; private set; }
    public decimal OffshoreBlockThreshold { get; private set; }
    public int ReviewDaysLowRisk { get; private set; }
    public int ReviewDaysMediumRisk { get; private set; }
    public int ReviewDaysHighRisk { get; private set; }
    public int ReviewDaysCriticalRisk { get; private set; }

    private CustomerAcceptancePolicy()
    {
    }

    public static CustomerAcceptancePolicy CreateV1(string approvedBy)
    {
        return new CustomerAcceptancePolicy
        {
            Id = Guid.NewGuid(),
            Version = "1.0.0",
            EffectiveFrom = DateTime.UtcNow,
            ApprovedBy = approvedBy,
            IsActive = true,
            OccasionalThreshold = 12500m,
            EnhancedDdThreshold = 15000m,
            HighRiskJurisdictionsJson = "[\"KP\",\"IR\",\"MM\",\"SY\"]",
            ProhibitedJurisdictionsJson = "[\"RU\",\"BY\"]",
            ProhibitedCaeActivitiesJson = "[\"92000\",\"64110\"]",
            BlockShellCompanies = true,
            BlockOffshoreAboveThreshold = true,
            OffshoreBlockThreshold = 25m,
            ReviewDaysLowRisk = 365,
            ReviewDaysMediumRisk = 180,
            ReviewDaysHighRisk = 90,
            ReviewDaysCriticalRisk = 30
        };
    }

    public void Deactivate() => IsActive = false;

    public static CustomerAcceptancePolicy CreateSuccessor(string version, string approvedBy, CustomerAcceptancePolicy source) =>
        new()
        {
            Id = Guid.NewGuid(),
            Version = version,
            EffectiveFrom = DateTime.UtcNow,
            ApprovedBy = approvedBy,
            IsActive = true,
            OccasionalThreshold = source.OccasionalThreshold,
            EnhancedDdThreshold = source.EnhancedDdThreshold,
            HighRiskJurisdictionsJson = source.HighRiskJurisdictionsJson,
            ProhibitedJurisdictionsJson = source.ProhibitedJurisdictionsJson,
            ProhibitedCaeActivitiesJson = source.ProhibitedCaeActivitiesJson,
            BlockShellCompanies = source.BlockShellCompanies,
            BlockOffshoreAboveThreshold = source.BlockOffshoreAboveThreshold,
            OffshoreBlockThreshold = source.OffshoreBlockThreshold,
            ReviewDaysLowRisk = source.ReviewDaysLowRisk,
            ReviewDaysMediumRisk = source.ReviewDaysMediumRisk,
            ReviewDaysHighRisk = source.ReviewDaysHighRisk,
            ReviewDaysCriticalRisk = source.ReviewDaysCriticalRisk
        };
}
