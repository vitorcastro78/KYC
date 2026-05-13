using KYC.Domain.Enums;

namespace KYC.Domain.ValueObjects;

public record RiskScore
{
    public int Overall { get; init; }
    public int? SanctionsScore { get; init; }
    public int? PepScore { get; init; }
    public int? AdverseMediaScore { get; init; }
    public int? FinancialScore { get; init; }
    public int? JudicialScore { get; init; }
    public int? UboStructureScore { get; init; }
    public string Justification { get; init; } = string.Empty;

    public RiskLevel Level => Overall switch
    {
        <= 30 => RiskLevel.Low,
        <= 60 => RiskLevel.Medium,
        <= 80 => RiskLevel.High,
        _ => RiskLevel.Critical
    };
}
