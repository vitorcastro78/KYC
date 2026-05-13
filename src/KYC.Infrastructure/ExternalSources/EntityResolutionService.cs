using KYC.Application.Interfaces;
using KYC.Application.Models;

namespace KYC.Infrastructure.ExternalSources;

public class EntityResolutionService(
    IRcbeClient rcbe,
    IGleifClient gleif,
    IWikidataCompanyClient wikidata,
    ILogger<EntityResolutionService> log) : IEntityResolutionService
{
    public async Task<EntityResolutionResult> ResolveByNifAsync(string nif, CancellationToken ct = default)
    {
        var rcbeHit = await rcbe.GetCompanyByNifAsync(nif, ct);
        if (rcbeHit is not null)
            return new EntityResolutionResult(nif, rcbeHit.LegalName, "PT", rcbeHit.RegistryId, true, null);

        var gleifHit = await gleif.FindByCommercialIdentifierAsync(nif, ct);
        if (gleifHit is not null)
        {
            return new EntityResolutionResult(
                nif,
                gleifHit.LegalName,
                gleifHit.CountryIso2,
                gleifHit.Lei,
                true,
                null);
        }

        var wd = await wikidata.FindCompanyByCommercialIdentifierAsync(nif, ct);
        if (wd is not null)
        {
            return new EntityResolutionResult(
                nif,
                wd.Label,
                wd.CountryIso2,
                wd.WikidataId,
                true,
                null);
        }

        log.LogInformation("Using placeholder resolution for commercial id {Id}.", nif);
        return new EntityResolutionResult(
            nif,
            $"Entidade {nif}",
            null,
            nif,
            true,
            null,
            UsedFallback: true);
    }

    public async Task<UboGraph> BuildUboGraphAsync(string nif, int maxDepth = 5, CancellationToken ct = default)
    {
        var resolved = await ResolveByNifAsync(nif, ct);
        var rootId = Guid.NewGuid();
        var nodes = new List<UboNode>
        {
            new(rootId, resolved.LegalName, nif, "Company", 0, 100m)
        };
        var edges = new List<UboEdge>();
        if (maxDepth > 0)
        {
            var holderId = Guid.NewGuid();
            nodes.Add(new UboNode(holderId, "Participada / UBO sintético", null, "Individual", 1, 25m));
            edges.Add(new UboEdge(holderId, rootId, 25m));
        }

        return new UboGraph(nodes, edges);
    }
}
