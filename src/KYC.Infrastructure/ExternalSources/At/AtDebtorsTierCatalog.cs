namespace KYC.Infrastructure.ExternalSources.At;

public enum AtDebtorsTaxpayerKind
{
    Singular,
    Coletivo
}

public sealed record AtDebtorsTier(
    string Code,
    AtDebtorsTaxpayerKind Kind,
    string PdfFileName,
    string DebtRangeLabel)
{
    public string FolderName => Kind switch
    {
        AtDebtorsTaxpayerKind.Singular => "Singulares",
        AtDebtorsTaxpayerKind.Coletivo => "Coletivos",
        _ => throw new ArgumentOutOfRangeException(nameof(Kind))
    };

    public string TaxpayerTypeLabel => Kind switch
    {
        AtDebtorsTaxpayerKind.Singular => "Singular",
        AtDebtorsTaxpayerKind.Coletivo => "Coletivo",
        _ => throw new ArgumentOutOfRangeException(nameof(Kind))
    };
}

public static class AtDebtorsTierCatalog
{
    public static IReadOnlyList<AtDebtorsTier> All { get; } =
    [
        new("S1", AtDebtorsTaxpayerKind.Singular, "listaFS1.pdf", "De 7.500 a 25.000 €"),
        new("S2", AtDebtorsTaxpayerKind.Singular, "listaFS2.pdf", "De 25.001 a 50.000 €"),
        new("S3", AtDebtorsTaxpayerKind.Singular, "listaFS3.pdf", "De 50.001 a 100.000 €"),
        new("S4", AtDebtorsTaxpayerKind.Singular, "listaFS4.pdf", "De 100.001 a 250.000 €"),
        new("S5", AtDebtorsTaxpayerKind.Singular, "listaFS5.pdf", "De 250.001 a 1.000.000 €"),
        new("S6", AtDebtorsTaxpayerKind.Singular, "listaFS6.pdf", "De mais de 1.000.000 €"),
        new("C1", AtDebtorsTaxpayerKind.Coletivo, "listaFC1.pdf", "De 10.000 a 50.000 €"),
        new("C2", AtDebtorsTaxpayerKind.Coletivo, "listaFC2.pdf", "De 50.001 a 100.000 €"),
        new("C3", AtDebtorsTaxpayerKind.Coletivo, "listaFC3.pdf", "De 100.001 a 500.000 €"),
        new("C4", AtDebtorsTaxpayerKind.Coletivo, "listaFC4.pdf", "De 500.001 a 1.000.000 €"),
        new("C5", AtDebtorsTaxpayerKind.Coletivo, "listaFC5.pdf", "De 1.000.001 a 5.000.000 €"),
        new("C6", AtDebtorsTaxpayerKind.Coletivo, "listaFC6.pdf", "De mais de 5.000.000 €")
    ];
}
