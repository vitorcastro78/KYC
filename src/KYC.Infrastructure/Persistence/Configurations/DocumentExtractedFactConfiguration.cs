using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Persistence.Configurations;

public class DocumentExtractedFactConfiguration : IEntityTypeConfiguration<DocumentExtractedFact>
{
    public void Configure(EntityTypeBuilder<DocumentExtractedFact> builder)
    {
        builder.ToTable("document_extracted_facts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FactValue).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Confidence).HasPrecision(5, 4);
        builder.HasIndex(x => new { x.KycCaseId, x.FactKey });
        builder.HasIndex(x => x.CaseDocumentId);
    }
}
