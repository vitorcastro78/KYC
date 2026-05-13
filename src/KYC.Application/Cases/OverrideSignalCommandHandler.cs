using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using MediatR;

namespace KYC.Application.Cases;

public class OverrideSignalCommandHandler(IKycCaseRepository repository)
    : IRequestHandler<OverrideSignalCommand, Unit>
{
    public async Task<Unit> Handle(OverrideSignalCommand request, CancellationToken cancellationToken)
    {
        var result = await repository.GetCaseWithSignalAsync(request.SignalId, cancellationToken);
        if (result is null)
            throw new KeyNotFoundException("Sinal não encontrado.");

        var (kyc, signal) = result.Value;
        signal.OverrideConfirmation(request.Confirm, request.Notes);
        kyc.AppendAudit(AuditEntry.Create(
            kyc.Id,
            "AnalystOverride",
            request.AnalystId,
            "User",
            $"Signal {request.SignalId} confirm={request.Confirm}"));
        await repository.UpdateAsync(kyc, cancellationToken);
        return Unit.Value;
    }
}
