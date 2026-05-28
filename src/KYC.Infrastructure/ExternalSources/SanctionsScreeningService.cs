using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Domain.Entities;

namespace KYC.Infrastructure.ExternalSources;

public class SanctionsScreeningService(
    IOpenSanctionsClient openSanctions,
    IOfacClient ofac,
    IEuSanctionsClient eu) : ISanctionsScreeningService
{
    public async Task<SanctionsResult> ScreenEntityAsync(CaseParty party, CancellationToken ct = default)
    {
        var openHits = await openSanctions.MatchAsync(
            party.Name,
            party.Type,
            party.Nationality,
            party.Nif,
            ct).ConfigureAwait(false);

        var ofacHits = await ofac.SearchAsync(party.Name, ct).ConfigureAwait(false);
        var euHits = await eu.SearchAsync(party.Name, ct).ConfigureAwait(false);

        return BuildResult(openHits, ofacHits, euHits);
    }

    public async Task<SanctionsResult> ScreenByNameAsync(string name, string? nationality = null, CancellationToken ct = default)
    {
        var openHits = await openSanctions.MatchAsync(name, nationality: nationality, ct: ct)
            .ConfigureAwait(false);

        var ofacHits = await ofac.SearchAsync(name, ct).ConfigureAwait(false);
        var euHits = await eu.SearchAsync(name, ct).ConfigureAwait(false);

        return BuildResult(openHits, ofacHits, euHits);
    }

    private static SanctionsResult BuildResult(
        IReadOnlyList<SanctionsListHit> openHits,
        IReadOnlyList<SanctionsListHit> ofacHits,
        IReadOnlyList<SanctionsListHit> euHits)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var matches = new List<SanctionsMatch>();

        foreach (var hit in openHits.Concat(ofacHits).Concat(euHits))
        {
            var key = $"{hit.ListName}|{hit.MatchedName}";
            if (!seen.Add(key))
                continue;

            matches.Add(new SanctionsMatch(hit.ListName, hit.MatchedName, hit.Score, hit.Details));
        }

        return new SanctionsResult(matches);
    }
}
