using KYC.Application.Interfaces;
using KYC.Application.Models;

namespace KYC.Infrastructure.ExternalSources;

public class FinancialHealthService(ILogger<FinancialHealthService> log) : IFinancialHealthService
{
    public Task<FinancialHealthResult> AnalyseAsync(string nif, CancellationToken ct = default)
    {
        log.LogDebug("Financial health (placeholder, AT pública futura).");
        return Task.FromResult(new FinancialHealthResult(nif, null, null, null, "Sem dados públicos agregados."));
    }
}
