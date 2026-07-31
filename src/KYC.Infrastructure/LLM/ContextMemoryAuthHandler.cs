using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace KYC.Infrastructure.LLM;

/// <summary>Injects ContextMemory auth headers on outbound LLM requests.</summary>
public sealed class ContextMemoryAuthHandler(IOptions<ContextMemoryOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var opts = options.Value;
        if (opts.IsConfigured)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", opts.ApiKey.Trim());
            request.Headers.TryAddWithoutValidation("X-App-Id", opts.AppId.Trim());
            var userId = string.IsNullOrWhiteSpace(opts.UserId) ? "kyc-jobs" : opts.UserId.Trim();
            request.Headers.TryAddWithoutValidation("X-User-Id", userId);
            request.Headers.TryAddWithoutValidation("X-Session-Id", Guid.NewGuid().ToString("N"));
        }

        return base.SendAsync(request, cancellationToken);
    }
}
