using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BdpComplianceAndGtm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedByAnalystId",
                table: "kyc_cases",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AssetFreezeNotified",
                table: "kyc_cases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssetFreezeNotifiedAt",
                table: "kyc_cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DueDiligenceJustification",
                table: "kyc_cases",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DueDiligenceLevel",
                table: "kyc_cases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FundsOriginDescription",
                table: "kyc_cases",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FundsOriginDocumentId",
                table: "kyc_cases",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FundsOriginVerified",
                table: "kyc_cases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReviewedAt",
                table: "kyc_cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalBasisRef",
                table: "kyc_cases",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewDue",
                table: "kyc_cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelationshipType",
                table: "kyc_cases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SarReferenceNumber",
                table: "kyc_cases",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SarStatus",
                table: "kyc_cases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SarSubmittedAt",
                table: "kyc_cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScoringEngineVersion",
                table: "kyc_cases",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScoringModelSnapshot",
                table: "kyc_cases",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondApproverId",
                table: "kyc_cases",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataCollectionBasis",
                table: "case_parties",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RcbeDiscrepancyDetected",
                table: "case_parties",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RcbeDiscrepancyReported",
                table: "case_parties",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RcbeDiscrepancyReportedAt",
                table: "case_parties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RcbeVerifiedAt",
                table: "case_parties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationMethod",
                table: "case_parties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VerificationSessionId",
                table: "case_parties",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "case_parties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "case_parties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "aml_compliance_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingYear = table.Column<int>(type: "integer", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BdpReferenceNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalAmlAnalysts = table.Column<int>(type: "integer", nullable: false),
                    TotalCasesProcessed = table.Column<int>(type: "integer", nullable: false),
                    TotalCasesApproved = table.Column<int>(type: "integer", nullable: false),
                    TotalCasesRejected = table.Column<int>(type: "integer", nullable: false),
                    TotalCasesUnderReview = table.Column<int>(type: "integer", nullable: false),
                    CasesLowRisk = table.Column<int>(type: "integer", nullable: false),
                    CasesMediumRisk = table.Column<int>(type: "integer", nullable: false),
                    CasesHighRisk = table.Column<int>(type: "integer", nullable: false),
                    CasesCriticalRisk = table.Column<int>(type: "integer", nullable: false),
                    TotalRiskSignalsDetected = table.Column<int>(type: "integer", nullable: false),
                    SanctionMatches = table.Column<int>(type: "integer", nullable: false),
                    PepMatches = table.Column<int>(type: "integer", nullable: false),
                    SarsSubmitted = table.Column<int>(type: "integer", nullable: false),
                    AssetFreezeNotifications = table.Column<int>(type: "integer", nullable: false),
                    CasesSimplifiedDd = table.Column<int>(type: "integer", nullable: false),
                    CasesStandardDd = table.Column<int>(type: "integer", nullable: false),
                    CasesEnhancedDd = table.Column<int>(type: "integer", nullable: false),
                    PeriodicReviewsCompleted = table.Column<int>(type: "integer", nullable: false),
                    PeriodicReviewsOverdue = table.Column<int>(type: "integer", nullable: false),
                    PlatformVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AiModelsUsed = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aml_compliance_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customer_acceptance_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OccasionalThreshold = table.Column<decimal>(type: "numeric", nullable: false),
                    EnhancedDdThreshold = table.Column<decimal>(type: "numeric", nullable: false),
                    HighRiskJurisdictionsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ProhibitedJurisdictionsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ProhibitedCaeActivitiesJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    BlockShellCompanies = table.Column<bool>(type: "boolean", nullable: false),
                    BlockOffshoreAboveThreshold = table.Column<bool>(type: "boolean", nullable: false),
                    OffshoreBlockThreshold = table.Column<decimal>(type: "numeric", nullable: false),
                    ReviewDaysLowRisk = table.Column<int>(type: "integer", nullable: false),
                    ReviewDaysMediumRisk = table.Column<int>(type: "integer", nullable: false),
                    ReviewDaysHighRisk = table.Column<int>(type: "integer", nullable: false),
                    ReviewDaysCriticalRisk = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_acceptance_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dpia_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NextReviewDue = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentStoragePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ProcessingActivitiesJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    MitigationMeasuresJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dpia_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "scoring_engine_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LocalModelName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LocalModelVersion = table.Column<string>(type: "text", nullable: false),
                    CloudModelName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SystemPromptHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WeightsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scoring_engine_configs", x => x.Id);
                });
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION prevent_audit_entry_mutation() RETURNS trigger AS $$
                BEGIN
                    RAISE EXCEPTION 'audit_entries are immutable';
                END;
                $$ LANGUAGE plpgsql;
                DROP TRIGGER IF EXISTS tr_audit_entries_immutable ON audit_entries;
                CREATE TRIGGER tr_audit_entries_immutable
                    BEFORE UPDATE OR DELETE ON audit_entries
                    FOR EACH ROW EXECUTE FUNCTION prevent_audit_entry_mutation();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aml_compliance_reports");

            migrationBuilder.DropTable(
                name: "customer_acceptance_policies");

            migrationBuilder.DropTable(
                name: "dpia_records");

            migrationBuilder.DropTable(
                name: "scoring_engine_configs");

            migrationBuilder.DropColumn(
                name: "ApprovedByAnalystId",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "AssetFreezeNotified",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "AssetFreezeNotifiedAt",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "DueDiligenceJustification",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "DueDiligenceLevel",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "FundsOriginDescription",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "FundsOriginDocumentId",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "FundsOriginVerified",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "LastReviewedAt",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "LegalBasisRef",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "NextReviewDue",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "RelationshipType",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "SarReferenceNumber",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "SarStatus",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "SarSubmittedAt",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "ScoringEngineVersion",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "ScoringModelSnapshot",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "SecondApproverId",
                table: "kyc_cases");

            migrationBuilder.DropColumn(
                name: "DataCollectionBasis",
                table: "case_parties");

            migrationBuilder.DropColumn(
                name: "RcbeDiscrepancyDetected",
                table: "case_parties");

            migrationBuilder.DropColumn(
                name: "RcbeDiscrepancyReported",
                table: "case_parties");

            migrationBuilder.DropColumn(
                name: "RcbeDiscrepancyReportedAt",
                table: "case_parties");

            migrationBuilder.DropColumn(
                name: "RcbeVerifiedAt",
                table: "case_parties");

            migrationBuilder.DropColumn(
                name: "VerificationMethod",
                table: "case_parties");

            migrationBuilder.DropColumn(
                name: "VerificationSessionId",
                table: "case_parties");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "case_parties");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "case_parties");
        }
    }
}
