using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

    public async Task<RiskScore> ComputeRiskScoreAsync(KycScanContext context, CancellationToken ct = default)
    {
        if (!configuration.GetValue("LLM:UseOllamaForScoring", false))
        {
            logger.LogDebug("LLM scoring via Ollama desactivado; score heurùstico.");
            return ComputeHeuristicRiskScore(context);
        }

        if (!await IsOllamaReachableAsync(ct).ConfigureAwait(false))
        {
            logger.LogWarning("Ollama indisponùvel em {Endpoint}; score heurùstico.",
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
        text.Length <= max ? text : text[..max] + "ù";

    private static RiskScore ComputeHeuristicRiskScore(KycScanContext context)
    {
        if (context.Signals.Count == 0)
        {
            return new RiskScore
            {
                Overall = 25,
                Justification = "Sem sinais automùticos detetados (score heurùstico)."
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
                $"Score heurùstico: {context.Signals.Count} sinais, severidade mùxima {maxSeverity}."
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
            return new RiskScore { Overall = 45, Justification = "Resposta LLM nùo estruturada." };
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

        var mode = (configuration["LLM:Mode"] ?? "LocalOnly").Trim();
        var localOnly = string.Equals(mode, "LocalOnly", StringComparison.OrdinalIgnoreCase);
        var riskWarrantsCloud = score.Level is RiskLevel.High or RiskLevel.Critical;
        var apiKey = configuration["Anthropic:ApiKey"];
        var useCloud = !localOnly && riskWarrantsCloud && !string.IsNullOrWhiteSpace(apiKey);

        var system = "Fornece APENAS o conteùdo interno HTML de <section class=\"ai-summary\"> com 1-3 parùgrafos curtos para analista KYC.";
        var user = JsonSerializer.Serialize(new { context, score });
        var hash = Sha256(system + user);
        logger.LogInformation("LLM report enrich mode={Mode} risk={Risk} useCloud={UseCloud} hash={Hash}",
            mode, score.Level, useCloud, hash);

        if (useCloud)
        {
            try
            {
                var text = await CallAnthropicAsync(system, user, ct);
                if (IsUsefulLlmSection(text))
                {
                    var model = configuration["LLM:CloudModel"];
                    return KycReport.Create(context.CaseId, AppendLlmHtmlSection(baselineHtml, text),
                        $"{templateModel}+{model}");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Anthropic report enrich failed; trying local or template only.");
            }
        }

        try
        {
            var local = await CallOllamaHtmlAsync(system, user, ct);
            if (IsUsefulLlmSection(local))
            {
                var model = configuration["LLM:LocalModel"] ?? "qwen3.5:9b";
                return KycReport.Create(context.CaseId, AppendLlmHtmlSection(baselineHtml, local),
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
        !string.IsNullOrWhiteSpace(text) && text.Trim().Length >= 80;

    private static string AppendLlmHtmlSection(string baselineHtml, string llmHtml)
    {
        var trimmed = llmHtml.Trim();
        if (!trimmed.StartsWith("<section", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = $"<section class=\"ai-summary\"><h2>Sùntese assistida por IA</h2>{trimmed}</section>";
        }

        return baselineHtml.Replace("</main>", trimmed + "\n</main>", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> CallAnthropicAsync(string system, string user, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("anthropic");
        var model = configuration["LLM:CloudModel"] ?? "claude-sonnet-4-20250514";
        var apiKey = configuration["Anthropic:ApiKey"]!;
        using var req = new HttpRequestMessage(HttpMethod.Post, "v1/messages");
        req.Headers.Add("x-api-key", apiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");
        req.Content = JsonContent.Create(new
        {
            model,
            max_tokens = 2000,
            system,
            messages = new[] { new { role = "user", content = user } }
        });
        using var res = await client.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        var doc = await res.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return doc.GetProperty("content")[0].GetProperty("text").GetString() ?? string.Empty;
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
        return doc.GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }

    public async Task<ConsistencyCheckResult> CheckConsistencyAsync(KycScanContext context, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        var issues = new List<string>();
        if (context.Parties.Count(p => p.Role.Contains("Ubo", StringComparison.OrdinalIgnoreCase)) == 0)
            issues.Add("Sem UBO declarado vs. estrutura encontrada ù rever manualmente.");
        return new ConsistencyCheckResult(issues.Count == 0, issues, issues.Count == 0 ? 90 : 60);
    }

    public Task<bool> IsLlmHealthyAsync(CancellationToken ct = default) =>
        IsOllamaReachableAsync(ct);
}
