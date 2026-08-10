using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    RequestedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DueDiligenceLevel = table.Column<int>(type: "integer", nullable: false),
                    DueDiligenceJustification = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RelationshipType = table.Column<int>(type: "integer", nullable: false),
                    NextReviewDue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SarStatus = table.Column<int>(type: "integer", nullable: false),
                    SarReferenceNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SarSubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssetFreezeNotified = table.Column<bool>(type: "boolean", nullable: false),
                    AssetFreezeNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScoringEngineVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ScoringModelSnapshot = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    LegalBasisRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FundsOriginDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FundsOriginVerified = table.Column<bool>(type: "boolean", nullable: false),
                    FundsOriginDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedByAnalystId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SecondApproverId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_cases", x => x.Id);
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
                    ModelName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ModelVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SystemPromptHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WeightsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scoring_engine_configs", x => x.Id);
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
                name: "case_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KycCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CasePartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageRelativePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DocumentKind = table.Column<int>(type: "integer", nullable: false),
                    IngestionStatus = table.Column<int>(type: "integer", nullable: false),
                    ExtractedText = table.Column<string>(type: "text", nullable: true),
                    RawExtractionJson = table.Column<string>(type: "text", nullable: true),
                    ExtractionModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExtractionPromptHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_case_documents_kyc_cases_KycCaseId",
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
                    party_score_justification = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    VerificationMethod = table.Column<int>(type: "integer", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationSessionId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    VerificationUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    LivenessScore = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    EidasLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    RcbeVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RcbeDiscrepancyDetected = table.Column<bool>(type: "boolean", nullable: false),
                    RcbeDiscrepancyReported = table.Column<bool>(type: "boolean", nullable: false),
                    RcbeDiscrepancyReportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCollectionBasis = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "document_extracted_facts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    KycCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactKey = table.Column<int>(type: "integer", nullable: false),
                    FactValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    SourcePage = table.Column<int>(type: "integer", nullable: true),
                    ExtractedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_extracted_facts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_extracted_facts_case_documents_CaseDocumentId",
                        column: x => x.CaseDocumentId,
                        principalTable: "case_documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_extracted_parties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    KycCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Nif = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    OwnershipPercentage = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExtractedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_extracted_parties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_extracted_parties_case_documents_CaseDocumentId",
                        column: x => x.CaseDocumentId,
                        principalTable: "case_documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_KycCaseId_Timestamp",
                table: "audit_entries",
                columns: new[] { "KycCaseId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_case_documents_KycCaseId",
                table: "case_documents",
                column: "KycCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_case_documents_KycCaseId_IngestionStatus",
                table: "case_documents",
                columns: new[] { "KycCaseId", "IngestionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_case_parties_KycCaseId_Nif",
                table: "case_parties",
                columns: new[] { "KycCaseId", "Nif" });

            migrationBuilder.CreateIndex(
                name: "IX_document_extracted_facts_CaseDocumentId",
                table: "document_extracted_facts",
                column: "CaseDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_extracted_facts_KycCaseId_FactKey",
                table: "document_extracted_facts",
                columns: new[] { "KycCaseId", "FactKey" });

            migrationBuilder.CreateIndex(
                name: "IX_document_extracted_parties_CaseDocumentId",
                table: "document_extracted_parties",
                column: "CaseDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_extracted_parties_KycCaseId",
                table: "document_extracted_parties",
                column: "KycCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_reports_KycCaseId",
                table: "kyc_reports",
                column: "KycCaseId",
                unique: true);

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
                name: "aml_compliance_reports");

            migrationBuilder.DropTable(
                name: "audit_entries");

            migrationBuilder.DropTable(
                name: "case_parties");

            migrationBuilder.DropTable(
                name: "customer_acceptance_policies");

            migrationBuilder.DropTable(
                name: "document_extracted_facts");

            migrationBuilder.DropTable(
                name: "document_extracted_parties");

            migrationBuilder.DropTable(
                name: "dpia_records");

            migrationBuilder.DropTable(
                name: "kyc_case_scan_progress");

            migrationBuilder.DropTable(
                name: "kyc_reports");

            migrationBuilder.DropTable(
                name: "risk_signals");

            migrationBuilder.DropTable(
                name: "scoring_engine_configs");

            migrationBuilder.DropTable(
                name: "case_documents");

            migrationBuilder.DropTable(
                name: "kyc_cases");
        }
    }
}
