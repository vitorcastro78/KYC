namespace KYC.Application.Models;

public record UboGraph(IReadOnlyList<UboNode> Nodes, IReadOnlyList<UboEdge> Edges);

public record UboNode(Guid Id, string Name, string? Nif, string Type, int Depth, decimal? OwnershipPct);

public record UboEdge(Guid FromId, Guid ToId, decimal OwnershipPct);
