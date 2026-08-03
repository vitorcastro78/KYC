using System.Net;
using KYC.Application.Interfaces;

using KYC.Infrastructure.BackgroundJobs;
using KYC.Application.Services;
using KYC.Infrastructure.Compliance;
using KYC.Infrastructure.Compliance.AssetFreeze;
using KYC.Infrastructure.Compliance.Uif;
using KYC.Infrastructure.Documents;
using KYC.Infrastructure.ExternalSources;
using KYC.Infrastructure.Health;
using KYC.Infrastructure.Identity;
using KYC.Infrastructure.ExternalSources.At;
using KYC.Infrastructure.LLM;
using KYC.Infrastructure.Messaging;
using KYC.Infrastructure.Pdf;
using KYC.Infrastructure.Persistence;
using KYC.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
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

        services.AddSingleton(_ => KycNpgsqlDataSource.Create(cs));
        services.AddSingleton<RegulatoryVersionSaveChangesInterceptor>();
        services.AddDbContext<KycDbContext>((sp, options) =>
            options
                .UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>())
                .AddInterceptors(sp.GetRequiredService<RegulatoryVersionSaveChangesInterceptor>()));

        services.AddScoped<IKycCaseRepository, KycCaseRepository>();
        services.AddScoped<IKycAnalyticsRepository, KycAnalyticsRepository>();
        services.AddScoped<IKycCaseScanProgressRepository, KycCaseScanProgressRepository>();
        services.AddScoped<IAuditReadRepository, AuditReadRepository>();
        services.AddScoped<IEntityResolutionService, EntityResolutionService>();
        services.AddScoped<ISanctionsScreeningService, SanctionsScreeningService>();
        services.AddScoped<IAdverseMediaService, AdverseMediaService>();
        services.AddSingleton<IAtDebtorsLocalIndex, AtDebtorsLocalIndex>();
        services.AddScoped<IFinancialHealthService, FinancialHealthService>();
        services.AddScoped<ICitiusClient, CitiusClient>();
        services.AddScoped<IJudicialIntelligenceService, JudicialIntelligenceService>();
        services.AddScoped<IIcijOffshoreService, IcijOffshoreService>();
        services.AddScoped<IKycLlmEngine, KycLlmEngine>();
        services.AddSingleton<IKycReportComposer, KycStructuredReportComposer>();
        services.AddScoped<IReportEmbeddingWriter, ReportEmbeddingWriter>();
        services.AddScoped<ILlmModelCatalog, LlmModelCatalog>();
        services.AddHttpClient<IContextMemoryWikiClient, ContextMemoryWikiClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ContextMemoryOptions>>().Value;
            var baseUrl = string.IsNullOrWhiteSpace(opts.BaseUrl)
                ? "http://localhost:5100"
                : opts.BaseUrl.TrimEnd('/');
            client.BaseAddress = new Uri(baseUrl + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });
        services.AddScoped<IKycCasePipelineRunner, KycCasePipelineRunner>();
        services.AddScoped<ICasePartyScreener, CasePartyScreener>();
        services.AddSingleton<IKycHtmlToPdfConverter, PuppeteerKycHtmlToPdfConverter>();
        services.AddScoped<IKycReportPdfGenerator, KycReportPdfGenerator>();

        services.AddScoped<ICustomerAcceptancePolicyRepository, CustomerAcceptancePolicyRepository>();
        services.AddScoped<IScoringEngineConfigRepository, ScoringEngineConfigRepository>();
        services.AddScoped<IDpiaRecordRepository, DpiaRecordRepository>();
        services.AddScoped<IAmlComplianceReportRepository, AmlComplianceReportRepository>();
        services.AddScoped<IAmlComplianceReportService, AmlComplianceReportService>();
        services.AddSingleton<IBdpRpbExporter, BdpRpbExporter>();
        services.AddScoped<IRcbePartyVerificationService, RcbePartyVerificationService>();
        services.AddScoped<IPeriodicReviewScheduler, PeriodicReviewScheduler>();
        services.AddScoped<IIdentityVerificationService, DigitalSignIdentityVerificationService>();
        services.AddScoped<IUifReportingService, UifReportingService>();
        services.AddScoped<IAssetFreezeNotificationService, AssetFreezeNotificationService>();
        services.AddScoped<IComplianceMetricsService, ComplianceMetricsService>();
        services.Configure<DataRetentionOptions>(configuration.GetSection(DataRetentionOptions.SectionName));

        services.AddKycHealthChecks(configuration);
        var disableBackground = configuration.GetValue("Testing:DisableBackgroundServices", false);
        if (!disableBackground)
        {
            services.AddHostedService<ComplianceSeedHostedService>();
            if (configuration.GetValue("Compliance:EnablePeriodicReviewScheduler", true))
                services.AddHostedService<PeriodicReviewSchedulerJob>();
            if (configuration.GetValue("IdentityVerification:EnablePolling", true))
            {
                services.AddScoped<IdentityVerificationPollingService>();
                services.AddHostedService<IdentityVerificationPollingHostedService>();
            }
        }

        services.AddSingleton<SarSubmissionQueue>();
        services.AddSingleton<ISarSubmissionQueue>(sp => sp.GetRequiredService<SarSubmissionQueue>());
        if (!disableBackground)
            services.AddHostedService<SarSubmissionHostedService>();

        services.AddSingleton<ICaseDocumentStorage, LocalCaseDocumentStorage>();
        services.AddSingleton<IDpiaDocumentStorage, LocalDpiaDocumentStorage>();
        services.AddScoped<ICaseDocumentRepository, CaseDocumentRepository>();
        services.AddSingleton<DocumentIngestionQueue>();
        services.AddSingleton<IDocumentIngestionQueue>(sp => sp.GetRequiredService<DocumentIngestionQueue>());
        services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
        services.AddSingleton<DocumentVisionExtractor>();
        services.AddSingleton<DocumentFieldExtractor>();
        services.AddScoped<IDocumentConsistencyChecker, DocumentConsistencyChecker>();
        if (!disableBackground)
            services.AddHostedService<DocumentIngestionHostedService>();

        RegisterMessaging(services, configuration, disableBackground);

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

        var ofacSlsBase = OfacSlsOptions.GetBaseUrl(configuration).TrimEnd('/') + "/";
        var ofacUserAgent = OfacSlsOptions.GetUserAgent(configuration);
        services.AddHttpClient("ofac-sls", c =>
        {
            c.BaseAddress = new Uri(ofacSlsBase);
            c.Timeout = TimeSpan.FromSeconds(30);
            c.DefaultRequestHeaders.UserAgent.ParseAdd(ofacUserAgent);
        }).AddPolicyHandler(GetRetryPolicy());

        services.RemoveAll<IOfacClient>();
        services.AddScoped<IOfacClient>(static sp => new OfacClient(
            sp.GetRequiredService<OfacSdnXmlLocalIndex>(),
            sp.GetRequiredService<ILogger<OfacClient>>()));

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

        var cmOptions = configuration.GetSection(ContextMemoryOptions.SectionName).Get<ContextMemoryOptions>()
            ?? new ContextMemoryOptions();
        services.Configure<ContextMemoryOptions>(configuration.GetSection(ContextMemoryOptions.SectionName));
        services.AddTransient<ContextMemoryAuthHandler>();

        var localLlm = configuration["LLM:LocalEndpoint"] ?? "http://localhost:11434";
        var llmBase = cmOptions.IsConfigured ? cmOptions.BaseUrl.TrimEnd('/') : localLlm.TrimEnd('/');
        var ollamaTimeoutSeconds = Math.Clamp(configuration.GetValue("LLM:RequestTimeoutSeconds", 300), 30, 3600);
        var scoringTimeoutSeconds = Math.Clamp(configuration.GetValue("LLM:ScoringTimeoutSeconds", 45), 5, 300);

        void ConfigureLlmClient(HttpClient c, int timeoutSeconds)
        {
            c.BaseAddress = new Uri(llmBase.TrimEnd('/') + "/");
            c.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        }

        var ollamaBuilder = services.AddHttpClient("ollama", c => ConfigureLlmClient(c, ollamaTimeoutSeconds))
            .AddPolicyHandler(GetRetryPolicy());
        var scoringBuilder = services.AddHttpClient("ollama-scoring", c => ConfigureLlmClient(c, scoringTimeoutSeconds));
        var healthBuilder = services.AddHttpClient("ollama-health", c =>
        {
            ConfigureLlmClient(c, 5);
        });

        if (cmOptions.IsConfigured)
        {
            ollamaBuilder.AddHttpMessageHandler<ContextMemoryAuthHandler>();
            scoringBuilder.AddHttpMessageHandler<ContextMemoryAuthHandler>();
            healthBuilder.AddHttpMessageHandler<ContextMemoryAuthHandler>();
        }

        var newsBase = configuration["NewsApi:BaseUrl"] ?? "https://newsapi.org/";
        var newsUserAgent = configuration["NewsApi:UserAgent"]
                            ?? "KYC/1.0 (+https://github.com/; adverse-media)";
        services.AddHttpClient("newsapi", c =>
        {
            c.BaseAddress = new Uri(newsBase);
            c.Timeout = TimeSpan.FromSeconds(30);
            c.DefaultRequestHeaders.UserAgent.ParseAdd(newsUserAgent);
        }).AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient("identity-verification", c => c.Timeout = TimeSpan.FromSeconds(60))
            .AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient("uif", c => c.Timeout = TimeSpan.FromSeconds(120))
            .AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient("bdp-freeze", c => c.Timeout = TimeSpan.FromSeconds(60))
            .AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient("citius", c => c.Timeout = TimeSpan.FromSeconds(45))
            .AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient("icij", c => c.Timeout = TimeSpan.FromSeconds(45))
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHostedService<DataRetentionHostedService>();

        return services;
    }

    private static void RegisterMessaging(IServiceCollection services, IConfiguration configuration, bool disableBackground = false)
    {
        var provider = (configuration["Messaging:Provider"] ?? "InMemory").Trim();
        var sbCs = configuration["KYC_SERVICEBUS_CONNECTION"] ?? configuration["ServiceBus:ConnectionString"];
        var rabbitCs = configuration["KYC_RABBITMQ_CONNECTION"] ?? configuration["RabbitMq:ConnectionString"];
        var useAzure = string.Equals(provider, "AzureServiceBus", StringComparison.OrdinalIgnoreCase)
                       && !string.IsNullOrWhiteSpace(sbCs);
        var useRabbit = string.Equals(provider, "RabbitMq", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(rabbitCs);

        if (string.Equals(provider, "AzureServiceBus", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(sbCs))
        {
            throw new InvalidOperationException(
                "Messaging:Provider está definido como AzureServiceBus mas falta KYC_SERVICEBUS_CONNECTION ou ServiceBus:ConnectionString.");
        }

        if (string.Equals(provider, "RabbitMq", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(rabbitCs))
        {
            throw new InvalidOperationException(
                "Messaging:Provider está definido como RabbitMq mas falta KYC_RABBITMQ_CONNECTION ou RabbitMq:ConnectionString.");
        }

        if (useAzure)
        {
            services.AddSingleton<IKycCaseMessageBus, AzureServiceBusKycCaseMessageBus>();
            return;
        }

        if (useRabbit)
        {
            services.AddSingleton<IKycCaseMessageBus, RabbitMqKycCaseMessageBus>();
            services.AddHostedService<RabbitMqKycCaseConsumerHostedService>();
            return;
        }

        services.AddSingleton<InMemoryCaseStartedQueue>();
        services.AddSingleton<IKycCaseMessageBus, InMemoryKycCaseMessageBus>();
        if (!disableBackground && configuration.GetValue("Messaging:HostInMemoryPipeline", true))
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

