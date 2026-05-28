using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KYC.Infrastructure.Persistence;

public class CaseDocumentRepository(KycDbContext db) : ICaseDocumentRepository
{
    public Task<CaseDocument?> GetByIdAsync(Guid documentId, CancellationToken ct = default) =>
        db.CaseDocuments
            .Include(d => d.ExtractedFacts)
            .Include(d => d.ExtractedParties)
            .FirstOrDefaultAsync(d => d.Id == documentId, ct);

    public async Task UpdateAsync(CaseDocument document, CancellationToken ct = default)
    {
        var entry = db.Entry(document);
        if (entry.State == EntityState.Detached)
            throw new InvalidOperationException("CaseDocument must be tracked before UpdateAsync.");

        await PromoteOrphanModifiedChildrenToAddedAsync(ct);
        await db.SaveChangesAsync(ct);
    }

    private async Task PromoteOrphanModifiedChildrenToAddedAsync(CancellationToken ct)
    {
        await PromoteIfMissingAsync(
            db.ChangeTracker.Entries<DocumentExtractedFact>().Where(e => e.State == EntityState.Modified),
            ids => db.DocumentExtractedFacts.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => x.Id),
            e => e.Id,
            ct);
        await PromoteIfMissingAsync(
            db.ChangeTracker.Entries<DocumentExtractedParty>().Where(e => e.State == EntityState.Modified),
            ids => db.DocumentExtractedParties.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => x.Id),
            e => e.Id,
            ct);
    }

    private static async Task PromoteIfMissingAsync<TEntity>(
        IEnumerable<EntityEntry<TEntity>> modifiedEntries,
        Func<IReadOnlyList<Guid>, IQueryable<Guid>> existingIdsQuery,
        Func<TEntity, Guid> getId,
        CancellationToken ct)
        where TEntity : class
    {
        var list = modifiedEntries.ToList();
        if (list.Count == 0)
            return;

        var ids = list.Select(e => getId(e.Entity)).Distinct().ToList();
        if (ids.Count == 0)
            return;

        var existing = await existingIdsQuery(ids).ToListAsync(ct);
        var set = existing.ToHashSet();
        foreach (var entry in list)
        {
            if (!set.Contains(getId(entry.Entity)))
                entry.State = EntityState.Added;
        }
    }
}
