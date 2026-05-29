using KYC.Application.Cases;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Application.Services;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using Moq;

namespace KYC.Application.Tests;

public class StartKycCaseCommandHandlerTests
{
    private static StartKycCaseCommandHandler CreateHandler(
        Mock<IKycCaseRepository> repo,
        Mock<IEntityResolutionService> res,
        Mock<IKycCaseMessageBus> bus)
    {
        var policyRepo = new Mock<ICustomerAcceptancePolicyRepository>();
        policyRepo.Setup(p => p.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CustomerAcceptancePolicy.CreateV1("test"));
        var validator = new PolicyComplianceValidator();
        return new StartKycCaseCommandHandler(
            repo.Object, res.Object, policyRepo.Object, validator, bus.Object);
    }

    [Fact]
    public async Task Creates_case_and_publishes_bus()
    {
        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetByNifAsync("123456789", It.IsAny<CancellationToken>())).ReturnsAsync((KycCase?)null);
        repo.Setup(r => r.AddAsync(It.IsAny<KycCase>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var res = new Mock<IEntityResolutionService>();
        res.Setup(s => s.ResolveByNifAsync("123456789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EntityResolutionResult("123456789", "ACME LDA", "PT", "x", true, null));

        var bus = new Mock<IKycCaseMessageBus>();
        bus.Setup(b => b.PublishCaseStartedAsync(It.IsAny<Guid>(), "123456789", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(repo, res, bus);
        var id = await handler.Handle(new StartKycCaseCommand("123456789", "u1", CreditAmount.Eur(1000)), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        repo.Verify(r => r.AddAsync(It.IsAny<KycCase>(), It.IsAny<CancellationToken>()), Times.Once);
        bus.Verify(b => b.PublishCaseStartedAsync(id, "123456789", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Uses_trimmed_input_as_company_name_when_resolution_is_fallback()
    {
        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetByNifAsync("ACMETEST", It.IsAny<CancellationToken>())).ReturnsAsync((KycCase?)null);
        KycCase? captured = null;
        repo.Setup(r => r.AddAsync(It.IsAny<KycCase>(), It.IsAny<CancellationToken>()))
            .Callback<KycCase, CancellationToken>((k, _) => captured = k)
            .Returns(Task.CompletedTask);

        var res = new Mock<IEntityResolutionService>();
        res.Setup(s => s.ResolveByNifAsync("ACMETEST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EntityResolutionResult("ACMETEST", "Entidade ACMETEST", null, "ACMETEST", true, null, true));

        var bus = new Mock<IKycCaseMessageBus>();
        bus.Setup(b => b.PublishCaseStartedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(repo, res, bus);
        await handler.Handle(new StartKycCaseCommand("  Acme Test  ", "u1", CreditAmount.Eur(1000)), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("Acme Test", captured!.CompanyName);
        Assert.Equal("ACMETEST", captured.Nif);
    }

    [Fact]
    public async Task Throws_when_policy_auto_rejects_prohibited_sector()
    {
        var repo = new Mock<IKycCaseRepository>();
        repo.Setup(r => r.GetByNifAsync("123456789", It.IsAny<CancellationToken>())).ReturnsAsync((KycCase?)null);

        var res = new Mock<IEntityResolutionService>();
        res.Setup(s => s.ResolveByNifAsync("123456789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EntityResolutionResult("123456789", "Casino X", "PT", "x", true, null));

        var bus = new Mock<IKycCaseMessageBus>();
        var policy = CustomerAcceptancePolicy.CreateV1("test");
        var policyRepo = new Mock<ICustomerAcceptancePolicyRepository>();
        policyRepo.Setup(p => p.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(policy);

        var handler = new StartKycCaseCommandHandler(
            repo.Object, res.Object, policyRepo.Object, new PolicyComplianceValidator(), bus.Object);

        await Assert.ThrowsAsync<PolicyViolationException>(() =>
            handler.Handle(new StartKycCaseCommand("123456789", "u1", CreditAmount.Eur(1000), CaeCode: "92000"),
                CancellationToken.None));
    }
}
