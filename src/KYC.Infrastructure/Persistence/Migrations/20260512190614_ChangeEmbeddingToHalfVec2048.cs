using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEmbeddingToHalfVec2048 : Migration
    {
        /// <summary>No-op: local embeddings removed in favour of ContextMemory wiki.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
