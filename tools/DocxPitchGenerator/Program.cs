using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

internal static class Program
{
    private const string Calibri = "Calibri";

    private static void Main(string[] args)
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var outDir = Path.Combine(repoRoot, "docs", "pitch");
        if (args.Length > 0)
            outDir = Path.GetFullPath(args[0]);

        Directory.CreateDirectory(outDir);
        WritePitch(Path.Combine(outDir, "KYC-Platform-Investor-Brief-EN.docx"), BuildEnglish());
        WritePitch(Path.Combine(outDir, "KYC-Platform-Investor-Brief-PT.docx"), BuildPortuguese());
        Console.WriteLine(
            $"Created:\n  {Path.Combine(outDir, "KYC-Platform-Investor-Brief-EN.docx")}\n  {Path.Combine(outDir, "KYC-Platform-Investor-Brief-PT.docx")}");
    }

    private static void WritePitch(string path, IReadOnlyList<Paragraph> paragraphs)
    {
        if (File.Exists(path))
            File.Delete(path);
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var main = doc.AddMainDocumentPart();
        main.Document = new Document();
        var body = main.Document.AppendChild(new Body());
        foreach (var p in paragraphs)
            body.AppendChild(p);
        main.Document.Save();
    }

    private static Paragraph P(string text, ParaKind kind = ParaKind.Body)
    {
        var props = new ParagraphProperties();
        props.AppendChild(new SpacingBetweenLines { After = kind == ParaKind.Title ? "360" : "200" });
        if (kind is ParaKind.Subtitle)
            props.AppendChild(new Justification { Val = JustificationValues.Center });
        if (kind == ParaKind.Title)
            props.AppendChild(new Justification { Val = JustificationValues.Center });

        var runProps = new RunProperties();
        runProps.AppendChild(new RunFonts { Ascii = Calibri, HighAnsi = Calibri });
        if (kind is ParaKind.Title or ParaKind.Section)
            runProps.AppendChild(new Bold());
        if (kind == ParaKind.Title)
            runProps.AppendChild(new FontSize { Val = "40" });
        else if (kind == ParaKind.Section)
            runProps.AppendChild(new FontSize { Val = "28" });
        else if (kind == ParaKind.Subtitle)
            runProps.AppendChild(new Italic());
        else
            runProps.AppendChild(new FontSize { Val = "22" });

        return new Paragraph(
            props,
            new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    }

    private static IReadOnlyList<Paragraph> BuildEnglish() =>
    [
        P("KYC AI Credit Intelligence Platform", ParaKind.Title),
        P("Investor brief — capabilities & market positioning (non-technical overview)", ParaKind.Subtitle),
        P("Document version: May 2026 · Confidential", ParaKind.Subtitle),

        P("Executive summary", ParaKind.Section),
        P("We are building a decision-support platform for corporate credit and compliance teams that need to onboard and monitor borrowers—especially in emerging markets—without drowning in manual research, fragmented data sources, or inconsistent narratives. The product combines structured screening (sanctions, corporate identity, signals) with AI-assisted synthesis so analysts get a clear story, faster, with an audit trail suitable for regulated environments."),

        P("The problem we address", ParaKind.Section),
        P("Medium and high-ticket corporate lending still relies heavily on analyst time: pulling identifiers, reconciling company names across jurisdictions, checking watchlists, scanning news, and writing a coherent risk memo. In developing economies, official registries and premium data vendors are often incomplete, expensive, or slow to integrate—while regulators still expect defensible documentation."),
        P("Banks, credit funds, trade-finance desks, and fintech lenders all feel the same tension: speed and cost pressure on one side; prudence and auditability on the other."),

        P("Our solution (in one line)", ParaKind.Section),
        P("A cloud-ready KYC / credit intelligence workflow that starts from a commercial identifier, enriches the borrower profile from authoritative and open global sources, runs parallel risk scans, and produces structured, reviewer-ready output—with optional local AI for cost control and data residency considerations."),

        P("What the platform delivers — business capabilities", ParaKind.Section),
        P("• Corporate identity & footprint: resolve who you are really lending to using LEI (GLEIF) and open knowledge bases (Wikidata), layered with local registry connectors where available—so you are not locked to a single paid aggregator."),
        P("• Ownership & control view: map parties and synthetic UBO-style structures to highlight concentration and complexity (with clear caveats where public data ends)."),
        P("• Sanctions & watchlist screening: integrate OFAC / EU-style lists through your chosen endpoints; designed for policy-driven escalation rather than “black box” scoring."),
        P("• Intelligence modules (adverse media, financial health, judicial/offshore signals): modular services that can be switched on as your data contracts mature."),
        P("• AI-assisted narrative: combine local LLM triage with cloud models for final synthesis—helping teams draft consistent, explainable memos instead of blank-page work."),
        P("• Operations & trust: case workflow, messaging for asynchronous processing, PDF reporting, embeddings for search, and retention controls aligned to your policy."),

        P("Who we serve (initial ICP)", ParaKind.Section),
        P("• Credit teams underwriting SMEs and mid-caps with cross-border exposure."),
        P("• Risk & compliance functions modernising KYC for lending, not only for payments."),
        P("• Regional banks and alternative lenders in emerging markets that need pragmatic, cost-aware tooling."),

        P("Why this can win", ParaKind.Section),
        P("• Pragmatic data strategy: start with high-quality open identifiers (LEI) and Wikidata, then deepen with national registries and paid sources as you scale—reducing vendor lock-in at the MVP stage."),
        P("• Architecture built for growth: clean separation of domain logic, integrations, and UI; ready for Azure enterprise patterns (identity, secrets, messaging)."),
        P("• Analyst-first UX: the goal is faster decisions with better documentation—not opaque automation that compliance cannot defend."),

        P("Product maturity & roadmap (honest framing)", ParaKind.Section),
        P("The codebase demonstrates an end-to-end backbone: case creation, external enrichment clients, screening services, pipeline orchestration, LLM integration points, and reporting. Some external feeds are stubbed or configurable for pilot deployments. Near-term roadmap typically includes: deepening registry coverage by country, tuning model prompts and evaluation sets, hardening SLAs, and expanding integrations with core banking / LOS systems."),

        P("Commercial model (illustrative)", ParaKind.Section),
        P("Typical go-to-market options include SaaS per seat or per case bundle, enterprise licensing with professional services for integrations, and tiered add-ons for premium data sources. Final packaging depends on segment and regulatory posture in each market."),

        P("Investment highlights (for discussion)", ParaKind.Section),
        P("• Large, recurring pain in corporate credit onboarding and periodic review."),
        P("• Differentiation through workflow + explainable AI + pragmatic open data—not generic “chat for compliance.”"),
        P("• Clear expansion path from MVP integrations to enterprise-grade data and hosting."),

        P("Disclaimer", ParaKind.Section),
        P("This document is for informational discussion with qualified parties only. It does not constitute an offer to sell securities. Features described reflect current or planned product direction and may change. AI-generated content must always be reviewed by qualified staff; the platform is designed to assist, not replace, human judgment in credit and compliance decisions."),
    ];

    private static IReadOnlyList<Paragraph> BuildPortuguese() =>
    [
        P("Plataforma KYC AI — Inteligência de crédito", ParaKind.Title),
        P("Briefing para investidores — funcionalidades e posicionamento (visão não técnica)", ParaKind.Subtitle),
        P("Versão do documento: maio de 2026 · Confidencial", ParaKind.Subtitle),

        P("Resumo executivo", ParaKind.Section),
        P("Estamos a desenvolver uma plataforma de apoio à decisão para equipas de crédito corporativo e compliance que precisam de integrar e monitorizar tomadores—em particular em mercados emergentes—sem ficar reféns de pesquisa manual, fontes dispersas ou narrativas inconsistentes. O produto combina triagem estruturada (sanções, identidade empresarial, sinais de risco) com síntese assistida por IA, para que os analistas obtenham uma história clara mais depressa, com trilho de auditoria adequado a contextos regulados."),

        P("O problema que endereçamos", ParaKind.Section),
        P("Operações de crédito médio e elevado continuam muito dependentes do tempo do analista: validar identificadores, conciliar denominações sociais entre jurisdições, consultar listas, rever notícias e redigir um memorando de risco coerente. Em economias em desenvolvimento, os registos oficiais e fornecedores premium de dados são por vezes incompletos, caros ou lentos a integrar—enquanto o supervisor continua a exigir documentação defendível."),
        P("Bancos, fundos de crédito, mesas de comércio internacional e fintechs de empréstimo sentem a mesma tensão: pressão de prazo e custo de um lado; prudência e auditabilidade do outro."),

        P("A nossa solução (numa frase)", ParaKind.Section),
        P("Um fluxo de trabalho KYC / inteligência de crédito preparado para cloud, que parte de um identificador comercial, enriquece o perfil do tomador com fontes globais abertas e autoritativas, executa verificações de risco em paralelo e produz um resultado estruturado, pronto para revisão humana—com IA local opcional para controlo de custos e considerações de residência de dados."),

        P("O que a plataforma oferece — capacidades de negócio", ParaKind.Section),
        P("• Identidade e pegada empresarial: identificar a entidade subjacente ao empréstimo via LEI (GLEIF) e bases abertas (Wikidata), complementada por conectores a registos nacionais quando disponíveis—sem dependência exclusiva de um agregador pago."),
        P("• Visão de propriedade e controlo: mapeamento de partes e estruturas tipo UBO para evidenciar concentração e complexidade (com transparência onde os dados públicos terminam)."),
        P("• Triagem de sanções e listas: integração com listas estilo OFAC/UE através dos endpoints que a instituição definir; desenhado para escalação orientada por política, não por “caixa negra”."),
        P("• Módulos de inteligência (media adversa, saúde financeira, sinais judiciais/offshore): serviços modulares activáveis à medida que os contratos de dados amadurecem."),
        P("• Narrativa assistida por IA: combinação de LLM local para pré-triagem com modelos cloud para síntese final—ajudando a produzir memorandos consistentes e explicáveis."),
        P("• Operações e confiança: casos, mensagens para processamento assíncrono, relatórios PDF, embeddings para pesquisa e controlos de retenção alinhados com a política interna."),

        P("Para quem é (ICP inicial)", ParaKind.Section),
        P("• Equipas de crédito que analisam PME e médias empresas com exposição transfronteiriça."),
        P("• Funções de risco e compliance que modernizam o KYC para concessão de crédito, não só para pagamentos."),
        P("• Bancos regionais e prestadores alternativos em mercados emergentes que precisam de ferramentas pragmáticas e conscientes do custo."),

        P("Porque pode ganhar tração", ParaKind.Section),
        P("• Estratégia de dados pragmática: começar com identificadores abertos de elevada qualidade (LEI) e Wikidata, aprofundando depois com registos nacionais e fontes pagas—reduzindo lock-in na fase MVP."),
        P("• Arquitectura preparada para escalar: separação clara entre lógica de domínio, integrações e interface; alinhada a padrões enterprise em Azure (identidade, segredos, mensagens)."),
        P("• Experiência centrada no analista: o objectivo é acelerar decisões com melhor documentação—não automação opaca que o compliance não consiga sustentar perante o regulador."),

        P("Maturidade do produto e roadmap (enquadramento transparente)", ParaKind.Section),
        P("O código demonstra um backbone extremo-a-extremo: abertura de caso, clientes de enriquecimento externos, serviços de triagem, orquestração de pipeline, pontos de integração LLM e reporting. Algumas fontes externas podem estar simuladas ou configuráveis para pilotos. O roadmap típico próximo inclui: maior cobertura de registos por país, afinação de prompts e conjuntos de avaliação, endurecimento de SLAs e integrações com core banking / LOS."),

        P("Modelo comercial (ilustrativo)", ParaKind.Section),
        P("Opções usuais de go-to-market incluem SaaS por lugar ou pacote por caso, licenciamento enterprise com serviços profissionais para integrações, e complementos por camadas de dados premium. O empacotamento final depende do segmento e do enquadramento regulador em cada mercado."),

        P("Destaques para investimento (para discussão)", ParaKind.Section),
        P("• Dor grande e recorrente no onboarding e na revisão periódica de crédito corporativo."),
        P("• Diferenciação por fluxo de trabalho + IA explicável + dados abertos pragmáticos—não um “chat genérico para compliance.”"),
        P("• Trajectória clara desde integrações MVP até dados e alojamento enterprise-grade."),

        P("Exoneração de responsabilidade", ParaKind.Section),
        P("Este documento destina-se apenas a discussão informativa com partes qualificadas. Não constitui oferta de venda de valores mobiliários. As funcionalidades descritas reflectem a direcção actual ou planeada do produto e podem alterar-se. Conteúdo assistido por IA deve ser sempre revisto por pessoal qualificado; a plataforma visa apoiar, e não substituir, o julgamento humano em decisões de crédito e compliance."),
    ];

    private enum ParaKind
    {
        Title,
        Subtitle,
        Section,
        Body
    }
}
