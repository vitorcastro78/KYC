namespace KYC.Infrastructure.ExternalSources;

/// <summary>Detecta URLs típicas de dev sem serviço real (evita chamadas HTTP inúteis).</summary>
internal static class LocalDevEndpoint
{
    internal static bool LooksLikeLocalStub(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
        return url.Contains("localhost", StringComparison.OrdinalIgnoreCase)
               || url.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);
    }
}
