using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Domain.Tests;

public class KycCaseAutoApproveTests
{
    [Fact]
    public void CanAutoApprove_when_low_score_no_high_signals()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(5000));
        kyc.MarkInProgress();
        kyc.SetScore(new RiskScore { Overall = 25, Justification = "ok" });

        Assert.True(kyc.CanAutoApproveLowRisk());
    }

    [Fact]
    public void Cannot_autoApprove_when_score_above_30()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(5000));
        kyc.MarkInProgress();
        kyc.SetScore(new RiskScore { Overall = 45, Justification = "medium band" });

        Assert.False(kyc.CanAutoApproveLowRisk());
    }

    [Fact]
    public void Cannot_autoApprove_with_high_severity_signal()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(5000));
        kyc.MarkInProgress();
        kyc.SetScore(new RiskScore { Overall = 20, Justification = "ok" });
        kyc.AddRiskSignal(RiskSignal.Create(
            kyc.Id, null, SignalType.AdverseMedia, SignalSeverity.High, "test", "News"));

        Assert.False(kyc.CanAutoApproveLowRisk());
    }
}
