using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Persistence.Configurations;

public class KycReportConfiguration : IEntityTypeConfiguration<KycReport>
{
    public void Configure(EntityTypeBuilder<KycReport> builder)
    {
        builder.ToTable("kyc_reports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NarrativeMarkdown).IsRequired();
        builder.Property(x => x.ModelUsed).HasMaxLength(128);
        builder.HasIndex(x => x.KycCaseId).IsUnique();
    }
}
