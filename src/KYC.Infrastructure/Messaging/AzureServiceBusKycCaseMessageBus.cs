using System.Text.Json;
using Azure.Messaging.ServiceBus;
using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace KYC.Infrastructure.Messaging;

public class AzureServiceBusKycCaseMessageBus : IKycCaseMessageBus, IAsyncDisposable
{
    private readonly ServiceBusClient? _client;
    private readonly ILogger<AzureServiceBusKycCaseMessageBus> _log;
    private const string CaseStartedQueue = "kyc-case-started";
    private const string EntityScanQueue = "kyc-entity-scan";
    private const string LlmQueue = "kyc-llm-synthesis";

    public AzureServiceBusKycCaseMessageBus(IConfiguration config, ILogger<AzureServiceBusKycCaseMessageBus> log)
    {
        _log = log;
        var cs = config["KYC_SERVICEBUS_CONNECTION"] ?? config["ServiceBus:ConnectionString"];
        _client = string.IsNullOrWhiteSpace(cs) ? null : new ServiceBusClient(cs);
    }

    public async Task PublishCaseStartedAsync(Guid caseId, string nif, CancellationToken ct = default)
    {
        if (_client is null)
        {
            _log.LogWarning("Service Bus not configured; skipping publish kyc-case-started.");
            return;
        }

        await using var sender = _client.CreateSender(CaseStartedQueue);
        var body = JsonSerializer.Serialize(new { caseId, nif });
        await sender.SendMessageAsync(new ServiceBusMessage(body), ct);
    }

    public async Task PublishEntityScanAsync(Guid caseId, Guid partyId, CancellationToken ct = default)
    {
        if (_client is null) return;
        await using var sender = _client.CreateSender(EntityScanQueue);
        var body = JsonSerializer.Serialize(new { caseId, partyId });
        await sender.SendMessageAsync(new ServiceBusMessage(body), ct);
    }

    public async Task PublishLlmSynthesisAsync(Guid caseId, CancellationToken ct = default)
    {
        if (_client is null) return;
        await using var sender = _client.CreateSender(LlmQueue);
        var body = JsonSerializer.Serialize(new { caseId });
        await sender.SendMessageAsync(new ServiceBusMessage(body), ct);
    }

    public ValueTask DisposeAsync() => _client is null ? ValueTask.CompletedTask : _client.DisposeAsync();
}
