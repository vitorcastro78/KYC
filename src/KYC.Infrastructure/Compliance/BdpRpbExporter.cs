using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using KYC.Domain.Entities;

namespace KYC.Infrastructure.Compliance;

/// <summary>
/// Export RPB alinhado com Instrução BdP 8/2024 (estrutura XML; validar schema oficial com compliance).
/// </summary>
public static class BdpRpbExporter
{
    public static byte[] ToXml(AmlComplianceReport report)
    {
        var root = new XElement("RelatorioPrevencaoBranqueamento",
            new XAttribute("xmlns", "https://kyc.local/bdp/rpb/v1"),
            new XAttribute("ano", report.ReportingYear),
            new XAttribute("versaoPlataforma", report.PlatformVersion),
            new XElement("Metadados",
                new XElement("GeradoEm", report.GeneratedAt.ToString("o")),
                new XElement("GeradoPor", report.GeneratedBy),
                new XElement("Estado", report.Status.ToString()),
                new XElement("ReferenciaBdP", report.BdpReferenceNumber ?? "")),
            new XElement("Secao1_EstruturaOrganizacional",
                new XElement("TotalAnalistasAml", report.TotalAmlAnalysts),
                new XElement("TotalCasosProcessados", report.TotalCasesProcessed)),
            new XElement("Secao2_DistribuicaoRisco",
                new XElement("CasosBaixoRisco", report.CasesLowRisk),
                new XElement("CasosMedioRisco", report.CasesMediumRisk),
                new XElement("CasosAltoRisco", report.CasesHighRisk),
                new XElement("CasosCriticoRisco", report.CasesCriticalRisk)),
            new XElement("Secao3_SinaisComunicacoes",
                new XElement("TotalSinais", report.TotalRiskSignalsDetected),
                new XElement("CorrespondenciasSancoes", report.SanctionMatches),
                new XElement("CorrespondenciasPep", report.PepMatches),
                new XElement("ComunicacoesUif", report.SarsSubmitted),
                new XElement("NotificacoesCongelamento", report.AssetFreezeNotifications)),
            new XElement("Secao4_Diligencia",
                new XElement("DdcSimplificada", report.CasesSimplifiedDd),
                new XElement("DdcStandard", report.CasesStandardDd),
                new XElement("DdcReforcada", report.CasesEnhancedDd),
                new XElement("RevisoesPeriodicasConcluidas", report.PeriodicReviewsCompleted),
                new XElement("RevisoesPeriodicasEmAtraso", report.PeriodicReviewsOverdue)),
            new XElement("Secao5_TecnologiaIA",
                new XElement("ModelosIa", report.AiModelsUsed)));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }

    public static byte[] ToBdpJson(AmlComplianceReport report)
    {
        var payload = new
        {
            instrucao = "BdP-8-2024",
            reportingYear = report.ReportingYear,
            generatedAt = report.GeneratedAt,
            status = report.Status.ToString(),
            bdpReferenceNumber = report.BdpReferenceNumber,
            secao1 = new { report.TotalAmlAnalysts, report.TotalCasesProcessed, report.TotalCasesApproved, report.TotalCasesRejected, report.TotalCasesUnderReview },
            secao2 = new { report.CasesLowRisk, report.CasesMediumRisk, report.CasesHighRisk, report.CasesCriticalRisk },
            secao3 = new { report.TotalRiskSignalsDetected, report.SanctionMatches, report.PepMatches, report.SarsSubmitted, report.AssetFreezeNotifications },
            secao4 = new { report.CasesSimplifiedDd, report.CasesStandardDd, report.CasesEnhancedDd, report.PeriodicReviewsCompleted, report.PeriodicReviewsOverdue },
            secao5 = new { report.PlatformVersion, report.AiModelsUsed }
        };
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
    }
}
