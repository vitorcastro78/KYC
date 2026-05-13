using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Domain.Entities;

namespace KYC.Infrastructure.ExternalSources;

public class SanctionsScreeningService(IOfacClient ofac, IEuSanctionsClient eu) : ISanctionsScreeningService
{
    public async Task<SanctionsResult> ScreenEntityAsync(CaseParty party, CancellationToken ct = default)
    {
        var ofacHits = await ofac.SearchAsync(party.Name, ct);
        var euHits = await eu.SearchAsync(party.Name, ct);
        var matches = ofacHits.Concat(euHits)
            .Select(h => new SanctionsMatch(h.ListName, h.MatchedName, h.Score, h.Details))
            .ToList();
        return new SanctionsResult(matches);
    }

    public async Task<SanctionsResult> ScreenByNameAsync(string name, string? nationality = null, CancellationToken ct = default)
    {
        var ofacHits = await ofac.SearchAsync(name, ct);
        var euHits = await eu.SearchAsync(name, ct);
        var matches = ofacHits.Concat(euHits)
            .Select(h => new SanctionsMatch(h.ListName, h.MatchedName, h.Score, h.Details))
            .ToList();
        return new SanctionsResult(matches);
    }
}
