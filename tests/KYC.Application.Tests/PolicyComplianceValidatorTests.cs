using KYC.Application.Services;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Application.Tests;

public class PolicyComplianceValidatorTests
{
    private readonly PolicyComplianceValidator _validator = new();
    private readonly CustomerAcceptancePolicy _policy = CustomerAcceptancePolicy.CreateV1("test");

    [Fact]
    public void Rejects_prohibited_offshore_jurisdiction()
    {
        var party = CaseParty.Create(Guid.NewGuid(), EntityType.Company, "Off Co", null,
            EntityRole.Shareholder, 100, 1, null);
        party.SetFlags(false, false, true, "RU");

        var result = _validator.Validate([party], null, _policy);
        Assert.True(result.AutoRejected);
    }

    [Fact]
    public void Allows_clean_structure()
    {
        var party = CaseParty.Create(Guid.NewGuid(), EntityType.Individual, "João", "123456789",
            EntityRole.Ubo, 100, 1, null);

        var result = _validator.Validate([party], "62010", _policy);
        Assert.True(result.IsCompliant);
        Assert.False(result.AutoRejected);
    }
}

public class DueDiligenceLevelEvaluatorTests
{
    [Fact]
    public void Occasional_low_amount_is_simplified()
    {
        var eval = new DueDiligenceLevelEvaluator();
        var party = CaseParty.Create(Guid.NewGuid(), EntityType.Company, "Acme", "123456789",
            EntityRole.Target, 100, 0, null);
        var decision = eval.Evaluate(5000m, RelationshipType.Occasional, [party],
            CustomerAcceptancePolicy.CreateV1("t"));
        Assert.Equal(DueDiligenceLevel.Simplified, decision.Level);
    }

    [Fact]
    public void Pep_triggers_enhanced()
    {
        var eval = new DueDiligenceLevelEvaluator();
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(10000));
        var pep = CaseParty.Create(kyc.Id, EntityType.Individual, "PEP", "111111111", EntityRole.Ubo, 100, 1, null);
        pep.SetFlags(isPep: true, isSanctioned: false, isOffshore: false, offshoreJurisdiction: null);
        var decision = eval.Evaluate(10000m, RelationshipType.Ongoing, [pep], CustomerAcceptancePolicy.CreateV1("t"));
        Assert.Equal(DueDiligenceLevel.Enhanced, decision.Level);
        Assert.Contains("PEP", decision.Justification);
    }

    [Fact]
    public void High_amount_triggers_enhanced()
    {
        var eval = new DueDiligenceLevelEvaluator();
        var decision = eval.Evaluate(500_000m, RelationshipType.Ongoing, [], CustomerAcceptancePolicy.CreateV1("t"));
        Assert.Equal(DueDiligenceLevel.Enhanced, decision.Level);
    }
}
