using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using KYC.Domain.Entities;
using KYC.Domain.Enums;

namespace KYC.Infrastructure.Documents;

public static class DocumentExtractionMapper
{
    private static readonly JsonSerializerOptions JsonWriteOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public static string SerializePayload(DocumentFieldExtractor.DocumentExtractionPayload payload) =>
        JsonSerializer.Serialize(payload, JsonWriteOptions);

    public static string ComputePromptHash(string prompt)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(prompt));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static (IReadOnlyList<DocumentExtractedFact> Facts, IReadOnlyList<DocumentExtractedParty> Parties)
        MapToEntities(Guid caseDocumentId, Guid kycCaseId, DocumentFieldExtractor.DocumentExtractionPayload payload)
    {
        var facts = new List<DocumentExtractedFact>();
        AddFact(facts, caseDocumentId, kycCaseId, DocumentFactKey.CompanyName, payload.CompanyName);
        AddFact(facts, caseDocumentId, kycCaseId, DocumentFactKey.Nif, payload.Nif);
        AddFact(facts, caseDocumentId, kycCaseId, DocumentFactKey.Address, payload.Address);
        AddFact(facts, caseDocumentId, kycCaseId, DocumentFactKey.Cae, payload.Cae);
        AddFact(facts, caseDocumentId, kycCaseId, DocumentFactKey.Revenue, payload.Revenue);
        AddFact(facts, caseDocumentId, kycCaseId, DocumentFactKey.Equity, payload.Equity);
        AddFact(facts, caseDocumentId, kycCaseId, DocumentFactKey.Iban, payload.Iban);
        AddFact(facts, caseDocumentId, kycCaseId, DocumentFactKey.DocumentDate, payload.DocumentDate);
        AddFact(facts, caseDocumentId, kycCaseId, DocumentFactKey.Summary, payload.Summary);

        var parties = new List<DocumentExtractedParty>();
        AddParties(parties, caseDocumentId, kycCaseId, payload.Shareholders, DocumentPartyRole.Shareholder);
        AddParties(parties, caseDocumentId, kycCaseId, payload.Ubos, DocumentPartyRole.Ubo);
        AddParties(parties, caseDocumentId, kycCaseId, payload.Directors, DocumentPartyRole.Director);

        return (facts, parties);
    }

    private static void AddFact(
        List<DocumentExtractedFact> facts,
        Guid caseDocumentId,
        Guid kycCaseId,
        DocumentFactKey key,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        facts.Add(DocumentExtractedFact.Create(caseDocumentId, kycCaseId, key, value));
    }

    private static void AddParties(
        List<DocumentExtractedParty> parties,
        Guid caseDocumentId,
        Guid kycCaseId,
        List<DocumentFieldExtractor.DocumentPartyPayload>? source,
        DocumentPartyRole role)
    {
        if (source is null)
            return;

        foreach (var item in source)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
                continue;
            parties.Add(DocumentExtractedParty.Create(
                caseDocumentId,
                kycCaseId,
                item.Name,
                item.Nif,
                role,
                item.Percentage,
                item.Nationality));
        }
    }
}
