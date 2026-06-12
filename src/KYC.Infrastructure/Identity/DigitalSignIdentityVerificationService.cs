using System.Net;
using System.Text.Json;
using KYC.Application.Interfaces;
using KYC.Domain.Enums;
using KYC.Infrastructure.Compliance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Identity;

/// <summary>Integração com prestador de identidade remota (DigitalSign/CMD/eIDAS).</summary>
public sealed class DigitalSignIdentityVerificationService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<DigitalSignIdentityVerificationService> log) : IIdentityVerificationService
{
    public async Task<IdentityVerificationSession> InitiateVerificationAsync(
        Guid partyId,
        IdentityVerificationMethod method,
        string entityName,
        string? email,
        CancellationToken ct = default)
    {
        var baseUrl = configuration["IdentityVerification:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            if (ComplianceIntegrationOptions.RequireLiveIntegrations(configuration, environment))
                throw new InvalidOperationException(
                    "Identidade remota não configurada (IdentityVerification:BaseUrl).");

            log.LogWarning("IdentityVerification:BaseUrl missing; local session (development only).");
            var devSessionId = $"local-{partyId:N}";
            return new IdentityVerificationSession(
                devSessionId,
                $"/cases/verify/{devSessionId}",
                method,
                DateTime.UtcNow.AddHours(24));
        }

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
            if (ComplianceIntegrationOptions.RequireLiveIntegrations(configuration, environment))
                throw new InvalidOperationException("Prestador de identidade indisponível.", ex);

            log.LogWarning(ex, "Identity provider unavailable; using local session stub (development only).");
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
        try
        {
            using var res = await client.GetAsync(
                $"{baseUrl.TrimEnd('/')}/sessions/{Uri.EscapeDataString(sessionId)}",
                ct);
            if (res.StatusCode == HttpStatusCode.NotFound)
                return new IdentityVerificationResult(sessionId, false, "Session not found", DateTime.UtcNow, null, null);

            res.EnsureSuccessStatusCode();
            var doc = await res.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
            return MapVerificationResult(sessionId, doc);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Identity provider poll failed for session {SessionId}", sessionId);
            return new IdentityVerificationResult(sessionId, false, "Provider unreachable", DateTime.UtcNow, null, null);
        }
    }

    private static IdentityVerificationResult MapVerificationResult(string sessionId, JsonElement doc)
    {
        var status = doc.TryGetProperty("status", out var st) ? st.GetString() : null;
        var verified = doc.TryGetProperty("verified", out var v) && v.ValueKind == JsonValueKind.True && v.GetBoolean();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.Equals("verified", StringComparison.OrdinalIgnoreCase)
                || status.Equals("completed", StringComparison.OrdinalIgnoreCase))
                verified = true;
            else if (status.Equals("failed", StringComparison.OrdinalIgnoreCase)
                     || status.Equals("rejected", StringComparison.OrdinalIgnoreCase))
                return new IdentityVerificationResult(
                    sessionId,
                    false,
                    doc.TryGetProperty("failureReason", out var frFail) ? frFail.GetString() : status,
                    ReadTimestamp(doc, "completedAt"),
                    ReadString(doc, "livenessScore"),
                    ReadString(doc, "eidasLevel"));
            else if (status.Equals("expired", StringComparison.OrdinalIgnoreCase))
                return new IdentityVerificationResult(sessionId, false, "Session expired", DateTime.UtcNow, null, null);
            else if (!verified)
                return new IdentityVerificationResult(sessionId, false, "Pending", DateTime.UtcNow, null, null);
        }

        return new IdentityVerificationResult(
            sessionId,
            verified,
            doc.TryGetProperty("failureReason", out var fr) ? fr.GetString() : null,
            ReadTimestamp(doc, "completedAt"),
            ReadString(doc, "livenessScore"),
            ReadString(doc, "eidasLevel"));
    }

    private static DateTime ReadTimestamp(JsonElement doc, string property) =>
        doc.TryGetProperty(property, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetDateTime()
            : DateTime.UtcNow;

    private static string? ReadString(JsonElement doc, string property) =>
        doc.TryGetProperty(property, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() : null;

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
