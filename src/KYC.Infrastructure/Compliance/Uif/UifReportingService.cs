using System.Text;
using System.Text.Json;
using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Compliance.Uif;

public sealed class UifReportingService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<UifReportingService> log) : IUifReportingService
{
    public async Task<UifSubmissionResult> SubmitSuspiciousActivityReportAsync(
        SuspiciousActivityReport report,
        CancellationToken ct = default)
    {
        var baseUrl = configuration["Uif:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            log.LogWarning("UIF API not configured; generating local reference.");
            var localRef = $"UIF-{DateTime.UtcNow:yyyyMMdd}-{report.KycCaseId:N}"[..32];
            return new UifSubmissionResult(true, localRef, null, DateTime.UtcNow);
        }

        var client = httpClientFactory.CreateClient("uif");
        var apiKey = configuration["Uif:ApiKey"] ?? "";
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/sar");
        req.Headers.Add("Authorization", $"Bearer {apiKey}");
        req.Content = JsonContent.Create(report);

        using var res = await client.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync(ct);
            log.LogError("UIF SAR rejected ({Status}): {Error}", res.StatusCode, err);
            return new UifSubmissionResult(false, null, err, DateTime.UtcNow);
        }

        var doc = await res.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var reference = doc.GetProperty("referenceNumber").GetString()!;
        log.LogInformation("UIF SAR submitted for case {CaseId}: {Reference}", report.KycCaseId, reference);
        return new UifSubmissionResult(true, reference, null, DateTime.UtcNow);
    }

    public async Task<UifSubmissionStatus> GetSubmissionStatusAsync(string referenceNumber, CancellationToken ct = default)
    {
        var baseUrl = configuration["Uif:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            return new UifSubmissionStatus(referenceNumber, "Submitted", DateTime.UtcNow);

        var client = httpClientFactory.CreateClient("uif");
        using var res = await client.GetAsync($"{baseUrl.TrimEnd('/')}/sar/{referenceNumber}", ct);
        res.EnsureSuccessStatusCode();
        var doc = await res.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return new UifSubmissionStatus(
            referenceNumber,
            doc.GetProperty("status").GetString() ?? "Unknown",
            DateTime.UtcNow);
    }
}
