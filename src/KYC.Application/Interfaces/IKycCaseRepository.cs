using KYC.Application.Common;
using KYC.Application.Filtering;
using KYC.Domain.Entities;

namespace KYC.Application.Interfaces;

public interface IKycCaseRepository
{
    Task<KycCase?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<KycCase?> GetByNifAsync(string nif, CancellationToken ct = default);
    Task<PagedResult<KycCase>> ListAsync(KycCaseFilter filter, CancellationToken ct = default);
    Task AddAsync(KycCase kycCase, CancellationToken ct = default);
    Task UpdateAsync(KycCase kycCase, CancellationToken ct = default);
    Task<(KycCase Case, RiskSignal Signal)?> GetCaseWithSignalAsync(Guid signalId, CancellationToken ct = default);
    Task<IReadOnlyList<KycCase>> GetCasesDueForReviewAsync(DateTime dueBy, CancellationToken ct = default);
    Task<(KycCase Case, CaseParty Party)?> GetCaseWithPartyAsync(Guid partyId, CancellationToken ct = default);
}
