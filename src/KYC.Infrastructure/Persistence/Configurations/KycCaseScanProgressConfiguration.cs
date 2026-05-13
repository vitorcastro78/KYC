using KYC.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Persistence.Configurations;

public class KycCaseScanProgressConfiguration : IEntityTypeConfiguration<KycCaseScanProgressRow>
{
    public void Configure(EntityTypeBuilder<KycCaseScanProgressRow> builder)
    {
        builder.ToTable("kyc_case_scan_progress");
        builder.HasKey(x => x.KycCaseId);
        builder.Property(x => x.KycCaseId).ValueGeneratedNever();
    }
}
