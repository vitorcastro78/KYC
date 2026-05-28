using KYC.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Messaging;

/// <summary>Processa casos iniciados enfileirados em memória (desenvolvimento / single-node).</summary>
public sealed class CaseStartedPipelineHostedService(
    InMemoryCaseStartedQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<CaseStartedPipelineHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Processador in-memory de pipeline KYC iniciado.");
        while (!stoppingToken.IsCancellationRequested)
        {
            CaseStartedWork work;
            try
            {
                work = await queue.Reader.ReadAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var runner = scope.ServiceProvider.GetRequiredService<IKycCasePipelineRunner>();
                if (work.Kind == CasePipelineKind.Rescreen)
                    await runner.RunRescreenAsync(work.CaseId, work.ActorId, stoppingToken).ConfigureAwait(false);
                else
                    await runner.RunCaseStartedAsync(work.CaseId, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao executar o pipeline para o caso {CaseId}", work.CaseId);
            }
        }
    }
}
