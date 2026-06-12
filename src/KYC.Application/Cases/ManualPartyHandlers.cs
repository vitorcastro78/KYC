using KYC.Application.Common;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using MediatR;

namespace KYC.Application.Cases;

public class AddManualCasePartyCommandHandler(
    IKycCaseRepository repository,
    ICasePartyScreener partyScreener) : IRequestHandler<AddManualCasePartyCommand, Guid>
{
    public async Task<Guid> Handle(AddManualCasePartyCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");

        string? normalizedNif = null;
        if (!string.IsNullOrWhiteSpace(request.Nif) && NifSanitizer.TryNormalize(request.Nif, out var n))
            normalizedNif = n;

        if (request.ParentPartyId is { } pid && kyc.Parties.All(p => p.Id != pid))
            throw new InvalidOperationException("A parte superior (ParentPartyId) não pertence a este caso.");

        var party = CaseParty.Create(
            kyc.Id,
            request.Type,
            request.Name.Trim(),
            normalizedNif,
            request.Role,
            request.OwnershipPercentage,
            request.UboDepthLevel,
            request.ParentPartyId,
            request.Nationality?.Trim());

        kyc.AddManualParty(party, request.ActorId, null);
        await repository.UpdateAsync(kyc, cancellationToken);

        if (request.RunScreeningAfterAdd)
        {
            await partyScreener.AppendScreeningSignalsAsync(
                request.CaseId,
                party.Id,
                request.ActorId,
                cancellationToken);
        }

        return party.Id;
    }
}

public class ScreenCasePartyCommandHandler(ICasePartyScreener partyScreener)
    : IRequestHandler<ScreenCasePartyCommand, int>
{
    public Task<int> Handle(ScreenCasePartyCommand request, CancellationToken cancellationToken) =>
        partyScreener.AppendScreeningSignalsAsync(
            request.CaseId,
            request.PartyId,
            request.ActorId,
            cancellationToken);
}
