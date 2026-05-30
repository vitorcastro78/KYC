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
        var polling = scope.ServiceProvider.GetRequiredService<IdentityVerificationPollingService>();
        await polling.PollPendingSessionsAsync(ct);
    }
}
