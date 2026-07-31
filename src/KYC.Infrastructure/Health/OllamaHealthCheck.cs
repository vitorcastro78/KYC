using KYC.Infrastructure.LLM;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KYC.Infrastructure.Health;

public sealed class OllamaHealthCheck(string endpoint, IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ollama-health");
            client.BaseAddress = new Uri(endpoint.TrimEnd('/') + "/");
            var ok = await OpenAiCompatibleClient.IsReachableAsync(client, cancellationToken);
            return ok
                ? HealthCheckResult.Healthy("LLM (OpenAI-compatible) reachable")
                : HealthCheckResult.Degraded("LLM /v1/models unreachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("LLM unreachable", ex);
        }
    }
}
