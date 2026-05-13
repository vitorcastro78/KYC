using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace KYC.Web.Security;

public sealed class HttpContextAuthenticationStateProvider(IHttpContextAccessor http) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = http.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(user));
    }
}
