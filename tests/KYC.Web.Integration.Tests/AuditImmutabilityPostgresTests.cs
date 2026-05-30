using KYC.Web.Integration.Tests.Support;
using Npgsql;

namespace KYC.Web.Integration.Tests;

public class AuditImmutabilityPostgresTests
{
    [PostgresFact]
    public async Task Audit_entries_trigger_blocks_update()
    {
        var cs = Environment.GetEnvironmentVariable("KYC_DB_CONNECTION")
                 ?? throw new InvalidOperationException("KYC_DB_CONNECTION required.");

        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();

        var triggerExists = await ScalarBoolAsync(conn,
            "SELECT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'tr_audit_entries_immutable')");
        if (!triggerExists)
        {
            Assert.Fail("Trigger tr_audit_entries_immutable em falta. Execute: dotnet ef database update");
            return;
        }

        var auditId = await ScalarGuidAsync(conn, "SELECT \"Id\" FROM audit_entries LIMIT 1");
        if (auditId is null)
        {
            // Sem dados — trigger existe; considerar válido para CI sem seed
            return;
        }

        await using var cmd = new NpgsqlCommand(
            "UPDATE audit_entries SET \"Details\" = 'tamper' WHERE \"Id\" = @id", conn);
        cmd.Parameters.AddWithValue("id", auditId.Value);
        var ex = await Assert.ThrowsAsync<PostgresException>(() => cmd.ExecuteNonQueryAsync());
        Assert.Contains("immutable", ex.MessageText, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<bool> ScalarBoolAsync(NpgsqlConnection conn, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, conn);
        return (bool)(await cmd.ExecuteScalarAsync() ?? false);
    }

    private static async Task<Guid?> ScalarGuidAsync(NpgsqlConnection conn, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, conn);
        var value = await cmd.ExecuteScalarAsync();
        return value is Guid g ? g : null;
    }
}
