using KYC.Application.Cases;
using KYC.Application.Interfaces;
using KYC.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace KYC.Infrastructure.Messaging;

public sealed class RabbitMqKycCaseConsumerHostedService(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqKycCaseConsumerHostedService> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cs = configuration["KYC_RABBITMQ_CONNECTION"] ?? configuration["RabbitMq:ConnectionString"];
        if (string.IsNullOrWhiteSpace(cs))
        {
            log.LogInformation("RabbitMQ not configured; consumer idle.");
            return;
        }

        var factory = new ConnectionFactory { Uri = new Uri(cs) };
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        foreach (var queue in new[] { "kyc-case-started", "kyc-case-rescreen" })
        {
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
                cancellationToken: stoppingToken);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var msg = JsonSerializer.Deserialize<CasePipelineMessage>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (msg is null || msg.CaseId == Guid.Empty)
                        return;

                    await using var scope = scopeFactory.CreateAsyncScope();
                    var runner = scope.ServiceProvider.GetRequiredService<IKycCasePipelineRunner>();
                    if (string.Equals(msg.Kind, "rescreen", StringComparison.OrdinalIgnoreCase))
                        await runner.RunRescreenAsync(msg.CaseId, msg.ActorId ?? "System", stoppingToken);
                    else
                        await runner.RunCaseStartedAsync(msg.CaseId, stoppingToken);

                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed processing RabbitMQ message.");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                }
            };
            await channel.BasicConsumeAsync(queue, autoAck: false, consumer, stoppingToken);
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
