using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Domain.Tests;

public class KycCaseCanApproveTests
{
    [Fact]
    public void Blocks_approval_when_ubo_not_verified()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(10000));
        kyc.MarkInProgress();
        var ubo = CaseParty.Create(kyc.Id, EntityType.Individual, "UBO", "987654321",
            EntityRole.Ubo, 50, 1, null);
        kyc.AddParty(ubo);

        var result = kyc.CanApprove();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Blocks_edd_without_funds_origin()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(10000));
        kyc.MarkInProgress();
        kyc.SetDueDiligenceLevel(DueDiligenceLevel.Enhanced, "PEP");

        var result = kyc.CanApprove();
        Assert.False(result.IsSuccess);
    }
}
