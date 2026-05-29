using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KYC.Infrastructure.Health;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddKycHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("KycDatabase")
                 ?? configuration["KYC_DB_CONNECTION"];

        var checks = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(cs))
            checks.AddDbContextCheck<KycDbContext>("postgres", failureStatus: HealthStatus.Unhealthy);

        var ollama = configuration["LLM:LocalEndpoint"] ?? "http://localhost:11434";
        checks.Add(new HealthCheckRegistration(
            "ofac-sls",
            sp => new OfacSlsHealthCheck(sp.GetRequiredService<IHttpClientFactory>()),
            HealthStatus.Degraded,
            ["external", "sanctions"]));

        checks.Add(new HealthCheckRegistration(
            "ollama",
            sp => new OllamaHealthCheck(ollama, sp.GetRequiredService<IHttpClientFactory>()),
            HealthStatus.Degraded,
            ["llm", "external"]));

        var rabbitCs = configuration["KYC_RABBITMQ_CONNECTION"] ?? configuration["RabbitMq:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(rabbitCs))
            checks.Add(new HealthCheckRegistration(
                "rabbitmq",
                _ => new RabbitMqHealthCheck(rabbitCs),
                HealthStatus.Degraded,
                ["messaging"]));

        return services;
    }
}
