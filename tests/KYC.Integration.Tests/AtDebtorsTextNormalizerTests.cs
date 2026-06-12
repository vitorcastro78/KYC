using System.Text.Json;
using KYC.Infrastructure.ExternalSources.At;

namespace KYC.Integration.Tests;

public class AtDebtorsTextNormalizerTests
{
    [Fact]
    public void Normalize_trims_leading_dot_and_collapses_spaces()
    {
        var result = AtDebtorsTextNormalizer.Normalize(". DE LUZ- ILUMINAÇÃO & DECORAÇÃO, LDA");

        Assert.Equal("DE LUZ- ILUMINAÇÃO & DECORAÇÃO, LDA", result);
    }

    [Fact]
    public void Json_serializer_writes_portuguese_characters_without_unicode_escapes()
    {
        var json = JsonSerializer.Serialize(
            new { name = "AUTOMOVEIS PEÇAS COMERCIO E REPARAÇÃO UNIPESSOAL LDA" },
            AtDebtorsJson.SerializerOptions);

        Assert.Contains("PEÇAS", json, StringComparison.Ordinal);
        Assert.Contains("REPARAÇÃO", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\\u00", json, StringComparison.Ordinal);
    }
}
