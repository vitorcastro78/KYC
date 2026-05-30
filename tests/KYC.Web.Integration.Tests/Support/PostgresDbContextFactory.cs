using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KYC.Web.Integration.Tests.Support;

internal static class PostgresDbContextFactory
{
    public static async Task<KycDbContext> CreateAsync(CancellationToken ct = default)
    {
        var ctx = Create();
        await ctx.Database.MigrateAsync(ct);
        return ctx;
    }

    public static KycDbContext Create()
    {
        var cs = Environment.GetEnvironmentVariable("KYC_DB_CONNECTION")
                 ?? throw new InvalidOperationException("KYC_DB_CONNECTION required.");

        var options = new DbContextOptionsBuilder<KycDbContext>()
            .UseNpgsql(cs, npgsql => npgsql.UseVector())
            .AddInterceptors(new RegulatoryVersionSaveChangesInterceptor())
            .Options;

        return new KycDbContext(options);
    }
}
