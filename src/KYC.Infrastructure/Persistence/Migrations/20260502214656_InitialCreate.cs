using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace KYC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "kyc_case_scan_progress",
                columns: table => new
                {
                    KycCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalScans = table.Column<int>(type: "integer", nullable: false),
                    CompletedScans = table.Column<int>(type: "integer", nullable: false),
                    FailedScans = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_case_scan_progress", x => x.KycCaseId);
                });

            migrationBuilder.CreateTable(
                name: "kyc_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nif = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    score_overall = table.Column<int>(type: "integer", nullable: true),
                    score_sanctions = table.Column<int>(type: "integer", nullable: true),
                    score_pep = table.Column<int>(type: "integer", nullable: true),
                    score_adverse_media = table.Column<int>(type: "integer", nullable: true),
                    score_financial = table.Column<int>(type: "integer", nullable: true),
                    score_judicial = table.Column<int>(type: "integer", nullable: true),
                    score_ubo = table.Column<int>(type: "integer", nullable: true),
                    score_justification = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedAnalystId = table.Column<string>(type: "text", nullable: true),
                    RequestedCreditAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RequestedCreditCurrency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "report_embeddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KycCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    ContentChunk = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_embeddings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KycCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActorId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActorType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Details = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    LlmPromptHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_entries_kyc_cases_KycCaseId",
                        column: x => x.KycCaseId,
                        principalTable: "kyc_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "case_parties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KycCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Nif = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    OwnershipPercentage = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    UboDepthLevel = table.Column<int>(type: "integer", nullable: false),
                    ParentPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPep = table.Column<bool>(type: "boolean", nullable: false),
                    IsSanctioned = table.Column<bool>(type: "boolean", nullable: false),
                    IsOffshore = table.Column<bool>(type: "boolean", nullable: false),
                    OffshoreJurisdiction = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    party_score_overall = table.Column<int>(type: "integer", nullable: true),
                    party_score_sanctions = table.Column<int>(type: "integer", nullable: true),
                    party_score_pep = table.Column<int>(type: "integer", nullable: true),
                    party_score_adverse_media = table.Column<int>(type: "integer", nullable: true),
                    party_score_financial = table.Column<int>(type: "integer", nullable: true),
                    party_score_judicial = table.Column<int>(type: "integer", nullable: true),
                    party_score_ubo = table.Column<int>(type: "integer", nullable: true),
                    party_score_justification = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_parties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_case_parties_kyc_cases_KycCaseId",
                        column: x => x.KycCaseId,
                        principalTable: "kyc_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kyc_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KycCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    NarrativeMarkdown = table.Column<string>(type: "text", nullable: false),
                    ModelUsed = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kyc_reports_kyc_cases_KycCaseId",
                        column: x => x.KycCaseId,
                        principalTable: "kyc_cases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "risk_signals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KycCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CasePartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Source = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    AnalystNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_signals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_signals_kyc_cases_KycCaseId",
                        column: x => x.KycCaseId,
                        principalTable: "kyc_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_KycCaseId_Timestamp",
                table: "audit_entries",
                columns: new[] { "KycCaseId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_case_parties_KycCaseId_Nif",
                table: "case_parties",
                columns: new[] { "KycCaseId", "Nif" });

            migrationBuilder.CreateIndex(
                name: "IX_kyc_reports_KycCaseId",
                table: "kyc_reports",
                column: "KycCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_embeddings_KycCaseId",
                table: "report_embeddings",
                column: "KycCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_signals_CasePartyId",
                table: "risk_signals",
                column: "CasePartyId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_signals_KycCaseId",
                table: "risk_signals",
                column: "KycCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_entries");

            migrationBuilder.DropTable(
                name: "case_parties");

            migrationBuilder.DropTable(
                name: "kyc_case_scan_progress");

            migrationBuilder.DropTable(
                name: "kyc_reports");

            migrationBuilder.DropTable(
                name: "report_embeddings");

            migrationBuilder.DropTable(
                name: "risk_signals");

            migrationBuilder.DropTable(
                name: "kyc_cases");
        }
    }
}
