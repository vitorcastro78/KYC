using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KYC.Application.Common;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using KYC.Infrastructure.Reports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.LLM;

public class KycLlmEngine(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IKycReportComposer reportComposer,
    ILogger<KycLlmEngine> logger) : IKycLlmEngine
{
    private static string Sha256(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }

    private bool UseOllamaForScoring =>
        configuration.GetValue(
            "LLM:UseOllamaForScoring",
            string.Equals(configuration["LLM:Mode"], "LocalOnly", StringComparison.OrdinalIgnoreCase));

    public async Task<RiskScore> ComputeRiskScoreAsync(KycScanContext context, CancellationToken ct = default)
    {
        if (!UseOllamaForScoring)
        {
            logger.LogDebug("LLM scoring via Ollama desactivado; score heur?stico.");
            return ComputeHeuristicRiskScore(context);
        }

        if (!await IsOllamaReachableAsync(ct).ConfigureAwait(false))
        {
            logger.LogWarning("Ollama indispon?vel em {Endpoint}; score heur?stico.",
                configuration["LLM:LocalEndpoint"] ?? "http://localhost:11434");
            return ComputeHeuristicRiskScore(context);
        }

        var system = "Senior KYC Risk Analyst EU. Responde APENAS JSON: {\"overall\":0-100,\"sanctions\":null,\"pep\":null,\"adverse\":null,\"financial\":null,\"judicial\":null,\"ubo\":null,\"justification\":\"pt\"}";
        var user = JsonSerializer.Serialize(BuildScoringPayload(context));
        var promptHash = Sha256(system + user);
        logger.LogInformation("LLM scoring hash={Hash} model=local", promptHash);

        var client = httpClientFactory.CreateClient("ollama-scoring");
        var model = configuration["LLM:LocalModel"] ?? "qwen3.5:9b";
        var payload = new
        {
            model,
            stream = false,
            options = new { num_predict = 256, temperature = 0.1 },
            messages = new object[]
            {
                new { role = "system", content = system },
                new { role = "user", content = user }
            }
        };

        try
        {
            using var response = await client.PostAsJsonAsync("/api/chat", payload, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var doc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct).ConfigureAwait(false);
            var content = doc.GetProperty("message").GetProperty("content").GetString() ?? "{}";
            return ParseRiskScore(content);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ollama scoring failed; using heuristic score.");
            return ComputeHeuristicRiskScore(context);
        }
    }

    private static object BuildScoringPayload(KycScanContext context) =>
        new
        {
            context.CompanyName,
            context.Nif,
            partyCount = context.Parties.Count,
            signals = context.Signals
                .Take(25)
                .Select(s => new { s.Type, s.Severity, description = Truncate(s.Description, 200) })
        };

    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max] + "?";

    private static RiskScore ComputeHeuristicRiskScore(KycScanContext context)
    {
        if (context.Signals.Count == 0)
        {
            return new RiskScore
            {
                Overall = 25,
                Justification = "Sem sinais autom?ticos detetados (score heur?stico)."
            };
        }

        var maxSeverity = context.Signals
            .Select(s => Enum.TryParse<SignalSeverity>(s.Severity, ignoreCase: true, out var sev)
                ? sev
                : SignalSeverity.Medium)
            .Max();

        var overall = maxSeverity switch
        {
            SignalSeverity.Critical => 88,
            SignalSeverity.High => 72,
            SignalSeverity.Medium => 55,
            _ => 38
        };
        overall = Math.Clamp(overall + Math.Min(12, context.Signals.Count * 2), 0, 100);

        return new RiskScore
        {
            Overall = overall,
            Justification =
                $"Score heur?stico: {context.Signals.Count} sinais, severidade m?xima {maxSeverity}."
        };
    }

    private async Task<bool> IsOllamaReachableAsync(CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ollama-health");
            using var res = await client.GetAsync("/api/tags", ct).ConfigureAwait(false);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static RiskScore ParseRiskScore(string content)
    {
        try
        {
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd < jsonStart)
                throw new FormatException("No JSON");
            var slice = content[jsonStart..(jsonEnd + 1)];
            using var doc = JsonDocument.Parse(slice);
            var root = doc.RootElement;
            var overall = root.TryGetProperty("overall", out var o) ? o.GetInt32() : 40;
            return new RiskScore
            {
                Overall = Math.Clamp(overall, 0, 100),
                SanctionsScore = ReadNullableInt(root, "sanctions"),
                PepScore = ReadNullableInt(root, "pep"),
                AdverseMediaScore = ReadNullableInt(root, "adverse"),
                FinancialScore = ReadNullableInt(root, "financial"),
                JudicialScore = ReadNullableInt(root, "judicial"),
                UboStructureScore = ReadNullableInt(root, "ubo"),
                Justification = root.TryGetProperty("justification", out var j) ? j.GetString() ?? "" : ""
            };
        }
        catch
        {
            return new RiskScore { Overall = 45, Justification = "Resposta LLM n?o estruturada." };
        }
    }

    private static int? ReadNullableInt(JsonElement root, string name) =>
        root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number ? p.GetInt32() : null;

    public async Task<KycReport> GenerateNarrativeReportAsync(
        KycScanContext context,
        RiskScore score,
        KycReportComposeRequest? composeRequest = null,
        CancellationToken ct = default)
    {
        var request = composeRequest ?? ToComposeRequest(context, score);
        var baselineHtml = reportComposer.ComposeHtml(request);
        const string templateModel = "KYC.StructuredReportHtml/v1";

        var tryLlm = configuration.GetValue("LLM:EnrichReportsWithLlm", true);
        if (!tryLlm)
            return KycReport.Create(context.CaseId, baselineHtml, templateModel);

        var system = "Fornece APENAS HTML interno de <section class=\"ai-summary\"> com 1-3 paragrafos. Menciona factores de risco e que a decisao final e humana (Art. 22 RGPD). Sem markdown.";
        var user = JsonSerializer.Serialize(new { context, score });
        var hash = Sha256(system + user);
        var model = configuration["LLM:LocalModel"] ?? "qwen3.5:9b";
        logger.LogInformation("LLM report enrich model={Model} risk={Risk} hash={Hash}", model, score.Level, hash);

        try
        {
            var local = await CallOllamaHtmlAsync(system, user, ct);
            if (IsUsefulLlmSection(local))
            {
                var section = LlmChatOutputSanitizer.ExtractReportHtmlFragment(local);
                return KycReport.Create(context.CaseId, AppendLlmHtmlSection(baselineHtml, section),
                    $"{templateModel}+{model}");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ollama report enrich failed; using structured template only.");
        }

        return KycReport.Create(context.CaseId, baselineHtml, templateModel);
    }

    private static KycReportComposeRequest ToComposeRequest(KycScanContext context, RiskScore score) =>
        new(
            context.CaseId,
            context.Nif,
            context.CompanyName,
            KycStatus.InProgress,
            0,
            "EUR",
            DateTime.UtcNow,
            context.Parties,
            context.Signals,
            score,
            DateTime.UtcNow);

    private static bool IsUsefulLlmSection(string? text) =>
        LlmChatOutputSanitizer.IsAcceptableReportHtml(text);

    private static string AppendLlmHtmlSection(string baselineHtml, string llmHtml)
    {
        var trimmed = llmHtml.Trim();
        if (!trimmed.StartsWith("<section", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = $"<section class=\"ai-summary\"><h2>S?ntese assistida por IA</h2>{trimmed}</section>";
        }

        return baselineHtml.Replace("</main>", trimmed + "\n</main>", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> CallOllamaHtmlAsync(string system, string user, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("ollama");
        var model = configuration["LLM:LocalModel"] ?? "qwen3.5:9b";
        var payload = new
        {
            model,
            stream = false,
            messages = new object[]
            {
                new { role = "system", content = system },
                new { role = "user", content = user }
            }
        };
        using var response = await client.PostAsJsonAsync("/api/chat", payload, ct);
        response.EnsureSuccessStatusCode();
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var raw = doc.GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        return LlmChatOutputSanitizer.StripChatArtifacts(raw);
    }

    public async Task<ConsistencyCheckResult> CheckConsistencyAsync(KycScanContext context, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        var issues = new List<string>();
        if (context.Parties.Count(p => p.Role.Contains("Ubo", StringComparison.OrdinalIgnoreCase)) == 0)
            issues.Add("Sem UBO declarado vs. estrutura encontrada ? rever manualmente.");

        var docNif = context.DeclaredFacts
            .FirstOrDefault(f => f.Key.Equals("Nif", StringComparison.OrdinalIgnoreCase))?.Value;
        if (!string.IsNullOrWhiteSpace(docNif) && !string.IsNullOrWhiteSpace(context.Nif))
        {
            var normalizedDoc = new string(docNif.Where(char.IsDigit).ToArray());
            var normalizedCase = new string(context.Nif.Where(char.IsDigit).ToArray());
            if (normalizedDoc.Length == 9 && normalizedCase.Length == 9 &&
                !string.Equals(normalizedDoc, normalizedCase, StringComparison.Ordinal))
            {
                issues.Add($"NIF documental ({docNif}) difere do NIF do caso ({context.Nif}).");
            }
        }

        return new ConsistencyCheckResult(issues.Count == 0, issues, issues.Count == 0 ? 90 : 60);
    }

    public Task<bool> IsLlmHealthyAsync(CancellationToken ct = default) =>
        IsOllamaReachableAsync(ct);
}
