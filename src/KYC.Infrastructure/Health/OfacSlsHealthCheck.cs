using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace KYC.Infrastructure.Health;

public sealed class OfacSlsHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ofac-sls");
            using var res = await client.GetAsync("/alive", cancellationToken).ConfigureAwait(false);
            return res.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("OFAC SLS reachable")
                : HealthCheckResult.Degraded($"OFAC SLS returned {(int)res.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("OFAC SLS unreachable", ex);
        }
    }
}
