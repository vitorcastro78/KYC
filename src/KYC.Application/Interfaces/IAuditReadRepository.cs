using KYC.Domain.Entities;

namespace KYC.Application.Interfaces;

public interface IAuditReadRepository
{
    Task<IReadOnlyList<AuditEntry>> ListGlobalAsync(int skip, int take, CancellationToken ct = default);
}
