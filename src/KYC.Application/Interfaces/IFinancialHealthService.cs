using KYC.Application.Models;

namespace KYC.Application.Interfaces;

public interface IFinancialHealthService
{
    Task<FinancialHealthResult> AnalyseAsync(string nif, CancellationToken ct = default);
}
