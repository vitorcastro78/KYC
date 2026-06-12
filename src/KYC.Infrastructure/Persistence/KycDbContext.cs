using KYC.Domain.Entities;
using KYC.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace KYC.Infrastructure.Persistence;

public class KycDbContext(DbContextOptions<KycDbContext> options) : DbContext(options)
{
    public DbSet<KycCase> KycCases => Set<KycCase>();
    public DbSet<CaseParty> CaseParties => Set<CaseParty>();
    public DbSet<RiskSignal> RiskSignals => Set<RiskSignal>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<KycReport> KycReports => Set<KycReport>();
    public DbSet<ReportEmbedding> ReportEmbeddings => Set<ReportEmbedding>();
    public DbSet<KycCaseScanProgressRow> KycCaseScanProgress => Set<KycCaseScanProgressRow>();
    public DbSet<CaseDocument> CaseDocuments => Set<CaseDocument>();
    public DbSet<DocumentExtractedFact> DocumentExtractedFacts => Set<DocumentExtractedFact>();
    public DbSet<DocumentExtractedParty> DocumentExtractedParties => Set<DocumentExtractedParty>();
    public DbSet<CustomerAcceptancePolicy> CustomerAcceptancePolicies => Set<CustomerAcceptancePolicy>();
    public DbSet<ScoringEngineConfig> ScoringEngineConfigs => Set<ScoringEngineConfig>();
    public DbSet<DpiaRecord> DpiaRecords => Set<DpiaRecord>();
    public DbSet<AmlComplianceReport> AmlComplianceReports => Set<AmlComplianceReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KycDbContext).Assembly);
    }
}
