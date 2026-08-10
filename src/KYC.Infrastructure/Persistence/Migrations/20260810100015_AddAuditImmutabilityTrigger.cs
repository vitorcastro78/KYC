using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditImmutabilityTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tr_audit_entries_immutable ON audit_entries;
                DROP FUNCTION IF EXISTS prevent_audit_entry_mutation();
                """);
        }
    }
}
