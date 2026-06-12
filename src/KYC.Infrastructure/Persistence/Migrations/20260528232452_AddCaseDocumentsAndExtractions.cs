using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseDocumentsAndExtractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_case_documents_KycCaseId",
                table: "case_documents",
                column: "KycCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_case_documents_KycCaseId_IngestionStatus",
                table: "case_documents",
                columns: new[] { "KycCaseId", "IngestionStatus" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_extracted_facts");

            migrationBuilder.DropTable(
                name: "document_extracted_parties");

            migrationBuilder.DropTable(
                name: "case_documents");
        }
    }
}
