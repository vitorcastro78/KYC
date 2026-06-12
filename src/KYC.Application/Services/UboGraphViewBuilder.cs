using KYC.Application.Common;
using KYC.Application.Dtos;
using KYC.Application.Models;
using KYC.Domain.Entities;
using KYC.Domain.Enums;

namespace KYC.Application.Services;

public static class UboGraphViewBuilder
{
    public static UboGraphViewDto Build(KycCase kyc, UboGraph graph)
    {
        var partiesByNif = kyc.Parties
            .Where(p => !string.IsNullOrWhiteSpace(p.Nif) && NifSanitizer.TryNormalize(p.Nif!, out _))
            .GroupBy(p => NormalizeKey(p.Nif!))
            .ToDictionary(g => g.Key, g => g.First());

        var nodeDtos = new List<UboGraphNodeDto>();
        var matchedPartyIds = new HashSet<Guid>();

        foreach (var n in graph.Nodes)
        {
            CaseParty? party = null;
            if (!string.IsNullOrWhiteSpace(n.Nif) && partiesByNif.TryGetValue(NormalizeKey(n.Nif), out var p))
            {
                party = p;
                matchedPartyIds.Add(p.Id);
            }

            nodeDtos.Add(ToNodeDto(n, party, n.Name.Contains("sintético", StringComparison.OrdinalIgnoreCase)));
        }

        var root = nodeDtos.FirstOrDefault(n => n.Depth == 0) ?? nodeDtos.FirstOrDefault();
        var rootId = root?.Id ?? Guid.NewGuid();

        foreach (var party in kyc.Parties.Where(p => !matchedPartyIds.Contains(p.Id)))
        {
            nodeDtos.Add(new UboGraphNodeDto(
                party.Id,
                party.Id,
                party.Name,
                party.Nif,
                party.Type == EntityType.Individual ? "Individual" : "Company",
                party.UboDepthLevel,
                party.OwnershipPercentage,
                RoleToRelationship(party.Role),
                party.Nationality,
                party.IsPep,
                party.IsSanctioned,
                party.IsOffshore,
                party.VerificationStatus.ToString(),
                RoleLabel(party.Role),
                IsSynthetic: false));
        }

        var edgeDtos = graph.Edges
            .Select(e => new UboGraphEdgeDto(e.FromId, e.ToId, e.OwnershipPct, EdgeLabel(e.OwnershipPct)))
            .ToList();

        foreach (var party in kyc.Parties.Where(p => !matchedPartyIds.Contains(p.Id)))
        {
            var ownedEntityId = party.ParentPartyId is { } pid && nodeDtos.Any(n => n.Id == pid)
                ? pid
                : rootId;
            if (!edgeDtos.Any(e => (e.FromId == party.Id && e.ToId == ownedEntityId)
                                   || (e.FromId == ownedEntityId && e.ToId == party.Id)))
            {
                edgeDtos.Add(new UboGraphEdgeDto(
                    party.Id,
                    ownedEntityId,
                    party.OwnershipPercentage,
                    EdgeLabel(party.OwnershipPercentage)));
            }
        }

        var hasLevel2 = graph.Nodes.Count > 2
                        && !graph.Nodes.Any(n => n.Name.Contains("sintético", StringComparison.OrdinalIgnoreCase));

        return new UboGraphViewDto(
            kyc.CompanyName,
            kyc.Nif,
            hasLevel2,
            nodeDtos,
            edgeDtos);
    }

    private static UboGraphNodeDto ToNodeDto(UboNode n, CaseParty? party, bool isSynthetic) =>
        new(
            n.Id,
            party?.Id,
            party?.Name ?? n.Name,
            party?.Nif ?? n.Nif,
            party is null ? n.Type : party.Type == EntityType.Individual ? "Individual" : "Company",
            party?.UboDepthLevel ?? n.Depth,
            party?.OwnershipPercentage ?? n.OwnershipPct,
            n.GleifRelationshipKind ?? (party is not null ? RoleToRelationship(party.Role) : null),
            n.CountryIso2 ?? party?.Nationality,
            party?.IsPep ?? false,
            party?.IsSanctioned ?? false,
            party?.IsOffshore ?? false,
            party?.VerificationStatus.ToString(),
            party is not null ? RoleLabel(party.Role) : null,
            isSynthetic);

    private static string NormalizeKey(string nif) =>
        NifSanitizer.TryNormalize(nif, out var n) ? n : nif.Trim().ToUpperInvariant();

    private static string RoleLabel(EntityRole role) => role switch
    {
        EntityRole.Target => "Tomador",
        EntityRole.Shareholder => "Accionista",
        EntityRole.Ubo => "UBO",
        EntityRole.BoardMember => "Direcção",
        EntityRole.Proxy => "Procurador",
        _ => role.ToString()
    };

    private static string RoleToRelationship(EntityRole role) => role switch
    {
        EntityRole.Ubo => "UBO",
        EntityRole.Shareholder => "Shareholder",
        EntityRole.BoardMember => "Board",
        EntityRole.Proxy => "Proxy",
        _ => "Related"
    };

    private static string? EdgeLabel(decimal pct) =>
        pct > 0 ? $"{pct:0.##}%" : null;
}
