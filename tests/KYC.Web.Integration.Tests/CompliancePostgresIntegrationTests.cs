using KYC.Application.Cases;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using KYC.Infrastructure.BackgroundJobs;
using KYC.Infrastructure.Compliance;
using KYC.Infrastructure.Persistence;
using KYC.Web.Integration.Tests.Support;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace KYC.Web.Integration.Tests;

/// <summary>E10-01 — fluxos compliance com PostgreSQL real (skip sem KYC_DB_CONNECTION).</summary>
public class CompliancePostgresIntegrationTests
{
    [PostgresFact]
    public async Task Sar_submission_persists_submitted_status_and_audit()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);

        var kyc = KycCase.Start($"9{Guid.NewGuid():N}"[..9], "PG SAR Co", "pg-test", CreditAmount.Eur(50000));
        kyc.SetScore(new RiskScore { Overall = 85, Justification = "Alto" });
        await repo.AddAsync(kyc, CancellationToken.None);

        var uif = new Mock<IUifReportingService>();
        uif.Setup(u => u.SubmitSuspiciousActivityReportAsync(It.IsAny<SuspiciousActivityReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UifSubmissionResult(true, $"UIF-PG-{Guid.NewGuid():N}"[..12], null, DateTime.UtcNow));

        var processor = new SarSubmissionProcessor(repo, uif.Object, new Mock<IMediator>().Object);
        var narrative = new string('x', 200);
        await processor.SubmitAsync(kyc.Id, narrative, "pg-analyst", isUrgent: true, CancellationToken.None);

        await using var verify = PostgresDbContextFactory.Create();
        var loaded = await verify.KycCases
            .Include(c => c.AuditTrail)
            .FirstAsync(c => c.Id == kyc.Id);

        Assert.Equal(SarStatus.Submitted, loaded.SarStatus);
        Assert.NotNull(loaded.SarReferenceNumber);
        Assert.Contains(loaded.AuditTrail, a => a.Action == "SarSubmitted");
    }

    [PostgresFact]
    public async Task Active_pac_exists_in_database()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var active = await db.CustomerAcceptancePolicies.FirstOrDefaultAsync(p => p.IsActive);
        if (active is null)
            return; // seed opcional em CI

        Assert.False(string.IsNullOrWhiteSpace(active.Version));
    }

    [PostgresFact]
    public async Task Periodic_review_completed_audit_can_be_queried()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);
        var kyc = KycCase.Start($"8{Guid.NewGuid():N}"[..9], "PG Review Co", "pg-test", CreditAmount.Eur(1000));
        await repo.AddAsync(kyc, CancellationToken.None);

        kyc.RecordPeriodicReviewCompleted("pg-scheduler");
        await repo.UpdateAsync(kyc, CancellationToken.None);

        await using var verify = PostgresDbContextFactory.Create();
        var count = await verify.AuditEntries.CountAsync(
            a => a.KycCaseId == kyc.Id && a.Action == "PeriodicReviewCompleted");
        Assert.Equal(1, count);
    }

    [PostgresFact]
    public async Task Asset_freeze_flow_sets_under_review_in_database()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);

        var kyc = KycCase.Start($"7{Guid.NewGuid():N}"[..9], "PG Freeze Co", "pg-test", CreditAmount.Eur(50000));
        kyc.MarkInProgress();
        var party = CaseParty.Create(kyc.Id, EntityType.Company, "PG Freeze Co", kyc.Nif, EntityRole.Target, 100, 0, null);
        kyc.AddParty(party);
        var signal = RiskSignal.Create(kyc.Id, party.Id, SignalType.Sanction, SignalSeverity.Critical, "OFAC", "OFAC-PG");
        kyc.AddRiskSignal(signal);
        await repo.AddAsync(kyc, CancellationToken.None);

        var freeze = new Mock<IAssetFreezeNotificationService>();
        freeze.Setup(f => f.NotifyAsync(
                kyc.Id, party.Id, signal.Source, signal.Id.ToString(), "pg-analyst", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssetFreezeNotificationResult(true, "FREEZE-PG-001", null, DateTime.UtcNow));

        var handler = new OverrideSignalCommandHandler(
            repo,
            freeze.Object,
            new Mock<IKycCaseRealtimeNotifier>().Object);

        await handler.Handle(
            new OverrideSignalCommand(signal.Id, "pg-analyst", true, "Confirmado"),
            CancellationToken.None);

        await using var verify = PostgresDbContextFactory.Create();
        var loaded = await verify.KycCases.FirstAsync(c => c.Id == kyc.Id);
        Assert.Equal(KycStatus.UnderReview, loaded.Status);
        Assert.True(loaded.AssetFreezeNotified);
    }

    [PostgresFact]
    public async Task Delete_active_dpia_record_is_blocked_by_interceptor()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var active = await db.DpiaRecords.FirstOrDefaultAsync(d => d.IsActive);
        if (active is null)
            return;

        db.DpiaRecords.Remove(active);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        Assert.Contains("DPIA activa", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [PostgresFact]
    public async Task Identity_polling_processes_pending_session()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);

        var kyc = KycCase.Start($"5{Guid.NewGuid():N}"[..9], "PG Id Co", "pg-test", CreditAmount.Eur(1000));
        kyc.MarkInProgress();
        var party = CaseParty.Create(kyc.Id, EntityType.Individual, "Maria", null, EntityRole.Ubo, 30, 1, null);
        party.StartVerification(IdentityVerificationMethod.CMD, "pg-session-001", "https://verify.example/pg");
        kyc.AddParty(party);
        await repo.AddAsync(kyc, CancellationToken.None);

        var identity = new Mock<IIdentityVerificationService>();
        identity.Setup(i => i.GetVerificationResultAsync("pg-session-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityVerificationResult(
                "pg-session-001", true, null, DateTime.UtcNow, null, "substantial"));

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<RecordVerificationResultCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var polling = new IdentityVerificationPollingService(
            db, identity.Object, mediator.Object, NullLogger<IdentityVerificationPollingService>.Instance);

        var processed = await polling.PollPendingSessionsAsync(CancellationToken.None);

        Assert.Equal(1, processed);
        mediator.Verify(m => m.Send(
            It.Is<RecordVerificationResultCommand>(c => c.PartyId == party.Id && c.IsVerified),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
