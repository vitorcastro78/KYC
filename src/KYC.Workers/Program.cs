using KYC.Application;
using KYC.Application.Interfaces;
using KYC.Infrastructure;
using KYC.Infrastructure.Messaging;
using KYC.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IKycCaseRealtimeNotifier, NoOpKycCaseRealtimeNotifier>();

builder.Services.Configure<OfacSdnDailyDownloadOptions>(
    builder.Configuration.GetSection(OfacSdnDailyDownloadOptions.SectionKey));
var ofacUserAgent = KYC.Infrastructure.ExternalSources.OfacSlsOptions.GetUserAgent(builder.Configuration);
builder.Services.AddHttpClient("ofac-sdn-export", client =>
{
    client.Timeout = TimeSpan.FromMinutes(20);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(ofacUserAgent);
});
builder.Services.AddHostedService<OfacSdnDailyDownloadHostedService>();

builder.Services.Configure<EuFsfXmlDailyDownloadOptions>(
    builder.Configuration.GetSection(EuFsfXmlDailyDownloadOptions.SectionKey));
builder.Services.AddHttpClient("eu-fsf-export", client =>
{
    client.Timeout = TimeSpan.FromMinutes(25);
    client.DefaultRequestHeaders.TryAddWithoutValidation(
        "User-Agent",
        "KYC-Workers/1.0 (+https://example.com; EU FSF XML daily sync)");
});
builder.Services.AddHostedService<EuFsfXmlDailyDownloadHostedService>();

builder.Services.Configure<AtDebtorsDailyDownloadOptions>(
    builder.Configuration.GetSection(AtDebtorsDailyDownloadOptions.SectionKey));
builder.Services.AddHttpClient("at-devedores-export", client =>
{
    client.Timeout = TimeSpan.FromMinutes(30);
    client.DefaultRequestHeaders.TryAddWithoutValidation(
        "User-Agent",
        "KYC-Workers/1.0 (+https://example.com; AT devedores daily sync)");
});
builder.Services.AddHostedService<AtDebtorsDailyDownloadHostedService>();

var provider = (builder.Configuration["Messaging:Provider"] ?? "InMemory").Trim();
var sbCs = builder.Configuration["KYC_SERVICEBUS_CONNECTION"] ?? builder.Configuration["ServiceBus:ConnectionString"];
if (string.Equals(provider, "AzureServiceBus", StringComparison.OrdinalIgnoreCase)
    && !string.IsNullOrWhiteSpace(sbCs))
{
    builder.Services.AddHostedService<KycServiceBusWorker>();
}

var host = builder.Build();
await host.RunAsync();
