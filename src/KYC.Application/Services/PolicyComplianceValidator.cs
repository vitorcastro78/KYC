using System.Text.Json;
using KYC.Domain.Entities;
using KYC.Domain.Enums;

namespace KYC.Application.Services;

public record PolicyValidationResult(
    bool IsCompliant,
    bool AutoRejected,
    IReadOnlyList<string> Violations,
    string PolicyVersion);

public class PolicyComplianceValidator
{
    public PolicyValidationResult Validate(
        IReadOnlyList<CaseParty> parties,
        string? caeCode,
        CustomerAcceptancePolicy policy)
    {
        var violations = new List<string>();
        var autoRejected = false;

        var prohibited = JsonSerializer.Deserialize<List<string>>(policy.ProhibitedJurisdictionsJson) ?? [];
        var prohibitedCae = JsonSerializer.Deserialize<List<string>>(policy.ProhibitedCaeActivitiesJson) ?? [];

        var prohibitedEntities = parties
            .Where(e => prohibited.Contains(e.OffshoreJurisdiction ?? ""))
            .ToList();
        if (prohibitedEntities.Count > 0)
        {
            violations.Add($"Entidade em jurisdição sob embargo: {string.Join(", ", prohibitedEntities.Select(e => e.Name))}");
            autoRejected = true;
        }

        if (caeCode != null && prohibitedCae.Contains(caeCode))
        {
            violations.Add($"Actividade económica proibida pela PAC: CAE {caeCode}");
            autoRejected = true;
        }

        // Shell company: só após existirem partes além do tomador (grafo UBO)
        var nonTarget = parties.Where(e => e.Role != EntityRole.Target).ToList();
        if (policy.BlockShellCompanies
            && nonTarget.Count > 0
            && nonTarget.All(e => e.Type == EntityType.Company)
            && !parties.Any(e => e.Role == EntityRole.Ubo && e.Type == EntityType.Individual))
        {
            violations.Add("Estrutura sem beneficiário efectivo individual identificado (shell company)");
            autoRejected = true;
        }

        if (policy.BlockOffshoreAboveThreshold)
        {
            var offshorePct = parties.Where(e => e.IsOffshore).Sum(e => e.OwnershipPercentage);
            if (offshorePct >= policy.OffshoreBlockThreshold)
            {
                violations.Add($"Participação offshore {offshorePct}% >= limite PAC {policy.OffshoreBlockThreshold}%");
                autoRejected = true;
            }
        }

        return new PolicyValidationResult(
            violations.Count == 0,
            autoRejected,
            violations,
            policy.Version);
    }
}
