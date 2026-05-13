using KYC.Application.Dtos;

namespace KYC.Application.Interfaces;

public interface IKycAnalyticsRepository
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CriticalAlertDto>> GetCriticalAlertsLast24hAsync(CancellationToken ct = default);
}
