using KYC.Domain.Entities;

namespace KYC.Application.Interfaces;

public interface ICustomerAcceptancePolicyRepository
{
    Task<CustomerAcceptancePolicy?> GetActiveAsync(CancellationToken ct = default);
    Task AddAsync(CustomerAcceptancePolicy policy, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerAcceptancePolicy>> ListAsync(CancellationToken ct = default);
}
