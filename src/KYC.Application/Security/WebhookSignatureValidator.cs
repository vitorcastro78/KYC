using System.Security.Cryptography;
using System.Text;

namespace KYC.Application.Security;

/// <summary>Validação HMAC-SHA256 para webhooks (header X-Webhook-Signature: sha256=&lt;hex&gt;).</summary>
public static class WebhookSignatureValidator
{
    public static bool Validate(string payload, string? signatureHeader, string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            return true;

        if (string.IsNullOrWhiteSpace(signatureHeader))
            return false;

        var provided = signatureHeader.Trim();
        if (provided.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
            provided = provided["sha256=".Length..];

        var expected = Convert.ToHexString(
                HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload)))
            .ToLowerInvariant();

        return provided.Length == expected.Length
               && CryptographicOperations.FixedTimeEquals(
                   Encoding.UTF8.GetBytes(provided.ToLowerInvariant()),
                   Encoding.UTF8.GetBytes(expected));
    }
}
