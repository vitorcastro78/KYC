using KYC.Application.Common;
using KYC.Application.Filtering;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;

namespace KYC.Web.Integration.Tests.Support;

/// <summary>Repositório em memória para testes HTTP do webhook.</summary>
public sealed class TestKycCaseRepository : IKycCaseRepository
{
    private readonly Dictionary<Guid, (KycCase Case, CaseParty Party)> _parties = new();
    private readonly Dictionary<string, (KycCase Case, CaseParty Party)> _bySession = new();

    public void Seed(KycCase kyc, CaseParty party, string sessionId)
    {
        _parties[party.Id] = (kyc, party);
        _bySession[sessionId] = (kyc, party);
    }

    public Task<(KycCase Case, CaseParty Party)?> GetCaseWithPartyBySessionIdAsync(string sessionId, CancellationToken ct = default)
    {
        if (!_bySession.TryGetValue(sessionId, out var match))
            return Task.FromResult<(KycCase Case, CaseParty Party)?>(null);
        return Task.FromResult<(KycCase Case, CaseParty Party)?>(match);
    }

    public Task<(KycCase Case, CaseParty Party)?> GetCaseWithPartyAsync(Guid partyId, CancellationToken ct = default)
    {
        if (!_parties.TryGetValue(partyId, out var match))
            return Task.FromResult<(KycCase Case, CaseParty Party)?>(null);
        return Task.FromResult<(KycCase Case, CaseParty Party)?>(match);
    }

    public Task UpdateAsync(KycCase kycCase, CancellationToken ct = default) => Task.CompletedTask;

    public Task<KycCase?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_parties.Values.Select(v => v.Case).FirstOrDefault(c => c.Id == id));

    public Task<KycCase?> GetByNifAsync(string nif, CancellationToken ct = default) => Task.FromResult<KycCase?>(null);

    public Task<PagedResult<KycCase>> ListAsync(KycCaseFilter filter, CancellationToken ct = default) =>
        Task.FromResult(new PagedResult<KycCase>([], 0, 1, 50));

    public Task AddAsync(KycCase kycCase, CancellationToken ct = default) => Task.CompletedTask;

    public Task<(KycCase Case, RiskSignal Signal)?> GetCaseWithSignalAsync(Guid signalId, CancellationToken ct = default) =>
        Task.FromResult<(KycCase Case, RiskSignal Signal)?>(null);

    public Task<IReadOnlyList<KycCase>> GetCasesDueForReviewAsync(DateTime dueBy, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<KycCase>>([]);
}
