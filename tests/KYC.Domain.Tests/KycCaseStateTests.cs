using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Domain.Tests;

public class KycCaseStateTests
{
    [Fact]
    public void Approve_from_in_progress_succeeds()
    {
        var kyc = KycCase.Start("123456789", "ACME", "u1", CreditAmount.Eur(1));
        kyc.MarkInProgress();
        kyc.Approve("analyst");
        Assert.Equal(KycStatus.Approved, kyc.Status);
    }

    [Fact]
    public void Approve_from_pending_throws()
    {
        var kyc = KycCase.Start("123456789", "ACME", "u1", CreditAmount.Eur(1));
        Assert.Throws<InvalidOperationException>(() => kyc.Approve("analyst"));
    }
}
