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

    public IdentityVerificationMethod VerificationMethod { get; private set; } = IdentityVerificationMethod.NotYetVerified;
    public DateTime? VerifiedAt { get; private set; }
    public string? VerificationSessionId { get; private set; }
    public IdentityVerificationStatus VerificationStatus { get; private set; } = IdentityVerificationStatus.Pending;
    public DateTime? RcbeVerifiedAt { get; private set; }
    public bool RcbeDiscrepancyDetected { get; private set; }
    public bool RcbeDiscrepancyReported { get; private set; }
    public DateTime? RcbeDiscrepancyReportedAt { get; private set; }
    public string DataCollectionBasis { get; private set; } = "Lei83/2017-Art24-n1";

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

    public void StartVerification(IdentityVerificationMethod method, string sessionId)
    {
        VerificationMethod = method;
        VerificationSessionId = sessionId;
        VerificationStatus = IdentityVerificationStatus.Pending;
    }

    public void RecordVerificationResult(bool verified, IdentityVerificationMethod method)
    {
        VerificationMethod = method;
        VerificationStatus = verified ? IdentityVerificationStatus.Verified : IdentityVerificationStatus.Failed;
        VerifiedAt = verified ? DateTime.UtcNow : null;
    }

    public void RecordPresentialVerification(string documentReference)
    {
        VerificationMethod = IdentityVerificationMethod.Presential;
        VerificationStatus = IdentityVerificationStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        VerificationSessionId = documentReference;
    }

    public void RecordRcbeVerification(bool discrepancyDetected)
    {
        RcbeVerifiedAt = DateTime.UtcNow;
        RcbeDiscrepancyDetected = discrepancyDetected;
    }

    public void ReportRcbeDiscrepancy()
    {
        RcbeDiscrepancyReported = true;
        RcbeDiscrepancyReportedAt = DateTime.UtcNow;
    }
}
