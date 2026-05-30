using KYC.Application.Cases;
using KYC.Application.Interfaces;
using KYC.Application.Services;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using Moq;

namespace KYC.Application.Tests;

/// <summary>Fluxos de compliance (handlers + domínio) sem BD.</summary>
public class ComplianceFlowTests
{
    [Fact]
    public async Task Override_sanction_confirm_notifies_freeze_and_under_review()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(50000));
        kyc.MarkInProgress();
        var party = CaseParty.Create(kyc.Id, EntityType.Company, "Acme", "123456789",
            EntityRole.Target, 100, 0, null);
        kyc.AddParty(party);
        var signal = RiskSignal.Create(kyc.Id, party.Id, SignalType.Sanction, SignalSeverity.Critical,
            "OFAC match", "OFAC");
        kyc.AddRiskSignal(signal);

        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetCaseWithSignalAsync(signal.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((kyc, signal));
        repo.Setup(r => r.UpdateAsync(kyc, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var freeze = new Mock<IAssetFreezeNotificationService>();
        freeze.Setup(f => f.NotifyAsync(
                kyc.Id, party.Id, signal.Source, signal.Id.ToString(), "analyst1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssetFreezeNotificationResult(true, "BDP-FREEZE-001", null, DateTime.UtcNow));

        var notifier = new Mock<IKycCaseRealtimeNotifier>();

        var handler = new OverrideSignalCommandHandler(repo.Object, freeze.Object, notifier.Object);
        await handler.Handle(new OverrideSignalCommand(signal.Id, "analyst1", true, "Confirmado"), CancellationToken.None);

        Assert.Equal(KycStatus.UnderReview, kyc.Status);
        Assert.True(kyc.AssetFreezeNotified);
        Assert.True(signal.IsConfirmed);
        freeze.Verify(f => f.NotifyAsync(
            kyc.Id, party.Id, "OFAC", signal.Id.ToString(), "analyst1", It.IsAny<CancellationToken>()), Times.Once);
        notifier.Verify(n => n.NotifyComplianceAlertAsync(
            kyc.Id, "AssetFreeze", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Submit_sar_records_reference_on_success()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(50000));
        kyc.MarkInProgress();
        kyc.SetScore(new RiskScore { Overall = 75, Justification = "Alto" });
        kyc.AddRiskSignal(RiskSignal.Create(kyc.Id, null, SignalType.AdverseMedia, SignalSeverity.High,
            "Media", "NewsAPI"));

        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetByIdAsync(kyc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(kyc);
        repo.Setup(r => r.UpdateAsync(kyc, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var uif = new Mock<IUifReportingService>();
        uif.Setup(u => u.SubmitSuspiciousActivityReportAsync(It.IsAny<SuspiciousActivityReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UifSubmissionResult(true, "UIF-2026-001", null, DateTime.UtcNow));

        var handler = new SubmitSarCommandHandler(repo.Object, uif.Object);
        var narrative = new string('x', 200);
        var result = await handler.Handle(
            new SubmitSarCommand(kyc.Id, narrative, "analyst1", false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SarStatus.Submitted, kyc.SarStatus);
        Assert.Equal("UIF-2026-001", kyc.SarReferenceNumber);
    }

    [Fact]
    public async Task Mark_sar_not_required_updates_status()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(1000));
        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetByIdAsync(kyc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(kyc);
        repo.Setup(r => r.UpdateAsync(kyc, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new MarkSarNotRequiredCommandHandler(repo.Object);
        await handler.Handle(
            new MarkSarNotRequiredCommand(kyc.Id, "analyst1", new string('j', 50)), CancellationToken.None);

        Assert.Equal(SarStatus.NotRequired, kyc.SarStatus);
    }

    [Fact]
    public async Task Record_verification_updates_party()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(1000));
        var party = CaseParty.Create(kyc.Id, EntityType.Individual, "UBO", "987654321", EntityRole.Ubo, 50, 1, null);
        party.StartVerification(IdentityVerificationMethod.VideoConference, "sess-abc");
        kyc.AddParty(party);

        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetCaseWithPartyBySessionIdAsync("sess-abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((kyc, party));
        repo.Setup(r => r.UpdateAsync(kyc, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new RecordVerificationResultCommandHandler(repo.Object);
        await handler.Handle(
            new RecordVerificationResultCommand(party.Id, "sess-abc", true, null, "High"), CancellationToken.None);

        Assert.Equal(IdentityVerificationStatus.Verified, party.VerificationStatus);
    }

    [Fact]
    public void Policy_validator_rejects_prohibited_cae_at_start()
    {
        var policy = CustomerAcceptancePolicy.CreateV1("test");
        var party = CaseParty.Create(Guid.NewGuid(), EntityType.Company, "Casino", "123456789",
            EntityRole.Target, 100, 0, null);
        var result = new PolicyComplianceValidator().Validate([party], "92000", policy);
        Assert.True(result.AutoRejected);
    }
}
