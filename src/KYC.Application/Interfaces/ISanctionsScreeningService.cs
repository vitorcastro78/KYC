using KYC.Application.Models;
using KYC.Domain.Entities;

namespace KYC.Application.Interfaces;

public interface ISanctionsScreeningService
{
    Task<SanctionsResult> ScreenEntityAsync(CaseParty party, CancellationToken ct = default);
    Task<SanctionsResult> ScreenByNameAsync(string name, string? nationality = null, CancellationToken ct = default);
}
