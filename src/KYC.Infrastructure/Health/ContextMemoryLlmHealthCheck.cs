using KYC.Infrastructure.LLM;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KYC.Infrastructure.Health;

public sealed class ContextMemoryLlmHealthCheck(string endpoint, IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("contextmemory-health");
            client.BaseAddress = new Uri(endpoint.TrimEnd('/') + "/");
            var ok = await ContextMemoryChatClient.IsReachableAsync(client, cancellationToken);
            return ok
                ? HealthCheckResult.Healthy("ContextMemory LLM reachable")
                : HealthCheckResult.Degraded("ContextMemory /v1/models unreachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("ContextMemory unreachable", ex);
        }
    }
}
