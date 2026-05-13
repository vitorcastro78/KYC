using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources;

public interface IOfacClient
{
    Task<IReadOnlyList<SanctionsListHit>> SearchAsync(string name, CancellationToken ct = default);
}

public record SanctionsListHit(string ListName, string MatchedName, double Score, string? Details);

public class OfacClient(
    HttpClient http,
    OfacSdnXmlLocalIndex localXml,
    IConfiguration configuration,
    ILogger<OfacClient> log) : IOfacClient
{
    public async Task<IReadOnlyList<SanctionsListHit>> SearchAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return [];

        var local = await localXml.TrySearchWhenLocalFileExistsAsync(name, ct).ConfigureAwait(false);
        if (local is not null)
            return local;

        var ofacBase = configuration["ExternalSources:OfacBaseUrl"] ?? "https://localhost/ofac/";
        if (LocalDevEndpoint.LooksLikeLocalStub(ofacBase))
        {
            log.LogDebug("OFAC HTTP: URL local ({BaseUrl}) — sem pedido HTTP (use SDN XML local ou um stub).", ofacBase);
            return [];
        }

        try
        {
            var response = await http.GetAsync($"search?name={Uri.EscapeDataString(name)}", ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return [];
            await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return [];
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "OFAC search failed.");
            return [];
        }
    }
}
