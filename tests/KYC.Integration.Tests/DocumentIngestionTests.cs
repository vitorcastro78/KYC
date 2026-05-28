using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using KYC.Infrastructure.Documents;

namespace KYC.Integration.Tests;

public class DocumentExtractionMapperTests
{
    [Fact]
    public void MapToEntities_persists_facts_and_parties()
    {
        var docId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var payload = new DocumentFieldExtractor.DocumentExtractionPayload
        {
            CompanyName = "Empresa Teste Lda",
            Nif = "123456789",
            Shareholders =
            [
                new DocumentFieldExtractor.DocumentPartyPayload { Name = "João Silva", Nif = "987654321", Percentage = 50 }
            ],
            Ubos =
            [
                new DocumentFieldExtractor.DocumentPartyPayload { Name = "Maria Costa", Percentage = 25 }
            ]
        };

        var (facts, parties) = DocumentExtractionMapper.MapToEntities(docId, caseId, payload);

        Assert.Contains(facts, f => f.FactKey == DocumentFactKey.Nif && f.FactValue == "123456789");
        Assert.Contains(facts, f => f.FactKey == DocumentFactKey.CompanyName);
        Assert.Equal(2, parties.Count);
        Assert.Contains(parties, p => p.Role == DocumentPartyRole.Shareholder);
        Assert.Contains(parties, p => p.Role == DocumentPartyRole.Ubo);
    }

    [Fact]
    public void SerializePayload_uses_utf8_without_unicode_escapes()
    {
        var payload = new DocumentFieldExtractor.DocumentExtractionPayload
        {
            CompanyName = "Açores & Companhia",
            Summary = "Morada em Lisboa"
        };

        var json = DocumentExtractionMapper.SerializePayload(payload);
        Assert.Contains("Açores", json);
        Assert.DoesNotContain("\\u00", json);
    }
}

public class DocumentConsistencyTests
{
    [Fact]
    public void Check_emits_high_severity_when_nif_differs()
    {
        var kyc = KycCase.Start("111111111", "Empresa A", "tester", new CreditAmount(1000, "EUR"));
        var doc = CaseDocument.Create(
            kyc.Id,
            "certidao.pdf",
            "application/pdf",
            100,
            "abc",
            "path/certidao.pdf",
            CaseDocumentKind.CommercialRegistry,
            "tester");
        doc.MarkCompleted("texto", "{}", "test", "hash");
        doc.ReplaceExtractedData(
            [DocumentExtractedFact.Create(doc.Id, kyc.Id, DocumentFactKey.Nif, "999999999")],
            []);
        kyc.AddDocument(doc, "tester");

        var checker = new DocumentConsistencyChecker();
        var signals = checker.Check(kyc);

        Assert.Contains(signals, s =>
            s.Type == SignalType.Inconsistency &&
            s.Severity == SignalSeverity.High &&
            s.Source.StartsWith("Document:"));
    }
}

public class DocumentTextExtractorTests
{
    [Fact]
    public void ExtractFromText_finds_portuguese_nif()
    {
        var extractor = new DocumentFieldExtractor();
        var payload = extractor.ExtractFromText("Contribuinte NIF 256789012 sede em Lisboa");
        Assert.Equal("256789012", payload.Nif);
    }
}
