using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;

namespace KYC.Infrastructure.Compliance;

/// <summary>Export RPB (Instrução BdP 8/2024). Substituir mapeamento quando template oficial X1 estiver disponível.</summary>
public sealed class BdpRpbExporter : IBdpRpbExporter
{
    private static readonly string[] RequiredSections =
    [
        "Metadados",
        "Secao1_EstruturaOrganizacional",
        "Secao2_DistribuicaoRisco",
        "Secao3_SinaisComunicacoes",
        "Secao4_Diligencia",
        "Secao5_TecnologiaIA"
    ];

    public byte[] ToOfficialXml(AmlComplianceReport report)
    {
        var root = new XElement("RelatorioPrevencaoBranqueamento",
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

    public byte[] ToInternalJson(AmlComplianceReport report) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));

    public BdpRpbValidationResult ValidateOfficialXml(byte[] xml)
    {
        var errors = new List<string>();
        try
        {
            var doc = XDocument.Load(new MemoryStream(xml));
            var root = doc.Root;
            if (root is null || root.Name.LocalName != "RelatorioPrevencaoBranqueamento")
            {
                errors.Add("Elemento raiz RelatorioPrevencaoBranqueamento em falta.");
                return new BdpRpbValidationResult(false, errors);
            }

            foreach (var section in RequiredSections)
            {
                if (root.Element(section) is null)
                    errors.Add($"Secção obrigatória em falta: {section}.");
            }

            if (!root.Attributes().Any(a => a.Name.LocalName == "ano"))
                errors.Add("Atributo ano em falta.");
        }
        catch (Exception ex)
        {
            errors.Add($"XML inválido: {ex.Message}");
        }

        return new BdpRpbValidationResult(errors.Count == 0, errors);
    }
}
