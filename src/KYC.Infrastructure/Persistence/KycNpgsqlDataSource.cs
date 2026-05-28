using Npgsql;

namespace KYC.Infrastructure.Persistence;

internal static class KycNpgsqlDataSource
{
    internal static NpgsqlDataSource Create(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseVector();
        return builder.Build();
    }
}
