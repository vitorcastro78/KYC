using KYC.Application.Cases;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using Moq;

namespace KYC.Application.Tests;

public class CreateCustomerAcceptancePolicyTests
{
    [Fact]
    public async Task Create_successor_deactivates_previous_and_activates_new_version()
    {
        var current = CustomerAcceptancePolicy.CreateV1("seed");
        var repo = new Mock<ICustomerAcceptancePolicyRepository>();
        repo.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(current);
        CustomerAcceptancePolicy? added = null;
        repo.Setup(r => r.AddAsync(It.IsAny<CustomerAcceptancePolicy>(), It.IsAny<CancellationToken>()))
            .Callback<CustomerAcceptancePolicy, CancellationToken>((p, _) =>
            {
                current.Deactivate();
                added = p;
            })
            .Returns(Task.CompletedTask);

        var handler = new CreateCustomerAcceptancePolicyCommandHandler(repo.Object);
        await handler.Handle(new CreateCustomerAcceptancePolicyCommand("2.0.0", "admin"), CancellationToken.None);

        Assert.False(current.IsActive);
        Assert.NotNull(added);
        Assert.Equal("2.0.0", added!.Version);
        Assert.True(added.IsActive);
        Assert.Equal(current.EnhancedDdThreshold, added.EnhancedDdThreshold);
    }
}
