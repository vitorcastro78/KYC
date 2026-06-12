using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Persistence.Configurations;

public class DocumentExtractedPartyConfiguration : IEntityTypeConfiguration<DocumentExtractedParty>
{
    public void Configure(EntityTypeBuilder<DocumentExtractedParty> builder)
    {
        builder.ToTable("document_extracted_parties");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Nif).HasMaxLength(32);
        builder.Property(x => x.Nationality).HasMaxLength(128);
        builder.Property(x => x.OwnershipPercentage).HasPrecision(9, 4);
        builder.HasIndex(x => x.CaseDocumentId);
        builder.HasIndex(x => x.KycCaseId);
    }
}
