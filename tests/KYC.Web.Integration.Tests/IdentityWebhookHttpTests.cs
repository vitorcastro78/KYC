using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KYC.Domain.Enums;
using KYC.Web.Integration.Tests.Support;

namespace KYC.Web.Integration.Tests;

public class IdentityWebhookHttpTests : IClassFixture<KycWebApplicationFactory>
{
    private readonly KycWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public IdentityWebhookHttpTests(KycWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.SeedVerificationParty();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Webhook_rejects_missing_signature_when_secret_configured()
    {
        var body = BuildPayload(_factory.TestPartyId, _factory.TestSessionId, verified: true);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/identity/webhook", content);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Webhook_accepts_valid_hmac_and_verifies_party()
    {
        var body = BuildPayload(_factory.TestPartyId, _factory.TestSessionId, verified: true);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/identity/webhook")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Webhook-Signature", KycWebApplicationFactory.Sign(body, KycWebApplicationFactory.WebhookSecret));

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var match = await _factory.Repository.GetCaseWithPartyAsync(_factory.TestPartyId);
        Assert.NotNull(match);
        Assert.Equal(IdentityVerificationStatus.Verified, match!.Value.Party.VerificationStatus);
    }

    [Fact]
    public async Task Webhook_verification_unblocks_can_approve_for_standard_dd()
    {
        var before = await _factory.Repository.GetCaseWithPartyAsync(_factory.TestPartyId);
        Assert.NotNull(before);
        Assert.False(before!.Value.Case.CanApprove().IsSuccess);

        var body = BuildPayload(_factory.TestPartyId, _factory.TestSessionId, verified: true);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/identity/webhook")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Webhook-Signature", KycWebApplicationFactory.Sign(body, KycWebApplicationFactory.WebhookSecret));
        Assert.Equal(HttpStatusCode.OK, (await _client.SendAsync(request)).StatusCode);

        var after = await _factory.Repository.GetCaseWithPartyAsync(_factory.TestPartyId);
        Assert.NotNull(after);
        Assert.Equal(IdentityVerificationStatus.Verified, after!.Value.Party.VerificationStatus);
        Assert.True(after.Value.Case.CanApprove().IsSuccess);
    }

    [Fact]
    public async Task Webhook_returns_bad_request_for_invalid_json()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/identity/webhook")
        {
            Content = new StringContent("{not-json", Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Webhook-Signature", KycWebApplicationFactory.Sign("{not-json", KycWebApplicationFactory.WebhookSecret));

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string BuildPayload(Guid partyId, string sessionId, bool verified) =>
        JsonSerializer.Serialize(new
        {
            partyId,
            sessionId,
            verified,
            failureReason = (string?)null,
            eidasLevel = "High"
        });
}
