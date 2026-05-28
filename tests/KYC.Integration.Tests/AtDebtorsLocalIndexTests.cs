using System.Text.Json;
using KYC.Infrastructure.ExternalSources.At;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace KYC.Integration.Tests;

public class AtDebtorsLocalIndexTests
{
    [Fact]
    public void FindByNif_returns_match_from_generated_json()
    {
        var root = Path.Combine(Path.GetTempPath(), "kyc-at-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "Coletivos"));

        var tierJson = Path.Combine(root, "Coletivos", "C1.json");
        File.WriteAllText(tierJson, JsonSerializer.Serialize(new AtDebtorsTierDocument
        {
            TierCode = "C1",
            TaxpayerType = "Coletivo",
            DebtRangeLabel = "De 10.000 a 50.000 €",
            SourceUpdatedAt = "2026-05-28",
            EntryCount = 1,
            Entries =
            [
                new AtDebtorsTierEntry { Nif = "504177672", Name = "EMPRESA TESTE LDA" }
            ]
        }));

        File.WriteAllText(Path.Combine(root, "manifest.json"), JsonSerializer.Serialize(new AtDebtorsManifest
        {
            SyncedAt = DateTimeOffset.UtcNow,
            TierCount = 1,
            TotalEntries = 1,
            Tiers =
            [
                new AtDebtorsManifestTier
                {
                    TierCode = "C1",
                    TaxpayerType = "Coletivo",
                    DebtRangeLabel = "De 10.000 a 50.000 €",
                    JsonRelativePath = "Coletivos/C1.json",
                    EntryCount = 1
                }
            ]
        }));

        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ExternalSources:AtDebtorsDailyDownload:DataRootPath"] = root
                })
                .Build();

            var index = new AtDebtorsLocalIndex(
                config,
                new FakeHostEnvironment(root),
                NullLogger<AtDebtorsLocalIndex>.Instance);

            var match = index.FindByNif("504177672");

            Assert.NotNull(match);
            Assert.Equal("C1", match!.TierCode);
            Assert.Equal("Coletivo", match.TaxpayerType);
            Assert.Equal("EMPRESA TESTE LDA", match.Name);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class FakeHostEnvironment(string contentRoot) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "KYC.Integration.Tests";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
