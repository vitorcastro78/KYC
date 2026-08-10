using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Persistence;

/// <summary>
/// After a migration squash, existing DBs keep the old schema but an outdated
/// <c>__EFMigrationsHistory</c>. Baseline so <see cref="DatabaseFacade.MigrateAsync"/>
/// does not re-run <c>InitialCreate</c>.
/// </summary>
public static class KycMigrationBaseline
{
    public const string InitialCreateId = "20260810020359_InitialCreate";
    public const string AuditTriggerId = "20260810100015_AddAuditImmutabilityTrigger";
    public const string ProductVersion = "9.0.0";

    public static async Task EnsureSquashBaselineAsync(KycDbContext db, ILogger? logger = null, CancellationToken ct = default)
    {
        if (!await db.Database.CanConnectAsync(ct))
            return;

        // Schema already present from pre-squash migrations?
        var hasSchema = await ScalarBoolAsync(db, """
            SELECT EXISTS (
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = 'public' AND table_name = 'aml_compliance_reports')
            """, ct);
        if (!hasSchema)
            return;

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );
            """, ct);

        var hasInitial = await ScalarBoolAsync(db, $"""
            SELECT EXISTS (
              SELECT 1 FROM "__EFMigrationsHistory"
              WHERE "MigrationId" = '{InitialCreateId}')
            """, ct);
        if (hasInitial)
            return;

        logger?.LogWarning(
            "Baselining EF history after migration squash (existing schema, missing {MigrationId})",
            InitialCreateId);

        // Drop obsolete history rows from the pre-squash chain.
        await db.Database.ExecuteSqlRawAsync("""
            DELETE FROM "__EFMigrationsHistory"
            WHERE "MigrationId" <> {0} AND "MigrationId" <> {1};
            """, InitialCreateId, AuditTriggerId);

        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ({0}, {1})
            ON CONFLICT ("MigrationId") DO NOTHING;
            """, InitialCreateId, ProductVersion);

        // Trigger migration still applies via MigrateAsync when missing.
    }

    private static async Task<bool> ScalarBoolAsync(KycDbContext db, string sql, CancellationToken ct)
    {
        await using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = sql;
        if (cmd.Connection!.State != System.Data.ConnectionState.Open)
            await db.Database.OpenConnectionAsync(ct);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is true || result is bool b && b;
    }
}
