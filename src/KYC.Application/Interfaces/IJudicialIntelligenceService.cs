using KYC.Application.Models;

namespace KYC.Application.Interfaces;

public interface IJudicialIntelligenceService
{
    Task<JudicialResult> SearchAsync(string nif, string name, CancellationToken ct = default);
}
