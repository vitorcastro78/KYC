using System.Text.Json;
using KYC.Domain.Entities;

namespace KYC.Application.Services;

public record DueDiligenceLevelDecision(Domain.Enums.DueDiligenceLevel Level, string Justification);

public class DueDiligenceLevelEvaluator
{
    public DueDiligenceLevelDecision Evaluate(
        decimal? creditAmount,
        Domain.Enums.RelationshipType relationshipType,
        IReadOnlyList<CaseParty> parties,
        CustomerAcceptancePolicy policy)
    {
        var highRisk = JsonSerializer.Deserialize<List<string>>(policy.HighRiskJurisdictionsJson) ?? [];

        if (relationshipType == Domain.Enums.RelationshipType.Occasional
            && creditAmount.GetValueOrDefault() < policy.OccasionalThreshold
            && !parties.Any(e => e.IsPep)
            && !parties.Any(e => e.IsOffshore))
        {
            return new(Domain.Enums.DueDiligenceLevel.Simplified,
                "Transação ocasional abaixo de limiar sem factores de risco (Art. 33.º Lei 83/2017)");
        }

        var eddReasons = new List<string>();

        if (parties.Any(e => e.IsPep))
            eddReasons.Add("Presença de PEP (Art. 36.º Lei 83/2017)");

        if (parties.Any(e => e.IsOffshore && highRisk.Contains(e.OffshoreJurisdiction ?? "")))
            eddReasons.Add("Jurisdição de alto risco FATF/UE");

        if (!parties.Any(e => e.VerificationMethod == Domain.Enums.IdentityVerificationMethod.Presential)
            && parties.Any(e => e.Role is Domain.Enums.EntityRole.Ubo or Domain.Enums.EntityRole.BoardMember))
            eddReasons.Add("Estabelecimento sem presença física (Art. 36.º, n.º 1, al. b))");

        if (creditAmount.GetValueOrDefault() >= policy.EnhancedDdThreshold)
            eddReasons.Add($"Montante >= {policy.EnhancedDdThreshold:C} EUR");

        if (eddReasons.Count > 0)
            return new(Domain.Enums.DueDiligenceLevel.Enhanced, string.Join("; ", eddReasons));

        return new(Domain.Enums.DueDiligenceLevel.Standard, "Sem factores de escalation identificados");
    }
}
