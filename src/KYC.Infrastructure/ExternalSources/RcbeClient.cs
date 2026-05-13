using System.Net.Http.Json;
using System.Net.Sockets;

namespace KYC.Infrastructure.ExternalSources;

public interface IRcbeClient
{
    Task<RcbeCompanyDto?> GetCompanyByNifAsync(string nif, CancellationToken ct = default);
}

public record RcbeCompanyDto(string Nif, string LegalName, string? RegistryId);

public class RcbeClient(HttpClient http, ILogger<RcbeClient> log) : IRcbeClient
{
    public async Task<RcbeCompanyDto?> GetCompanyByNifAsync(string nif, CancellationToken ct = default)
    {
        try
        {
            var response = await http.GetAsync($"companies/{nif}", ct);
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<RcbeCompanyDto>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            if (IsLikelyLocalDevConnectionRefused(ex))
                log.LogDebug(ex, "RCBE indisponível em {BaseUrl} (esperado sem stub local).", http.BaseAddress);
            else
                log.LogWarning(ex, "RCBE lookup failed; falling back to placeholder.");
            return null;
        }
    }

    private static bool IsLikelyLocalDevConnectionRefused(Exception ex)
    {
        for (var e = ex; e is not null; e = e.InnerException)
        {
            if (e is SocketException se && se.SocketErrorCode == SocketError.ConnectionRefused)
                return true;
        }

        return false;
    }
}
