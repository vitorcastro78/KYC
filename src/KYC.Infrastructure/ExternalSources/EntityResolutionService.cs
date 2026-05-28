using KYC.Application.Interfaces;
using KYC.Application.Models;
using Microsoft.Extensions.Logging;

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

        var gleifHit = await TryFetchGleifAsync(nif, null, ct);

        if (gleifHit is not null)
        {
            return new EntityResolutionResult(
                nif,
                gleifHit.LegalName,
                gleifHit.Jurisdiction ?? gleifHit.LegalAddressCountry,
                gleifHit.Lei,
                true,
                null,
                Gleif: gleifHit);
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

    public Task<GleifCompanySnapshot?> FetchGleifProfileAsync(
        string caseKey,
        string? companyName = null,
        CancellationToken ct = default) =>
        TryFetchGleifAsync(caseKey, companyName, ct);

    public async Task<GleifEnrichment> FetchGleifEnrichmentAsync(
        string caseKey,
        string? companyName = null,
        CancellationToken ct = default)
    {
        var snapshot = await TryFetchGleifAsync(caseKey, companyName, ct).ConfigureAwait(false);
        if (snapshot is null)
            return new GleifEnrichment(null, []);

        var network = await gleif.BuildCorporateNetworkAsync(snapshot.Lei, ct).ConfigureAwait(false);
        return new GleifEnrichment(network.Root ?? snapshot, network.RelatedParties);
    }

    public async Task<UboGraph> BuildUboGraphAsync(string nif, int maxDepth = 5, CancellationToken ct = default)
    {
        var resolved = await ResolveByNifAsync(nif, ct).ConfigureAwait(false);
        var rootId = Guid.NewGuid();
        var nodes = new List<UboNode>
        {
            new(rootId, resolved.LegalName, nif, "Company", 0, 100m, "Self", resolved.Gleif?.LegalAddressCountry)
        };
        var edges = new List<UboEdge>();

        var lei = resolved.Gleif?.Lei ?? (LooksLikeLei(resolved.RegistryId) ? resolved.RegistryId : null);
        if (!string.IsNullOrEmpty(lei))
        {
            var network = await gleif.BuildCorporateNetworkAsync(lei, ct).ConfigureAwait(false);
            if (network.HasLevel2Data && network.RelatedParties.Count > 0)
            {
                var idByLei = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase) { [lei] = rootId };

                foreach (var rel in network.RelatedParties)
                {
                    if (idByLei.ContainsKey(rel.Lei))
                        continue;

                    var nodeId = Guid.NewGuid();
                    idByLei[rel.Lei] = nodeId;
                    nodes.Add(new UboNode(
                        nodeId,
                        rel.LegalName,
                        rel.RegisteredAs ?? rel.Lei,
                        "Company",
                        MapDepth(rel.RelationshipKind),
                        null,
                        rel.RelationshipKind,
                        rel.CountryIso2));
                }

                Guid? directParentId = null;
                var direct = network.RelatedParties.FirstOrDefault(p =>
                    p.RelationshipKind.Equals("DirectParent", StringComparison.OrdinalIgnoreCase));
                if (direct is not null && idByLei.TryGetValue(direct.Lei, out var dpId))
                {
                    directParentId = dpId;
                    edges.Add(new UboEdge(dpId, rootId, 0m));
                }

                var ultimate = network.RelatedParties.FirstOrDefault(p =>
                    p.RelationshipKind.Equals("UltimateParent", StringComparison.OrdinalIgnoreCase));
                if (ultimate is not null && idByLei.TryGetValue(ultimate.Lei, out var upId) && upId != directParentId)
                    edges.Add(new UboEdge(upId, directParentId ?? rootId, 0m));

                foreach (var rel in network.RelatedParties.Where(p =>
                             p.RelationshipKind.Contains("Child", StringComparison.OrdinalIgnoreCase)))
                {
                    if (idByLei.TryGetValue(rel.Lei, out var childId))
                        edges.Add(new UboEdge(rootId, childId, 0m));
                }

                return new UboGraph(nodes, edges);
            }
        }

        if (maxDepth > 0)
        {
            var holderId = Guid.NewGuid();
            nodes.Add(new UboNode(holderId, "Participada / UBO sintético (sem Level 2 GLEIF)", null, "Individual", 1,
                25m));
            edges.Add(new UboEdge(holderId, rootId, 25m));
            log.LogDebug("GLEIF Level 2 indisponível para {Nif}; grafo sintético.", nif);
        }

        return new UboGraph(nodes, edges);
    }

    private static int MapDepth(string relationshipKind) =>
        relationshipKind switch
        {
            "UltimateParent" => 2,
            "DirectParent" => 1,
            "DirectChild" => 1,
            "UltimateChild" => 2,
            _ => 1
        };

    private async Task<GleifCompanySnapshot?> TryFetchGleifAsync(
        string caseKey,
        string? companyName,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(caseKey))
            return null;

        if (LooksLikeLei(caseKey))
            return await gleif.GetByLeiAsync(caseKey, ct).ConfigureAwait(false);

        var hit = await gleif.FindByCommercialIdentifierAsync(caseKey, ct).ConfigureAwait(false);
        if (hit is not null)
            return hit;

        if (!IsMostlyNumericIdentifier(caseKey))
        {
            hit = await gleif.FindByLegalNameAsync(caseKey, "PT", ct).ConfigureAwait(false)
                  ?? await gleif.FindByLegalNameAsync(caseKey, null, ct).ConfigureAwait(false);
            if (hit is not null)
                return hit;
        }

        if (!string.IsNullOrWhiteSpace(companyName) &&
            !string.Equals(companyName.Trim(), caseKey.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return await gleif.FindByLegalNameAsync(companyName, "PT", ct).ConfigureAwait(false)
                   ?? await gleif.FindByLegalNameAsync(companyName, null, ct).ConfigureAwait(false);
        }

        return null;
    }

    private static bool LooksLikeLei(string? id) =>
        !string.IsNullOrWhiteSpace(id) && id.Length == 20 && id.All(char.IsLetterOrDigit);

    private static bool IsMostlyNumericIdentifier(string id)
    {
        var digits = id.Count(char.IsDigit);
        return digits >= 6 && digits >= id.Length / 2;
    }
}
