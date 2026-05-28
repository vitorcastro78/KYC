using KYC.Domain.Enums;

namespace KYC.Domain.Entities;

public class DocumentExtractedParty
{
    public Guid Id { get; private set; }
    public Guid CaseDocumentId { get; private set; }
    public Guid KycCaseId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Nif { get; private set; }
    public DocumentPartyRole Role { get; private set; }
    public decimal? OwnershipPercentage { get; private set; }
    public string? Nationality { get; private set; }
    public DateTime ExtractedAt { get; private set; }

    private DocumentExtractedParty()
    {
    }

    public static DocumentExtractedParty Create(
        Guid caseDocumentId,
        Guid kycCaseId,
        string name,
        string? nif,
        DocumentPartyRole role,
        decimal? ownershipPercentage = null,
        string? nationality = null)
    {
        return new DocumentExtractedParty
        {
            Id = Guid.NewGuid(),
            CaseDocumentId = caseDocumentId,
            KycCaseId = kycCaseId,
            Name = name.Trim(),
            Nif = string.IsNullOrWhiteSpace(nif) ? null : nif.Trim(),
            Role = role,
            OwnershipPercentage = ownershipPercentage,
            Nationality = string.IsNullOrWhiteSpace(nationality) ? null : nationality.Trim(),
            ExtractedAt = DateTime.UtcNow
        };
    }
}
