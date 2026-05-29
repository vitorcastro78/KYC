using KYC.Application.Common;
using KYC.Application.Filtering;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KYC.Infrastructure.Persistence;

public class KycCaseRepository(KycDbContext db) : IKycCaseRepository
{
    public async Task AddAsync(KycCase kycCase, CancellationToken ct = default)
    {
        db.KycCases.Add(kycCase);
        await db.SaveChangesAsync(ct);
    }

    public async Task<KycCase?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.KycCases
            .Include(c => c.Parties)
            .Include(c => c.RiskSignals)
            .Include(c => c.AuditTrail)
            .Include(c => c.FinalReport)
            .Include(c => c.Documents)
                .ThenInclude(d => d.ExtractedFacts)
            .Include(c => c.Documents)
                .ThenInclude(d => d.ExtractedParties)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<KycCase?> GetByNifAsync(string nif, CancellationToken ct = default) =>
        await db.KycCases
            .Include(c => c.Parties)
            .Include(c => c.RiskSignals)
            .Include(c => c.AuditTrail)
            .Include(c => c.FinalReport)
            .AsSplitQuery()
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(c => c.Nif == nif, ct);

    public async Task<PagedResult<KycCase>> ListAsync(KycCaseFilter filter, CancellationToken ct = default)
    {
        var q = db.KycCases.AsNoTracking().AsQueryable();
        if (filter.Status is { } st)
            q = q.Where(c => c.Status == st);
        if (!string.IsNullOrWhiteSpace(filter.SearchNif))
            q = q.Where(c => c.Nif.Contains(filter.SearchNif));
        if (filter.From is { } from)
            q = q.Where(c => c.CreatedAt >= from);
        if (filter.To is { } to)
            q = q.Where(c => c.CreatedAt <= to);

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);
        var items = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new PagedResult<KycCase>(items, total, page, pageSize);
    }

    public async Task UpdateAsync(KycCase kycCase, CancellationToken ct = default)
    {
        // Nunca usar DbSet.Update() no agregado: com filhos Added marca-os como Modified e gera UPDATE com 0 linhas.
        // O caso deve estar sempre rastreado (GetByIdAsync / GetCaseWithSignalAsync na mesma scope / DbContext).
        var entry = db.Entry(kycCase);
        if (entry.State == EntityState.Detached)
        {
            throw new InvalidOperationException(
                "KycCase não está rastreado pelo DbContext. Carregue com GetByIdAsync ou GetCaseWithSignalAsync " +
                "no mesmo âmbito (scoped) antes de UpdateAsync — não construa o agregado só em memória para gravar.");
        }

        await PromoteOrphanModifiedChildrenToAddedAsync(ct);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Se um filho novo ficou <see cref="EntityState.Modified"/> (ex.: após materialização incorrecta com multi-include),
    /// o UPDATE falha com 0 linhas. Promove para <see cref="EntityState.Added"/> quando o Id ainda não existe na BD.
    /// </summary>
    private async Task PromoteOrphanModifiedChildrenToAddedAsync(CancellationToken ct)
    {
        await PromoteIfMissingAsync(
            db.ChangeTracker.Entries<RiskSignal>().Where(e => e.State == EntityState.Modified),
            ids => db.RiskSignals.AsNoTracking().Where(r => ids.Contains(r.Id)).Select(r => r.Id),
            e => e.Id,
            ct);
        await PromoteIfMissingAsync(
            db.ChangeTracker.Entries<AuditEntry>().Where(e => e.State == EntityState.Modified),
            ids => db.AuditEntries.AsNoTracking().Where(a => ids.Contains(a.Id)).Select(a => a.Id),
            e => e.Id,
            ct);
        await PromoteIfMissingAsync(
            db.ChangeTracker.Entries<CaseParty>().Where(e => e.State == EntityState.Modified),
            ids => db.CaseParties.AsNoTracking().Where(p => ids.Contains(p.Id)).Select(p => p.Id),
            e => e.Id,
            ct);
        await PromoteIfMissingAsync(
            db.ChangeTracker.Entries<KycReport>().Where(e => e.State == EntityState.Modified),
            ids => db.KycReports.AsNoTracking().Where(r => ids.Contains(r.Id)).Select(r => r.Id),
            e => e.Id,
            ct);
        await PromoteIfMissingAsync(
            db.ChangeTracker.Entries<ReportEmbedding>().Where(e => e.State == EntityState.Modified),
            ids => db.ReportEmbeddings.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => x.Id),
            e => e.Id,
            ct);
        await PromoteIfMissingAsync(
            db.ChangeTracker.Entries<CaseDocument>().Where(e => e.State == EntityState.Modified),
            ids => db.CaseDocuments.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => x.Id),
            e => e.Id,
            ct);
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

    /// <summary>
    /// Filhos novos por vezes ficam <see cref="EntityState.Modified"/> (substituição 1:1, grafo, etc.) — UPDATE com 0 linhas.
    /// </summary>
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

    public async Task<(KycCase Case, RiskSignal Signal)?> GetCaseWithSignalAsync(Guid signalId, CancellationToken ct = default)
    {
        // Não carregar RiskSignal e depois o caso com Include: evita duas instâncias do mesmo Id no tracker.
        var row = await db.RiskSignals.AsNoTracking()
            .Where(s => s.Id == signalId)
            .Select(s => new { s.KycCaseId })
            .FirstOrDefaultAsync(ct);
        if (row is null)
            return null;

        var kyc = await GetByIdAsync(row.KycCaseId, ct);
        if (kyc is null)
            return null;

        var signal = kyc.RiskSignals.FirstOrDefault(s => s.Id == signalId);
        return signal is null ? null : (kyc, signal);
    }

    public async Task<IReadOnlyList<KycCase>> GetCasesDueForReviewAsync(DateTime dueBy, CancellationToken ct = default) =>
        await db.KycCases
            .Where(c => c.Status == Domain.Enums.KycStatus.Approved
                        && c.NextReviewDue != null
                        && c.NextReviewDue <= dueBy)
            .ToListAsync(ct);

    public async Task<(KycCase Case, CaseParty Party)?> GetCaseWithPartyAsync(Guid partyId, CancellationToken ct = default)
    {
        var row = await db.CaseParties.AsNoTracking()
            .Where(p => p.Id == partyId)
            .Select(p => new { p.KycCaseId })
            .FirstOrDefaultAsync(ct);
        if (row is null)
            return null;

        var kyc = await GetByIdAsync(row.KycCaseId, ct);
        if (kyc is null)
            return null;

        var party = kyc.Parties.FirstOrDefault(p => p.Id == partyId);
        return party is null ? null : (kyc, party);
    }
}
