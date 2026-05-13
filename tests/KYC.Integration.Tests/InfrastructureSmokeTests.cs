using KYC.Infrastructure.Persistence;

namespace KYC.Integration.Tests;

public class InfrastructureSmokeTests
{
    [Fact]
    public void KycDbContext_type_loads() => Assert.NotNull(typeof(KycDbContext));
}
