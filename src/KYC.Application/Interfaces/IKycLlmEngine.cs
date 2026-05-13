using KYC.Application.Models;
using KYC.Domain.Entities;
using KYC.Domain.ValueObjects;

namespace KYC.Application.Interfaces;

public interface IKycLlmEngine
{
    Task<RiskScore> ComputeRiskScoreAsync(KycScanContext context, CancellationToken ct = default);
    Task<KycReport> GenerateNarrativeReportAsync(KycScanContext context, RiskScore score, CancellationToken ct = default);
    Task<ConsistencyCheckResult> CheckConsistencyAsync(KycScanContext context, CancellationToken ct = default);
    Task<bool> IsLlmHealthyAsync(CancellationToken ct = default);
}
