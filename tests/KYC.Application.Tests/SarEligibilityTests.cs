using KYC.Application.Services;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Application.Tests;

/// <summary>E3-11 — elegibilidade SAR e regras de sugestão.</summary>
public class SarEligibilityTests
{
    private readonly SarEligibilityEvaluator _evaluator = new();

    [Fact]
    public void Suggests_sar_when_party_is_sanctioned()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(1000));
        var party = CaseParty.Create(kyc.Id, EntityType.Company, "Acme", "123456789", EntityRole.Target, 100, 0, null);
        party.SetFlags(false, true, false, null);
        kyc.AddParty(party);

        Assert.True(_evaluator.ShouldSuggestSar(kyc));
    }

    [Fact]
    public void Suggests_sar_when_high_score_and_unconfirmed_critical_signal()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(1000));
        kyc.SetScore(new RiskScore { Overall = 75, Justification = "Alto" });
        kyc.AddRiskSignal(RiskSignal.Create(kyc.Id, null, SignalType.Sanction, SignalSeverity.Critical, "x", "OFAC"));

        Assert.True(_evaluator.ShouldSuggestSar(kyc));
    }

    [Fact]
    public void Does_not_suggest_sar_for_low_risk_clean_case()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(1000));
        kyc.SetScore(new RiskScore { Overall = 15, Justification = "Baixo" });

        Assert.False(_evaluator.ShouldSuggestSar(kyc));
    }
}
