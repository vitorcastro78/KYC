using KYC.Application.Interfaces;
using MediatR;

namespace KYC.Application.Cases;

public class ApproveKycCaseCommandHandler(IKycCaseRepository repository)
    : IRequestHandler<ApproveKycCaseCommand, Unit>
{
    public async Task<Unit> Handle(ApproveKycCaseCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");
        kyc.Approve(request.AnalystId);
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
