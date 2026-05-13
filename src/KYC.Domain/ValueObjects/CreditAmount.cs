namespace KYC.Domain.ValueObjects;

public record CreditAmount(decimal Amount, string Currency)
{
    public static CreditAmount Eur(decimal amount) => new(amount, "EUR");
}
