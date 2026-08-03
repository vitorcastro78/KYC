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
        // Idempotent: table may never have existed on fresh installs without pgvector.
        migrationBuilder.Sql("""DROP TABLE IF EXISTS report_embeddings;""");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Not restored: embeddings live in ContextMemory Global Wiki.
    }
}
