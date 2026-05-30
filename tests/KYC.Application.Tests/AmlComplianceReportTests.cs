using System.Text.Json;
using KYC.Domain.Entities;
using KYC.Infrastructure.Compliance;

namespace KYC.Application.Tests;

public class AmlComplianceReportTests
{
    [Fact]
    public void PopulateMetrics_reflects_aggregated_counts()
    {
        var report = AmlComplianceReport.CreateDraft(2025, "tester");
        report.PopulateMetrics(
            totalCases: 100,
            approved: 60,
            rejected: 10,
            underReview: 30,
            low: 40,
            medium: 35,
            high: 20,
            critical: 5,
            signals: 250,
            sanctions: 3,
            peps: 8,
            sars: 2,
            freezes: 1,
            simplified: 20,
            standard: 50,
            enhanced: 30,
            reviewsCompleted: 15,
            reviewsOverdue: 4,
            platformVersion: "1.0.0",
            aiModelsJson: AmlComplianceReportService.BuildOllamaOnlyModelsJson(null));

        Assert.Equal(100, report.TotalCasesProcessed);
        Assert.Equal(60, report.TotalCasesApproved);
        Assert.Equal(250, report.TotalRiskSignalsDetected);
        Assert.Equal(30, report.CasesEnhancedDd);
    }

    [Fact]
    public void Ai_models_json_is_ollama_only_without_cloud_providers()
    {
        var scoring = ScoringEngineConfig.CreateDefault("test", "abc123");
        var json = AmlComplianceReportService.BuildOllamaOnlyModelsJson(scoring);
        Assert.Contains("ollama-local", json);
        Assert.DoesNotContain("openai", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("azure", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("gpt", json, StringComparison.OrdinalIgnoreCase);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("qwen3.5:9b", doc.RootElement.GetProperty("local").GetString());
    }
}
