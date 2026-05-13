using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Domain.Entities;
using KYC.Domain.Enums;

namespace KYC.Infrastructure.Messaging;

/// <summary>Lógica partilhada entre o pipeline inicial e triagens manuais por parte.</summary>
internal static class CasePartyScanOperations
{
    public static async Task<IReadOnlyList<RiskSignal>> CollectSignalsForPartyAsync(
        KycCase kyc,
        CaseParty party,
        ISanctionsScreeningService sanctions,
        IAdverseMediaService adverse,
        IFinancialHealthService financial,
        IJudicialIntelligenceService judicial,
        IIcijOffshoreService icij,
        CancellationToken ct)
    {
        var signals = new List<RiskSignal>();

        var s = await sanctions.ScreenEntityAsync(party, ct);
        foreach (var m in s.Matches)
        {
            signals.Add(RiskSignal.Create(
                kyc.Id,
                party.Id,
                SignalType.Sanction,
                SignalSeverity.High,
                $"{m.ListName}: {m.MatchedName}",
                m.ListName));
        }

        var am = await adverse.ScanAsync(party.Name, party.Nif, ct);
        foreach (var hit in am.Hits)
        {
            signals.Add(RiskSignal.Create(
                kyc.Id,
                party.Id,
                SignalType.AdverseMedia,
                SignalSeverity.Medium,
                hit.Title,
                hit.Url,
                hit.PublishedAt));
        }

        if (party.Nif is not null)
        {
            var fin = await financial.AnalyseAsync(party.Nif, ct);
            signals.Add(RiskSignal.Create(
                kyc.Id,
                party.Id,
                SignalType.Financial,
                SignalSeverity.Low,
                fin.Summary,
                "Financial"));
        }

        var jud = await judicial.SearchAsync(party.Nif ?? kyc.Nif, party.Name, ct);
        foreach (var jc in jud.Cases)
        {
            signals.Add(RiskSignal.Create(
                kyc.Id,
                party.Id,
                SignalType.Judicial,
                SignalSeverity.Medium,
                $"{jc.Reference} @ {jc.Court}",
                jc.Court,
                jc.Date));
        }

        var off = await icij.SearchAsync(party.Name, ct);
        foreach (var o in off)
        {
            signals.Add(RiskSignal.Create(
                kyc.Id,
                party.Id,
                SignalType.UboAnomaly,
                SignalSeverity.High,
                o.Details ?? o.EntityName,
                o.SourceDataset));
        }

        return signals;
    }
}
