using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace KYC.Infrastructure.Compliance;

/// <summary>Integrações UIF, BdP freeze e identidade — modo produção vs. desenvolvimento.</summary>
public static class ComplianceIntegrationOptions
{
    public static bool RequireLiveIntegrations(IConfiguration configuration, IHostEnvironment? environment = null) =>
        configuration.GetValue("Compliance:RequireLiveIntegrations",
            environment?.IsProduction() == true);

    public static bool AllowManualUifRegistration(IConfiguration configuration) =>
        configuration.GetValue("Uif:AllowManualRegistration", true);

    public static void EnsureConfigured(string integrationName, string? baseUrl, IConfiguration configuration, IHostEnvironment? environment = null)
    {
        if (!RequireLiveIntegrations(configuration, environment) || !string.IsNullOrWhiteSpace(baseUrl))
            return;

        throw new InvalidOperationException(
            $"{integrationName} não está configurada. Defina o BaseUrl em produção ou desactive Compliance:RequireLiveIntegrations apenas em desenvolvimento.");
    }
}
