using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Persistence.Configurations;

public class CasePartyConfiguration : IEntityTypeConfiguration<CaseParty>
{
    public void Configure(EntityTypeBuilder<CaseParty> builder)
    {
        builder.ToTable("case_parties");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Nif).HasMaxLength(32);
        builder.Property(x => x.Nationality).HasMaxLength(128);
        builder.Property(x => x.OwnershipPercentage).HasPrecision(9, 4);
        builder.Property(x => x.OffshoreJurisdiction).HasMaxLength(128);
        builder.Property(x => x.VerificationSessionId).HasMaxLength(256);
        builder.Property(x => x.VerificationUrl).HasMaxLength(2048);
        builder.Property(x => x.DataCollectionBasis).HasMaxLength(64).IsRequired();

        builder.OwnsOne(x => x.PartyScore, owned =>
        {
            owned.Property(s => s!.Overall).HasColumnName("party_score_overall");
            owned.Property(s => s!.SanctionsScore).HasColumnName("party_score_sanctions");
            owned.Property(s => s!.PepScore).HasColumnName("party_score_pep");
            owned.Property(s => s!.AdverseMediaScore).HasColumnName("party_score_adverse_media");
            owned.Property(s => s!.FinancialScore).HasColumnName("party_score_financial");
            owned.Property(s => s!.JudicialScore).HasColumnName("party_score_judicial");
            owned.Property(s => s!.UboStructureScore).HasColumnName("party_score_ubo");
            owned.Property(s => s!.Justification).HasColumnName("party_score_justification").HasMaxLength(8000);
        });
        builder.Navigation(x => x.PartyScore).IsRequired(false);

        builder.HasIndex(x => new { x.KycCaseId, x.Nif });
    }
}
