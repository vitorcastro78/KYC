using System.Security.Claims;
using KYC.Application.Interfaces;

namespace KYC.Web.Services;

public sealed class HttpContextAnalystAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentAnalystAccessor
{
    public Task<string> GetAnalystIdAsync(CancellationToken ct = default)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var id = user?.FindFirstValue(ClaimTypes.Email)
                 ?? user?.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? user?.Identity?.Name
                 ?? "system";
        return Task.FromResult(id);
    }
}
