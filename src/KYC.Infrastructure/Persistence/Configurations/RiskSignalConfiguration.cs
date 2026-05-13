using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Persistence.Configurations;

public class RiskSignalConfiguration : IEntityTypeConfiguration<RiskSignal>
{
    public void Configure(EntityTypeBuilder<RiskSignal> builder)
    {
        builder.ToTable("risk_signals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(256).IsRequired();
        builder.Property(x => x.AnalystNotes).HasMaxLength(4000);

        builder.HasIndex(x => x.KycCaseId);
        builder.HasIndex(x => x.CasePartyId);
    }
}
