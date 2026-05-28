using System.Text.Json;
using KYC.Application.Models;
using KYC.Infrastructure.ExternalSources.Gleif;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources;

public interface IGleifClient
{
    Task<GleifCompanySnapshot?> FindByCommercialIdentifierAsync(string commercialId, CancellationToken ct = default);
    Task<GleifCompanySnapshot?> FindByLegalNameAsync(string legalName, string? countryIso2 = null,
        CancellationToken ct = default);
    Task<GleifCompanySnapshot?> GetByLeiAsync(string lei, CancellationToken ct = default);
    Task<GleifCompanySnapshot?> GetDirectParentAsync(string lei, CancellationToken ct = default);
    Task<GleifCompanySnapshot?> GetUltimateParentAsync(string lei, CancellationToken ct = default);

    /// <summary>Rede Level 2: self, pais e filhas reportadas (limitada por configuração).</summary>
    Task<GleifCorporateNetwork> BuildCorporateNetworkAsync(string lei, CancellationToken ct = default);

    Task<GleifOwnershipChain> BuildOwnershipChainAsync(string lei, int maxDepth = 3, CancellationToken ct = default);
}

public sealed record GleifOwnershipNode(
    string Lei,
    string Name,
    string? RegisteredAs,
    string? Country,
    int Depth,
    string RelationshipKind);

public sealed record GleifOwnershipChain(
    IReadOnlyList<GleifOwnershipNode> Nodes,
    bool HasReportingData);

public sealed record GleifCorporateNetwork(
    GleifCompanySnapshot? Root,
    IReadOnlyList<GleifRelatedParty> RelatedParties,
    bool HasLevel2Data);

public sealed class GleifClient(
    HttpClient http,
    IConfiguration configuration,
    ILogger<GleifClient> log) : IGleifClient
{
    public async Task<GleifCompanySnapshot?> FindByCommercialIdentifierAsync(string commercialId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(commercialId))
            return null;

        var trimmed = commercialId.Trim();
        if (IsLeiFormat(trimmed.ToUpperInvariant()))
            return await GetByLeiAsync(trimmed, ct).ConfigureAwait(false);

        var query = $"lei-records?page[size]=5&filter[entity.registeredAs]={Uri.EscapeDataString(trimmed)}";
        return await GetFirstFromListQueryAsync(query, ct).ConfigureAwait(false);
    }

    public async Task<GleifCompanySnapshot?> FindByLegalNameAsync(string legalName, string? countryIso2 = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(legalName))
            return null;

        var q = Uri.EscapeDataString(legalName.Trim());
        var query = $"lei-records?page[size]=10&filter[entity.legalName]={q}";
        if (!string.IsNullOrWhiteSpace(countryIso2))
            query +=
                $"&filter[entity.legalAddress.country]={Uri.EscapeDataString(countryIso2.Trim().ToUpperInvariant())}";

        return await GetFirstFromListQueryAsync(query, ct).ConfigureAwait(false);
    }

    public async Task<GleifCompanySnapshot?> GetByLeiAsync(string lei, CancellationToken ct = default)
    {
        if (!IsLeiFormat(lei))
            return null;

        try
        {
            using var doc = await GleifJsonMapper
                .GetJsonAsync(http, $"lei-records/{Uri.EscapeDataString(lei.ToUpperInvariant())}", ct)
                .ConfigureAwait(false);
            if (doc is null)
                return null;

            if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
                return null;

            return GleifJsonMapper.MapLeiRecordDataElement(data);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "GLEIF GET lei-records/{Lei} failed.", lei);
            return null;
        }
    }

    public Task<GleifCompanySnapshot?> GetDirectParentAsync(string lei, CancellationToken ct = default) =>
        GetRelatedEntityRecordAsync(lei, "direct-parent", ct);

    public Task<GleifCompanySnapshot?> GetUltimateParentAsync(string lei, CancellationToken ct = default) =>
        GetRelatedEntityRecordAsync(lei, "ultimate-parent", ct);

    public async Task<GleifCorporateNetwork> BuildCorporateNetworkAsync(string lei, CancellationToken ct = default)
    {
        if (!IsLeiFormat(lei))
            return new GleifCorporateNetwork(null, [], false);

        var root = await GetByLeiAsync(lei, ct).ConfigureAwait(false);
        if (root is null)
            return new GleifCorporateNetwork(null, [], false);

        var maxPerDirection = Math.Clamp(configuration.GetValue("ExternalSources:Gleif:MaxRelatedPerDirection", 15), 1,
            50);
        var related = new List<GleifRelatedParty>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { root.Lei };
        var hasData = false;

        await AddParentAsync(related, seen, () => GetDirectParentAsync(lei, ct), "DirectParent").ConfigureAwait(false);
        await AddParentAsync(related, seen, () => GetUltimateParentAsync(lei, ct), "UltimateParent")
            .ConfigureAwait(false);

        hasData |= await AddChildrenAsync(related, seen, lei, "direct-children", "DirectChild", maxPerDirection, ct)
            .ConfigureAwait(false);
        hasData |= await AddChildrenAsync(related, seen, lei, "ultimate-children", "UltimateChild", maxPerDirection, ct)
            .ConfigureAwait(false);

        if (related.Count > 0)
            hasData = true;

        log.LogInformation(
            "GLEIF rede {Lei}: {Count} entidades relacionadas para triagem (pais/filhas).",
            lei,
            related.Count);

        return new GleifCorporateNetwork(root, related, hasData);
    }

    public async Task<GleifOwnershipChain> BuildOwnershipChainAsync(string lei, int maxDepth = 3,
        CancellationToken ct = default)
    {
        var network = await BuildCorporateNetworkAsync(lei, ct).ConfigureAwait(false);
        if (network.Root is null)
            return new GleifOwnershipChain([], false);

        var nodes = new List<GleifOwnershipNode>
        {
            new(network.Root.Lei, network.Root.LegalName, network.Root.RegisteredAs,
                network.Root.LegalAddressCountry ?? network.Root.Jurisdiction, 0, "Self")
        };

        var depth = 1;
        foreach (var kind in new[] { "DirectParent", "UltimateParent" })
        {
            var hit = network.RelatedParties.FirstOrDefault(p =>
                p.RelationshipKind.Equals(kind, StringComparison.OrdinalIgnoreCase));
            if (hit is not null)
            {
                nodes.Add(new GleifOwnershipNode(hit.Lei, hit.LegalName, hit.RegisteredAs, hit.CountryIso2, depth,
                    kind));
                depth++;
            }
        }

        foreach (var child in network.RelatedParties.Where(p => p.RelationshipKind.Contains("Child", StringComparison.OrdinalIgnoreCase)))
            nodes.Add(new GleifOwnershipNode(child.Lei, child.LegalName, child.RegisteredAs, child.CountryIso2, 1,
                child.RelationshipKind));

        return new GleifOwnershipChain(nodes, network.HasLevel2Data);
    }

    private async Task AddParentAsync(
        List<GleifRelatedParty> related,
        HashSet<string> seen,
        Func<Task<GleifCompanySnapshot?>> fetch,
        string kind)
    {
        var profile = await fetch().ConfigureAwait(false);
        if (profile is null || !seen.Add(profile.Lei))
            return;

        related.Add(ToRelated(profile, kind));
    }

    private async Task<bool> AddChildrenAsync(
        List<GleifRelatedParty> related,
        HashSet<string> seen,
        string lei,
        string segment,
        string kind,
        int maxItems,
        CancellationToken ct)
    {
        var children = await GetRelatedChildrenAsync(lei, segment, maxItems, ct).ConfigureAwait(false);
        foreach (var child in children)
        {
            if (!seen.Add(child.Lei))
                continue;
            related.Add(ToRelated(child, kind));
        }

        return children.Count > 0;
    }

    private async Task<IReadOnlyList<GleifCompanySnapshot>> GetRelatedChildrenAsync(
        string lei,
        string segment,
        int maxItems,
        CancellationToken ct)
    {
        if (!IsLeiFormat(lei))
            return [];

        try
        {
            var path =
                $"lei-records/{Uri.EscapeDataString(lei.ToUpperInvariant())}/{segment}?page[size]={maxItems}";
            using var doc = await GleifJsonMapper.GetJsonAsync(http, path, ct).ConfigureAwait(false);
            if (doc is null)
                return [];

            if (!doc.RootElement.TryGetProperty("data", out var data))
                return [];

            return GleifJsonMapper.MapLeiRecordList(data);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "GLEIF {Segment} failed for {Lei}.", segment, lei);
            return [];
        }
    }

    private async Task<GleifCompanySnapshot?> GetRelatedEntityRecordAsync(string lei, string relationshipSegment,
        CancellationToken ct)
    {
        if (!IsLeiFormat(lei))
            return null;

        try
        {
            var path = $"lei-records/{Uri.EscapeDataString(lei.ToUpperInvariant())}/{relationshipSegment}";
            using var doc = await GleifJsonMapper.GetJsonAsync(http, path, ct).ConfigureAwait(false);
            if (doc is null)
                return null;

            if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind == JsonValueKind.Null)
                return null;

            return GleifJsonMapper.MapLeiRecordDataElement(data);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "GLEIF {Segment} failed for {Lei}.", relationshipSegment, lei);
            return null;
        }
    }

    private async Task<GleifCompanySnapshot?> GetFirstFromListQueryAsync(string query, CancellationToken ct)
    {
        try
        {
            using var doc = await GleifJsonMapper.GetJsonAsync(http, query, ct).ConfigureAwait(false);
            if (doc is null)
                return null;

            if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array ||
                data.GetArrayLength() == 0)
                return null;

            return GleifJsonMapper.MapLeiRecordDataElement(data[0]);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "GLEIF list query failed: {Query}", query);
            return null;
        }
    }

    private static GleifRelatedParty ToRelated(GleifCompanySnapshot profile, string kind) =>
        new(profile.Lei, profile.LegalName, profile.RegisteredAs,
            profile.LegalAddressCountry ?? profile.Jurisdiction, kind);

    private static bool IsLeiFormat(string id) =>
        id.Length == 20 && id.All(char.IsLetterOrDigit);
}

public sealed record GleifEntityMatch(string LegalName, string? CountryIso2, string Lei, string? RegisteredAs)
{
    public static GleifEntityMatch FromSnapshot(GleifCompanySnapshot s) =>
        new(s.LegalName, s.LegalAddressCountry ?? s.Jurisdiction, s.Lei, s.RegisteredAs);
}
