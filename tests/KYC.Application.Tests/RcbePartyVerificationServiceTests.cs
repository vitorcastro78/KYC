using KYC.Application.Interfaces;
using KYC.Application.Services;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using Moq;

namespace KYC.Application.Tests;

public class RcbePartyVerificationServiceTests
{
    [Theory]
    [InlineData("ACME LDA", "ACME LDA", false)]
    [InlineData("ACME LIMITADA", "ACME LDA", true)]
    [InlineData("BETA SA", "GAMMA UNIPESSOAL", true)]
    public void Has_discrepancy_detects_name_mismatch(string rcbe, string declared, bool expected) =>
        Assert.Equal(expected, RcbePartyVerificationService.HasDiscrepancy(rcbe, declared));

    [Fact]
    public async Task Verify_adds_signal_when_rcbe_name_differs()
    {
        var kyc = KycCase.Start("500000001", "Nome Errado", "u1", CreditAmount.Eur(1000));
        var party = CaseParty.Create(kyc.Id, EntityType.Company, "Nome Errado", "500000001",
            EntityRole.Target, 100, 0, null);
        kyc.AddParty(party);

        var rcbe = new Mock<IRcbeClient>();
        rcbe.Setup(r => r.GetCompanyByNifAsync("500000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RcbeCompanyDto("500000001", "Nome Oficial RCBE", "RCBE-1"));

        var svc = new RcbePartyVerificationService(rcbe.Object);
        await svc.VerifyCasePartiesAsync(kyc);

        Assert.True(party.RcbeDiscrepancyDetected);
        Assert.Contains(kyc.RiskSignals, s => s.Source == "RCBE");
        Assert.Contains(kyc.AuditTrail, a => a.Action == "RcbeDiscrepancyDetected");
    }
}
