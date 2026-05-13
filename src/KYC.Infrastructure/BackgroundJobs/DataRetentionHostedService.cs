using KYC.Domain.Enums;
using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.BackgroundJobs;

/// <summary>Job diário: regras de retenção/anónimização (GDPR) — lógica mínima, expandir por jurisdição.</summary>
public class DataRetentionHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<DataRetentionHostedService> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
                var cutoffRejected = DateTime.UtcNow.AddYears(-5);
                var stale = await db.KycCases
                    .Where(c => c.Status == KycStatus.Rejected && c.CompletedAt < cutoffRejected)
                    .ToListAsync(stoppingToken);
                foreach (var c in stale)
                    c.AnonymizeForRetention();

                if (stale.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
                log.LogInformation("Data retention pass anonymized {Count} rejected cases (placeholder).", stale.Count);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Data retention job failed.");
            }
        }
    }
}
