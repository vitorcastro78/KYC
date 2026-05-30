using Azure.Identity;
using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace KYC.Web.Services;

/// <summary>Supervisores a partir de um grupo Microsoft Entra (app-only Graph).</summary>
public sealed class EntraGraphSupervisorUserDirectory(
    IConfiguration configuration,
    ILogger<EntraGraphSupervisorUserDirectory> log) : ISupervisorUserDirectory
{
    public async Task<IReadOnlyList<SupervisorUserDto>> ListSupervisorsAsync(CancellationToken ct = default)
    {
        var groupId = configuration["Compliance:SupervisorGroupObjectId"];
        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];
        var clientSecret = configuration["AzureAd:ClientSecret"]
                           ?? Environment.GetEnvironmentVariable("AZURE_AD_CLIENT_SECRET");

        if (string.IsNullOrWhiteSpace(groupId)
            || string.IsNullOrWhiteSpace(tenantId)
            || string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret))
        {
            log.LogWarning("Graph supervisores: falta SupervisorGroupObjectId ou credenciais app-only; usar ConfigSupervisorUserDirectory.");
            return [];
        }

        try
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var graph = new GraphServiceClient(credential);

            var members = await graph.Groups[groupId].Members.GetAsync(cancellationToken: ct);
            var list = new List<SupervisorUserDto>();

            foreach (var member in members?.Value ?? [])
            {
                if (member is not User user)
                    continue;

                var email = user.Mail ?? user.UserPrincipalName ?? user.Id ?? "";
                if (string.IsNullOrWhiteSpace(email))
                    continue;

                list.Add(new SupervisorUserDto(
                    email,
                    user.DisplayName ?? email,
                    email));
            }

            return list.OrderBy(s => s.DisplayName).ToList();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Falha ao listar supervisores via Microsoft Graph.");
            return [];
        }
    }
}
