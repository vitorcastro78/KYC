using System.Text.RegularExpressions;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;

namespace KYC.Application.Services;

public sealed partial class RcbePartyVerificationService(IRcbeClient rcbe) : IRcbePartyVerificationService
{
    public async Task VerifyCasePartiesAsync(KycCase kycCase, CancellationToken ct = default)
    {
        foreach (var party in kycCase.Parties.Where(p =>
                     p.Role == EntityRole.Target && !string.IsNullOrWhiteSpace(p.Nif)))
        {
            var rcbeHit = await rcbe.GetCompanyByNifAsync(party.Nif!, ct);
            if (rcbeHit is null)
                continue;

            var discrepancy = HasDiscrepancy(rcbeHit.LegalName, party.Name);
            party.RecordRcbeVerification(discrepancy);

            if (!discrepancy)
                continue;

            kycCase.AddRiskSignal(RiskSignal.Create(
                kycCase.Id,
                party.Id,
                SignalType.Inconsistency,
                SignalSeverity.Medium,
                $"Discrepância RCBE: declarado «{party.Name}», registo «{rcbeHit.LegalName}»",
                "RCBE"));
            kycCase.AppendAudit(AuditEntry.Create(
                kycCase.Id,
                "RcbeDiscrepancyDetected",
                "System",
                "Agent",
                $"{party.Nif}: {party.Name} vs {rcbeHit.LegalName}"));
        }
    }

    public static bool HasDiscrepancy(string rcbeName, string declaredName)
    {
        var a = NormalizeName(rcbeName);
        var b = NormalizeName(declaredName);
        if (a.Length == 0 || b.Length == 0)
            return false;
        if (a == b)
            return false;
        return !a.Contains(b, StringComparison.Ordinal) && !b.Contains(a, StringComparison.Ordinal);
    }

    internal static string NormalizeName(string name) =>
        NonAlnumRegex().Replace(name.ToUpperInvariant(), "");

    [GeneratedRegex(@"[^A-Z0-9]", RegexOptions.CultureInvariant)]
    private static partial Regex NonAlnumRegex();
}
