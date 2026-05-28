using KYC.Application.Models;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;

namespace KYC.Application.Interfaces;

public record KycReportComposeRequest(
    Guid CaseId,
    string Nif,
    string CompanyName,
    KycStatus Status,
    decimal RequestedCreditAmount,
    string RequestedCreditCurrency,
    DateTime CreatedAtUtc,
    IReadOnlyList<PartyScanDto> Parties,
    IReadOnlyList<RiskSignalScanDto> Signals,
    RiskScore Score,
    DateTime GeneratedAtUtc);

public interface IKycReportComposer
{
    string ComposeHtml(KycReportComposeRequest request);
}
