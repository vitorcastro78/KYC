using KYC.Application.Models;

namespace KYC.Application.Interfaces;

public interface IEntityResolutionService
{
    Task<EntityResolutionResult> ResolveByNifAsync(string nif, CancellationToken ct = default);
    Task<UboGraph> BuildUboGraphAsync(string nif, int maxDepth = 5, CancellationToken ct = default);
}
