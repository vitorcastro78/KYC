using System.Text.Json;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using KYC.Infrastructure.Compliance;

namespace KYC.Application.Tests;

public class AmlComplianceReportTests
{
    [Fact]
    public void Metrics_builder_reflects_aggregated_counts()
    {
        var cases = new List<KycCase>();
        for (var i = 0; i < 60; i++)
        {
            var k = KycCase.Start($"5000000{i:D2}", $"Co{i}", "u", CreditAmount.Eur(1000));
            k.SetScore(new RiskScore { Overall = 25, Justification = "low" });
            cases.Add(k);
        }

        var report = AmlComplianceReport.CreateDraft(2025, "tester");
        AmlComplianceMetricsBuilder.Apply(report, cases, reviewsCompleted: 15, scoring: null);

        Assert.Equal(60, report.TotalCasesProcessed);
        Assert.Equal(60, report.CasesLowRisk);
        Assert.Equal(15, report.PeriodicReviewsCompleted);
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
