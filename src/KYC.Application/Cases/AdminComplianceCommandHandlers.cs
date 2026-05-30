using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using MediatR;

namespace KYC.Application.Cases;

public class CreateScoringEngineConfigCommandHandler(IScoringEngineConfigRepository repo)
    : IRequestHandler<CreateScoringEngineConfigCommand, Guid>
{
    public async Task<Guid> Handle(CreateScoringEngineConfigCommand request, CancellationToken cancellationToken)
    {
        var config = ScoringEngineConfig.CreateVersion(
            request.Version,
            request.LocalModelName,
            request.SystemPromptHash,
            request.ApprovedBy);
        await repo.AddAsync(config, cancellationToken);
        return config.Id;
    }
}

public class CreateDpiaRecordCommandHandler(IDpiaRecordRepository repo)
    : IRequestHandler<CreateDpiaRecordCommand, Guid>
{
    public async Task<Guid> Handle(CreateDpiaRecordCommand request, CancellationToken cancellationToken)
    {
        var record = DpiaRecord.Create(request.Version, request.ApprovedBy, request.DocumentPath);
        await repo.AddAsync(record, cancellationToken);
        return record.Id;
    }
}

public class CreateCustomerAcceptancePolicyCommandHandler(ICustomerAcceptancePolicyRepository repo)
    : IRequestHandler<CreateCustomerAcceptancePolicyCommand, Guid>
{
    public async Task<Guid> Handle(CreateCustomerAcceptancePolicyCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version))
            throw new ArgumentException("Versão da PAC é obrigatória.");

        var current = await repo.GetActiveAsync(cancellationToken);
        var source = current ?? CustomerAcceptancePolicy.CreateV1(request.ApprovedBy);
        var policy = CustomerAcceptancePolicy.CreateSuccessor(request.Version, request.ApprovedBy, source);

        await repo.AddAsync(policy, cancellationToken);
        return policy.Id;
    }
}

public class SubmitAmlReportToBdpCommandHandler(IAmlComplianceReportService service)
    : IRequestHandler<SubmitAmlReportToBdpCommand, string>
{
    public Task<string> Handle(SubmitAmlReportToBdpCommand request, CancellationToken cancellationToken) =>
        service.SubmitToBdpAsync(request.ReportId, request.SubmittedBy, cancellationToken);
}
