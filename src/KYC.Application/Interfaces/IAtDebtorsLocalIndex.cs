namespace KYC.Application.Interfaces;

public interface IAtDebtorsLocalIndex
{
    AtDebtorsIndexMatch? FindByNif(string nif);
}

public sealed record AtDebtorsIndexMatch(
    string Nif,
    string Name,
    string TierCode,
    string TaxpayerType,
    string DebtRangeLabel,
    string? SourceUpdatedAt);
