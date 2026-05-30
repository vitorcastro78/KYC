using System.Text.Json;
using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Compliance.AssetFreeze;

public sealed class AssetFreezeNotificationService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<AssetFreezeNotificationService> log) : IAssetFreezeNotificationService
{
    public async Task<AssetFreezeNotificationResult> NotifyAsync(
        Guid kycCaseId,
        Guid partyId,
        string sanctionListSource,
        string matchReference,
        string notifiedBy,
        CancellationToken ct = default)
    {
        var baseUrl = configuration["BdpAssetFreeze:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            log.LogWarning("BdP asset freeze API not configured; local confirmation.");
            var local = $"FREEZE-{DateTime.UtcNow:yyyyMMddHHmmss}-{kycCaseId:N}"[..40];
            return new AssetFreezeNotificationResult(true, local, null, DateTime.UtcNow);
        }

        var client = httpClientFactory.CreateClient("bdp-freeze");
        var payload = new
        {
            kycCaseId,
            partyId,
            sanctionListSource,
            matchReference,
            notifiedBy,
            notifiedAt = DateTime.UtcNow
        };

        using var res = await client.PostAsJsonAsync($"{baseUrl.TrimEnd('/')}/notifications", payload, ct);
        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync(ct);
            return new AssetFreezeNotificationResult(false, null, err, DateTime.UtcNow);
        }

        var doc = await res.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var confirmation = doc.GetProperty("confirmationNumber").GetString()!;
        log.LogInformation(
            "BdP asset freeze notified for case {CaseId}, party {PartyId}: {Confirmation}",
            kycCaseId,
            partyId,
            confirmation);
        return new AssetFreezeNotificationResult(true, confirmation, null, DateTime.UtcNow);
    }
}
