using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using KYC.Infrastructure.Reports;

namespace KYC.Application.Tests;

public class KycStructuredReportComposerTests
{
    private readonly IKycReportComposer _composer = new KycStructuredReportComposer();

    [Fact]
    public void ComposeHtml_includes_case_metadata_and_sections()
    {
        var request = new KycReportComposeRequest(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "500697256",
            "EDP, S.A.",
            KycStatus.UnderReview,
            1_000_000,
            "EUR",
            DateTime.UtcNow.AddDays(-1),
            [
                new PartyScanDto(Guid.NewGuid(), "EDP, S.A.", "500697256", "Target", 0, false, false),
                new PartyScanDto(Guid.NewGuid(), "Grupo X", "213800WAVVOPS85N2205", "Shareholder", 1, false, true)
            ],
            [
                new RiskSignalScanDto("Sanction", "High", "Correspondência OFAC", "OFAC")
            ],
            new RiskScore { Overall = 72, Justification = "Sinais elevados em sanções." },
            DateTime.UtcNow);

        var html = _composer.ComposeHtml(request);

        Assert.Contains("Relatório KYC", html);
        Assert.Contains("Transparência e RGPD", html);
        Assert.Contains("Sumário executivo", html);
        Assert.Contains("Metodologia e parâmetros utilizados", html);
        Assert.Contains("Lógica de cálculo do score de risco", html);
        Assert.Contains("Inventário de sinais de risco", html);
        Assert.Contains("Estrutura de partes", html);
        Assert.Contains("Sinais de risco", html);
        Assert.Contains("72", html);
        Assert.Contains("EDP, S.A.", html);
        Assert.Contains("OFAC", html);
        Assert.Contains("Recomendação", html);
    }
}
