using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Domain.Tests;

public class RiskScoreTests
{
    [Theory]
    [InlineData(10, RiskLevel.Low)]
    [InlineData(30, RiskLevel.Low)]
    [InlineData(31, RiskLevel.Medium)]
    [InlineData(60, RiskLevel.Medium)]
    [InlineData(61, RiskLevel.High)]
    [InlineData(80, RiskLevel.High)]
    [InlineData(81, RiskLevel.Critical)]
    public void Level_maps_from_overall(int overall, RiskLevel expected)
    {
        var score = new RiskScore { Overall = overall, Justification = "t" };
        Assert.Equal(expected, score.Level);
    }
}
