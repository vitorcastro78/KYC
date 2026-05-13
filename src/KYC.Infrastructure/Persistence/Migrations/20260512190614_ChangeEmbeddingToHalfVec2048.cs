using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEmbeddingToHalfVec2048 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE report_embeddings;");
            migrationBuilder.Sql("ALTER TABLE report_embeddings DROP COLUMN \"Embedding\";");
            migrationBuilder.Sql("ALTER TABLE report_embeddings ADD COLUMN \"Embedding\" halfvec(2048) NOT NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE report_embeddings;");
            migrationBuilder.Sql("ALTER TABLE report_embeddings DROP COLUMN \"Embedding\";");
            migrationBuilder.Sql("ALTER TABLE report_embeddings ADD COLUMN \"Embedding\" vector(1536) NOT NULL;");
        }
    }
}
