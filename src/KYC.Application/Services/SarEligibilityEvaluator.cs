using KYC.Domain.Entities;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Application.Services;

public class SarEligibilityEvaluator
{
    public bool ShouldSuggestSar(KycCase kycCase) =>
        (kycCase.Score?.Level >= RiskLevel.High
         && kycCase.RiskSignals.Any(s =>
             s.Severity == SignalSeverity.Critical && !s.IsConfirmed))
        || kycCase.Parties.Any(e => e.IsSanctioned)
        || kycCase.Parties.Any(e => e.IsOffshore && e.OwnershipPercentage >= 25m);
}
