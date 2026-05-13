namespace KYC.Application.Interfaces;

/// <summary>Triagens externas (sanções, media, etc.) para uma parte concreta do caso.</summary>
public interface ICasePartyScreener
{
    Task AppendScreeningSignalsAsync(Guid caseId, Guid partyId, string actorId, CancellationToken ct = default);
}
