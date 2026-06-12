using System.Globalization;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;

namespace KYC.Infrastructure.Documents;

public sealed class DocumentConsistencyChecker : IDocumentConsistencyChecker
{
    public IReadOnlyList<RiskSignal> Check(KycCase kycCase)
    {
        var signals = new List<RiskSignal>();
        var completedDocs = kycCase.Documents
            .Where(d => d.IngestionStatus == DocumentIngestionStatus.Completed)
            .ToList();

        foreach (var doc in completedDocs)
        {
            var source = $"Document:{doc.Id}";
            CheckNif(kycCase, doc, source, signals);
            CheckCompanyName(kycCase, doc, source, signals);
            CheckUbos(kycCase, doc, source, signals);
        }

        return signals;
    }

    private static void CheckNif(KycCase kycCase, CaseDocument doc, string source, List<RiskSignal> signals)
    {
        var docNif = doc.ExtractedFacts
            .FirstOrDefault(f => f.FactKey == DocumentFactKey.Nif)?.FactValue;
        if (string.IsNullOrWhiteSpace(docNif) || string.IsNullOrWhiteSpace(kycCase.Nif))
            return;

        var normalizedDoc = NormalizeDigits(docNif);
        var normalizedCase = NormalizeDigits(kycCase.Nif);
        if (normalizedDoc.Length == 9 && normalizedCase.Length == 9 &&
            !string.Equals(normalizedDoc, normalizedCase, StringComparison.Ordinal))
        {
            signals.Add(RiskSignal.Create(
                kycCase.Id,
                null,
                SignalType.Inconsistency,
                SignalSeverity.High,
                $"NIF no documento ({docNif}) difere do NIF do caso ({kycCase.Nif}).",
                source));
        }
    }

    private static void CheckCompanyName(KycCase kycCase, CaseDocument doc, string source, List<RiskSignal> signals)
    {
        var docName = doc.ExtractedFacts
            .FirstOrDefault(f => f.FactKey == DocumentFactKey.CompanyName)?.FactValue;
        if (string.IsNullOrWhiteSpace(docName))
            return;

        if (!NamesSimilar(docName, kycCase.CompanyName))
        {
            signals.Add(RiskSignal.Create(
                kycCase.Id,
                null,
                SignalType.Inconsistency,
                SignalSeverity.Medium,
                $"Denominação no documento ({docName}) difere da entidade do caso ({kycCase.CompanyName}).",
                source));
        }
    }

    private static void CheckUbos(KycCase kycCase, CaseDocument doc, string source, List<RiskSignal> signals)
    {
        var declaredUbos = doc.ExtractedParties
            .Where(p => p.Role == DocumentPartyRole.Ubo)
            .ToList();
        if (declaredUbos.Count == 0)
            return;

        var gleifUbos = kycCase.Parties
            .Where(p => p.Role is EntityRole.Ubo or EntityRole.Shareholder)
            .ToList();

        foreach (var declared in declaredUbos)
        {
            var match = gleifUbos.FirstOrDefault(g =>
                (!string.IsNullOrWhiteSpace(declared.Nif) &&
                 string.Equals(NormalizeDigits(declared.Nif), NormalizeDigits(g.Nif ?? string.Empty), StringComparison.Ordinal)) ||
                NamesSimilar(declared.Name, g.Name));

            if (match is null)
            {
                signals.Add(RiskSignal.Create(
                    kycCase.Id,
                    null,
                    SignalType.Inconsistency,
                    SignalSeverity.Medium,
                    $"UBO declarado no documento ({declared.Name}) não encontrado no grafo GLEIF/partes.",
                    source));
            }
        }
    }

    private static string NormalizeDigits(string value) =>
        new string(value.Where(char.IsDigit).ToArray());

    private static bool NamesSimilar(string a, string b)
    {
        var na = NormalizeName(a);
        var nb = NormalizeName(b);
        if (na.Length == 0 || nb.Length == 0)
            return false;
        return na.Contains(nb, StringComparison.Ordinal) ||
               nb.Contains(na, StringComparison.Ordinal) ||
               string.Equals(na, nb, StringComparison.Ordinal);
    }

    private static string NormalizeName(string name) =>
        new string(name.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
}
