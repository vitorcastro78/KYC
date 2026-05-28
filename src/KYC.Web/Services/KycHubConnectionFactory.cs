using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace KYC.Web.Services;

/// <summary>
/// Cria ligações SignalR client autenticadas no Blazor Server (reencaminha cookies da sessão).
/// </summary>
public sealed class KycHubConnectionFactory(
    IHttpContextAccessor httpContextAccessor,
    NavigationManager navigation)
{
    public HubConnection Create()
    {
        var hubUri = navigation.ToAbsoluteUri("/hubs/kyc-case");
        return new HubConnectionBuilder()
            .WithUrl(hubUri, options =>
            {
                options.HttpMessageHandlerFactory = _ => CreateHandlerWithRequestCookies(hubUri);
            })
            .WithAutomaticReconnect()
            .Build();
    }

    private HttpClientHandler CreateHandlerWithRequestCookies(Uri hubUri)
    {
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };

        var cookieHeader = httpContextAccessor.HttpContext?.Request.Headers.Cookie.ToString();
        if (string.IsNullOrWhiteSpace(cookieHeader))
            return handler;

        foreach (var segment in cookieHeader.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = segment.IndexOf('=');
            if (separator <= 0)
                continue;

            var name = segment[..separator].Trim();
            var value = segment[(separator + 1)..].Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            try
            {
                handler.CookieContainer.Add(hubUri, new Cookie(name, value));
            }
            catch (CookieException)
            {
                // ignorar cookies inválidos para o container
            }
        }

        return handler;
    }
}
