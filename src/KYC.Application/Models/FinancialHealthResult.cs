namespace KYC.Application.Models;

public record FinancialHealthResult(
    string Nif,
    decimal? Revenue,
    decimal? Equity,
    decimal? ZScore,
    string Summary);
