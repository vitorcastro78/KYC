using KYC.Application.Interfaces;
using KYC.Application.Services;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using MediatR;

namespace KYC.Application.Cases;

public class StartKycCaseCommandHandler(
    IKycCaseRepository repository,
    IEntityResolutionService resolution,
    ICustomerAcceptancePolicyRepository policyRepository,
    PolicyComplianceValidator policyValidator,
    IKycCaseMessageBus messageBus) : IRequestHandler<StartKycCaseCommand, Guid>
{
    public async Task<Guid> Handle(StartKycCaseCommand request, CancellationToken cancellationToken)
    {
        if (!Common.NifSanitizer.TryNormalizeCaseKey(request.Nif, out var nif))
            throw new ArgumentException("Identificador comercial inválido.");

        var existing = await repository.GetByNifAsync(nif, cancellationToken);
        if (existing is { Status: KycStatus.Pending or KycStatus.InProgress or KycStatus.UnderReview })
            throw new InvalidOperationException("Já existe um caso activo para este identificador ou nome.");

        var policy = await policyRepository.GetActiveAsync(cancellationToken)
                     ?? CustomerAcceptancePolicy.CreateV1("System");

        var resolved = await resolution.ResolveByNifAsync(nif, cancellationToken);
        var companyName = resolved.UsedFallback ? request.Nif.Trim() : resolved.LegalName;
        if (!resolved.Success || string.IsNullOrWhiteSpace(companyName))
            throw new InvalidOperationException(resolved.ErrorMessage ?? "Não foi possível resolver a entidade.");

        var kyc = KycCase.Start(nif, companyName, request.RequestedBy, request.RequestedAmount, request.RelationshipType);
        kyc.SetLegalBasisRef(policy.ResolveLegalBasisRef());
        kyc.MarkInProgress();

        var target = CaseParty.Create(
            kyc.Id,
            EntityType.Company,
            companyName,
            nif,
            EntityRole.Target,
            ownershipPercentage: 100,
            uboDepthLevel: 0,
            parentPartyId: null);
        kyc.AddParty(target);

        var policyResult = policyValidator.Validate(kyc.Parties.ToList(), request.CaeCode, policy);
        if (policyResult.AutoRejected || !policyResult.IsCompliant)
            throw new PolicyViolationException(policyResult.Violations);

        await repository.AddAsync(kyc, cancellationToken);
        await messageBus.PublishCaseStartedAsync(kyc.Id, nif, cancellationToken);
        return kyc.Id;
    }
}

public class PolicyViolationException(IReadOnlyList<string> violations)
    : Exception($"Violação da PAC: {string.Join("; ", violations)}")
{
    public IReadOnlyList<string> Violations { get; } = violations;
}
