using System.Globalization;
using System.Net;
using System.Text;
using KYC.Application.Interfaces;
using KYC.Domain.Enums;

namespace KYC.Infrastructure.Reports;

/// <summary>Gera relatório KYC em HTML (visualização web e exportação PDF).</summary>
public sealed class KycStructuredReportComposer : IKycReportComposer
{
    private static readonly CultureInfo Pt = CultureInfo.GetCultureInfo("pt-PT");

    public string ComposeHtml(KycReportComposeRequest request)
    {
        var generated = request.GeneratedAtUtc.ToLocalTime();
        var body = new StringBuilder(8192);

        body.AppendLine("<header class=\"doc-header\">");
        body.AppendLine("<h1>Relatório KYC — Due Diligence</h1>");
        body.AppendLine(
            $"<p class=\"meta\">Gerado em {generated:dd/MM/yyyy HH:mm} · Caso <code>{request.CaseId}</code></p>");
        body.AppendLine("</header>");
        body.AppendLine("<main>");

        AppendMetadataTable(body, request, generated);
        KycExecutiveSummaryBuilder.Append(body, request);
        AppendParties(body, request);
        AppendSignals(body, request);
        AppendRiskScore(body, request);
        AppendConsistency(body, request);
        AppendRecommendation(body, request);
        AppendRegulatoryDisclosures(body, request);
        AppendSources(body, generated);

        body.AppendLine("</main>");
        body.AppendLine(
            $"<footer class=\"doc-footer\">KYC AI Platform · {WebUtility.HtmlEncode(request.CompanyName)}</footer>");

        return KycReportHtmlDocument.Wrap(body.ToString(), $"Relatório KYC — {request.CompanyName}");
    }

    private static void AppendMetadataTable(StringBuilder body, KycReportComposeRequest request, DateTime generated)
    {
        body.AppendLine("<table><tbody>");
        AppendMetaRow(body, "Tomador", request.CompanyName);
        AppendMetaRow(body, "Identificador", request.Nif);
        AppendMetaRow(body, "Estado do caso", StatusLabel(request.Status));
        AppendMetaRow(body, "Crédito solicitado",
            $"{request.RequestedCreditAmount:N2} {request.RequestedCreditCurrency}");
        AppendMetaRow(body, "Abertura do caso", request.CreatedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm", Pt));
        AppendMetaRow(body, "Data do relatório", generated.ToString("dd/MM/yyyy HH:mm", Pt));
        body.AppendLine("</tbody></table>");
    }

    private static void AppendMetaRow(StringBuilder body, string label, string value) =>
        body.AppendLine(
            $"<tr><th>{Enc(label)}</th><td>{Enc(value)}</td></tr>");

    private static void AppendParties(StringBuilder body, KycReportComposeRequest request)
    {
        body.AppendLine("<h2>Estrutura de partes e UBO</h2>");

        if (request.Parties.Count == 0)
        {
            body.AppendLine("<p><em>Nenhuma parte registada no caso.</em></p>");
            return;
        }

        body.AppendLine(
            "<table><thead><tr><th>Nome</th><th>Identificador</th><th>Papel</th><th>Prof.</th><th>PEP</th><th>Sanções</th></tr></thead><tbody>");
        foreach (var p in request.Parties.OrderBy(x => x.Depth).ThenBy(x => x.Name))
        {
            body.AppendLine(
                $"<tr><td>{Enc(p.Name)}</td><td>{Enc(p.Nif ?? "—")}</td><td>{Enc(RoleLabel(p.Role))}</td><td>{p.Depth}</td><td>{Enc(BoolLabel(p.IsPep))}</td><td>{Enc(BoolLabel(p.IsSanctioned))}</td></tr>");
        }

        body.AppendLine("</tbody></table>");
    }

    private static void AppendSignals(StringBuilder body, KycReportComposeRequest request)
    {
        body.AppendLine("<h2>Sinais de risco identificados</h2>");

        if (request.Signals.Count == 0)
        {
            body.AppendLine("<p>Não foram detetados sinais automáticos nesta execução.</p>");
            return;
        }

        foreach (var group in request.Signals.GroupBy(s => s.Type).OrderBy(g => g.Key))
        {
            body.AppendLine($"<h3>{Enc(SignalTypeLabel(group.Key))}</h3><ul>");
            foreach (var s in group.OrderByDescending(x => x.Severity))
            {
                body.AppendLine(
                    $"<li><span class=\"{SeverityBadgeClass(s.Severity)}\">{Enc(SeverityLabel(s.Severity))}</span> {Enc(s.Description)} <em>(fonte: {Enc(s.Source)})</em></li>");
            }

            body.AppendLine("</ul>");
        }
    }

    private static void AppendRiskScore(StringBuilder body, KycReportComposeRequest request)
    {
        body.AppendLine("<h2>Avaliação de risco</h2>");
        body.AppendLine("<table><thead><tr><th>Dimensão</th><th>Pontuação</th></tr></thead><tbody>");
        body.AppendLine(
            $"<tr><td><strong>Global</strong></td><td class=\"score-global\"><strong>{request.Score.Overall}</strong> ({Enc(RiskLevelLabel(request.Score.Level))})</td></tr>");
        AppendScoreRow(body, "Sanções", request.Score.SanctionsScore);
        AppendScoreRow(body, "PEP", request.Score.PepScore);
        AppendScoreRow(body, "Media adversa", request.Score.AdverseMediaScore);
        AppendScoreRow(body, "Saúde financeira", request.Score.FinancialScore);
        AppendScoreRow(body, "Judicial", request.Score.JudicialScore);
        AppendScoreRow(body, "Estrutura UBO", request.Score.UboStructureScore);
        body.AppendLine("</tbody></table>");
    }

    private static void AppendScoreRow(StringBuilder body, string label, int? value) =>
        body.AppendLine($"<tr><td>{Enc(label)}</td><td>{(value.HasValue ? value.Value.ToString(Pt) : "n/d")}</td></tr>");

    private static void AppendConsistency(StringBuilder body, KycReportComposeRequest request)
    {
        body.AppendLine("<h2>Consistência e lacunas</h2>");

        var inconsistencies = request.Signals
            .Where(s => s.Type.Contains("Inconsistency", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (inconsistencies.Count == 0)
            body.AppendLine("<p>Não foram reportadas inconsistências estruturais automáticas.</p>");
        else
        {
            body.AppendLine("<ul>");
            foreach (var i in inconsistencies)
                body.AppendLine($"<li>{Enc(i.Description)}</li>");
            body.AppendLine("</ul>");
        }

        if (request.Parties.All(p => !p.Role.Contains("Ubo", StringComparison.OrdinalIgnoreCase)))
        {
            body.AppendLine(
                "<p><strong>Lacuna UBO:</strong> não há beneficiário efectivo final declarado no grafo — validar quadro societário em fonte primária (RCBE / registo comercial).</p>");
        }
    }

    private static void AppendRecommendation(StringBuilder body, KycReportComposeRequest request)
    {
        body.AppendLine("<h2>Recomendação</h2>");
        body.AppendLine($"<p>{RecommendedAction(request)}</p>");
        body.AppendLine(
            "<p class=\"footnote\">Esta recomendação é gerada por regras automáticas e não substitui a decisão de crédito ou compliance do analista.</p>");
    }

    private static void AppendRegulatoryDisclosures(StringBuilder body, KycReportComposeRequest request)
    {
        body.AppendLine("<h2>10. Transparência e RGPD (Art. 22)</h2>");
        body.AppendLine("<p>Decisão assistida por sistemas automatizados de triagem e scoring. O analista pode solicitar revisão humana, contestar o resultado e obter explicação dos factores considerados.</p>");
        body.AppendLine("<ul>");
        body.AppendLine("<li><strong>Base legal:</strong> cumprimento de obrigações legais AML/CFT (Lei 83/2017, Reg. UE 2015/847).</li>");
        body.AppendLine("<li><strong>Retenção:</strong> 7 anos após encerramento do relacionamento (conforme política interna).</li>");
        body.AppendLine("<li><strong>Direitos do titular:</strong> acesso, rectificação, oposição limitada — contacto DPO da instituição.</li>");
        body.AppendLine("</ul>");
        if (!string.IsNullOrWhiteSpace(request.LegalBasisRef))
            body.AppendLine($"<p><strong>Referência PAC/base legal:</strong> {Enc(request.LegalBasisRef)}</p>");
    }

    private static void AppendSources(StringBuilder body, DateTime generated)
    {
        body.AppendLine("<h2>Fontes e método</h2>");
        body.AppendLine("<p>Triagem automática com base em:</p><ul>");
        body.AppendLine("<li>Listas de sanções (OFAC SDN, UE FSF, OpenSanctions quando configurado)</li>");
        body.AppendLine("<li>Media adversa (NewsAPI / feeds configurados)</li>");
        body.AppendLine("<li>Indicadores financeiros e judiciais (serviços internos)</li>");
        body.AppendLine("<li>ICIJ Offshore Leaks (quando aplicável)</li>");
        body.AppendLine("<li>Identificação LEI / rede corporativa GLEIF (Level 2, quando disponível)</li>");
        body.AppendLine("<li>Resolução de entidade: RCBE (PT), GLEIF, Wikidata</li>");
        body.AppendLine("</ul>");
        body.AppendLine(
            $"<p class=\"footnote\">Documento gerado pela plataforma KYC em {generated:dd/MM/yyyy HH:mm} — versão estruturada HTML.</p>");
    }

    private static string RecommendedAction(KycReportComposeRequest request)
    {
        var hasHigh = request.Signals.Any(s =>
            s.Severity.Equals("High", StringComparison.OrdinalIgnoreCase) ||
            s.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase));

        var text = request.Score.Level switch
        {
            RiskLevel.Low when !hasHigh =>
                "Prosseguir com diligência standard. Monitorização periódica conforme política interna.",
            RiskLevel.Medium =>
                "Revisão humana antes de decisão. Confirmar sinais médios e documentar mitigantes.",
            RiskLevel.High =>
                "Escalar para compliance sénior. Não aprovar crédito até esclarecimento dos sinais elevados.",
            RiskLevel.Critical =>
                "Não prosseguir sem comité de risco. Bloquear relação até conclusão de investigação reforçada.",
            _ => "Revisão humana obrigatória — score ou sinais exigem validação pelo analista responsável."
        };

        return $"<strong>{Enc(text)}</strong>";
    }

    private static string Enc(string? text) => KycReportHtmlDocument.HtmlEncode(text);

    private static string SeverityBadgeClass(string severity) => severity switch
    {
        "High" or "Critical" => "badge badge-high",
        "Medium" => "badge badge-medium",
        _ => "badge badge-low"
    };

    private static string BoolLabel(bool value) => value ? "Sim" : "Não";

    private static string StatusLabel(KycStatus status) => status switch
    {
        KycStatus.Pending => "Pendente",
        KycStatus.InProgress => "Em progresso",
        KycStatus.UnderReview => "Em revisão",
        KycStatus.Approved => "Aprovado",
        KycStatus.Rejected => "Rejeitado",
        _ => status.ToString()
    };

    private static string RiskLevelLabel(RiskLevel level) => level switch
    {
        RiskLevel.Low => "Baixo",
        RiskLevel.Medium => "Médio",
        RiskLevel.High => "Alto",
        RiskLevel.Critical => "Crítico",
        _ => level.ToString()
    };

    private static string SeverityLabel(string severity) => severity switch
    {
        "Low" => "Baixa",
        "Medium" => "Média",
        "High" => "Alta",
        "Critical" => "Crítica",
        _ => severity
    };

    private static string SignalTypeLabel(string type) => type switch
    {
        "Sanction" => "Sanções",
        "Pep" => "PEP",
        "AdverseMedia" => "Media adversa",
        "Judicial" => "Judicial",
        "Financial" => "Financeiro",
        "UboAnomaly" => "Anomalia UBO",
        "Inconsistency" => "Inconsistência",
        _ => type
    };

    private static string RoleLabel(string role) => role switch
    {
        "Target" => "Tomador",
        "Shareholder" => "Accionista",
        "Ubo" => "UBO",
        "BoardMember" => "Órgão social",
        "Proxy" => "Procurador",
        _ => role
    };
}
