using System.Security.Cryptography;
using System.Text;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Infrastructure.Messaging;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KYC.Web.Integration.Tests.Support;

public sealed class KycWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string WebhookSecret = "integration-webhook-secret";
    public TestKycCaseRepository Repository { get; } = new();

    public Guid TestPartyId { get; private set; }
    public string TestSessionId { get; } = "integration-sess-001";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAd:Enabled"] = "false",
                ["Testing:DisableBackgroundServices"] = "true",
                ["Messaging:HostInMemoryPipeline"] = "false",
                ["ConnectionStrings:KycDatabase"] = "Host=127.0.0.1;Port=5432;Database=kyc_test;Username=postgres;Password=test",
                ["IdentityVerification:WebhookSecret"] = WebhookSecret
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IKycCaseRepository>();
            services.AddSingleton<IKycCaseRepository>(Repository);
            services.RemoveAll<IKycCaseRealtimeNotifier>();
            services.AddSingleton<IKycCaseRealtimeNotifier, NoOpKycCaseRealtimeNotifier>();
        });
    }

    public void SeedVerificationParty()
    {
        var kyc = KycCase.Start("123456789", "Acme Integration", "test", CreditAmount.Eur(10000));
        kyc.MarkInProgress();
        var party = CaseParty.Create(kyc.Id, EntityType.Individual, "UBO Test", "987654321",
            EntityRole.Ubo, 50, 1, null);
        party.StartVerification(IdentityVerificationMethod.VideoConference, TestSessionId);
        kyc.AddParty(party);
        TestPartyId = party.Id;
        Repository.Seed(kyc, party, TestSessionId);
    }

    public static string Sign(string body, string secret)
    {
        var hash = Convert.ToHexString(
                HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body)))
            .ToLowerInvariant();
        return $"sha256={hash}";
    }
}
