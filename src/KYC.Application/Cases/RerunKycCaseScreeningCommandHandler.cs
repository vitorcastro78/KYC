using KYC.Application.Interfaces;
using KYC.Domain.Enums;
using MediatR;

namespace KYC.Application.Cases;

public class RerunKycCaseScreeningCommandHandler(
    IKycCaseRepository repository,
    IKycCaseMessageBus messageBus) : IRequestHandler<RerunKycCaseScreeningCommand, Unit>
{
    public async Task<Unit> Handle(RerunKycCaseScreeningCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");

        if (kyc.Status is KycStatus.Pending or KycStatus.Rejected)
        {
            throw new InvalidOperationException(
                "A re-triagem automática só está disponível para casos em análise ou já concluídos (aprovados).");
        }

        await messageBus.PublishCaseRescreenAsync(request.CaseId, request.ActorId, cancellationToken);
        return Unit.Value;
    }
}
