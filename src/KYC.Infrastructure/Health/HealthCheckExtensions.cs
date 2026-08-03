using KYC.Infrastructure.LLM;
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

        var cm = configuration.GetSection(ContextMemoryOptions.SectionName);
        var cmBaseUrl = cm["BaseUrl"];
        var cmApiKey = cm["ApiKey"];
        if (string.IsNullOrWhiteSpace(cmBaseUrl) || string.IsNullOrWhiteSpace(cmApiKey))
            throw new InvalidOperationException(
                "ContextMemory:BaseUrl and ContextMemory:ApiKey are required for LLM health checks.");

        checks.Add(new HealthCheckRegistration(
            "ofac-sls",
            sp => new OfacSlsHealthCheck(sp.GetRequiredService<IHttpClientFactory>()),
            HealthStatus.Degraded,
            ["external", "sanctions"]));

        checks.Add(new HealthCheckRegistration(
            "contextmemory",
            sp => new ContextMemoryLlmHealthCheck(cmBaseUrl, sp.GetRequiredService<IHttpClientFactory>()),
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
