using System.Text;
using System.Text.Json;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Compliance;

public sealed class AmlComplianceReportService(
    KycDbContext db,
    IAmlComplianceReportRepository reportRepo,
    ILogger<AmlComplianceReportService> log) : IAmlComplianceReportService
{
    public async Task<AmlComplianceReport> GenerateAnnualReportAsync(
        int year,
        string requestedBy,
        CancellationToken ct = default)
    {
        var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddYears(1);

        var cases = await db.KycCases
            .Include(c => c.RiskSignals)
            .Include(c => c.Parties)
            .Where(c => c.CreatedAt >= start && c.CreatedAt < end)
            .AsNoTracking()
            .ToListAsync(ct);

        var report = AmlComplianceReport.CreateDraft(year, requestedBy);
        report.PopulateMetrics(
            totalCases: cases.Count,
            approved: cases.Count(c => c.Status == KycStatus.Approved),
            rejected: cases.Count(c => c.Status == KycStatus.Rejected),
            underReview: cases.Count(c => c.Status == KycStatus.UnderReview),
            low: cases.Count(c => c.Score?.Level == RiskLevel.Low),
            medium: cases.Count(c => c.Score?.Level == RiskLevel.Medium),
            high: cases.Count(c => c.Score?.Level == RiskLevel.High),
            critical: cases.Count(c => c.Score?.Level == RiskLevel.Critical),
            signals: cases.Sum(c => c.RiskSignals.Count),
            sanctions: cases.Sum(c => c.RiskSignals.Count(s => s.Type == SignalType.Sanction)),
            peps: cases.Count(c => c.Parties.Any(p => p.IsPep)),
            sars: cases.Count(c => c.SarStatus == SarStatus.Submitted),
            freezes: cases.Count(c => c.AssetFreezeNotified),
            simplified: cases.Count(c => c.DueDiligenceLevel == DueDiligenceLevel.Simplified),
            standard: cases.Count(c => c.DueDiligenceLevel == DueDiligenceLevel.Standard),
            enhanced: cases.Count(c => c.DueDiligenceLevel == DueDiligenceLevel.Enhanced),
            reviewsCompleted: await db.AuditEntries.CountAsync(
                a => a.Action == "PeriodicReviewCompleted" && a.Timestamp >= start && a.Timestamp < end, ct),
            reviewsOverdue: cases.Count(c => c.NextReviewDue < DateTime.UtcNow && c.Status == KycStatus.Approved),
            platformVersion: "1.0.0",
            aiModelsJson: JsonSerializer.Serialize(new { local = "qwen3.5:9b", cloud = "claude-sonnet-4-20250514" }));

        await reportRepo.AddAsync(report, ct);
        log.LogInformation("RPB {Year} generated with {Count} cases.", year, cases.Count);
        return report;
    }

    public async Task<Stream> ExportRpbAsync(Guid reportId, CancellationToken ct = default)
    {
        var report = await reportRepo.GetByIdAsync(reportId, ct)
                     ?? throw new KeyNotFoundException("Relatório não encontrado.");
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        return new MemoryStream(Encoding.UTF8.GetBytes(json));
    }

    public async Task<string> SubmitToBdpAsync(Guid reportId, string submittedBy, CancellationToken ct = default)
    {
        var report = await reportRepo.GetByIdAsync(reportId, ct)
                     ?? throw new KeyNotFoundException("Relatório não encontrado.");
        var reference = $"RPB-{report.ReportingYear}-{DateTime.UtcNow:yyyyMMdd}";
        report.MarkSubmitted(reference);
        await reportRepo.UpdateAsync(report, ct);
        return reference;
    }
}
