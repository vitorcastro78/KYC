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
        var cmConfigured = !string.IsNullOrWhiteSpace(cm["BaseUrl"]) && !string.IsNullOrWhiteSpace(cm["ApiKey"]);
        var llmEndpoint = cmConfigured
            ? cm["BaseUrl"]!
            : (configuration["LLM:LocalEndpoint"] ?? "http://localhost:11434");

        checks.Add(new HealthCheckRegistration(
            "ofac-sls",
            sp => new OfacSlsHealthCheck(sp.GetRequiredService<IHttpClientFactory>()),
            HealthStatus.Degraded,
            ["external", "sanctions"]));

        checks.Add(new HealthCheckRegistration(
            cmConfigured ? "contextmemory" : "ollama",
            sp => new OllamaHealthCheck(llmEndpoint, sp.GetRequiredService<IHttpClientFactory>()),
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
