using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;

namespace KYC.Infrastructure.Compliance;

/// <summary>Export RPB (Instrução BdP 8/2024). Substituir mapeamento quando template oficial X1 estiver disponível.</summary>
public sealed class BdpRpbExporter : IBdpRpbExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

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
            new XElement("Secao5_TecnologiaIA", BuildModelosIaXml(report.AiModelsUsed)));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }

    public byte[] ToInternalJson(AmlComplianceReport report) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(ToExportPayload(report), JsonOptions));

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

    private static object ToExportPayload(AmlComplianceReport report) => new
    {
        report.Id,
        report.ReportingYear,
        report.GeneratedAt,
        report.GeneratedBy,
        Status = report.Status.ToString(),
        report.BdpReferenceNumber,
        report.SubmittedAt,
        report.TotalAmlAnalysts,
        report.TotalCasesProcessed,
        report.TotalCasesApproved,
        report.TotalCasesRejected,
        report.TotalCasesUnderReview,
        report.CasesLowRisk,
        report.CasesMediumRisk,
        report.CasesHighRisk,
        report.CasesCriticalRisk,
        report.TotalRiskSignalsDetected,
        report.SanctionMatches,
        report.PepMatches,
        report.SarsSubmitted,
        report.AssetFreezeNotifications,
        report.CasesSimplifiedDd,
        report.CasesStandardDd,
        report.CasesEnhancedDd,
        report.PeriodicReviewsCompleted,
        report.PeriodicReviewsOverdue,
        report.PlatformVersion,
        AiModelsUsed = ParseAiModelsJson(report.AiModelsUsed)
    };

    /// <summary>Converte o JSON persistido em coluna para objecto no export (evita string escapada).</summary>
    internal static JsonElement ParseAiModelsJson(string? storedJson)
    {
        if (string.IsNullOrWhiteSpace(storedJson))
            return CloneRoot("{}");

        try
        {
            using var doc = JsonDocument.Parse(storedJson);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return CloneRoot(JsonSerializer.Serialize(new { raw = storedJson, parseError = true }));
        }
    }

    private static JsonElement CloneRoot(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static XElement BuildModelosIaXml(string? storedJson)
    {
        var root = new XElement("ModelosIa");
        if (string.IsNullOrWhiteSpace(storedJson))
            return root;

        try
        {
            using var doc = JsonDocument.Parse(storedJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                root.Add(new XElement("Valor", doc.RootElement.ToString()));
                return root;
            }

            foreach (var prop in doc.RootElement.EnumerateObject())
                root.Add(new XElement(ToXmlElementName(prop.Name), FormatJsonValue(prop.Value)));
        }
        catch (JsonException)
        {
            root.Add(new XElement("Raw", storedJson));
        }

        return root;
    }

    private static string FormatJsonValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? "",
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null => "",
        _ => value.GetRawText()
    };

    private static string ToXmlElementName(string jsonPropertyName)
    {
        if (string.IsNullOrEmpty(jsonPropertyName))
            return "Campo";

        var sb = new StringBuilder();
        var upperNext = true;
        foreach (var c in jsonPropertyName)
        {
            if (!char.IsLetterOrDigit(c))
            {
                upperNext = true;
                continue;
            }

            sb.Append(upperNext ? char.ToUpperInvariant(c) : c);
            upperNext = false;
        }

        return sb.Length > 0 ? sb.ToString() : "Campo";
    }
}
