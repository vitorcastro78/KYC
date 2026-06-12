using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCasePartyBiometricFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EidasLevel",
                table: "case_parties",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LivenessScore",
                table: "case_parties",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EidasLevel",
                table: "case_parties");

            migrationBuilder.DropColumn(
                name: "LivenessScore",
                table: "case_parties");
        }
    }
}
