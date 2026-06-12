using KYC.Application.Interfaces;
using KYC.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources;

public interface ICitiusClient
{
    Task<IReadOnlyList<JudicialCaseRef>> SearchAsync(string nif, string name, CancellationToken ct = default);
}

public sealed class CitiusClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<CitiusClient> log) : ICitiusClient
{
    public async Task<IReadOnlyList<JudicialCaseRef>> SearchAsync(string nif, string name, CancellationToken ct = default)
    {
        var enabled = configuration.GetValue("ExternalSources:Citius:Enabled", false);
        if (!enabled)
        {
            log.LogDebug("CITIUS disabled.");
            return [];
        }

        var baseUrl = configuration["ExternalSources:Citius:BaseUrl"] ?? "https://www.citius.mj.pt/";
        var client = httpClientFactory.CreateClient("citius");
        var query = Uri.EscapeDataString(nif.Length >= 9 ? nif : name);
        try
        {
            using var res = await client.GetAsync($"{baseUrl.TrimEnd('/')}/api/search?q={query}", ct);
            if (!res.IsSuccessStatusCode)
                return [];

            var doc = await res.Content.ReadFromJsonAsync<CitiusSearchResponse>(cancellationToken: ct);
            return doc?.Results?.Select(r => new JudicialCaseRef(r.CaseNumber, r.Court, "Open", r.Date)).ToList()
                   ?? [];
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "CITIUS search failed for {Query}.", query);
            return [];
        }
    }

    private sealed class CitiusSearchResponse
    {
        public List<CitiusResult>? Results { get; set; }
    }

    private sealed class CitiusResult
    {
        public string CaseNumber { get; set; } = "";
        public string Court { get; set; } = "";
        public DateTime? Date { get; set; }
        public string Summary { get; set; } = "";
    }
}

public class JudicialIntelligenceService(
    ICitiusClient citius,
    ILogger<JudicialIntelligenceService> log) : IJudicialIntelligenceService
{
    public async Task<JudicialResult> SearchAsync(string nif, string name, CancellationToken ct = default)
    {
        var cases = await citius.SearchAsync(nif, name, ct);
        log.LogDebug("Judicial intelligence returned {Count} cases.", cases.Count);
        return new JudicialResult(cases);
    }
}
