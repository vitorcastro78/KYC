using KYC.Application.Dtos;
using KYC.Application.Interfaces;
using KYC.Domain.Enums;
using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KYC.Infrastructure.Compliance;

public sealed class ComplianceMetricsService(KycDbContext db) : IComplianceMetricsService
{
    public async Task<ComplianceMetricsBundleDto> GetMetricsAsync(CancellationToken ct = default)
    {
        var signals = await db.RiskSignals.AsNoTracking().ToListAsync(ct);
        var parties = await db.CaseParties.AsNoTracking()
            .Where(p => p.VerificationStatus != IdentityVerificationStatus.Pending
                        || p.VerifiedAt != null)
            .ToListAsync(ct);

        return new ComplianceMetricsBundleDto(
            BuildScreening(signals),
            BuildBiometric(parties),
            DateTime.UtcNow);
    }

    private static ScreeningMetricsDto BuildScreening(IReadOnlyList<Domain.Entities.RiskSignal> signals)
    {
        var sanction = signals.Where(s => s.Type == SignalType.Sanction).ToList();
        var confirmed = sanction.Count(s => s.IsConfirmed);
        var dismissed = sanction.Count(s => !s.IsConfirmed);
        var highUnconfirmed = signals.Count(s => s.Severity >= SignalSeverity.High && !s.IsConfirmed);

        var totalScreened = Math.Max(1, signals.Count);
        var fpRate = Math.Round((decimal)dismissed / totalScreened * 100, 2);
        var fnEstimate = Math.Round((decimal)highUnconfirmed / totalScreened * 100, 2);

        return new ScreeningMetricsDto(
            signals.Count,
            sanction.Count,
            confirmed,
            dismissed,
            highUnconfirmed,
            fpRate,
            fnEstimate,
            "FP: correspondências sanções não confirmadas pelo analista. FN (estimativa): sinais High+ não confirmados — requer validação periódica com amostra manual.");
    }

    private static BiometricMetricsDto BuildBiometric(IReadOnlyList<Domain.Entities.CaseParty> parties)
    {
        var attempts = parties.Where(p =>
            p.VerificationStatus is IdentityVerificationStatus.Verified or IdentityVerificationStatus.Failed).ToList();
        var verified = attempts.Count(p => p.VerificationStatus == IdentityVerificationStatus.Verified);
        var failed = attempts.Count(p => p.VerificationStatus == IdentityVerificationStatus.Failed);
        var withLiveness = attempts.Where(p => !string.IsNullOrWhiteSpace(p.LivenessScore)).ToList();

        decimal? avgLiveness = null;
        var parsed = withLiveness
            .Select(p => decimal.TryParse(p.LivenessScore, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? (decimal?)v : null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();
        if (parsed.Count > 0)
            avgLiveness = Math.Round(parsed.Average(), 4);

        var total = Math.Max(1, attempts.Count);
        var frr = Math.Round((decimal)failed / total * 100, 2);
        var far = 0m;

        return new BiometricMetricsDto(
            attempts.Count,
            verified,
            failed,
            withLiveness.Count,
            avgLiveness,
            far,
            frr,
            "Liveness conforme prestador certificado ISO/IEC 30107-3; FAR oficial requer relatório de laboratório do prestador.",
            "FRR operacional: falhas de verificação / total tentativas. FAR=0 até integração de relatório certificado do prestador.");
    }
}
