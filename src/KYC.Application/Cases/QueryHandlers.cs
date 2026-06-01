using KYC.Application.Common;
using KYC.Application.Dtos;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Application.Services;
using KYC.Domain.Enums;
using MediatR;

namespace KYC.Application.Cases;

public class GetEntityResolutionPreviewQueryHandler(IEntityResolutionService resolution)
    : IRequestHandler<GetEntityResolutionPreviewQuery, EntityResolutionPreviewDto?>
{
    public async Task<EntityResolutionPreviewDto?> Handle(
        GetEntityResolutionPreviewQuery request,
        CancellationToken cancellationToken)
    {
        if (!NifSanitizer.TryNormalizeCaseKey(request.Nif, out var nif))
            return null;

        var resolved = await resolution.ResolveByNifAsync(nif, cancellationToken);
        var registry = resolved.UsedFallback
            ? null
            : resolved.Gleif is not null
                ? "GLEIF"
                : resolved.RegistryId is not null
                    ? "RCBE/Registo"
                    : "Wikidata";

        var message = resolved.UsedFallback
            ? "Sem correspondência RCBE/GLEIF — indique a denominação social manualmente."
            : $"Resolvido: {resolved.LegalName}";

        return new EntityResolutionPreviewDto(
            nif,
            resolved.LegalName,
            resolved.UsedFallback,
            resolved.Success,
            registry,
            message);
    }
}

public class GetKycCaseQueryHandler(
    IKycCaseRepository repository,
    IEntityResolutionService resolution)
    : IRequestHandler<GetKycCaseQuery, KycCaseDetailDto?>
{
    public async Task<KycCaseDetailDto?> Handle(GetKycCaseQuery request, CancellationToken cancellationToken)
    {
        var c = await repository.GetByIdAsync(request.CaseId, cancellationToken);
        var dto = KycCaseMapping.ToDetailDto(c);
        if (dto is null)
            return null;

        var gleif = await resolution.FetchGleifEnrichmentAsync(dto.Nif, dto.CompanyName, cancellationToken);
        return dto with { Gleif = gleif.Snapshot, GleifRelatedParties = gleif.RelatedParties };
    }
}

public class GetCasePartyContextQueryHandler(IKycCaseRepository repository)
    : IRequestHandler<GetCasePartyContextQuery, CasePartyContextDto?>
{
    public async Task<CasePartyContextDto?> Handle(GetCasePartyContextQuery request, CancellationToken cancellationToken)
    {
        var match = await repository.GetCaseWithPartyAsync(request.PartyId, cancellationToken);
        if (match is null)
            return null;

        var (kyc, party) = match.Value;
        var partyDto = KycCaseMapping.ToDetailDto(kyc)?.Parties.FirstOrDefault(p => p.Id == party.Id);
        if (partyDto is null)
            return null;

        return new CasePartyContextDto(kyc.Id, kyc.CompanyName, partyDto);
    }
}

public class ListKycCasesQueryHandler(IKycCaseRepository repository)
    : IRequestHandler<ListKycCasesQuery, PagedResult<KycCaseDto>>
{
    public async Task<PagedResult<KycCaseDto>> Handle(ListKycCasesQuery request, CancellationToken cancellationToken)
    {
        var page = await repository.ListAsync(request.Filter, cancellationToken);
        var dtos = page.Items.Select(KycCaseMapping.ToListDto).ToList();
        return new PagedResult<KycCaseDto>(dtos, page.TotalCount, page.Page, page.PageSize);
    }
}

public class GetUboGraphQueryHandler(IKycCaseRepository cases, IEntityResolutionService resolution)
    : IRequestHandler<GetUboGraphQuery, UboGraphViewDto?>
{
    public async Task<UboGraphViewDto?> Handle(GetUboGraphQuery request, CancellationToken cancellationToken)
    {
        var kyc = await cases.GetByIdAsync(request.CaseId, cancellationToken);
        if (kyc is null) return null;
        var graph = await resolution.BuildUboGraphAsync(kyc.Nif, maxDepth: 5, cancellationToken);
        return UboGraphViewBuilder.Build(kyc, graph);
    }
}

public class GetRiskTimelineQueryHandler(IKycCaseRepository repository)
    : IRequestHandler<GetRiskTimelineQuery, RiskTimelineDto?>
{
    public async Task<RiskTimelineDto?> Handle(GetRiskTimelineQuery request, CancellationToken cancellationToken)
    {
        var c = await repository.GetByIdAsync(request.CaseId, cancellationToken);
        if (c is null) return null;

        var entries = new List<RiskTimelineEntryDto>();
        foreach (var s in c.RiskSignals.OrderBy(s => s.DetectedAt))
        {
            entries.Add(new RiskTimelineEntryDto(
                s.DetectedAt,
                s.Type.ToString(),
                s.Description,
                s.Severity.ToString()));
        }

        foreach (var a in c.AuditTrail.OrderBy(a => a.Timestamp))
        {
            entries.Add(new RiskTimelineEntryDto(
                a.Timestamp,
                a.Action,
                a.Details ?? string.Empty,
                "Info"));
        }

        entries.Sort((a, b) => a.At.CompareTo(b.At));
        return new RiskTimelineDto(entries);
    }
}

public class GetKycReportQueryHandler(IKycCaseRepository repository)
    : IRequestHandler<GetKycReportQuery, KycReportDto?>
{
    public async Task<KycReportDto?> Handle(GetKycReportQuery request, CancellationToken cancellationToken)
    {
        var c = await repository.GetByIdAsync(request.CaseId, cancellationToken);
        if (c?.FinalReport is null) return null;
        return new KycReportDto(
            c.Id,
            LlmChatOutputSanitizer.CleanStoredReportHtml(c.FinalReport.NarrativeHtml),
            c.FinalReport.ModelUsed,
            c.FinalReport.GeneratedAt);
    }
}

public class GetKycCaseScanProgressQueryHandler(IKycCaseScanProgressRepository progress)
    : IRequestHandler<GetKycCaseScanProgressQuery, KycCaseScanProgressDto?>
{
    public async Task<KycCaseScanProgressDto?> Handle(
        GetKycCaseScanProgressQuery request,
        CancellationToken cancellationToken)
    {
        var state = await progress.GetAsync(request.CaseId, cancellationToken);
        if (state is null)
            return null;

        return new KycCaseScanProgressDto(
            state.KycCaseId,
            state.TotalScans,
            state.CompletedScans,
            state.FailedScans);
    }
}

public class GetDashboardSummaryQueryHandler(IKycAnalyticsRepository analytics)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    public Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken) =>
        analytics.GetDashboardSummaryAsync(cancellationToken);
}

public class GetCriticalAlertsQueryHandler(IKycAnalyticsRepository analytics)
    : IRequestHandler<GetCriticalAlertsQuery, IReadOnlyList<CriticalAlertDto>>
{
    public Task<IReadOnlyList<CriticalAlertDto>> Handle(GetCriticalAlertsQuery request, CancellationToken cancellationToken) =>
        analytics.GetCriticalAlertsLast24hAsync(cancellationToken);
}

public class ListAmlComplianceReportsQueryHandler(IAmlComplianceReportRepository reports)
    : IRequestHandler<ListAmlComplianceReportsQuery, IReadOnlyList<AmlComplianceReportListItemDto>>
{
    public async Task<IReadOnlyList<AmlComplianceReportListItemDto>> Handle(
        ListAmlComplianceReportsQuery request,
        CancellationToken cancellationToken)
    {
        var list = await reports.ListAsync(cancellationToken);
        return list.Select(r => new AmlComplianceReportListItemDto(
            r.Id,
            r.ReportingYear,
            r.Status,
            r.GeneratedAt,
            r.BdpReferenceNumber)).ToList();
    }
}

public class GetUifSubmissionStatusQueryHandler(IUifReportingService uif)
    : IRequestHandler<GetUifSubmissionStatusQuery, UifSubmissionStatusDto>
{
    public async Task<UifSubmissionStatusDto> Handle(
        GetUifSubmissionStatusQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ReferenceNumber))
            throw new ArgumentException("Referência UIF obrigatória.");

        var status = await uif.GetSubmissionStatusAsync(request.ReferenceNumber, cancellationToken);
        return new UifSubmissionStatusDto(status.ReferenceNumber, status.Status, status.LastUpdated);
    }
}

public class GetAmlComplianceReportQueryHandler(IAmlComplianceReportRepository reports)
    : IRequestHandler<GetAmlComplianceReportQuery, AmlComplianceReportDetailDto?>
{
    public async Task<AmlComplianceReportDetailDto?> Handle(
        GetAmlComplianceReportQuery request,
        CancellationToken cancellationToken)
    {
        var r = await reports.GetByIdAsync(request.ReportId, cancellationToken);
        if (r is null)
            return null;

        return new AmlComplianceReportDetailDto(
            r.Id,
            r.ReportingYear,
            r.Status,
            r.GeneratedAt,
            r.GeneratedBy,
            r.BdpReferenceNumber,
            r.TotalCasesProcessed,
            r.TotalCasesApproved,
            r.TotalCasesRejected,
            r.TotalCasesUnderReview,
            r.CasesLowRisk,
            r.CasesMediumRisk,
            r.CasesHighRisk,
            r.CasesCriticalRisk,
            r.TotalRiskSignalsDetected,
            r.SanctionMatches,
            r.PepMatches,
            r.SarsSubmitted,
            r.AssetFreezeNotifications,
            r.CasesSimplifiedDd,
            r.CasesStandardDd,
            r.CasesEnhancedDd,
            r.PeriodicReviewsCompleted,
            r.PeriodicReviewsOverdue,
            r.PlatformVersion,
            r.AiModelsUsed);
    }
}
