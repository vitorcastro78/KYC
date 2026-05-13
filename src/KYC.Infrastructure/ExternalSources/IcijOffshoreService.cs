using KYC.Application.Interfaces;

namespace KYC.Infrastructure.ExternalSources;

public class IcijOffshoreService(ILogger<IcijOffshoreService> log) : IIcijOffshoreService
{
    public Task<IReadOnlyList<IcijMatch>> SearchAsync(string name, CancellationToken ct = default)
    {
        log.LogDebug("ICIJ offshore search (placeholder).");
        return Task.FromResult<IReadOnlyList<IcijMatch>>(Array.Empty<IcijMatch>());
    }
}
