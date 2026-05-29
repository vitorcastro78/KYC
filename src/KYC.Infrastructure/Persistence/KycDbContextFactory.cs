using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace KYC.Infrastructure.Persistence;

public class KycDbContextFactory : IDesignTimeDbContextFactory<KycDbContext>
{
    public KycDbContext CreateDbContext(string[] args)
    {
        // EF CLI usa esta factory (não o appsettings do startup). Alinhar com bootstrap-dev-db.ps1.
        var cs = Environment.GetEnvironmentVariable("KYC_DB_CONNECTION")
                 ?? Environment.GetEnvironmentVariable("ConnectionStrings__KycDatabase")
                 ?? "Host=localhost;Port=5433;Database=kyc_dev;Username=postgres;Password=dev123";
        var dataSource = KycNpgsqlDataSource.Create(cs);
        var opts = new DbContextOptionsBuilder<KycDbContext>()
            .UseNpgsql(dataSource, x => x.UseVector())
            .Options;
        return new KycDbContext(opts);
    }
}
