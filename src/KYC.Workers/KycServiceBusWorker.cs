using System.Text.Json;
using Azure.Messaging.ServiceBus;
using KYC.Application;
using KYC.Application.Interfaces;
using KYC.Infrastructure;
using KYC.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KYC.Workers;

public sealed class KycServiceBusWorker(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<KycServiceBusWorker> logger) : BackgroundService
{
    private const string QueueName = "kyc-case-started";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cs = configuration["KYC_SERVICEBUS_CONNECTION"] ?? configuration["ServiceBus:ConnectionString"];
        if (string.IsNullOrWhiteSpace(cs))
        {
            logger.LogWarning("Service Bus não configurado — worker idle.");
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        await using var client = new ServiceBusClient(cs);
        await using var processor = client.CreateProcessor(QueueName, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var json = args.Message.Body.ToString();
                var msg = JsonSerializer.Deserialize<CasePipelineMessage>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (msg is null || msg.CaseId == Guid.Empty)
                {
                    await args.DeadLetterMessageAsync(args.Message, "BadPayload", "Missing caseId", cancellationToken: args.CancellationToken);
                    return;
                }

                await using var scope = scopeFactory.CreateAsyncScope();
                var runner = scope.ServiceProvider.GetRequiredService<IKycCasePipelineRunner>();
                if (string.Equals(msg.Kind, "rescreen", StringComparison.OrdinalIgnoreCase))
                    await runner.RunRescreenAsync(msg.CaseId, msg.ActorId ?? "System", args.CancellationToken);
                else
                    await runner.RunCaseStartedAsync(msg.CaseId, args.CancellationToken);
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar mensagem Service Bus.");
                await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Erro Service Bus");
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);
        logger.LogInformation("Processor {Queue} iniciado.", QueueName);
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        finally
        {
            await processor.StopProcessingAsync(stoppingToken);
        }
    }

    private sealed record CasePipelineMessage(Guid CaseId, string? Nif, string? Kind, string? ActorId);
}
