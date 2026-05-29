using KYC.Application.Interfaces;
using MediatR;

namespace KYC.Application.Cases;

public class ApproveKycCaseCommandHandler(
    IKycCaseRepository repository,
    ICustomerAcceptancePolicyRepository policyRepo) : IRequestHandler<ApproveKycCaseCommand, Unit>
{
    public async Task<Unit> Handle(ApproveKycCaseCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");
        kyc.Approve(request.AnalystId, request.SecondApproverId);
        var policy = await policyRepo.GetActiveAsync(cancellationToken);
        if (policy is not null)
            kyc.ScheduleNextReview(policy);
        await repository.UpdateAsync(kyc, cancellationToken);
        return Unit.Value;
    }
}

public class RejectKycCaseCommandHandler(IKycCaseRepository repository)
    : IRequestHandler<RejectKycCaseCommand, Unit>
{
    public async Task<Unit> Handle(RejectKycCaseCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");
        kyc.Reject(request.AnalystId, request.Reason);
        await repository.UpdateAsync(kyc, cancellationToken);
        return Unit.Value;
    }
}

public class RequestManualReviewCommandHandler(IKycCaseRepository repository)
    : IRequestHandler<RequestManualReviewCommand, Unit>
{
    public async Task<Unit> Handle(RequestManualReviewCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");
        kyc.RequestManualReview(request.Reason);
        await repository.UpdateAsync(kyc, cancellationToken);
        return Unit.Value;
    }
}

public class SetFundsOriginCommandHandler(IKycCaseRepository repository)
    : IRequestHandler<SetFundsOriginCommand, Unit>
{
    public async Task<Unit> Handle(SetFundsOriginCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");
        kyc.SetFundsOrigin(request.Description, request.Verified, request.DocumentId);
        await repository.UpdateAsync(kyc, cancellationToken);
        return Unit.Value;
    }
}

public class EscalateToSupervisorCommandHandler(IKycCaseRepository repository)
    : IRequestHandler<EscalateToSupervisorCommand, Unit>
{
    public async Task<Unit> Handle(EscalateToSupervisorCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");
        kyc.RequestManualReview($"Escalado ao supervisor: {request.Reason}");
        await repository.UpdateAsync(kyc, cancellationToken);
        return Unit.Value;
    }
}

public class TriggerPeriodicReviewCommandHandler(
    IKycCaseRepository repository,
    IKycCaseMessageBus messageBus) : IRequestHandler<TriggerPeriodicReviewCommand, Unit>
{
    public async Task<Unit> Handle(TriggerPeriodicReviewCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");
        kyc.PrepareForAutomaticRescreen(request.InitiatedBy);
        kyc.RecordPeriodicReviewCompleted(request.InitiatedBy);
        await repository.UpdateAsync(kyc, cancellationToken);
        await messageBus.PublishCaseRescreenAsync(request.CaseId, request.InitiatedBy, cancellationToken);
        return Unit.Value;
    }
}
