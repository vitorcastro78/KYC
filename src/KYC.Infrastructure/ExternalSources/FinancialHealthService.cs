using KYC.Application.Interfaces;
using KYC.Application.Models;

namespace KYC.Infrastructure.ExternalSources;

public sealed class FinancialHealthService(
    IAtDebtorsLocalIndex atDebtors,
    ILogger<FinancialHealthService> log) : IFinancialHealthService
{
    public Task<FinancialHealthResult> AnalyseAsync(string nif, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var match = atDebtors.FindByNif(nif);
        if (match is null)
        {
            log.LogDebug("NIF {Nif} não encontrado nas listas públicas AT devedores.", nif);
            return Task.FromResult(new FinancialHealthResult(
                nif,
                null,
                null,
                null,
                "Sem registo na lista pública de devedores AT."));
        }

        var summary =
            $"Listado como devedor AT ({match.TaxpayerType}, escalão {match.TierCode}: {match.DebtRangeLabel}" +
            (match.SourceUpdatedAt is not null ? $", actualizado {match.SourceUpdatedAt}" : string.Empty) +
            ").";

        log.LogInformation("NIF {Nif} encontrado na lista AT devedores ({Tier}).", nif, match.TierCode);

        return Task.FromResult(new FinancialHealthResult(
            nif,
            null,
            null,
            null,
            summary,
            IsAtPublicDebtor: true,
            AtDebtRangeLabel: match.DebtRangeLabel));
    }
}
