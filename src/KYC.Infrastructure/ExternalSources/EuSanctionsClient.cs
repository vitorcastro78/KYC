using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources;

public interface IEuSanctionsClient
{
    Task<IReadOnlyList<SanctionsListHit>> SearchAsync(string name, CancellationToken ct = default);
}

public class EuSanctionsClient(
    HttpClient http,
    EuFsfXmlLocalIndex localXml,
    IConfiguration configuration,
    ILogger<EuSanctionsClient> log) : IEuSanctionsClient
{
    public async Task<IReadOnlyList<SanctionsListHit>> SearchAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return [];

        var local = await localXml.TrySearchWhenLocalFileExistsAsync(name, ct).ConfigureAwait(false);
        if (local is not null)
            return local;

        var baseUrl = configuration["ExternalSources:EuSanctionsBaseUrl"] ?? "https://localhost/eu-sanctions/";
        if (LocalDevEndpoint.LooksLikeLocalStub(baseUrl))
        {
            log.LogDebug("EU sanctions: URL local ({BaseUrl}) — sem pedido HTTP.", baseUrl);
            return [];
        }

        try
        {
            var response = await http.GetAsync($"?name={Uri.EscapeDataString(name)}", ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return [];
            await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return [];
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "EU sanctions search failed.");
            return [];
        }
    }
}
