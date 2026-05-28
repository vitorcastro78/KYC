using System.Net;
using KYC.Application.Interfaces;

using KYC.Infrastructure.BackgroundJobs;
using KYC.Infrastructure.ExternalSources;
using KYC.Infrastructure.LLM;
using KYC.Infrastructure.Messaging;
using KYC.Infrastructure.Pdf;
using KYC.Infrastructure.Persistence;
using KYC.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace KYC.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("KycDatabase")
                 ?? configuration["KYC_DB_CONNECTION"]
                 ?? throw new InvalidOperationException("Connection string KycDatabase or KYC_DB_CONNECTION required.");

        services.AddDbContext<KycDbContext>(options =>
            options.UseNpgsql(cs, npgsql => npgsql.UseVector()));

        services.AddScoped<IKycCaseRepository, KycCaseRepository>();
        services.AddScoped<IKycAnalyticsRepository, KycAnalyticsRepository>();
        services.AddScoped<IKycCaseScanProgressRepository, KycCaseScanProgressRepository>();
        services.AddScoped<IAuditReadRepository, AuditReadRepository>();
        services.AddScoped<IEntityResolutionService, EntityResolutionService>();
        services.AddScoped<ISanctionsScreeningService, SanctionsScreeningService>();
        services.AddScoped<IAdverseMediaService, AdverseMediaService>();
        services.AddScoped<IFinancialHealthService, FinancialHealthService>();
        services.AddScoped<IJudicialIntelligenceService, JudicialIntelligenceService>();
        services.AddScoped<IIcijOffshoreService, IcijOffshoreService>();
        services.AddScoped<IKycLlmEngine, KycLlmEngine>();
        services.AddSingleton<IKycReportComposer, KycStructuredReportComposer>();
        services.AddScoped<IReportEmbeddingWriter, ReportEmbeddingWriter>();
        services.AddScoped<IKycCasePipelineRunner, KycCasePipelineRunner>();
        services.AddScoped<ICasePartyScreener, CasePartyScreener>();
        services.AddSingleton<IKycHtmlToPdfConverter, PuppeteerKycHtmlToPdfConverter>();
        services.AddScoped<IKycReportPdfGenerator, KycReportPdfGenerator>();

        RegisterMessaging(services, configuration);

        // http + porta explÃ­cita: https://localhost/rcbe/ usa 443 e falha sem reverse proxy local.
        var rcbeBase = configuration["ExternalSources:RcbeBaseUrl"] ?? "http://localhost:5055/rcbe/";
        var rcbeBuilder = services.AddHttpClient<IRcbeClient, RcbeClient>((_, c) =>
        {
            c.BaseAddress = new Uri(rcbeBase);
        }).AddPolicyHandler(GetRetryPolicy());
        if (!LocalDevEndpoint.LooksLikeLocalStub(rcbeBase))
            rcbeBuilder.AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddHttpClient<IGleifClient, GleifClient>((sp, c) =>
        {
            var baseUrl = configuration["ExternalSources:GleifBaseUrl"] ?? "https://api.gleif.org/api/v1/";
            c.BaseAddress = new Uri(baseUrl);
            c.Timeout = TimeSpan.FromSeconds(
                Math.Clamp(configuration.GetValue("ExternalSources:Gleif:TimeoutSeconds", 45), 10, 120));
            c.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.api+json");
        }).AddPolicyHandler(GetRetryPolicy()).AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddHttpClient<IWikidataCompanyClient, WikidataCompanyClient>((sp, c) =>
        {
            var baseUrl = configuration["ExternalSources:WikidataApiBaseUrl"] ?? "https://www.wikidata.org/";
            c.BaseAddress = new Uri(baseUrl);
            c.Timeout = TimeSpan.FromSeconds(45);
            var ua = configuration["ExternalSources:WikidataUserAgent"]
                     ?? "KYC/1.0 (https://example.com/contact; entity-resolution)";
            c.DefaultRequestHeaders.UserAgent.ParseAdd(ua);
        }).AddPolicyHandler(GetRetryPolicy()).AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddSingleton<OfacSdnXmlLocalIndex>();
        services.AddSingleton<EuFsfXmlLocalIndex>();

        var ofacBase = configuration["ExternalSources:OfacBaseUrl"] ?? "http://localhost:5056/ofac/";
        var ofacBuilder = services.AddHttpClient<IOfacClient, OfacClient>((_, c) =>
        {
            c.BaseAddress = new Uri(ofacBase);
        }).AddPolicyHandler(GetRetryPolicy());
        if (!LocalDevEndpoint.LooksLikeLocalStub(ofacBase))
            ofacBuilder.AddPolicyHandler(GetCircuitBreakerPolicy());

        var euSanctionsBase = configuration["ExternalSources:EuSanctionsBaseUrl"] ?? "http://localhost:5057/eu-sanctions/";
        var euBuilder = services.AddHttpClient<IEuSanctionsClient, EuSanctionsClient>((_, c) =>
        {
            c.BaseAddress = new Uri(euSanctionsBase);
        }).AddPolicyHandler(GetRetryPolicy());
        if (!LocalDevEndpoint.LooksLikeLocalStub(euSanctionsBase))
            euBuilder.AddPolicyHandler(GetCircuitBreakerPolicy());

        var openSanctionsBase = configuration["ExternalSources:OpenSanctions:BaseUrl"] ?? "https://api.opensanctions.org/";
        var openSanctionsTimeout = Math.Clamp(configuration.GetValue("ExternalSources:OpenSanctions:TimeoutSeconds", 60), 10, 300);
        services.AddTransient<OpenSanctionsApiKeyHandler>();
        var openSanctionsBuilder = services.AddHttpClient<IOpenSanctionsClient, OpenSanctionsClient>((_, c) =>
            {
                c.BaseAddress = new Uri(openSanctionsBase);
                c.Timeout = TimeSpan.FromSeconds(openSanctionsTimeout);
                c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .AddHttpMessageHandler<OpenSanctionsApiKeyHandler>()
            .AddPolicyHandler(GetRetryPolicy());
        openSanctionsBuilder.AddPolicyHandler(GetCircuitBreakerPolicy());

        var ollama = configuration["LLM:LocalEndpoint"] ?? "http://localhost:11434";
        var ollamaTimeoutSeconds = Math.Clamp(configuration.GetValue("LLM:RequestTimeoutSeconds", 300), 30, 3600);
        services.AddHttpClient("ollama", c =>
        {
            c.BaseAddress = new Uri(ollama);
            c.Timeout = TimeSpan.FromSeconds(ollamaTimeoutSeconds);
        }).AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient("anthropic", c =>
        {
            c.BaseAddress = new Uri("https://api.anthropic.com/");
            c.Timeout = TimeSpan.FromSeconds(120);
        }).AddPolicyHandler(GetRetryPolicy());

        var newsBase = configuration["NewsApi:BaseUrl"] ?? "https://newsapi.org/";
        var newsUserAgent = configuration["NewsApi:UserAgent"]
                            ?? "KYC/1.0 (+https://github.com/; adverse-media)";
        services.AddHttpClient("newsapi", c =>
        {
            c.BaseAddress = new Uri(newsBase);
            c.Timeout = TimeSpan.FromSeconds(30);
            c.DefaultRequestHeaders.UserAgent.ParseAdd(newsUserAgent);
        }).AddPolicyHandler(GetRetryPolicy());

        if (configuration.GetValue("DataRetention:EnableHostedService", false))
            services.AddHostedService<DataRetentionHostedService>();

        return services;
    }

    private static void RegisterMessaging(IServiceCollection services, IConfiguration configuration)
    {
        var provider = (configuration["Messaging:Provider"] ?? "InMemory").Trim();
        var sbCs = configuration["KYC_SERVICEBUS_CONNECTION"] ?? configuration["ServiceBus:ConnectionString"];
        var useAzure = string.Equals(provider, "AzureServiceBus", StringComparison.OrdinalIgnoreCase)
                       && !string.IsNullOrWhiteSpace(sbCs);

        if (string.Equals(provider, "AzureServiceBus", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(sbCs))
        {
            throw new InvalidOperationException(
                "Messaging:Provider estÃ¡ definido como AzureServiceBus mas falta KYC_SERVICEBUS_CONNECTION ou ServiceBus:ConnectionString.");
        }

        if (useAzure)
        {
            services.AddSingleton<IKycCaseMessageBus, AzureServiceBusKycCaseMessageBus>();
            return;
        }

        services.AddSingleton<InMemoryCaseStartedQueue>();
        services.AddSingleton<IKycCaseMessageBus, InMemoryKycCaseMessageBus>();
        if (configuration.GetValue("Messaging:HostInMemoryPipeline", true))
            services.AddHostedService<CaseStartedPipelineHostedService>();
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * attempt));

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

}

