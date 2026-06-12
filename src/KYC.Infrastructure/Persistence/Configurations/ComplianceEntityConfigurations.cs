using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Persistence.Configurations;

public class CustomerAcceptancePolicyConfiguration : IEntityTypeConfiguration<CustomerAcceptancePolicy>
{
    public void Configure(EntityTypeBuilder<CustomerAcceptancePolicy> builder)
    {
        builder.ToTable("customer_acceptance_policies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Version).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ApprovedBy).HasMaxLength(256).IsRequired();
        builder.Property(x => x.HighRiskJurisdictionsJson).HasMaxLength(4000);
        builder.Property(x => x.ProhibitedJurisdictionsJson).HasMaxLength(4000);
        builder.Property(x => x.ProhibitedCaeActivitiesJson).HasMaxLength(4000);
    }
}

public class ScoringEngineConfigConfiguration : IEntityTypeConfiguration<ScoringEngineConfig>
{
    public void Configure(EntityTypeBuilder<ScoringEngineConfig> builder)
    {
        builder.ToTable("scoring_engine_configs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Version).HasMaxLength(32).IsRequired();
        builder.Property(x => x.LocalModelName).HasMaxLength(128);
        builder.Property(x => x.CloudModelName).HasMaxLength(128);
        builder.Property(x => x.SystemPromptHash).HasMaxLength(128);
        builder.Property(x => x.WeightsJson).HasMaxLength(4000);
        builder.Property(x => x.ApprovedBy).HasMaxLength(256);
    }
}

public class DpiaRecordConfiguration : IEntityTypeConfiguration<DpiaRecord>
{
    public void Configure(EntityTypeBuilder<DpiaRecord> builder)
    {
        builder.ToTable("dpia_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Version).HasMaxLength(32);
        builder.Property(x => x.ApprovedBy).HasMaxLength(256);
        builder.Property(x => x.DocumentStoragePath).HasMaxLength(1024);
        builder.Property(x => x.ProcessingActivitiesJson).HasMaxLength(4000);
        builder.Property(x => x.MitigationMeasuresJson).HasMaxLength(4000);
    }
}

public class AmlComplianceReportConfiguration : IEntityTypeConfiguration<AmlComplianceReport>
{
    public void Configure(EntityTypeBuilder<AmlComplianceReport> builder)
    {
        builder.ToTable("aml_compliance_reports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GeneratedBy).HasMaxLength(256);
        builder.Property(x => x.BdpReferenceNumber).HasMaxLength(128);
        builder.Property(x => x.PlatformVersion).HasMaxLength(32);
        builder.Property(x => x.AiModelsUsed).HasMaxLength(4000);
    }
}
