using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace KYC.Infrastructure.Persistence;

public class KycDbContextFactory : IDesignTimeDbContextFactory<KycDbContext>
{
    public KycDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("KYC_DB_CONNECTION")
                 ?? "Host=localhost;Database=kyc_dev;Username=postgres;Password=dev123";
        var dataSource = KycNpgsqlDataSource.Create(cs);
        var opts = new DbContextOptionsBuilder<KycDbContext>()
            .UseNpgsql(dataSource, x => x.UseVector())
            .Options;
        return new KycDbContext(opts);
    }
}
