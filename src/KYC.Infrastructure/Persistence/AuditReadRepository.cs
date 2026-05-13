using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KYC.Infrastructure.Persistence;

public class AuditReadRepository(KycDbContext db) : IAuditReadRepository
{
    public async Task<IReadOnlyList<AuditEntry>> ListGlobalAsync(int skip, int take, CancellationToken ct = default) =>
        await db.AuditEntries
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(ct);
}
