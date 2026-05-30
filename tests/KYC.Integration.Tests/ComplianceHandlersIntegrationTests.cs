using KYC.Application.Cases;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Application.Services;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using KYC.Infrastructure.Compliance;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace KYC.Integration.Tests;

/// <summary>E10-01 — fluxos compliance end-to-end com handlers reais (repositório em memória/mock).</summary>
public class ComplianceHandlersIntegrationTests
{
    [Fact]
    public async Task Start_case_with_prohibited_cae_throws_policy_violation()
    {
        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetByNifAsync("123456789", It.IsAny<CancellationToken>())).ReturnsAsync((KycCase?)null);

        var res = new Mock<IEntityResolutionService>();
        res.Setup(s => s.ResolveByNifAsync("123456789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EntityResolutionResult("123456789", "Casino", "PT", "x", true, null));

        var policyRepo = new Mock<ICustomerAcceptancePolicyRepository>();
        policyRepo.Setup(p => p.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CustomerAcceptancePolicy.CreateV1("test"));

        var handler = new StartKycCaseCommandHandler(
            repo.Object,
            res.Object,
            policyRepo.Object,
            new PolicyComplianceValidator(),
            new Mock<IKycCaseMessageBus>().Object);

        await Assert.ThrowsAsync<PolicyViolationException>(() =>
            handler.Handle(
                new StartKycCaseCommand("123456789", "u1", CreditAmount.Eur(1000), CaeCode: "92000"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Confirm_sanction_triggers_freeze_and_under_review()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(50000));
        kyc.MarkInProgress();
        var party = CaseParty.Create(kyc.Id, EntityType.Company, "Acme", "123456789", EntityRole.Target, 100, 0, null);
        kyc.AddParty(party);
        var signal = RiskSignal.Create(kyc.Id, party.Id, SignalType.Sanction, SignalSeverity.Critical, "OFAC", "OFAC");
        kyc.AddRiskSignal(signal);

        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetCaseWithSignalAsync(signal.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((kyc, signal));
        repo.Setup(r => r.UpdateAsync(kyc, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var freeze = new Mock<IAssetFreezeNotificationService>();
        freeze.Setup(f => f.NotifyAsync(
                kyc.Id, party.Id, "OFAC", signal.Id.ToString(), "analyst1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssetFreezeNotificationResult(true, "FREEZE-INT-001", null, DateTime.UtcNow));

        var handler = new OverrideSignalCommandHandler(
            repo.Object,
            freeze.Object,
            new Mock<IKycCaseRealtimeNotifier>().Object);

        await handler.Handle(new OverrideSignalCommand(signal.Id, "analyst1", true, "Confirmado"), CancellationToken.None);

        Assert.Equal(KycStatus.UnderReview, kyc.Status);
        Assert.True(kyc.AssetFreezeNotified);
        Assert.Contains(kyc.AuditTrail, a =>
            a.Action == "AssetFreezeNotificationSent" && a.Details == "FREEZE-INT-001");
    }

    [Fact]
    public async Task Submit_sar_rejects_low_risk_without_critical_signal()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(1000));
        kyc.SetScore(new RiskScore { Overall = 20, Justification = "Baixo" });

        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetByIdAsync(kyc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(kyc);

        var handler = new SubmitSarCommandHandler(
            repo.Object,
            new Mock<IUifReportingService>().Object,
            new Mock<MediatR.IMediator>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new SubmitSarCommand(kyc.Id, new string('n', 200), "analyst1", false),
                CancellationToken.None));
    }

    [Fact]
    public async Task Submit_sar_urgent_records_urgent_audit_entry()
    {
        var kyc = KycCase.Start("123456789", "Acme", "u1", CreditAmount.Eur(50000));
        kyc.SetScore(new RiskScore { Overall = 80, Justification = "Alto" });

        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetByIdAsync(kyc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(kyc);
        repo.Setup(r => r.UpdateAsync(kyc, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var uif = new Mock<IUifReportingService>();
        uif.Setup(u => u.SubmitSuspiciousActivityReportAsync(It.IsAny<SuspiciousActivityReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UifSubmissionResult(true, "UIF-URGENT-1", null, DateTime.UtcNow));

        var mediator = new Moq.Mock<MediatR.IMediator>();
        mediator.Setup(m => m.Publish(It.IsAny<MediatR.INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var handler = new SubmitSarCommandHandler(repo.Object, uif.Object, mediator.Object);
        await handler.Handle(
            new SubmitSarCommand(kyc.Id, new string('x', 200), "analyst1", IsUrgent: true),
            CancellationToken.None);

        Assert.Contains(kyc.AuditTrail, a => a.Action == "SarUrgentSubmitted");
        Assert.Equal("UIF-URGENT-1", kyc.SarReferenceNumber);
    }

    [Fact]
    public async Task Periodic_review_scheduler_publishes_rescreen_for_due_cases()
    {
        var due = KycCase.Start("111111111", "Due Co", "sys", CreditAmount.Eur(1000));

        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetCasesDueForReviewAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KycCase> { due });

        var bus = new Mock<IKycCaseMessageBus>();
        bus.Setup(b => b.PublishCaseRescreenAsync(due.Id, "System", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var scheduler = new PeriodicReviewScheduler(
            repo.Object,
            bus.Object,
            NullLogger<PeriodicReviewScheduler>.Instance);

        var count = await scheduler.PublishDueReviewsAsync(DateTime.UtcNow.AddDays(14));

        Assert.Equal(1, count);
        bus.Verify(b => b.PublishCaseRescreenAsync(due.Id, "System", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Rpb_metrics_builder_aggregates_case_counts()
    {
        var a = KycCase.Start("111111111", "A", "u", CreditAmount.Eur(1000));
        a.SetScore(new RiskScore { Overall = 25, Justification = "ok" });
        a.MarkInProgress();

        var b = KycCase.Start("222222222", "B", "u", CreditAmount.Eur(2000));
        b.SetScore(new RiskScore { Overall = 70, Justification = "high" });
        b.RecordSarSubmitted("UIF-1", "analyst");

        var report = AmlComplianceReport.CreateDraft(2025, "test");
        AmlComplianceMetricsBuilder.Apply(report, [a, b], reviewsCompleted: 3, scoring: null);

        Assert.Equal(2, report.TotalCasesProcessed);
        Assert.Equal(1, report.CasesLowRisk);
        Assert.Equal(1, report.CasesHighRisk);
        Assert.Equal(1, report.SarsSubmitted);
        Assert.Contains("ollama-local", report.AiModelsUsed);
    }
}
