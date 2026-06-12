using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Persistence.Configurations;

public class KycCaseConfiguration : IEntityTypeConfiguration<KycCase>
{
    public void Configure(EntityTypeBuilder<KycCase> builder)
    {
        builder.ToTable("kyc_cases");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nif).HasMaxLength(32).IsRequired();
        builder.Property(x => x.CompanyName).HasMaxLength(512).IsRequired();
        builder.Property(x => x.RequestedBy).HasMaxLength(256).IsRequired();
        builder.Property(x => x.RequestedCreditCurrency).HasMaxLength(8).IsRequired();
        builder.Property(x => x.DueDiligenceJustification).HasMaxLength(4000);
        builder.Property(x => x.SarReferenceNumber).HasMaxLength(128);
        builder.Property(x => x.ScoringEngineVersion).HasMaxLength(32);
        builder.Property(x => x.ScoringModelSnapshot).HasMaxLength(8000);
        builder.Property(x => x.LegalBasisRef).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FundsOriginDescription).HasMaxLength(4000);
        builder.Property(x => x.FundsOriginDocumentId).HasMaxLength(128);
        builder.Property(x => x.ApprovedByAnalystId).HasMaxLength(256);
        builder.Property(x => x.SecondApproverId).HasMaxLength(256);

        builder.OwnsOne(x => x.Score, owned =>
        {
            owned.Property(s => s!.Overall).HasColumnName("score_overall");
            owned.Property(s => s!.SanctionsScore).HasColumnName("score_sanctions");
            owned.Property(s => s!.PepScore).HasColumnName("score_pep");
            owned.Property(s => s!.AdverseMediaScore).HasColumnName("score_adverse_media");
            owned.Property(s => s!.FinancialScore).HasColumnName("score_financial");
            owned.Property(s => s!.JudicialScore).HasColumnName("score_judicial");
            owned.Property(s => s!.UboStructureScore).HasColumnName("score_ubo");
            owned.Property(s => s!.Justification).HasColumnName("score_justification").HasMaxLength(8000);
        });

        builder.Navigation(x => x.Score).IsRequired(false);

        builder.HasMany(x => x.Parties)
            .WithOne()
            .HasForeignKey(p => p.KycCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RiskSignals)
            .WithOne()
            .HasForeignKey(r => r.KycCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AuditTrail)
            .WithOne()
            .HasForeignKey(a => a.KycCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FinalReport)
            .WithOne()
            .HasForeignKey<KycReport>(r => r.KycCaseId)
            .IsRequired(false);

        builder.HasMany(x => x.Documents)
            .WithOne()
            .HasForeignKey(d => d.KycCaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
