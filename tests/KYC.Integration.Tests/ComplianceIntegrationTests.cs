using KYC.Application.Security;
using KYC.Application.Services;
using KYC.Domain.Entities;

namespace KYC.Integration.Tests;

/// <summary>Testes de integração leves (sem BD) para fluxos regulatórios.</summary>
public class ComplianceIntegrationTests
{
    [Fact]
    public void Webhook_hmac_roundtrip_matches_validator()
    {
        const string body = "{\"partyId\":\"00000000-0000-0000-0000-000000000001\",\"sessionId\":\"s1\",\"verified\":true}";
        const string secret = "integration-secret";
        var hash = Convert.ToHexString(
                System.Security.Cryptography.HMACSHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(secret),
                    System.Text.Encoding.UTF8.GetBytes(body)))
            .ToLowerInvariant();

        Assert.True(WebhookSignatureValidator.Validate(body, $"sha256={hash}", secret));
    }

    [Fact]
    public void Due_diligence_evaluator_simplified_for_occasional_low_amount()
    {
        var eval = new DueDiligenceLevelEvaluator();
        var decision = eval.Evaluate(1000m, Domain.Enums.RelationshipType.Occasional, [],
            CustomerAcceptancePolicy.CreateV1("t"));
        Assert.Equal(Domain.Enums.DueDiligenceLevel.Simplified, decision.Level);
    }
}
