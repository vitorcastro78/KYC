using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Domain.Entities;

/// <summary>Empresa ou pessoa no grafo KYC (tomador, accionista, UBO, etc.).</summary>
public class CaseParty
{
    public Guid Id { get; private set; }
    public Guid KycCaseId { get; private set; }
    public EntityType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Nif { get; private set; }
    public string? Nationality { get; private set; }
    public EntityRole Role { get; private set; }
    public decimal OwnershipPercentage { get; private set; }
    public int UboDepthLevel { get; private set; }
    public Guid? ParentPartyId { get; private set; }
    public bool IsPep { get; private set; }
    public bool IsSanctioned { get; private set; }
    public bool IsOffshore { get; private set; }
    public string? OffshoreJurisdiction { get; private set; }
    public RiskScore? PartyScore { get; private set; }

    private CaseParty()
    {
    }

    public static CaseParty Create(
        Guid kycCaseId,
        EntityType type,
        string name,
        string? nif,
        EntityRole role,
        decimal ownershipPercentage,
        int uboDepthLevel,
        Guid? parentPartyId,
        string? nationality = null)
    {
        return new CaseParty
        {
            Id = Guid.NewGuid(),
            KycCaseId = kycCaseId,
            Type = type,
            Name = name,
            Nif = nif,
            Nationality = nationality,
            Role = role,
            OwnershipPercentage = ownershipPercentage,
            UboDepthLevel = uboDepthLevel,
            ParentPartyId = parentPartyId,
            IsPep = false,
            IsSanctioned = false,
            IsOffshore = false
        };
    }

    public void SetFlags(bool isPep, bool isSanctioned, bool isOffshore, string? offshoreJurisdiction)
    {
        IsPep = isPep;
        IsSanctioned = isSanctioned;
        IsOffshore = isOffshore;
        OffshoreJurisdiction = offshoreJurisdiction;
    }

    public void SetPartyScore(RiskScore? score) => PartyScore = score;
}
