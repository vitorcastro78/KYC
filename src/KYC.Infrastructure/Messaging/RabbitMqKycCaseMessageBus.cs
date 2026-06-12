using System.Text;
using System.Text.Json;
using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace KYC.Infrastructure.Messaging;

public sealed class RabbitMqKycCaseMessageBus(
    IConfiguration configuration,
    ILogger<RabbitMqKycCaseMessageBus> log) : IKycCaseMessageBus, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private async Task<IChannel> GetChannelAsync(CancellationToken ct)
    {
        if (_channel is not null)
            return _channel;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_channel is not null)
                return _channel;

            var cs = configuration["KYC_RABBITMQ_CONNECTION"] ?? configuration["RabbitMq:ConnectionString"]
                     ?? throw new InvalidOperationException("RabbitMQ connection string required.");
            var factory = new ConnectionFactory { Uri = new Uri(cs) };
            _connection = await factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

            foreach (var queue in new[] { "kyc-case-started", "kyc-case-rescreen", "kyc-llm-synthesis" })
            {
                await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
                    cancellationToken: ct);
            }

            return _channel;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task PublishCaseStartedAsync(Guid caseId, string nif, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(new CasePipelineMessage(caseId, nif, "started", "System"));
        await PublishAsync("kyc-case-started", body, ct);
    }

    public async Task PublishCaseRescreenAsync(Guid caseId, string actorId, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(new CasePipelineMessage(caseId, null, "rescreen", actorId));
        await PublishAsync("kyc-case-rescreen", body, ct);
    }

    public Task PublishEntityScanAsync(Guid caseId, Guid partyId, CancellationToken ct = default) =>
        Task.CompletedTask;

    public async Task PublishLlmSynthesisAsync(Guid caseId, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(new CasePipelineMessage(caseId, null, "llm", "System"));
        await PublishAsync("kyc-llm-synthesis", body, ct);
    }

    private async Task PublishAsync(string queue, byte[] body, CancellationToken ct)
    {
        var channel = await GetChannelAsync(ct);
        var props = new BasicProperties { Persistent = true, ContentType = "application/json" };
        await channel.BasicPublishAsync("", queue, false, props, body, ct);
        log.LogDebug("Published to {Queue}", queue);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.CloseAsync();
        if (_connection is not null)
            await _connection.CloseAsync();
        _initLock.Dispose();
    }
}

public record CasePipelineMessage(Guid CaseId, string? Nif, string Kind, string? ActorId);
