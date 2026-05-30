using KYC.Application.Common;
using KYC.Application.Dtos;
using KYC.Application.Filtering;
using MediatR;

namespace KYC.Application.Cases;

public record GetKycCaseQuery(Guid CaseId) : IRequest<KycCaseDetailDto?>;

public record ListKycCasesQuery(KycCaseFilter Filter) : IRequest<PagedResult<KycCaseDto>>;

public record GetUboGraphQuery(Guid CaseId) : IRequest<UboGraphViewDto?>;

public record GetRiskTimelineQuery(Guid CaseId) : IRequest<RiskTimelineDto?>;

public record GetKycReportQuery(Guid CaseId) : IRequest<KycReportDto?>;

public record GetKycCaseScanProgressQuery(Guid CaseId) : IRequest<KycCaseScanProgressDto?>;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public record GetCriticalAlertsQuery : IRequest<IReadOnlyList<CriticalAlertDto>>;

public record ListAmlComplianceReportsQuery : IRequest<IReadOnlyList<AmlComplianceReportListItemDto>>;

public record GetAmlComplianceReportQuery(Guid ReportId) : IRequest<AmlComplianceReportDetailDto?>;

public record GetUifSubmissionStatusQuery(string ReferenceNumber) : IRequest<UifSubmissionStatusDto>;
