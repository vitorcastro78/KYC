using KYC.Domain.Entities;
using KYC.Infrastructure.Compliance;

namespace KYC.Application.Tests;

public class BdpRpbExporterTests
{
    private readonly BdpRpbExporter _exporter = new();

    [Fact]
    public void Official_xml_passes_validation()
    {
        var report = AmlComplianceReport.CreateDraft(2025, "analyst");
        report.PopulateMetrics(
            totalCases: 10,
            approved: 5,
            rejected: 2,
            underReview: 3,
            low: 4,
            medium: 3,
            high: 2,
            critical: 1,
            signals: 20,
            sanctions: 1,
            peps: 2,
            sars: 1,
            freezes: 0,
            simplified: 2,
            standard: 5,
            enhanced: 3,
            reviewsCompleted: 4,
            reviewsOverdue: 1,
            platformVersion: "1.0.0",
            aiModelsJson: "{}");

        var xml = _exporter.ToOfficialXml(report);
        var result = _exporter.ValidateOfficialXml(xml);

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Internal_json_is_non_empty()
    {
        var report = AmlComplianceReport.CreateDraft(2024, "system");
        var json = _exporter.ToInternalJson(report);
        Assert.True(json.Length > 10);
    }
}
