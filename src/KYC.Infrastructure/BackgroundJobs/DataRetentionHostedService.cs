using KYC.Domain.Enums;
using KYC.Infrastructure.Compliance;
using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KYC.Infrastructure.BackgroundJobs;

/// <summary>Job diário: retenção RGPD (5–7 anos) — anónimização de rejeitados e marcação de aprovados expirados.</summary>
public class DataRetentionHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<DataRetentionOptions> options,
    ILogger<DataRetentionHostedService> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cfg = options.Value;
        if (!cfg.EnableHostedService)
        {
            log.LogInformation("Data retention hosted service desactivado (DataRetention:EnableHostedService=false).");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var next = now.Date.AddDays(1).AddHours(2);
            var delay = next - now;
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<KycDbContext>();

                var rejectedCutoff = DateTime.UtcNow.AddYears(-cfg.RejectedCaseRetentionYears);
                var approvedCutoff = DateTime.UtcNow.AddYears(-cfg.ApprovedCaseRetentionYears);

                var anonymized = 0;
                if (cfg.AnonymizeRejectedAfterRetention)
                {
                    var staleRejected = await db.KycCases
                        .Where(c => c.Status == KycStatus.Rejected
                                    && c.CompletedAt < rejectedCutoff
                                    && c.CompanyName != "ANON")
                        .ToListAsync(stoppingToken);
                    foreach (var c in staleRejected)
                        c.AnonymizeForRetention();
                    anonymized = staleRejected.Count;
                    if (anonymized > 0)
                        await db.SaveChangesAsync(stoppingToken);
                }

                var marked = 0;
                if (cfg.MarkApprovedCasesPastRetention)
                {
                    var expiredApproved = await db.KycCases
                        .Where(c => c.Status == KycStatus.Approved
                                    && c.CompletedAt < approvedCutoff)
                        .ToListAsync(stoppingToken);
                    foreach (var c in expiredApproved)
                    {
                        c.AppendAudit(Domain.Entities.AuditEntry.Create(
                            c.Id,
                            "RetentionReviewDue",
                            "System",
                            "Agent",
                            $"Caso aprovado anterior a {approvedCutoff:yyyy-MM-dd} — revisar arquivo legal ({cfg.ApprovedCaseRetentionYears}a)."));
                    }

                    marked = expiredApproved.Count;
                    if (marked > 0)
                        await db.SaveChangesAsync(stoppingToken);
                }

                log.LogInformation(
                    "Data retention: {Anonymized} rejeitados anónimizados (>{RejectedYears}a), {Marked} aprovados marcados para revisão (>{ApprovedYears}a).",
                    anonymized,
                    cfg.RejectedCaseRetentionYears,
                    marked,
                    cfg.ApprovedCaseRetentionYears);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Data retention job failed.");
            }
        }
    }
}
