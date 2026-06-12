using KYC.Application.Cases;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Application.Services;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using KYC.Infrastructure.Compliance;
using KYC.Infrastructure.Persistence;
using KYC.Web.Integration.Tests.Support;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KYC.Web.Integration.Tests;

/// <summary>
/// Automatiza os 10 cenários de docs/E2E_HOMOLOGACAO.md (handlers + PostgreSQL).
/// UI Blazor não coberta — evidência técnica para dossier/09-e2e/.
/// </summary>
public class HomologationE2eAutomatedTests
{
    private const string AnalystId = "e2e-homologation";

    [PostgresFact(DisplayName = "E2E-01 PAC: CAE 92000 rejeitado")]
    public async Task E2e01_Pac_rejects_prohibited_cae()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);
        var policy = await db.CustomerAcceptancePolicies.FirstOrDefaultAsync(p => p.IsActive)
                     ?? CustomerAcceptancePolicy.CreateV1("e2e");
        if (!await db.CustomerAcceptancePolicies.AnyAsync(p => p.IsActive))
        {
            db.CustomerAcceptancePolicies.Add(policy);
            await db.SaveChangesAsync();
        }

        var res = new Mock<IEntityResolutionService>();
        res.Setup(s => s.ResolveByNifAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EntityResolutionResult("123456789", "Test Co", "PT", "x", true, null));

        var handler = new StartKycCaseCommandHandler(
            repo,
            res.Object,
            new CustomerAcceptancePolicyRepository(db),
            new PolicyComplianceValidator(),
            new Mock<IKycCaseMessageBus>().Object);

        await Assert.ThrowsAsync<PolicyViolationException>(() =>
            handler.Handle(
                new StartKycCaseCommand("123456789", AnalystId, CreditAmount.Eur(1000), CaeCode: "92000"),
                CancellationToken.None));
    }

    [PostgresFact(DisplayName = "E2E-06 Nome legal manual em fallback RCBE/GLEIF")]
    public async Task E2e06_Manual_legal_name_on_fallback_persists()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);
        await EnsureActivePacAsync(db);

        var nif = $"5{Guid.NewGuid():N}"[..9];
        var res = new Mock<IEntityResolutionService>();
        res.Setup(s => s.ResolveByNifAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EntityResolutionResult(nif, $"Entidade {nif}", null, nif, true, null, UsedFallback: true));

        var bus = new Mock<IKycCaseMessageBus>();
        bus.Setup(b => b.PublishCaseStartedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new StartKycCaseCommandHandler(
            repo,
            res.Object,
            new CustomerAcceptancePolicyRepository(db),
            new PolicyComplianceValidator(),
            bus.Object);

        // Rejeição sem nome legal: StartKycCaseCommandHandlerTests.Throws_when_fallback_without_manual_legal_name
        var id = await handler.Handle(
            new StartKycCaseCommand(nif, AnalystId, CreditAmount.Eur(5000), LegalCompanyName: "Empresa Manual E2E Lda"),
            CancellationToken.None);

        await using var verify = PostgresDbContextFactory.Create();
        var loaded = await verify.KycCases.Include(c => c.Parties).FirstAsync(c => c.Id == id);
        Assert.Equal("Empresa Manual E2E Lda", loaded.CompanyName);
        Assert.Equal("Empresa Manual E2E Lda", loaded.Parties.First(p => p.Role == EntityRole.Target).Name);
    }

    [PostgresFact(DisplayName = "E2E-07 SAR urgente falha → Pending → registo manual")]
    public async Task E2e07_Sar_urgent_failure_then_manual_reference()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);
        var kyc = KycCase.Start($"S{Guid.NewGuid():N}"[..9], "SAR E2E Co", AnalystId, CreditAmount.Eur(50000));
        kyc.SetScore(new RiskScore { Overall = 85, Justification = "Alto" });
        await repo.AddAsync(kyc, CancellationToken.None);

        var uif = new Mock<IUifReportingService>();
        uif.Setup(u => u.SubmitSuspiciousActivityReportAsync(It.IsAny<SuspiciousActivityReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UifSubmissionResult(false, null, "UIF indisponível E2E", DateTime.UtcNow));

        var processor = new SarSubmissionProcessor(repo, uif.Object, new Mock<IMediator>().Object);
        var narrative = new string('x', 200);
        await processor.SubmitAsync(kyc.Id, narrative, AnalystId, isUrgent: true, CancellationToken.None);

        await using var mid = PostgresDbContextFactory.Create();
        var pending = await mid.KycCases.Include(c => c.AuditTrail).FirstAsync(c => c.Id == kyc.Id);
        Assert.Equal(SarStatus.Pending, pending.SarStatus);
        Assert.Contains(pending.AuditTrail, a => a.Action == "SarApiFailedPendingManual");

        var manual = new RegisterManualUifReferenceCommandHandler(repo, new Mock<IMediator>().Object);
        await manual.Handle(new RegisterManualUifReferenceCommand(kyc.Id, "UIF-E2E-MANUAL-001", AnalystId), CancellationToken.None);

        await using var final = PostgresDbContextFactory.Create();
        var done = await final.KycCases.Include(c => c.AuditTrail).FirstAsync(c => c.Id == kyc.Id);
        Assert.Equal(SarStatus.Submitted, done.SarStatus);
        Assert.Equal("UIF-E2E-MANUAL-001", done.SarReferenceNumber);
        Assert.Contains(done.AuditTrail, a => a.Action == "SarManualRegistered");
    }

    [PostgresFact(DisplayName = "E2E-08 Sanção + falha BdP → congelamento manual")]
    public async Task E2e08_Sanction_freeze_api_fail_then_manual_ref()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);
        var kyc = KycCase.Start($"F{Guid.NewGuid():N}"[..9], "Freeze E2E", AnalystId, CreditAmount.Eur(50000));
        kyc.MarkInProgress();
        var party = CaseParty.Create(kyc.Id, EntityType.Company, "Freeze E2E", kyc.Nif, EntityRole.Target, 100, 0, null);
        var signal = RiskSignal.Create(kyc.Id, party.Id, SignalType.Sanction, SignalSeverity.Critical, "Match", "OFAC-E2E");
        kyc.AddParty(party);
        kyc.AddRiskSignal(signal);
        await repo.AddAsync(kyc, CancellationToken.None);

        var freeze = new Mock<IAssetFreezeNotificationService>();
        freeze.Setup(f => f.NotifyAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                AnalystId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssetFreezeNotificationResult(false, null, "BdP API down E2E", DateTime.UtcNow));

        await new OverrideSignalCommandHandler(repo, freeze.Object, new Mock<IKycCaseRealtimeNotifier>().Object)
            .Handle(new OverrideSignalCommand(signal.Id, AnalystId, true, "Confirmado E2E"), CancellationToken.None);

        await using var mid = PostgresDbContextFactory.Create();
        var after = await mid.KycCases.Include(c => c.AuditTrail).FirstAsync(c => c.Id == kyc.Id);
        Assert.False(after.AssetFreezeNotified);
        Assert.Equal(KycStatus.UnderReview, after.Status);
        Assert.Contains(after.AuditTrail, a => a.Action == "AssetFreezeNotificationFailed");

        await new RegisterManualAssetFreezeReferenceCommandHandler(repo)
            .Handle(new RegisterManualAssetFreezeReferenceCommand(kyc.Id, "BDP-E2E-FREEZE-99", AnalystId), CancellationToken.None);

        await using var final = PostgresDbContextFactory.Create();
        var done = await final.KycCases.Include(c => c.AuditTrail).FirstAsync(c => c.Id == kyc.Id);
        Assert.True(done.AssetFreezeNotified);
        Assert.Contains(done.AuditTrail, a => a.Action == "AssetFreezeManualRegistered");
    }

    [PostgresFact(DisplayName = "E2E-09 Identidade manual sem API")]
    public async Task E2e09_Manual_identity_verification()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);
        var kyc = KycCase.Start($"I{Guid.NewGuid():N}"[..9], "Id E2E", AnalystId, CreditAmount.Eur(1000));
        var party = CaseParty.Create(kyc.Id, EntityType.Individual, "UBO E2E", null, EntityRole.Ubo, 25, 1, null);
        kyc.AddParty(party);
        await repo.AddAsync(kyc, CancellationToken.None);

        await new RecordManualIdentityVerificationCommandHandler(repo)
            .Handle(new RecordManualIdentityVerificationCommand(
                kyc.Id, party.Id, AnalystId,
                "Verificação presencial equivalente em contingência E2E homologação.",
                "BI-E2E-12345"),
                CancellationToken.None);

        await using var verify = PostgresDbContextFactory.Create();
        var loaded = await verify.CaseParties.FirstAsync(p => p.Id == party.Id);
        Assert.Equal(IdentityVerificationStatus.Verified, loaded.VerificationStatus);
        Assert.Equal(IdentityVerificationMethod.ThirdPartyReliance, loaded.VerificationMethod);
    }

    [PostgresFact(DisplayName = "E2E-10 Sinal manual + confirmar sanção")]
    public async Task E2e10_Manual_signal_and_override_confirm()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);
        var kyc = KycCase.Start($"M{Guid.NewGuid():N}"[..9], "Signal E2E", AnalystId, CreditAmount.Eur(1000));
        await repo.AddAsync(kyc, CancellationToken.None);

        var signalId = await new AddManualRiskSignalCommandHandler(repo)
            .Handle(new AddManualRiskSignalCommand(
                kyc.Id, AnalystId, SignalType.AdverseMedia, SignalSeverity.High,
                "Notícia adversa registada manualmente em homologação E2E.",
                "Imprensa nacional",
                null),
                CancellationToken.None);

        await using var mid = PostgresDbContextFactory.Create();
        var signal = await mid.RiskSignals.FirstAsync(s => s.Id == signalId);
        Assert.StartsWith("Manual:", signal.Source, StringComparison.OrdinalIgnoreCase);
        Assert.False(signal.IsConfirmed);

        var freeze = new Mock<IAssetFreezeNotificationService>();
        freeze.Setup(f => f.NotifyAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                AnalystId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssetFreezeNotificationResult(true, "OK", null, DateTime.UtcNow));

        var sanction = RiskSignal.Create(kyc.Id, null, SignalType.Sanction, SignalSeverity.Critical, "Sanction E2E", "Manual:Test");
        kyc.AddRiskSignal(sanction);
        await repo.UpdateAsync(kyc, CancellationToken.None);

        await new OverrideSignalCommandHandler(repo, freeze.Object, new Mock<IKycCaseRealtimeNotifier>().Object)
            .Handle(new OverrideSignalCommand(sanction.Id, AnalystId, true, "Confirmado E2E"), CancellationToken.None);

        await using var final = PostgresDbContextFactory.Create();
        var confirmed = await final.RiskSignals.FirstAsync(s => s.Id == sanction.Id);
        Assert.True(confirmed.IsConfirmed);
    }

    [PostgresFact(DisplayName = "E2E-UI-PREP casos Playwright cenários 2–4")]
    public async Task E2e_ui_prep_playwright_cases_json()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var repo = new KycCaseRepository(db);

        var identityCase = KycCase.Start($"U{Guid.NewGuid():N}"[..9], "Identity UI E2E", AnalystId, CreditAmount.Eur(10000));
        identityCase.MarkInProgress();
        var uboVerified = CaseParty.Create(identityCase.Id, EntityType.Individual, "UBO Verificado UI", null, EntityRole.Ubo, 30, 1, null);
        var uboPending = CaseParty.Create(identityCase.Id, EntityType.Individual, "UBO Pendente UI", null, EntityRole.Ubo, 20, 1, null);
        identityCase.AddParty(uboVerified);
        identityCase.AddParty(uboPending);
        await repo.AddAsync(identityCase, CancellationToken.None);

        var sarCase = KycCase.Start($"S{Guid.NewGuid():N}"[..9], "SAR UI E2E", AnalystId, CreditAmount.Eur(50000));
        sarCase.SetScore(new RiskScore { Overall = 88, Justification = "Alto risco UI" });
        sarCase.MarkInProgress();
        await repo.AddAsync(sarCase, CancellationToken.None);

        var eddCase = KycCase.Start($"E{Guid.NewGuid():N}"[..9], "EDD UI E2E", AnalystId, CreditAmount.Eur(250000));
        eddCase.MarkInProgress();
        eddCase.SetDueDiligenceLevel(DueDiligenceLevel.Enhanced, "Montante >= limiar EDD (homologação UI)");
        var eddUbo = CaseParty.Create(eddCase.Id, EntityType.Individual, "UBO EDD UI", null, EntityRole.Ubo, 40, 1, null);
        eddCase.AddParty(eddUbo);
        await repo.AddAsync(eddCase, CancellationToken.None);

        var payload = new
        {
            GeneratedAtUtc = DateTime.UtcNow,
            IdentityCaseId = identityCase.Id,
            UboVerifiedPartyId = uboVerified.Id,
            UboPendingPartyId = uboPending.Id,
            SarCaseId = sarCase.Id,
            EddCaseId = eddCase.Id,
            EddUboPartyId = eddUbo.Id
        };

        var repoRoot = FindRepoRoot();
        var path = Path.Combine(repoRoot, "docs", "dossier", "09-e2e", "e2e-ui-cases.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(
            path,
            System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        Assert.True(File.Exists(path));
    }

    [PostgresFact(DisplayName = "E2E-EXPORT gerar ficheiros de evidência no dossier")]
    public async Task E2e_export_dossier_evidence_files()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var cases = await db.KycCases
            .AsNoTracking()
            .Where(c => c.RequestedBy == AnalystId || c.CompanyName.Contains("E2E"))
            .OrderByDescending(c => c.CreatedAt)
            .Take(20)
            .Select(c => new { c.Id, c.Nif, c.CompanyName, c.Status, c.SarStatus, c.AssetFreezeNotified })
            .ToListAsync();

        var audits = await db.AuditEntries
            .AsNoTracking()
            .Where(a => cases.Select(x => x.Id).Contains(a.KycCaseId))
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new
            {
                a.KycCaseId,
                a.Action,
                a.ActorId,
                a.Timestamp,
                a.Details
            })
            .ToListAsync();

        var parties = await db.CaseParties
            .AsNoTracking()
            .Where(p => cases.Select(x => x.Id).Contains(p.KycCaseId))
            .Select(p => new
            {
                p.Id,
                p.KycCaseId,
                p.Name,
                p.VerificationStatus,
                p.VerificationMethod
            })
            .ToListAsync();

        var repoRoot = FindRepoRoot();
        var dossier = Path.Combine(repoRoot, "docs", "dossier");
        Directory.CreateDirectory(Path.Combine(dossier, "09-e2e"));
        Directory.CreateDirectory(Path.Combine(dossier, "08-audit"));

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var payload = new
        {
            GeneratedAtUtc = DateTime.UtcNow,
            AnalystId,
            Cases = cases,
            Parties = parties,
            AuditTrail = audits,
            ScenariosCovered = new[]
            {
                "E2E-01 PAC", "E2E-06 Nome manual", "E2E-07 SAR manual", "E2E-08 Congelamento manual",
                "E2E-09 Identidade manual", "E2E-10 Sinais manuais"
            }
        };

        var jsonPath = Path.Combine(dossier, "09-e2e", $"audit-export-{stamp}.json");
        await File.WriteAllTextAsync(
            jsonPath,
            System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var auditOnlyPath = Path.Combine(dossier, "08-audit", $"audit-trail-e2e-{stamp}.json");
        await File.WriteAllTextAsync(
            auditOnlyPath,
            System.Text.Json.JsonSerializer.Serialize(audits, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        Assert.True(File.Exists(jsonPath));
        Assert.NotEmpty(cases);
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "KYC.sln")) || File.Exists(Path.Combine(dir, "docs", "E2E_HOMOLOGACAO.md")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName ?? "";
        }

        throw new InvalidOperationException("Raiz do repositório não encontrada.");
    }

    private static async Task EnsureActivePacAsync(KycDbContext db)
    {
        if (await db.CustomerAcceptancePolicies.AnyAsync(p => p.IsActive))
            return;
        db.CustomerAcceptancePolicies.Add(CustomerAcceptancePolicy.CreateV1("e2e-seed"));
        await db.SaveChangesAsync();
    }
}
