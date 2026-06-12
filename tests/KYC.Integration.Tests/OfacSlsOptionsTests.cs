using KYC.Infrastructure.ExternalSources;
using Microsoft.Extensions.Configuration;

namespace KYC.Integration.Tests;

public class OfacSlsOptionsTests
{
    [Fact]
    public void ResolveDownloadUrl_uses_sls_api_download_by_default()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var url = OfacSlsOptions.ResolveDownloadUrl(config);

        Assert.Equal(
            "https://sanctionslistservice.ofac.treas.gov/api/download/SDN_ADVANCED.XML",
            url);
    }

    [Fact]
    public void ResolveDownloadUrl_honours_explicit_export_url()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExternalSources:OfacSdnDailyDownload:ExportUrl"] = "https://example.com/custom.xml"
            })
            .Build();

        Assert.Equal("https://example.com/custom.xml", OfacSlsOptions.ResolveDownloadUrl(config));
    }
}
