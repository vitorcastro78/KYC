using Microsoft.OpenApi.Models;

namespace KYC.Web.OpenApi;

public static class KycOpenApiExtensions
{
    public static IServiceCollection AddKycOpenApiDocumentation(this IServiceCollection services) =>
        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "KYC AI Platform API",
                    Version = "v1",
                    Description =
                        "APIs REST da plataforma KYC: documentos, relatórios, webhook identidade, métricas compliance e export RPB. " +
                        "Autenticação: Bearer OIDC (Entra) ou cookie Identity (dev)."
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Token JWT Entra ID (produção).",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

    public static WebApplication UseKycOpenApiDocumentation(this WebApplication app)
    {
        var enabled = app.Configuration.GetValue("OpenApi:Enable", app.Environment.IsDevelopment());
        if (!enabled)
            return app;

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "KYC Platform API v1");
            c.DocumentTitle = "KYC API";
        });
        return app;
    }
}
