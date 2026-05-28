using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Persistence.Configurations;

public class CaseDocumentConfiguration : IEntityTypeConfiguration<CaseDocument>
{
    public void Configure(EntityTypeBuilder<CaseDocument> builder)
    {
        builder.ToTable("case_documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(512).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        builder.Property(x => x.StorageRelativePath).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.ExtractionModel).HasMaxLength(128);
        builder.Property(x => x.ExtractionPromptHash).HasMaxLength(64);
        builder.Property(x => x.FailureReason).HasMaxLength(2000);
        builder.Property(x => x.UploadedBy).HasMaxLength(256).IsRequired();

        builder.HasMany(x => x.ExtractedFacts)
            .WithOne()
            .HasForeignKey(f => f.CaseDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ExtractedParties)
            .WithOne()
            .HasForeignKey(p => p.CaseDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.KycCaseId);
        builder.HasIndex(x => new { x.KycCaseId, x.IngestionStatus });
    }
}
