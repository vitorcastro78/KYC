using System.Text.Json;
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

    [Fact]
    public void Internal_json_expands_ai_models_as_object_not_escaped_string()
    {
        const string modelsJson =
            """{"provider":"ollama-local","local":"qwen3.5:9b","localVersion":"latest","scoringVersion":"1.0.0","promptHash":"abc","embeddings":"qwen3-embedding:8b"}""";

        var report = AmlComplianceReport.CreateDraft(2025, "admin");
        report.PopulateMetrics(
            totalCases: 1,
            approved: 0,
            rejected: 0,
            underReview: 1,
            low: 0,
            medium: 0,
            high: 0,
            critical: 0,
            signals: 0,
            sanctions: 0,
            peps: 0,
            sars: 0,
            freezes: 0,
            simplified: 0,
            standard: 1,
            enhanced: 0,
            reviewsCompleted: 0,
            reviewsOverdue: 0,
            platformVersion: "1.0.0",
            aiModelsJson: modelsJson);

        using var doc = JsonDocument.Parse(_exporter.ToInternalJson(report));
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.String, root.GetProperty("Status").ValueKind);
        Assert.Equal("Draft", root.GetProperty("Status").GetString());

        var ai = root.GetProperty("AiModelsUsed");
        Assert.Equal(JsonValueKind.Object, ai.ValueKind);
        Assert.Equal("ollama-local", ai.GetProperty("provider").GetString());
        Assert.Equal("qwen3.5:9b", ai.GetProperty("local").GetString());
    }
}
