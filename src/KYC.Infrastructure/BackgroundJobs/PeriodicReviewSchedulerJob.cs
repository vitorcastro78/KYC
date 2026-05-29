using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.BackgroundJobs;

public sealed class PeriodicReviewSchedulerJob(
    IServiceScopeFactory scopeFactory,
    ILogger<PeriodicReviewSchedulerJob> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var next = now.Date.AddDays(1).AddHours(7);
            try
            {
                await Task.Delay(next - now, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var cases = scope.ServiceProvider.GetRequiredService<IKycCaseRepository>();
                var bus = scope.ServiceProvider.GetRequiredService<IKycCaseMessageBus>();
                var due = await cases.GetCasesDueForReviewAsync(DateTime.UtcNow.AddDays(14), stoppingToken);

                foreach (var kyc in due)
                {
                    log.LogInformation(
                        "Revisão periódica agendada para caso {CaseId} — vence {DueDate:yyyy-MM-dd}",
                        kyc.Id, kyc.NextReviewDue);
                    await bus.PublishCaseRescreenAsync(kyc.Id, "System", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Periodic review scheduler failed.");
            }
        }
    }
}

public sealed class ComplianceSeedHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<ComplianceSeedHostedService> log) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var policyRepo = scope.ServiceProvider.GetRequiredService<ICustomerAcceptancePolicyRepository>();
        var scoringRepo = scope.ServiceProvider.GetRequiredService<IScoringEngineConfigRepository>();
        var dpiaRepo = scope.ServiceProvider.GetRequiredService<IDpiaRecordRepository>();

        if (await policyRepo.GetActiveAsync(cancellationToken) is null)
        {
            await policyRepo.AddAsync(Domain.Entities.CustomerAcceptancePolicy.CreateV1("System"), cancellationToken);
            log.LogInformation("PAC v1.0.0 seeded.");
        }

        if (await scoringRepo.GetActiveAsync(cancellationToken) is null)
        {
            await scoringRepo.AddAsync(
                Domain.Entities.ScoringEngineConfig.CreateDefault("System", "0000000000000000000000000000000000000000000000000000000000000000"),
                cancellationToken);
        }

        if (await dpiaRepo.GetActiveAsync(cancellationToken) is null)
        {
            await dpiaRepo.AddAsync(
                Domain.Entities.DpiaRecord.Create("1.0", "System", "docs/dpia-v1.pdf"),
                cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
