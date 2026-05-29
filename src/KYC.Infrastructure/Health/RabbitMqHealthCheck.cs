using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace KYC.Infrastructure.Health;

public sealed class RabbitMqHealthCheck(string connectionString) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
            using var conn = factory.CreateConnectionAsync(cancellationToken).GetAwaiter().GetResult();
            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ reachable"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Degraded("RabbitMQ unreachable", ex));
        }
    }
}
