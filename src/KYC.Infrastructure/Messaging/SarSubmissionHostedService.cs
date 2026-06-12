using KYC.Application.Cases;
using KYC.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Messaging;

/// <summary>Processa SAR não urgentes enfileirados (comunicação assíncrona à UIF).</summary>
public sealed class SarSubmissionHostedService(
    SarSubmissionQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<SarSubmissionHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Processador de fila SAR iniciado.");
        while (!stoppingToken.IsCancellationRequested)
        {
            SarSubmissionWork work;
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
                var processor = scope.ServiceProvider.GetRequiredService<SarSubmissionProcessor>();
                await processor.ProcessQueuedAsync(work, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao processar SAR em fila para o caso {CaseId}", work.CaseId);
            }
        }
    }
}
