using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYC.Infrastructure.Persistence.Migrations;

[DbContext(typeof(KycDbContext))]
[Migration("20260803140000_RemoveReportEmbeddings")]
public partial class RemoveReportEmbeddings : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "report_embeddings");

        migrationBuilder.Sql("""DROP EXTENSION IF EXISTS vector;""");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""CREATE EXTENSION IF NOT EXISTS vector;""");

        migrationBuilder.CreateTable(
            name: "report_embeddings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ContentChunk = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Embedding = table.Column<string>(type: "halfvec(2048)", nullable: false),
                KycCaseId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_report_embeddings", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_report_embeddings_KycCaseId",
            table: "report_embeddings",
            column: "KycCaseId");
    }
}
