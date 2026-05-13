using KYC.Application.Common;

namespace KYC.Application.Tests;

public class NifSanitizerCaseKeyTests
{
    [Fact]
    public void TryNormalizeCaseKey_accepts_Angolaves_Empreendimentos_Avicolas_like_ui()
    {
        const string fromUi = "Angolaves Empreendimentos Avícolas";
        Assert.True(NifSanitizer.TryNormalizeCaseKey(fromUi, out var key));
        Assert.NotEmpty(key);
        Assert.True(key.Length <= NifSanitizer.MaxCaseKeyLength);
        Assert.Contains("ANGOLAVES", key, StringComparison.Ordinal);
    }

    [Fact]
    public void TryNormalizeCaseKey_normalizes_NFD_to_composed_form()
    {
        var nfd = "Angolaves Avi\u0301colas";
        Assert.True(NifSanitizer.TryNormalizeCaseKey(nfd, out var key));
        Assert.Contains("ANGOLAVES", key, StringComparison.Ordinal);
        Assert.Contains("COLAS", key, StringComparison.Ordinal);
    }
}
