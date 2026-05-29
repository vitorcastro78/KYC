using System.Text.Json;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KYC.Infrastructure.Persistence;

public class CustomerAcceptancePolicyRepository(KycDbContext db) : ICustomerAcceptancePolicyRepository
{
    public Task<CustomerAcceptancePolicy?> GetActiveAsync(CancellationToken ct = default) =>
        db.CustomerAcceptancePolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsActive, ct);

    public async Task AddAsync(CustomerAcceptancePolicy policy, CancellationToken ct = default)
    {
        var active = await db.CustomerAcceptancePolicies.Where(p => p.IsActive).ToListAsync(ct);
        foreach (var p in active)
            p.Deactivate();
        db.CustomerAcceptancePolicies.Add(policy);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<CustomerAcceptancePolicy>> ListAsync(CancellationToken ct = default) =>
        await db.CustomerAcceptancePolicies.AsNoTracking().OrderByDescending(p => p.EffectiveFrom).ToListAsync(ct);
}

public class ScoringEngineConfigRepository(KycDbContext db) : IScoringEngineConfigRepository
{
    public Task<ScoringEngineConfig?> GetActiveAsync(CancellationToken ct = default) =>
        db.ScoringEngineConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.IsActive, ct);

    public async Task AddAsync(ScoringEngineConfig config, CancellationToken ct = default)
    {
        var active = await db.ScoringEngineConfigs.Where(c => c.IsActive).ToListAsync(ct);
        foreach (var c in active)
            c.Deactivate();
        db.ScoringEngineConfigs.Add(config);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ScoringEngineConfig>> ListAsync(CancellationToken ct = default) =>
        await db.ScoringEngineConfigs.AsNoTracking().OrderByDescending(c => c.ActiveFrom).ToListAsync(ct);
}

public class DpiaRecordRepository(KycDbContext db) : IDpiaRecordRepository
{
    public Task<DpiaRecord?> GetActiveAsync(CancellationToken ct = default) =>
        db.DpiaRecords.AsNoTracking().FirstOrDefaultAsync(d => d.IsActive, ct);

    public async Task AddAsync(DpiaRecord record, CancellationToken ct = default)
    {
        var active = await db.DpiaRecords.Where(d => d.IsActive).ToListAsync(ct);
        foreach (var d in active)
            d.Deactivate();
        db.DpiaRecords.Add(record);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<DpiaRecord>> ListAsync(CancellationToken ct = default) =>
        await db.DpiaRecords.AsNoTracking().OrderByDescending(d => d.ApprovedAt).ToListAsync(ct);
}

public class AmlComplianceReportRepository(KycDbContext db) : IAmlComplianceReportRepository
{
    public Task<AmlComplianceReport?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.AmlComplianceReports.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(AmlComplianceReport report, CancellationToken ct = default)
    {
        db.AmlComplianceReports.Add(report);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AmlComplianceReport report, CancellationToken ct = default)
    {
        db.AmlComplianceReports.Update(report);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AmlComplianceReport>> ListAsync(CancellationToken ct = default) =>
        await db.AmlComplianceReports.AsNoTracking().OrderByDescending(r => r.ReportingYear).ToListAsync(ct);
}
