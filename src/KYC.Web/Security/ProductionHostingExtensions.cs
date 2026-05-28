using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

namespace KYC.Web.Security;

public static class ProductionHostingExtensions
{
    public static bool IsBehindReverseProxy(this IConfiguration configuration, IHostEnvironment environment) =>
        configuration.GetValue("Hosting:BehindReverseProxy", !environment.IsDevelopment());

    public static IServiceCollection AddKycProductionHosting(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string contentRootPath)
    {
        if (!configuration.IsBehindReverseProxy(environment))
            return services;

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        var keysPath = configuration["DataProtection:KeysPath"];
        if (string.IsNullOrWhiteSpace(keysPath))
            keysPath = Path.Combine(contentRootPath, "DataProtection-Keys");

        Directory.CreateDirectory(keysPath);
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
            .SetApplicationName("KYC.Web");

        return services;
    }

    public static WebApplication UseKycProductionHosting(this WebApplication app)
    {
        if (!app.Configuration.IsBehindReverseProxy(app.Environment))
            return app;

        app.UseForwardedHeaders();
        return app;
    }
}
