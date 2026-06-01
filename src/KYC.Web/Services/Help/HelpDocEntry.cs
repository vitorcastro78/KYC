namespace KYC.Web.Services.Help;

public sealed record HelpDocEntry(
    string Id,
    string Title,
    string FileName,
    string Section,
    string Description,
    string Icon = "oi-document",
    string? AppRoute = null,
    bool Technical = false,
    params string[] Keywords)
{
    public bool MatchesSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return true;

        return Title.Contains(query, StringComparison.OrdinalIgnoreCase)
               || Description.Contains(query, StringComparison.OrdinalIgnoreCase)
               || Section.Contains(query, StringComparison.OrdinalIgnoreCase)
               || FileName.Contains(query, StringComparison.OrdinalIgnoreCase)
               || Keywords.Any(k => k.Contains(query, StringComparison.OrdinalIgnoreCase));
    }
}

public static class HelpDocManifest
{
    public const string DefaultDocId = "inicio";

    public static IReadOnlyList<HelpDocEntry> UserManual { get; } =
    [
        new("acesso", "Acesso e perfis", "01-acesso-perfis.md", "Começar por aqui",
            "Como entrar, perfis Analyst/Supervisor e navegação no menu.", "oi-account-login",
            "/dashboard", false, "login", "entra", "roles", "perfil"),
        new("novo-caso", "Novo caso e triagem", "02-novo-caso-triagem.md", "Guia passo a passo",
            "Abrir caso, PAC, barra de progresso, sinais e re-triagem.", "oi-plus",
            "/cases/new", false, "triagem", "nif", "pac", "progresso"),
        new("conformidade", "Conformidade BdP", "03-conformidade-bdp.md", "Guia passo a passo",
            "Identidade, SAR, congelamento, EDD e RCBE.", "oi-shield",
            Keywords: ["sar", "uif", "ubo", "identidade", "edd", "congelamento"]),
        new("decisoes", "Decisões e relatórios", "04-decisoes-relatorios.md", "Guia passo a passo",
            "Aprovar, rejeitar, relatório PDF e upload de documentos.", "oi-check",
            Keywords: ["aprovar", "rejeitar", "pdf", "relatório", "documento"]),
        new("ecras", "Guia por ecrã", "05-guia-ecras.md", "Referência rápida",
            "O que encontrar no dashboard, lista, detalhe e admin.", "oi-layers",
            Keywords: ["dashboard", "lista", "detalhe", "ecrã"]),
        new("funcionalidades", "Funcionalidades", "06-funcionalidades.md", "Referência rápida",
            "Capacidades da plataforma organizadas por área de negócio.", "oi-list-rich",
            Keywords: ["catálogo", "funcionalidade", "módulo"]),
        new("problemas", "Resolução de problemas", "07-resolucao-problemas.md", "Ajuda e suporte",
            "Sintomas frequentes, causas e acções para analistas.", "oi-wrench",
            Keywords: ["erro", "falha", "bloqueado", "troubleshooting"]),
        new("faq", "Perguntas frequentes", "08-perguntas-frequentes.md", "Ajuda e suporte",
            "Respostas curtas às dúvidas mais comuns.", "oi-question-mark",
            Keywords: ["faq", "dúvida", "pergunta"]),
    ];

    public static IReadOnlyList<HelpDocEntry> Technical { get; } =
    [
        new("catalogo-tecnico", "Catálogo completo (homologação)", "CATALOGO_FUNCIONALIDADES.md", "Documentação técnica",
            "Inventário funcional com IDs e base legal.", "oi-spreadsheet", Technical: true),
        new("doc-aplicacao", "Documentação da aplicação", "DOCUMENTACAO_APLICACAO.md", "Documentação técnica",
            "Arquitectura, APIs e configuração.", "oi-code", Technical: true),
        new("operacoes", "Operações e homologação", "OPERACOES_E_HOMOLOGACAO.md", "Documentação técnica",
            "Deploy, E2E e evidências.", "oi-cog", Technical: true),
    ];

    public static IReadOnlyList<HelpDocEntry> All(bool includeTechnical) =>
        includeTechnical ? UserManual.Concat(Technical).ToList() : UserManual.ToList();

    public static HelpDocEntry? FindById(string? id, bool includeTechnical = true)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Equals(DefaultDocId, StringComparison.OrdinalIgnoreCase))
            return null;

        return All(includeTechnical).FirstOrDefault(d =>
            string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public static HelpDocEntry? FindByFileName(string fileName, bool includeTechnical = true) =>
        All(includeTechnical).FirstOrDefault(d =>
            string.Equals(d.FileName, fileName, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyDictionary<string, string> FileNameToIdMap(bool includeTechnical = true) =>
        All(includeTechnical).ToDictionary(d => d.FileName, d => d.Id, StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<IGrouping<string, HelpDocEntry>> UserManualBySection() =>
        UserManual.GroupBy(d => d.Section).OrderBy(g => SectionOrder(g.Key));

    private static int SectionOrder(string section) => section switch
    {
        "Começar por aqui" => 0,
        "Guia passo a passo" => 1,
        "Referência rápida" => 2,
        "Ajuda e suporte" => 3,
        "Documentação técnica" => 4,
        _ => 99
    };
}
