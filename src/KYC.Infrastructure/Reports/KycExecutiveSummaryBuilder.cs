using System.Globalization;
using System.Net;
using System.Text;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using KYC.Domain.Enums;

namespace KYC.Infrastructure.Reports;

/// <summary>Texto narrativo detalhado do sumário executivo do relatório KYC.</summary>
internal static class KycExecutiveSummaryBuilder
{
    private static readonly CultureInfo Pt = CultureInfo.GetCultureInfo("pt-PT");

    private const int GleifUboMaxDepth = 5;

    public static void Append(StringBuilder body, KycReportComposeRequest request)
    {
        body.AppendLine("<h2>Sumário executivo</h2>");

        var stats = Analyze(request);

        AppendSection1Object(body, request, stats);
        AppendSection2Methodology(body, request, stats);
        AppendSection3Universe(body, request, stats);
        AppendSection4Signals(body, request, stats);
        AppendSection5ScoringEngine(body, request, stats);
        AppendSection6Conclusion(body, request, stats);
        AppendSection7Recommendation(body, request, stats);
    }

    private static void AppendSection1Object(StringBuilder body, KycReportComposeRequest request, SummaryStats stats)
    {
        body.AppendLine("<h3>1. Objecto e enquadramento</h3>");
        body.AppendLine("<p>");
        body.Append(
            $"O presente relatório documenta a due diligence KYC sobre <strong>{Enc(request.CompanyName)}</strong> ");
        body.Append($"(identificador fiscal/LEI: <code>{Enc(request.Nif)}</code>), ");
        body.Append(
            $"com exposição de crédito declarada de <strong>{request.RequestedCreditAmount:N2} {Enc(request.RequestedCreditCurrency)}</strong>. ");
        body.Append(
            $"O caso encontra-se no estado <strong>{Enc(StatusLabel(request.Status))}</strong> ");
        body.Append(
            $"desde a abertura em {request.CreatedAtUtc.ToLocalTime():dd/MM/yyyy HH:mm} (hora local). ");
        body.Append(
            $"A síntese abaixo descreve como a plataforma chegou ao resultado actual — score <strong>{request.Score.Overall}/100</strong> ");
        body.Append($"({Enc(RiskLevelLabel(request.Score.Level))}) — com base nos dados recolhidos em {stats.GeneratedLocal:dd/MM/yyyy HH:mm}.");
        body.AppendLine("</p>");
    }

    private static void AppendSection2Methodology(StringBuilder body, KycReportComposeRequest request, SummaryStats stats)
    {
        body.AppendLine("<h3>2. Metodologia e parâmetros utilizados</h3>");
        body.AppendLine("<p>A análise seguiu o pipeline automático da plataforma KYC, executado na seguinte sequência lógica:</p>");
        body.AppendLine("<ol>");
        body.AppendLine(
            "<li><strong>Resolução de entidade e grafo societário:</strong> identificação do tomador e expansão da rede de partes relacionadas via RCBE (entidades portuguesas), GLEIF (rede LEI, Level 2 quando disponível) e Wikidata, até profundidade máxima de <strong>" +
            GleifUboMaxDepth +
            "</strong> níveis no grafo UBO.</li>");
        body.AppendLine(
            "<li><strong>Triagem por parte:</strong> para cada entidade no grafo, execução paralela de motores de sanções (OFAC SDN, lista UE FSF, OpenSanctions quando configurado), media adversa (NewsAPI/feeds), indicadores financeiros (quando existe NIF), processos judiciais e pesquisa ICIJ Offshore Leaks.</li>");
        body.AppendLine(
            "<li><strong>Agregação de sinais:</strong> cada correspondência ou alerta gera um registo tipado (<em>Sanções, PEP, Media adversa, Judicial, Financeiro, Anomalia UBO, Inconsistência</em>) com severidade (<em>Baixa, Média, Alta, Crítica</em>) e fonte rastreável.</li>");
        body.AppendLine(
            "<li><strong>Motor de scoring (LLM local/cloud):</strong> com base no inventário de sinais e partes, cálculo do score global (0–100) e das dimensões: Sanções, PEP, Media adversa, Saúde financeira, Judicial e Estrutura UBO. Dimensões sem dados de fonte ficam como <em>n/d</em> (não inventadas).</li>");
        body.AppendLine(
            "<li><strong>Verificação de consistência:</strong> validação cruzada entre estrutura declarada/encontrada; inconsistências geram sinais adicionais de severidade média.</li>");
        body.AppendLine(
            "<li><strong>Recomendação e decisão automática:</strong> casos com score ≤ 30 (baixo) e sem sinais altos/críticos podem ser auto-aprovados; caso contrário, o fluxo passa a <em>revisão humana</em>.</li>");
        body.AppendLine("</ol>");

        body.AppendLine("<p><strong>Parâmetros efectivos nesta execução:</strong></p>");
        body.AppendLine("<ul>");
        body.AppendLine($"<li>Profundidade máxima do grafo UBO: <strong>{GleifUboMaxDepth}</strong> níveis (profundidade observada: <strong>{stats.MaxPartyDepth}</strong>).</li>");
        body.AppendLine($"<li>Partes triadas: <strong>{stats.PartyCount}</strong> (tomador + relacionadas).</li>");
        body.AppendLine($"<li>Moeda e montante de crédito analisado: <strong>{request.RequestedCreditAmount:N2} {Enc(request.RequestedCreditCurrency)}</strong>.</li>");
        body.AppendLine(
            "<li>Limiares de classificação do score global: ≤ 30 = Baixo; 31–60 = Médio; 61–80 = Alto; &gt; 80 = Crítico.</li>");
        body.AppendLine("</ul>");
    }

    private static void AppendSection3Universe(StringBuilder body, KycReportComposeRequest request, SummaryStats stats)
    {
        body.AppendLine("<h3>3. Universo de entidades analisadas</h3>");
        body.AppendLine("<p>");
        body.Append(
            $"Foram identificadas <strong>{stats.PartyCount}</strong> parte(s) no grafo do caso: ");
        body.Append(
            $"<strong>{stats.TargetCount}</strong> tomador, <strong>{stats.ShareholderCount}</strong> accionista(s)/sócio(s), ");
        body.Append(
            $"<strong>{stats.UboCount}</strong> beneficiário(s) efectivo(s) (UBO), <strong>{stats.BoardCount}</strong> elemento(s) de órgãos sociais");
        if (stats.OtherRoleCount > 0)
            body.Append($", <strong>{stats.OtherRoleCount}</strong> outro(s) papel(is)");
        body.AppendLine(".</p>");

        body.AppendLine("<ul>");
        body.AppendLine($"<li>Partes com flag PEP: <strong>{stats.PepCount}</strong>.</li>");
        body.AppendLine($"<li>Partes com indício de sanções: <strong>{stats.SanctionedCount}</strong>.</li>");
        if (stats.PepNames.Count > 0)
        {
            body.Append("<li>PEP identificados: ");
            body.Append(string.Join("; ", stats.PepNames.Select(Enc)));
            body.AppendLine(".</li>");
        }

        if (stats.SanctionedNames.Count > 0)
        {
            body.Append("<li>Correspondências em listas de sanções (nível de parte): ");
            body.Append(string.Join("; ", stats.SanctionedNames.Select(Enc)));
            body.AppendLine(".</li>");
        }

        body.AppendLine("</ul>");

        if (stats.MissingUbo)
        {
            body.AppendLine(
                "<p><strong>Lacuna estrutural:</strong> não foi identificado beneficiário efectivo final (UBO) no grafo — recomenda-se validação em RCBE/registo comercial antes de decisão final.</p>");
        }
    }

    private static void AppendSection4Signals(StringBuilder body, KycReportComposeRequest request, SummaryStats stats)
    {
        body.AppendLine("<h3>4. Inventário de sinais de risco</h3>");
        body.AppendLine("<p>");
        body.Append(
            $"A triagem produziu <strong>{stats.SignalCount}</strong> sinal(is) automático(s), distribuídos por severidade: ");
        body.Append(
            $"<strong>{stats.BySeverity.GetValueOrDefault("Low")}</strong> baixa(s), ");
        body.Append(
            $"<strong>{stats.BySeverity.GetValueOrDefault("Medium")}</strong> média(s), ");
        body.Append(
            $"<strong>{stats.BySeverity.GetValueOrDefault("High")}</strong> alta(s), ");
        body.Append(
            $"<strong>{stats.BySeverity.GetValueOrDefault("Critical")}</strong> crítica(s).");
        body.AppendLine("</p>");

        if (stats.SignalCount > 0)
        {
            body.AppendLine("<p><strong>Distribuição por dimensão de risco:</strong></p><ul>");
            foreach (var (type, count) in stats.ByType.OrderByDescending(x => x.Value))
                body.AppendLine($"<li>{Enc(SignalTypeLabel(type))}: <strong>{count}</strong></li>");
            body.AppendLine("</ul>");

            body.AppendLine("<p><strong>Fontes que originaram alertas:</strong> ");
            body.Append(string.Join(", ", stats.Sources.Select(Enc)));
            body.AppendLine(".</p>");

            body.AppendLine("<p><strong>Detalhe de cada sinal registado:</strong></p><ol>");
            foreach (var s in stats.OrderedSignals)
            {
                body.AppendLine(
                    $"<li><span class=\"{SeverityBadgeClass(s.Severity)}\">{Enc(SeverityLabel(s.Severity))}</span> " +
                    $"<em>[{Enc(SignalTypeLabel(s.Type))}]</em> {Enc(s.Description)} " +
                    $"(fonte: {Enc(s.Source)})</li>");
            }

            body.AppendLine("</ol>");
        }
        else
        {
            body.AppendLine("<p>Não foram gerados sinais automáticos nesta execução.</p>");
        }
    }

    private static void AppendSection5ScoringEngine(StringBuilder body, KycReportComposeRequest request, SummaryStats stats)
    {
        body.AppendLine("<h3>5. Lógica de cálculo do score de risco</h3>");
        body.AppendLine("<p>");
        body.Append(
            "O score global resulta da síntese automática (motor LLM) sobre o inventário de sinais e dimensões abaixo. ");
        body.Append(
            "Cada dimensão é pontuada de 0 a 100 (quanto maior, maior o risco contributivo). ");
        body.Append(
            "Quando uma fonte não devolveu dados utilizáveis, a dimensão permanece <em>n/d</em> e não é assumida como risco zero fictício.");
        body.AppendLine("</p>");

        body.AppendLine("<table><thead><tr><th>Dimensão</th><th>Pontuação</th><th>Interpretação automática</th></tr></thead><tbody>");
        AppendDimensionRow(body, "Score global (agregado)", request.Score.Overall, true, request.Score.Level);
        AppendDimensionRow(body, "Sanções", request.Score.SanctionsScore, false, null);
        AppendDimensionRow(body, "PEP", request.Score.PepScore, false, null);
        AppendDimensionRow(body, "Media adversa", request.Score.AdverseMediaScore, false, null);
        AppendDimensionRow(body, "Saúde financeira", request.Score.FinancialScore, false, null);
        AppendDimensionRow(body, "Judicial", request.Score.JudicialScore, false, null);
        AppendDimensionRow(body, "Estrutura UBO", request.Score.UboStructureScore, false, null);
        body.AppendLine("</tbody></table>");

        body.AppendLine("<p><strong>Regras de coerência aplicadas na classificação final:</strong></p><ul>");
        body.AppendLine(
            $"<li>Score global {request.Score.Overall}/100 → nível <strong>{Enc(RiskLevelLabel(request.Score.Level))}</strong> (limiar {Enc(ThresholdLabel(request.Score.Level))}).</li>");
        body.AppendLine(
            $"<li>Sinais de severidade alta ou crítica no caso: <strong>{stats.HighOrCriticalCount}</strong> " +
            (stats.HighOrCriticalCount == 0
                ? "(nenhum — reduz pressão para escalonamento imediato)."
                : "(existem — exigem revisão humana reforçada independentemente do score)."));
        body.AppendLine("</ul>");

        if (stats.InconsistencyCount > 0)
        {
            body.AppendLine("<p><strong>Inconsistências estruturais detetadas:</strong></p><ul>");
            foreach (var i in stats.InconsistencySignals)
                body.AppendLine($"<li>{Enc(i.Description)}</li>");
            body.AppendLine("</ul>");
        }
    }

    private static void AppendSection6Conclusion(StringBuilder body, KycReportComposeRequest request, SummaryStats stats)
    {
        body.AppendLine("<h3>6. Síntese: como se chegou ao resultado actual</h3>");
        body.AppendLine("<p>");
        body.Append(
            $"Após triagem de <strong>{stats.PartyCount}</strong> parte(s) e registo de <strong>{stats.SignalCount}</strong> sinal(is), ");
        body.Append(
            $"o motor de risco atribuiu <strong>{request.Score.Overall}/100</strong> ({Enc(RiskLevelLabel(request.Score.Level))}). ");
        if (stats.HighOrCriticalCount == 0 && stats.SanctionedCount == 0)
            body.Append("Não foram identificados sinais de severidade alta/crítica nem correspondências directas em listas de sanções ao nível das partes. ");
        else if (stats.SanctionedCount > 0)
            body.Append(
                $"Foram identificadas <strong>{stats.SanctionedCount}</strong> parte(s) com indício em listas de sanções — factor determinante para revisão obrigatória. ");
        if (stats.HighOrCriticalCount > 0)
            body.Append(
                $"Existem <strong>{stats.HighOrCriticalCount}</strong> sinal(is) alto(s)/crítico(s) que reforçam a necessidade de diligência acrescida. ");
        body.AppendLine("</p>");

        if (!string.IsNullOrWhiteSpace(request.Score.Justification))
        {
            body.AppendLine("<p><strong>Fundamentação narrativa do motor de scoring:</strong></p>");
            body.AppendLine($"<p>{Enc(request.Score.Justification)}</p>");
        }

        if (stats.SanctionedCount > 0)
        {
            body.AppendLine("<div class=\"alert-warning\">");
            body.Append(
                $"<strong>Atenção:</strong> {stats.SanctionedCount} parte(s) com indício de correspondência em listas de sanções — revisão humana obrigatória antes de qualquer decisão de crédito.");
            body.AppendLine("</div>");
        }
    }

    private static void AppendSection7Recommendation(StringBuilder body, KycReportComposeRequest request, SummaryStats stats)
    {
        body.AppendLine("<h3>7. Implicação para decisão</h3>");
        body.AppendLine($"<p>{RecommendedActionHtml(request, stats)}</p>");
        body.AppendLine(
            "<p class=\"footnote\">Esta síntese é gerada automaticamente a partir dos dados e regras da plataforma; não substitui o juízo do analista de compliance ou de crédito.</p>");
    }

    private static SummaryStats Analyze(KycReportComposeRequest request)
    {
        var parties = request.Parties;
        var signals = request.Signals;

        var bySeverity = signals
            .GroupBy(s => NormalizeSeverity(s.Severity))
            .ToDictionary(g => g.Key, g => g.Count());

        var byType = signals
            .GroupBy(s => s.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        var pepParties = parties.Where(p => p.IsPep).ToList();
        var sanctionedParties = parties.Where(p => p.IsSanctioned).ToList();

        return new SummaryStats(
            GeneratedLocal: request.GeneratedAtUtc.ToLocalTime(),
            PartyCount: parties.Count,
            TargetCount: parties.Count(p => IsRole(p.Role, "Target")),
            ShareholderCount: parties.Count(p => IsRole(p.Role, "Shareholder")),
            UboCount: parties.Count(p => IsRole(p.Role, "Ubo")),
            BoardCount: parties.Count(p => IsRole(p.Role, "BoardMember")),
            OtherRoleCount: parties.Count(p =>
                !IsRole(p.Role, "Target") && !IsRole(p.Role, "Shareholder") && !IsRole(p.Role, "Ubo") &&
                !IsRole(p.Role, "BoardMember")),
            MaxPartyDepth: parties.Count == 0 ? 0 : parties.Max(p => p.Depth),
            PepCount: pepParties.Count,
            SanctionedCount: sanctionedParties.Count,
            PepNames: pepParties.Select(p => p.Name).Distinct().Take(15).ToList(),
            SanctionedNames: sanctionedParties.Select(p => p.Name).Distinct().Take(15).ToList(),
            MissingUbo: parties.All(p => !IsRole(p.Role, "Ubo")),
            SignalCount: signals.Count,
            BySeverity: bySeverity,
            ByType: byType,
            Sources: signals.Select(s => s.Source).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s).ToList(),
            OrderedSignals: signals
                .OrderByDescending(s => SeverityRank(s.Severity))
                .ThenBy(s => s.Type)
                .ThenBy(s => s.Description)
                .ToList(),
            HighOrCriticalCount: signals.Count(s =>
                s.Severity.Equals("High", StringComparison.OrdinalIgnoreCase) ||
                s.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase)),
            InconsistencyCount: signals.Count(s =>
                s.Type.Contains("Inconsistency", StringComparison.OrdinalIgnoreCase)),
            InconsistencySignals: signals
                .Where(s => s.Type.Contains("Inconsistency", StringComparison.OrdinalIgnoreCase))
                .ToList());
    }

    private static void AppendDimensionRow(
        StringBuilder body,
        string label,
        int? value,
        bool isOverall,
        RiskLevel? level)
    {
        var scoreText = value.HasValue ? $"{value.Value}" : "n/d";
        var interpretation = value.HasValue
            ? DimensionInterpretation(value.Value)
            : "Sem dados de fonte — dimensão não calculada.";
        if (isOverall && level.HasValue)
            interpretation =
                $"Classificação final: {RiskLevelLabel(level.Value)} ({ThresholdLabel(level.Value)}). {interpretation}";

        var css = isOverall ? "score-global" : string.Empty;
        body.AppendLine(
            $"<tr><td><strong>{Enc(label)}</strong></td><td class=\"{css}\"><strong>{scoreText}</strong></td><td>{Enc(interpretation)}</td></tr>");
    }

    private static string DimensionInterpretation(int score) => score switch
    {
        <= 15 => "Contribuição de risco muito baixa nesta dimensão.",
        <= 30 => "Contribuição de risco baixa; sem alertas materiais.",
        <= 60 => "Contribuição moderada; validar com analista.",
        <= 80 => "Contribuição elevada; requer mitigação ou escalonamento.",
        _ => "Contribuição crítica; bloquear até investigação reforçada."
    };

    private static string ThresholdLabel(RiskLevel level) => level switch
    {
        RiskLevel.Low => "≤ 30",
        RiskLevel.Medium => "31–60",
        RiskLevel.High => "61–80",
        RiskLevel.Critical => "> 80",
        _ => string.Empty
    };

    private static string RecommendedActionHtml(KycReportComposeRequest request, SummaryStats stats)
    {
        var hasHigh = stats.HighOrCriticalCount > 0;
        var text = request.Score.Level switch
        {
            RiskLevel.Low when !hasHigh =>
                "Com score baixo e ausência de sinais altos/críticos, a política automática permite prosseguir com diligência standard e monitorização periódica; o caso pode ter sido encaminhado para auto-aprovação se não houver impedimentos adicionais.",
            RiskLevel.Medium =>
                "Score médio: recomenda-se revisão humana antes de decisão, confirmando sinais médios e documentando mitigantes.",
            RiskLevel.High =>
                "Score alto: escalar para compliance sénior; não aprovar crédito até esclarecimento dos sinais elevados.",
            RiskLevel.Critical =>
                "Score crítico: não prosseguir sem comité de risco; bloquear relação até conclusão de investigação reforçada.",
            _ => "Revisão humana obrigatória — score ou sinais exigem validação pelo analista responsável."
        };

        return $"<strong>{Enc(text)}</strong>";
    }

    private static int SeverityRank(string severity) => NormalizeSeverity(severity) switch
    {
        "Critical" => 4,
        "High" => 3,
        "Medium" => 2,
        _ => 1
    };

    private static string NormalizeSeverity(string severity) =>
        severity.Equals("Critical", StringComparison.OrdinalIgnoreCase) ? "Critical" :
        severity.Equals("High", StringComparison.OrdinalIgnoreCase) ? "High" :
        severity.Equals("Medium", StringComparison.OrdinalIgnoreCase) ? "Medium" : "Low";

    private static bool IsRole(string role, string expected) =>
        role.Equals(expected, StringComparison.OrdinalIgnoreCase);

    private static string Enc(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);

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

    private static string SeverityBadgeClass(string severity) => severity switch
    {
        "High" or "Critical" => "badge badge-high",
        "Medium" => "badge badge-medium",
        _ => "badge badge-low"
    };

    private sealed record SummaryStats(
        DateTime GeneratedLocal,
        int PartyCount,
        int TargetCount,
        int ShareholderCount,
        int UboCount,
        int BoardCount,
        int OtherRoleCount,
        int MaxPartyDepth,
        int PepCount,
        int SanctionedCount,
        IReadOnlyList<string> PepNames,
        IReadOnlyList<string> SanctionedNames,
        bool MissingUbo,
        int SignalCount,
        IReadOnlyDictionary<string, int> BySeverity,
        IReadOnlyDictionary<string, int> ByType,
        IReadOnlyList<string> Sources,
        IReadOnlyList<RiskSignalScanDto> OrderedSignals,
        int HighOrCriticalCount,
        int InconsistencyCount,
        IReadOnlyList<RiskSignalScanDto> InconsistencySignals);
}
