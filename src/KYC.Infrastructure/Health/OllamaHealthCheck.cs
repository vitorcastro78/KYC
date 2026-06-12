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
            using var res = await client.GetAsync("api/tags", cancellationToken);
            return res.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Ollama reachable")
                : HealthCheckResult.Degraded($"Ollama returned {(int)res.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Ollama unreachable", ex);
        }
    }
}
