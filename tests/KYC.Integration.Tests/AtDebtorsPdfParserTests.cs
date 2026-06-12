using KYC.Infrastructure.ExternalSources.At;

namespace KYC.Integration.Tests;

public class AtDebtorsPdfParserTests
{
    [Fact]
    public void ParseText_extracts_nif_name_multiline_and_updated_date()
    {
        const string text = """
            Contribuintes Colectivos
            Devedores de 10.000 a 50.000 €
            Informação actualizada em 2026-05-28
            NIPC DESIGNAÇÃO
            504177672 EMPRESA A LDA
            504431722 EMPRESA B ASSOC PARA O DESENVOLVIMENTO
            DE VILA FRANCA DE XIRA
            Página: 1
            """;

        var parsed = AtDebtorsPdfParser.ParseText(text);

        Assert.Equal(new DateOnly(2026, 5, 28), parsed.SourceUpdatedAt);
        Assert.Equal(2, parsed.Entries.Count);
        Assert.Equal("504177672", parsed.Entries[0].Nif);
        Assert.Equal("504431722", parsed.Entries[1].Nif);
        Assert.Contains("VILA FRANCA", parsed.Entries[1].Name, StringComparison.Ordinal);
    }
}
