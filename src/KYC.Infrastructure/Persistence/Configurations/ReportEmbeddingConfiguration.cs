using KYC.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KYC.Infrastructure.Persistence.Configurations;

public class ReportEmbeddingConfiguration : IEntityTypeConfiguration<ReportEmbedding>
{
    public void Configure(EntityTypeBuilder<ReportEmbedding> builder)
    {
        builder.ToTable("report_embeddings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ContentChunk).IsRequired();
        builder.Property(x => x.Embedding).HasColumnType("halfvec(2048)");
        builder.HasIndex(x => x.KycCaseId);
    }
}
