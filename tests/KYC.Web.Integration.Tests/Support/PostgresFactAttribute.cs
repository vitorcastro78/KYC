namespace KYC.Web.Integration.Tests.Support;

/// <summary>Ignora o teste quando KYC_DB_CONNECTION não está definida.</summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("KYC_DB_CONNECTION")))
            Skip = "Defina KYC_DB_CONNECTION para executar testes PostgreSQL.";
    }
}
