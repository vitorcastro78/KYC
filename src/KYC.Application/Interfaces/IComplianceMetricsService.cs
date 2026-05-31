using KYC.Application.Dtos;

namespace KYC.Application.Interfaces;

public interface IComplianceMetricsService
{
    Task<ComplianceMetricsBundleDto> GetMetricsAsync(CancellationToken ct = default);
}
