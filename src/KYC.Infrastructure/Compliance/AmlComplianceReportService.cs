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
    IScoringEngineConfigRepository scoringRepo,
    IBdpRpbExporter rpbExporter,
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

        var scoring = await scoringRepo.GetActiveAsync(ct);
        var reviewsCompleted = await db.AuditEntries.CountAsync(
            a => a.Action == "PeriodicReviewCompleted" && a.Timestamp >= start && a.Timestamp < end, ct);
        var report = AmlComplianceReport.CreateDraft(year, requestedBy);
        AmlComplianceMetricsBuilder.Apply(report, cases, reviewsCompleted, scoring);

        await reportRepo.AddAsync(report, ct);
        log.LogInformation("RPB {Year} generated with {Count} cases.", year, cases.Count);
        return report;
    }

    public Task<Stream> ExportRpbAsync(Guid reportId, CancellationToken ct = default) =>
        ExportInternalAsync(reportId, internalFormat: true, ct);

    public Task<Stream> ExportRpbBdpAsync(Guid reportId, CancellationToken ct = default) =>
        ExportInternalAsync(reportId, internalFormat: false, ct);

    private async Task<Stream> ExportInternalAsync(Guid reportId, bool internalFormat, CancellationToken ct)
    {
        var report = await reportRepo.GetByIdAsync(reportId, ct)
                     ?? throw new KeyNotFoundException("Relatório não encontrado.");
        byte[] bytes = internalFormat
            ? rpbExporter.ToInternalJson(report)
            : rpbExporter.ToOfficialXml(report);
        if (!internalFormat)
        {
            var validation = rpbExporter.ValidateOfficialXml(bytes);
            if (!validation.IsValid)
                log.LogWarning("RPB XML inválido: {Errors}", string.Join("; ", validation.Errors));
        }

        return new MemoryStream(bytes);
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

    internal static string BuildOllamaOnlyModelsJson(ScoringEngineConfig? scoring) =>
        JsonSerializer.Serialize(new
        {
            provider = "ollama-local",
            local = scoring?.LocalModelName ?? "qwen3.5:9b",
            localVersion = scoring?.LocalModelVersion ?? "latest",
            scoringVersion = scoring?.Version,
            promptHash = scoring?.SystemPromptHash,
            embeddings = "qwen3-embedding:8b"
        });
}
