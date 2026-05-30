using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace KYC.Web.Services;

/// <summary>Fallback Entra/produção: supervisores configurados até integração Graph.</summary>
public sealed class ConfigSupervisorUserDirectory(IConfiguration configuration) : ISupervisorUserDirectory
{
    public Task<IReadOnlyList<SupervisorUserDto>> ListSupervisorsAsync(CancellationToken ct = default)
    {
        var ids = configuration.GetSection("Compliance:SupervisorUserIds").Get<string[]>()
                  ?? ["supervisor@kyc.local", "admin@kyc.local"];
        var list = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => new SupervisorUserDto(id, id, id))
            .ToList();
        return Task.FromResult<IReadOnlyList<SupervisorUserDto>>(list);
    }
}
