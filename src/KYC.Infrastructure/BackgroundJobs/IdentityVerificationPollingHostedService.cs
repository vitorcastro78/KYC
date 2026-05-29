using KYC.Application.Cases;
using KYC.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.BackgroundJobs;

/// <summary>Polling de sessões de identidade pendentes (fallback quando webhook não está disponível).</summary>
public sealed class IdentityVerificationPollingHostedService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<IdentityVerificationPollingHostedService> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue("IdentityVerification:EnablePolling", true))
        {
            log.LogInformation("Identity verification polling disabled.");
            return;
        }

        var interval = TimeSpan.FromSeconds(
            Math.Clamp(configuration.GetValue("IdentityVerification:PollingIntervalSeconds", 60), 15, 600));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                log.LogWarning(ex, "Identity verification polling failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task PollOnceAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<Persistence.KycDbContext>();
        var identity = scope.ServiceProvider.GetRequiredService<IIdentityVerificationService>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var pending = await db.CaseParties
            .Where(p => p.VerificationStatus == Domain.Enums.IdentityVerificationStatus.Pending
                        && p.VerificationSessionId != null
                        && !p.VerificationSessionId.StartsWith("local-"))
            .Select(p => new { p.Id, p.VerificationSessionId })
            .Take(20)
            .ToListAsync(ct);

        foreach (var row in pending)
        {
            if (string.IsNullOrWhiteSpace(row.VerificationSessionId))
                continue;

            var result = await identity.GetVerificationResultAsync(row.VerificationSessionId, ct);
            if (result.FailureReason?.Contains("Pending", StringComparison.OrdinalIgnoreCase) == true)
                continue;

            await mediator.Send(new RecordVerificationResultCommand(
                row.Id,
                row.VerificationSessionId,
                result.IsVerified,
                result.FailureReason,
                result.EidasLevel), ct);
        }
    }
}
