using System.Text;
using System.Text.Json;
using KYC.Application.Interfaces;
using KYC.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Identity;

/// <summary>Integração com prestador de identidade remota (DigitalSign/CMD/eIDAS).</summary>
public sealed class DigitalSignIdentityVerificationService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<DigitalSignIdentityVerificationService> log) : IIdentityVerificationService
{
    public async Task<IdentityVerificationSession> InitiateVerificationAsync(
        Guid partyId,
        IdentityVerificationMethod method,
        string entityName,
        string? email,
        CancellationToken ct = default)
    {
        var baseUrl = configuration["IdentityVerification:BaseUrl"]
                      ?? throw new InvalidOperationException("IdentityVerification:BaseUrl required.");
        var apiKey = configuration["IdentityVerification:ApiKey"] ?? "";

        var client = httpClientFactory.CreateClient("identity-verification");
        var payload = new { partyId, method = method.ToString(), entityName, email };
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/sessions");
        req.Headers.Add("X-Api-Key", apiKey);
        req.Content = JsonContent.Create(payload);

        try
        {
            using var res = await client.SendAsync(req, ct);
            res.EnsureSuccessStatusCode();
            var doc = await res.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
            return new IdentityVerificationSession(
                doc.GetProperty("sessionId").GetString()!,
                doc.GetProperty("verificationUrl").GetString()!,
                method,
                doc.TryGetProperty("expiresAt", out var exp) ? exp.GetDateTime() : DateTime.UtcNow.AddHours(24));
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Identity provider unavailable; using local session stub.");
            var sessionId = $"local-{partyId:N}";
            return new IdentityVerificationSession(
                sessionId,
                $"/cases/verify/{sessionId}",
                method,
                DateTime.UtcNow.AddHours(24));
        }
    }

    public async Task<IdentityVerificationResult> GetVerificationResultAsync(string sessionId, CancellationToken ct = default)
    {
        var baseUrl = configuration["IdentityVerification:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl) || sessionId.StartsWith("local-", StringComparison.Ordinal))
        {
            return new IdentityVerificationResult(sessionId, false, "Pending external verification", DateTime.UtcNow, null, null);
        }

        var client = httpClientFactory.CreateClient("identity-verification");
        using var res = await client.GetAsync($"{baseUrl.TrimEnd('/')}/sessions/{sessionId}", ct);
        res.EnsureSuccessStatusCode();
        var doc = await res.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return new IdentityVerificationResult(
            sessionId,
            doc.GetProperty("verified").GetBoolean(),
            doc.TryGetProperty("failureReason", out var fr) ? fr.GetString() : null,
            DateTime.UtcNow,
            doc.TryGetProperty("livenessScore", out var ls) ? ls.GetString() : null,
            doc.TryGetProperty("eidasLevel", out var el) ? el.GetString() : null);
    }

    public Task RecordPresentialVerificationAsync(
        Guid partyId,
        string analystId,
        string documentReference,
        CancellationToken ct = default)
    {
        log.LogInformation("Presential verification recorded for party {PartyId} by {Analyst}", partyId, analystId);
        return Task.CompletedTask;
    }
}
