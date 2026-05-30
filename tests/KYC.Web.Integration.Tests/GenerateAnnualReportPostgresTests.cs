using KYC.Domain.Entities;
using KYC.Domain.ValueObjects;
using KYC.Infrastructure.Compliance;
using KYC.Infrastructure.Persistence;
using KYC.Web.Integration.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace KYC.Web.Integration.Tests;

/// <summary>E7-08 — GenerateAnnualReportAsync contra PostgreSQL.</summary>
public class GenerateAnnualReportPostgresTests
{
    [PostgresFact]
    public async Task GenerateAnnualReportAsync_persists_draft_with_case_metrics()
    {
        await using var db = await PostgresDbContextFactory.CreateAsync();
        var year = DateTime.UtcNow.Year;
        var nif = $"6{Guid.NewGuid():N}"[..9];

        var kyc = KycCase.Start(nif, "PG RPB Co", "pg-rpb", CreditAmount.Eur(1000));
        kyc.SetScore(new RiskScore { Overall = 30, Justification = "low" });
        db.KycCases.Add(kyc);
        await db.SaveChangesAsync();

        var reportRepo = new AmlComplianceReportRepository(db);
        var scoringRepo = new ScoringEngineConfigRepository(db);
        var service = new AmlComplianceReportService(
            db,
            reportRepo,
            scoringRepo,
            new BdpRpbExporter(),
            NullLogger<AmlComplianceReportService>.Instance);

        var report = await service.GenerateAnnualReportAsync(year, "pg-rpb-test", CancellationToken.None);

        Assert.True(report.Id != Guid.Empty);
        Assert.Equal(year, report.ReportingYear);
        Assert.True(report.TotalCasesProcessed >= 1);

        await using var verify = PostgresDbContextFactory.Create();
        var stored = await verify.AmlComplianceReports.FirstOrDefaultAsync(r => r.Id == report.Id);
        Assert.NotNull(stored);
    }
}
