using KYC.Domain.Entities;

namespace KYC.Application.Interfaces;

public interface IScoringEngineConfigRepository
{
    Task<ScoringEngineConfig?> GetActiveAsync(CancellationToken ct = default);
    Task AddAsync(ScoringEngineConfig config, CancellationToken ct = default);
    Task<IReadOnlyList<ScoringEngineConfig>> ListAsync(CancellationToken ct = default);
}
