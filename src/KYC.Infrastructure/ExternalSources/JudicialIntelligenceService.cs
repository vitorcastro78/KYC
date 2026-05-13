using KYC.Application.Interfaces;
using KYC.Application.Models;

namespace KYC.Infrastructure.ExternalSources;

public class JudicialIntelligenceService(ILogger<JudicialIntelligenceService> log) : IJudicialIntelligenceService
{
    public Task<JudicialResult> SearchAsync(string nif, string name, CancellationToken ct = default)
    {
        log.LogDebug("Judicial intelligence (CITIUS placeholder).");
        return Task.FromResult(new JudicialResult(Array.Empty<JudicialCaseRef>()));
    }
}
