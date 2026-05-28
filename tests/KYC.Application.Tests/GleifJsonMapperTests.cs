using System.Text.Json;
using KYC.Infrastructure.ExternalSources.Gleif;
using Xunit;

namespace KYC.Application.Tests;

public class GleifJsonMapperTests
{
    [Fact]
    public void MapLeiRecord_extracts_core_fields()
    {
        const string json = """
            {
              "data": {
                "type": "lei-records",
                "id": "213800WAVVOPS85N2205",
                "attributes": {
                  "lei": "213800WAVVOPS85N2205",
                  "entity": {
                    "legalName": { "name": "LONDON STOCK EXCHANGE LEI LIMITED", "language": "en" },
                    "legalAddress": {
                      "addressLines": ["10 PATERNOSTER SQUARE"],
                      "city": "LONDON",
                      "country": "GB",
                      "postalCode": "EC4M 7LS"
                    },
                    "registeredAs": "08530763",
                    "jurisdiction": "GB",
                    "status": "ACTIVE",
                    "legalForm": { "id": "H0PO" }
                  },
                  "registration": {
                    "status": "ISSUED",
                    "initialRegistrationDate": "2015-12-14T00:00:00Z"
                  },
                  "ocid": "gb/08530763",
                  "conformityFlag": "CONFORMING"
                }
              }
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var snapshot = GleifJsonMapper.MapLeiRecordDataElement(doc.RootElement.GetProperty("data"));

        Assert.NotNull(snapshot);
        Assert.Equal("213800WAVVOPS85N2205", snapshot!.Lei);
        Assert.Equal("LONDON STOCK EXCHANGE LEI LIMITED", snapshot.LegalName);
        Assert.Equal("GB", snapshot.Jurisdiction);
        Assert.Equal("08530763", snapshot.RegisteredAs);
        Assert.Equal("H0PO", snapshot.LegalFormId);
        Assert.Equal("ACTIVE", snapshot.EntityStatus);
        Assert.Contains("LONDON", snapshot.LegalAddressSummary);
    }
}
