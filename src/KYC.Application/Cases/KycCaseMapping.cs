using KYC.Application.Dtos;
using KYC.Domain.Entities;

namespace KYC.Application.Cases;

internal static class KycCaseMapping
{
    public static KycCaseDto ToListDto(KycCase c) =>
        new(
            c.Id,
            c.Nif,
            c.CompanyName,
            c.Status,
            c.Score,
            c.CreatedAt,
            c.CompletedAt,
            c.AssignedAnalystId,
            c.Parties.Count,
            c.RiskSignals.Count);

    public static KycCaseDetailDto? ToDetailDto(KycCase? c)
    {
        if (c is null) return null;
        var parties = c.Parties.Select(p => new CasePartyDto(
            p.Id,
            p.Name,
            p.Nif,
            p.Role,
            p.UboDepthLevel,
            p.OwnershipPercentage,
            p.IsPep,
            p.IsSanctioned,
            p.IsOffshore)).ToList();

        var signals = c.RiskSignals.Select(s => new RiskSignalDetailDto(
            s.Id,
            s.Type,
            s.Severity,
            s.Description,
            s.Source,
            s.DetectedAt,
            s.IsConfirmed)).ToList();

        var audit = c.AuditTrail.Select(a => new AuditEntryDto(
            a.Action,
            a.ActorId,
            a.ActorType,
            a.Timestamp,
            a.Details)).ToList();

        var report = c.FinalReport is null
            ? null
            : new KycReportDto(c.Id, c.FinalReport.NarrativeHtml, c.FinalReport.ModelUsed, c.FinalReport.GeneratedAt);

        return new KycCaseDetailDto(
            c.Id,
            c.Nif,
            c.CompanyName,
            c.Status,
            c.Score,
            c.CreatedAt,
            c.CompletedAt,
            c.AssignedAnalystId,
            parties,
            signals,
            audit,
            report);
    }
}
