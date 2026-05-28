using KYC.Application.Dtos;
using KYC.Application.Interfaces;
using KYC.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KYC.Infrastructure.Persistence;

public class KycAnalyticsRepository(KycDbContext db) : IKycAnalyticsRepository
{
    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var openStatuses = new[] { KycStatus.Pending, KycStatus.InProgress, KycStatus.UnderReview };
        var approvalActions = new[] { "Approved", "AutoApproved" };

        var open = await db.KycCases.CountAsync(c => openStatuses.Contains(c.Status), ct);
        var approvedToday = await db.AuditEntries
            .Where(a => approvalActions.Contains(a.Action) && a.Timestamp >= today)
            .Select(a => a.KycCaseId)
            .Distinct()
            .CountAsync(ct);
        var underReview = await db.KycCases.CountAsync(c => c.Status == KycStatus.UnderReview, ct);

        var total = await db.KycCases.CountAsync(ct);
        var approved = await db.KycCases.CountAsync(c => c.Status == KycStatus.Approved, ct);
        var rate = total == 0 ? 0 : Math.Round(100.0 * approved / total, 1);

        return new DashboardSummaryDto(total, open, approvedToday, underReview, rate);
    }

    public async Task<IReadOnlyList<CriticalAlertDto>> GetCriticalAlertsLast24hAsync(CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddHours(-24);
        var rows = await db.RiskSignals
            .AsNoTracking()
            .Where(s => s.DetectedAt >= since && s.Severity == SignalSeverity.Critical)
            .Join(db.KycCases.AsNoTracking(), s => s.KycCaseId, c => c.Id, (s, c) => new { s, c })
            .OrderByDescending(x => x.s.DetectedAt)
            .Take(50)
            .Select(x => new CriticalAlertDto(x.c.Id, x.c.CompanyName, x.s.Description, x.s.DetectedAt))
            .ToListAsync(ct);
        return rows;
    }
}
