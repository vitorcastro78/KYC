using KYC.Application.Interfaces;
using KYC.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;

namespace KYC.Infrastructure.Persistence;

public class KycCaseScanProgressRepository(KycDbContext db) : IKycCaseScanProgressRepository
{
    public async Task<KycCaseScanProgressState?> GetAsync(Guid caseId, CancellationToken ct = default)
    {
        var row = await db.KycCaseScanProgress.AsNoTracking().FirstOrDefaultAsync(x => x.KycCaseId == caseId, ct);
        return row is null
            ? null
            : new KycCaseScanProgressState(row.KycCaseId, row.TotalScans, row.CompletedScans, row.FailedScans);
    }

    public Task UpsertAsync(KycCaseScanProgressState state, CancellationToken ct = default)
    {
        var (table, colCaseId, colTotal, colCompleted, colFailed) = ResolveStoreNames();
        var sql = $"""
            INSERT INTO {table} ({colCaseId}, {colTotal}, {colCompleted}, {colFailed})
            VALUES (@caseId, @total, @completed, @failed)
            ON CONFLICT ({colCaseId}) DO UPDATE SET
              {colTotal} = EXCLUDED.{colTotal},
              {colCompleted} = EXCLUDED.{colCompleted},
              {colFailed} = EXCLUDED.{colFailed}
            """;
        return db.Database.ExecuteSqlRawAsync(
            sql,
            [
                new NpgsqlParameter("caseId", state.KycCaseId),
                new NpgsqlParameter("total", state.TotalScans),
                new NpgsqlParameter("completed", state.CompletedScans),
                new NpgsqlParameter("failed", state.FailedScans)
            ],
            ct);
    }

    public Task IncrementCompletedAsync(Guid caseId, CancellationToken ct = default)
    {
        var (table, colCaseId, _, colCompleted, _) = ResolveStoreNames();
        var sql = $"UPDATE {table} SET {colCompleted} = {colCompleted} + 1 WHERE {colCaseId} = @caseId";
        return db.Database.ExecuteSqlRawAsync(sql, [new NpgsqlParameter("caseId", caseId)], ct);
    }

    public Task IncrementFailedAsync(Guid caseId, CancellationToken ct = default)
    {
        var (table, colCaseId, _, _, colFailed) = ResolveStoreNames();
        var sql = $"UPDATE {table} SET {colFailed} = {colFailed} + 1 WHERE {colCaseId} = @caseId";
        return db.Database.ExecuteSqlRawAsync(sql, [new NpgsqlParameter("caseId", caseId)], ct);
    }

    private (string Table, string ColCaseId, string ColTotal, string ColCompleted, string ColFailed) ResolveStoreNames()
    {
        var entityType = db.Model.FindEntityType(typeof(KycCaseScanProgressRow))
                         ?? throw new InvalidOperationException("KycCaseScanProgressRow não está mapeado.");
        var tableName = entityType.GetTableName()
                        ?? throw new InvalidOperationException("Nome de tabela em falta para KycCaseScanProgressRow.");
        var schema = entityType.GetSchema();
        var store = StoreObjectIdentifier.Table(tableName, schema);
        static string Q(string name) => "\"" + name.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
        var fqTable = string.IsNullOrEmpty(schema) ? Q(tableName) : $"{Q(schema)}.{Q(tableName)}";
        string C(string propertyName) =>
            Q(entityType.FindProperty(propertyName)!.GetColumnName(store)
              ?? throw new InvalidOperationException($"Coluna em falta: {propertyName}"));
        return (
            fqTable,
            C(nameof(KycCaseScanProgressRow.KycCaseId)),
            C(nameof(KycCaseScanProgressRow.TotalScans)),
            C(nameof(KycCaseScanProgressRow.CompletedScans)),
            C(nameof(KycCaseScanProgressRow.FailedScans)));
    }
}
