using System.Security.Cryptography;
using System.Text;
using KYC.Application.Security;

namespace KYC.Application.Tests;

public class WebhookSignatureValidatorTests
{
    [Fact]
    public void Accepts_when_secret_not_configured()
    {
        Assert.True(WebhookSignatureValidator.Validate("{}", null, null));
        Assert.True(WebhookSignatureValidator.Validate("{}", "", "   "));
    }

    [Fact]
    public void Rejects_invalid_signature()
    {
        Assert.False(WebhookSignatureValidator.Validate("{}", "sha256=deadbeef", "my-secret"));
    }

    [Fact]
    public void Accepts_valid_hmac()
    {
        const string payload = "{\"partyId\":\"abc\"}";
        const string secret = "test-secret";
        var hash = Convert.ToHexString(
                HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload)))
            .ToLowerInvariant();

        Assert.True(WebhookSignatureValidator.Validate(payload, $"sha256={hash}", secret));
    }
}
