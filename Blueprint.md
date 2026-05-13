# KYC AI Platform — Cursor AI Blueprint

> **Stack:** .NET 9 · Blazor Server · PostgreSQL 16 + pgvector · Azure Service Bus · Semantic Kernel · Ollama (Qwen3.5:9b) · Anthropic API (Claude Sonnet)  
> **Auth:** Microsoft Entra ID (OIDC)  
> **Infra:** Azure App Service · Azure Container Registry · Azure Key Vault  
> **Versão:** 1.0 · Maio 2026

---

## Como usar este documento com o Cursor

1. Coloca este ficheiro na raiz do repositório como `BLUEPRINT.md`
2. Abre o Cursor e inicia uma conversa com `@BLUEPRINT.md`
3. Usa os prompts de cada secção para guiar o Cursor passo a passo
4. O Cursor deve seguir a arquitectura Clean Architecture descrita abaixo

---

## 1. Visão Geral do Sistema

A plataforma KYC AI avalia o risco de crédito corporativo analisando automaticamente:

- A **entidade tomadora** (empresa que pede crédito)
- Todos os **sócios, administradores e UBOs** (Beneficiários Efectivos) até N níveis
- Cruzamento com **listas de sanções, PEP, adverse media, dados judiciais e financeiros**
- Geração de **relatório narrativo de risco** via LLM (Claude API + Qwen3.5 local)

### Fluxo principal

```
Input (NIF/NIPC) 
  → Entity Resolution (RCBE + OpenCorporates)
  → UBO Graph (recursivo N níveis)
  → Parallel Scan: [Sanctions | Adverse Media | Financial | Judicial]
  → LLM Synthesis (Qwen3.5 pré-triagem → Claude relatório final)
  → Risk Score (0–100) + Relatório Narrativo
  → Workflow: Auto-Approve / Revisão Humana / Rejeição
  → Audit Trail (append-only)
```

---

## 2. Arquitectura — Clean Architecture

```
src/
├── KYC.Domain/                  # Entidades, Value Objects, Domain Events
├── KYC.Application/             # Use Cases, Interfaces, DTOs, Commands/Queries
├── KYC.Infrastructure/          # EF Core, APIs externas, LLM, Service Bus
├── KYC.Web/                     # Blazor Server (UI, componentes, páginas)
└── KYC.Workers/                 # Background Services (Azure Service Bus consumers)

tests/
├── KYC.Domain.Tests/
├── KYC.Application.Tests/
└── KYC.Integration.Tests/
```

### Regras de dependência (NUNCA violar)

- `Domain` não depende de nada
- `Application` depende apenas de `Domain`
- `Infrastructure` depende de `Application` e `Domain`
- `Web` depende de `Application` (nunca de `Infrastructure` directamente)
- Workers dependem de `Application`

---

## 3. Modelo de Dados — Domain Entities

### 3.1 Entidades principais

```csharp
// KYC.Domain/Entities/KycCase.cs
public class KycCase
{
    public Guid Id { get; private set; }
    public string Nif { get; private set; }              // NIF/NIPC da empresa
    public string CompanyName { get; private set; }
    public KycStatus Status { get; private set; }        // Pending | InProgress | Approved | Rejected | UnderReview
    public RiskScore? Score { get; private set; }        // Value Object
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? AssignedAnalystId { get; private set; }
    public IReadOnlyList<Entity> EntitiesAnalysed { get; private set; }
    public IReadOnlyList<RiskSignal> RiskSignals { get; private set; }
    public IReadOnlyList<AuditEntry> AuditTrail { get; private set; }
    public KycReport? FinalReport { get; private set; }
}

// KYC.Domain/Entities/Entity.cs
public class Entity
{
    public Guid Id { get; private set; }
    public EntityType Type { get; private set; }         // Company | Individual | Trust | Foundation
    public string Name { get; private set; }
    public string? Nif { get; private set; }
    public string? Nationality { get; private set; }
    public EntityRole Role { get; private set; }         // Target | Shareholder | UBO | BoardMember | Proxy
    public decimal OwnershipPercentage { get; private set; }
    public int UboDepthLevel { get; private set; }       // 0 = target, 1 = direct shareholder, etc.
    public Guid? ParentEntityId { get; private set; }    // Para grafo de controlo
    public bool IsPep { get; private set; }
    public bool IsSanctioned { get; private set; }
    public bool IsOffshore { get; private set; }
    public string? OffshoreJurisdiction { get; private set; }
    public RiskScore? EntityScore { get; private set; }
    public IReadOnlyList<RiskSignal> Signals { get; private set; }
}

// KYC.Domain/ValueObjects/RiskScore.cs
public record RiskScore
{
    public int Overall { get; init; }                    // 0–100
    public int SanctionsScore { get; init; }
    public int PepScore { get; init; }
    public int AdverseMediaScore { get; init; }
    public int FinancialScore { get; init; }
    public int JudicialScore { get; init; }
    public int UboStructureScore { get; init; }
    public RiskLevel Level => Overall switch              // Low | Medium | High | Critical
    {
        <= 30 => RiskLevel.Low,
        <= 60 => RiskLevel.Medium,
        <= 80 => RiskLevel.High,
        _     => RiskLevel.Critical
    };
    public string Justification { get; init; }           // Gerado pelo LLM
}

// KYC.Domain/Entities/RiskSignal.cs
public class RiskSignal
{
    public Guid Id { get; private set; }
    public SignalType Type { get; private set; }         // Sanction | Pep | AdverseMedia | Judicial | Financial | UboAnomaly | Inconsistency
    public SignalSeverity Severity { get; private set; } // Low | Medium | High | Critical
    public string Description { get; private set; }
    public string Source { get; private set; }           // "OFAC", "EU List", "CITIUS", etc.
    public DateTime DetectedAt { get; private set; }
    public DateTime? EventDate { get; private set; }     // Data do evento (para timeline)
    public bool IsConfirmed { get; private set; }
    public string? AnalystNotes { get; private set; }
}

// KYC.Domain/Entities/AuditEntry.cs
public class AuditEntry
{
    public Guid Id { get; private set; }
    public Guid KycCaseId { get; private set; }
    public string Action { get; private set; }           // "ScanStarted", "SignalDetected", "AnalystOverride", etc.
    public string ActorId { get; private set; }          // userId ou "System"
    public string ActorType { get; private set; }        // "User" | "LLM" | "Agent"
    public string? Details { get; private set; }         // JSON com contexto
    public string? LlmPromptHash { get; private set; }   // SHA256 do prompt usado (auditabilidade IA)
    public DateTime Timestamp { get; private set; }
    // NUNCA editar — append-only. Sem Update nem Delete no repositório.
}
```

---

## 4. Application Layer — Use Cases & Interfaces

### 4.1 Interfaces de infra (definidas em Application, implementadas em Infrastructure)

```csharp
// KYC.Application/Interfaces/IEntityResolutionService.cs
public interface IEntityResolutionService
{
    Task<EntityResolutionResult> ResolveByNifAsync(string nif, CancellationToken ct = default);
    Task<UboGraph> BuildUboGraphAsync(string nif, int maxDepth = 5, CancellationToken ct = default);
}

// KYC.Application/Interfaces/ISanctionsScreeningService.cs
public interface ISanctionsScreeningService
{
    Task<SanctionsResult> ScreenEntityAsync(Entity entity, CancellationToken ct = default);
    Task<SanctionsResult> ScreenByNameAsync(string name, string? nationality = null, CancellationToken ct = default);
}

// KYC.Application/Interfaces/IAdverseMediaService.cs
public interface IAdverseMediaService
{
    Task<AdverseMediaResult> ScanAsync(string entityName, string? nif = null, CancellationToken ct = default);
}

// KYC.Application/Interfaces/IFinancialHealthService.cs
public interface IFinancialHealthService
{
    Task<FinancialHealthResult> AnalyseAsync(string nif, CancellationToken ct = default);
}

// KYC.Application/Interfaces/IJudicialIntelligenceService.cs
public interface IJudicialIntelligenceService
{
    Task<JudicialResult> SearchAsync(string nif, string name, CancellationToken ct = default);
}

// KYC.Application/Interfaces/IKycLlmEngine.cs
public interface IKycLlmEngine
{
    Task<RiskScore> ComputeRiskScoreAsync(KycScanContext context, CancellationToken ct = default);
    Task<KycReport> GenerateNarrativeReportAsync(KycScanContext context, RiskScore score, CancellationToken ct = default);
    Task<ConsistencyCheckResult> CheckConsistencyAsync(KycScanContext context, CancellationToken ct = default);
    Task<bool> IsLlmHealthyAsync(CancellationToken ct = default);
}

// KYC.Application/Interfaces/IKycCaseRepository.cs
public interface IKycCaseRepository
{
    Task<KycCase?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<KycCase?> GetByNifAsync(string nif, CancellationToken ct = default);
    Task<PagedResult<KycCase>> ListAsync(KycCaseFilter filter, CancellationToken ct = default);
    Task AddAsync(KycCase kycCase, CancellationToken ct = default);
    Task UpdateAsync(KycCase kycCase, CancellationToken ct = default);
    // SEM DeleteAsync — casos KYC são imutáveis por regulamentação (AMLD6)
}
```

### 4.2 Use Cases (CQRS com MediatR)

```csharp
// Comandos
public record StartKycCaseCommand(string Nif, string RequestedBy, CreditAmount RequestedAmount) : IRequest<Guid>;
public record ApproveKycCaseCommand(Guid CaseId, string AnalystId, string Notes) : IRequest<Unit>;
public record RejectKycCaseCommand(Guid CaseId, string AnalystId, string Reason) : IRequest<Unit>;
public record RequestManualReviewCommand(Guid CaseId, string Reason) : IRequest<Unit>;
public record OverrideSignalCommand(Guid SignalId, string AnalystId, bool Confirm, string Notes) : IRequest<Unit>;

// Queries
public record GetKycCaseQuery(Guid CaseId) : IRequest<KycCaseDto>;
public record ListKycCasesQuery(KycCaseFilter Filter) : IRequest<PagedResult<KycCaseDto>>;
public record GetUboGraphQuery(Guid CaseId) : IRequest<UboGraphDto>;
public record GetRiskTimelineQuery(Guid CaseId) : IRequest<RiskTimelineDto>;
public record GetKycReportQuery(Guid CaseId) : IRequest<KycReportDto>;
```

---

## 5. Infrastructure Layer

### 5.1 Database — EF Core + PostgreSQL + pgvector

```csharp
// KYC.Infrastructure/Persistence/KycDbContext.cs
public class KycDbContext : DbContext
{
    public DbSet<KycCase> KycCases => Set<KycCase>();
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<RiskSignal> RiskSignals => Set<RiskSignal>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<KycReport> KycReports => Set<KycReport>();
    public DbSet<ReportEmbedding> ReportEmbeddings => Set<ReportEmbedding>(); // pgvector

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.HasPostgresExtension("vector");
        mb.ApplyConfigurationsFromAssembly(typeof(KycDbContext).Assembly);
        // AuditEntries: sem Update/Delete — enforced aqui
        mb.Entity<AuditEntry>().ToTable(tb => tb.HasCheckConstraint("CK_AuditEntry_Immutable", "1=1"));
    }
}

// KYC.Infrastructure/Persistence/Configurations/AuditEntryConfiguration.cs
// Mapear vector(1536) para ReportEmbedding usando Pgvector
public class ReportEmbedding
{
    public Guid Id { get; set; }
    public Guid KycCaseId { get; set; }
    public Vector Embedding { get; set; }    // Pgvector.Vector
    public string ContentChunk { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Migrations:**
```bash
dotnet ef migrations add InitialCreate --project KYC.Infrastructure --startup-project KYC.Web
dotnet ef database update --project KYC.Infrastructure --startup-project KYC.Web
```

### 5.2 LLM Engine — Semantic Kernel + Dual LLM

```csharp
// KYC.Infrastructure/LLM/KycLlmEngine.cs
// Arquitectura:
// 1. Pré-triagem → Qwen3.5:9b via Ollama (local, privado, custo zero)
// 2. Relatório final → Claude Sonnet via Anthropic API (casos high/critical risk)
// 3. Fallback: se Claude API indisponível → Qwen3.5 para relatório básico

public class KycLlmEngine : IKycLlmEngine
{
    private readonly Kernel _localKernel;    // Ollama Qwen3.5:9b
    private readonly Kernel _cloudKernel;   // Claude Sonnet (Anthropic)
    private readonly ILogger<KycLlmEngine> _logger;

    // Prompt de scoring — Cursor deve implementar com estas secções:
    // [SYSTEM] Role: Senior KYC Risk Analyst especializado em crédito corporativo EU
    // [CONTEXT] Dados recolhidos: entidade, UBOs, sinais de risco, fontes consultadas
    // [TASK] Calcular score 0-100 por dimensão + justificação em PT
    // [FORMAT] JSON estruturado: { overall, dimensions: {...}, justification, recommendation }
    // [CONSTRAINTS] Nunca inventar dados. Se fonte ausente, score = null para essa dimensão.

    // Prompt de relatório narrativo — estrutura esperada:
    // 1. Sumário Executivo (3 parágrafos)
    // 2. Análise da Entidade Tomadora
    // 3. Análise da Estrutura de Controlo (UBO chain)
    // 4. Sinais de Risco Identificados (por dimensão)
    // 5. Timeline de Incidentes
    // 6. Análise de Consistência (declarado vs. encontrado)
    // 7. Score de Risco Detalhado
    // 8. Recomendação de Decisão com Fundamentação
    // 9. Fontes Consultadas e Limitações
}
```

**Configuração no appsettings.json:**
```json
{
  "LLM": {
    "LocalEndpoint": "http://localhost:11434",
    "LocalModel": "qwen3.5:9b",
    "CloudModel": "claude-sonnet-4-20250514",
    "UseCloudForRiskLevel": "High",
    "MaxTokensReport": 4000,
    "MaxTokensScore": 1000
  },
  "Anthropic": {
    "ApiKeySecretName": "anthropic-api-key"
  }
}
```

### 5.3 Azure Service Bus — Orquestração assíncrona

```csharp
// Queues/Topics a criar no Azure Service Bus:
// kyc-case-started          → dispara scan completo
// kyc-entity-scan           → scan individual de uma entidade
// kyc-sanctions-results     → resultados de sanctions screening
// kyc-adversemedia-results  → resultados de adverse media
// kyc-financial-results     → resultados de financial health
// kyc-judicial-results      → resultados de judicial intelligence
// kyc-llm-synthesis         → trigger para síntese LLM final
// kyc-report-ready          → notificação para analista

// KYC.Workers/Consumers/KycCaseStartedConsumer.cs
// Ao receber StartKycCase:
// 1. Resolve entidade principal via RCBE
// 2. Publica N mensagens kyc-entity-scan (uma por UBO encontrado)
// 3. Para cada entidade, publica em paralelo: sanctions + adversemedia + financial + judicial
// 4. Quando todos os scans terminam → publica kyc-llm-synthesis
// 5. Após síntese → publica kyc-report-ready

// Usar Saga Pattern (Choreography) com estado em PostgreSQL
// Tabela: KycCaseScanProgress (CaseId, TotalScans, CompletedScans, FailedScans)
```

### 5.4 Fontes de dados externas — HTTP Clients tipados

```csharp
// KYC.Infrastructure/ExternalSources/
// ├── RcbeClient.cs          → IRN / RCBE API (PT)
// ├── OpenCorporatesClient.cs → OpenCorporates REST API
// ├── OfacClient.cs          → OFAC SDN API (US Treasury)
// ├── EuSanctionsClient.cs   → EU Consolidated Sanctions List
// ├── IcijClient.cs          → ICIJ Offshore Leaks (GraphQL)
// ├── CitiusClient.cs        → CITIUS scraping (Playwright)
// └── NewsAggregatorClient.cs → Web news scraping + NLP

// Registo no DI (KYC.Infrastructure/DependencyInjection.cs):
services.AddHttpClient<IRcbeClient, RcbeClient>(c =>
    c.BaseAddress = new Uri(config["ExternalSources:RcbeBaseUrl"]!))
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
// Aplicar padrão idêntico para todos os clients
// Usar Polly para retry (3x) + circuit breaker + timeout (30s)
```

---

## 6. Web Layer — Blazor Server

### 6.1 Estrutura de páginas

```
KYC.Web/
├── Pages/
│   ├── Dashboard.razor          # KPIs, casos recentes, alertas
│   ├── Cases/
│   │   ├── CaseList.razor       # Lista paginada com filtros e search
│   │   ├── NewCase.razor        # Formulário de abertura (NIF + montante)
│   │   ├── CaseDetail.razor     # Vista completa do caso
│   │   └── CaseReport.razor     # Relatório narrativo final (PDF export)
│   ├── Entities/
│   │   ├── EntityDetail.razor   # Perfil de uma entidade (empresa ou pessoa)
│   │   └── UboGraph.razor       # Visualização do grafo de controlo (D3.js)
│   ├── Admin/
│   │   ├── Users.razor          # Gestão de utilizadores e roles
│   │   ├── Settings.razor       # Configurações da plataforma
│   │   └── AuditLog.razor       # Audit trail global (read-only)
│   └── Auth/
│       └── Login.razor          # Redirect para Entra ID OIDC
├── Components/
│   ├── RiskScoreBadge.razor     # Badge colorido com score e nível
│   ├── SignalCard.razor         # Card de um sinal de risco
│   ├── RiskTimeline.razor       # Timeline cronológica de incidentes
│   ├── EntityCard.razor         # Card de pessoa/empresa com flags
│   ├── ScanProgressBar.razor    # Progresso do scan em tempo real
│   └── ConfirmDialog.razor      # Modal de confirmação (aprovação/rejeição)
└── Services/
    └── ToastService.cs          # Notificações in-app
```

### 6.2 Real-time updates — Blazor + SignalR

```csharp
// KYC.Web/Hubs/KycCaseHub.cs
// Grupos: "case-{caseId}" → analistas a ver aquele caso recebem updates em tempo real
// Eventos emitidos:
// - ScanProgressUpdated(caseId, module, percentComplete)
// - SignalDetected(caseId, signal)
// - ReportReady(caseId, riskLevel)
// - StatusChanged(caseId, newStatus)

// Blazor components subscrevem no OnInitializedAsync:
// await HubConnection.SendAsync("JoinCaseGroup", CaseId);
```

### 6.3 Autenticação — Microsoft Entra ID

```csharp
// Program.cs
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Analyst", p => p.RequireRole("KYC.Analyst"));
    options.AddPolicy("Supervisor", p => p.RequireRole("KYC.Supervisor"));
    options.AddPolicy("Admin", p => p.RequireRole("KYC.Admin"));
    options.AddPolicy("Auditor", p => p.RequireRole("KYC.Auditor")); // read-only
});

// Roles necessários no Entra ID App Registration:
// KYC.Analyst    → executa scans, vê casos, submete para revisão
// KYC.Supervisor → aprova/rejeita, override de sinais, 4-eyes
// KYC.Admin      → configurações, gestão de users, watchlists
// KYC.Auditor    → leitura total do audit trail (sem edições)
```

**appsettings.json:**
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "SEU_TENANT_ID",
    "ClientId": "SEU_CLIENT_ID",
    "ClientSecret": "USAR_AZURE_KEY_VAULT",
    "CallbackPath": "/signin-oidc"
  }
}
```

---

## 7. Segurança & Configuração

### 7.1 Azure Key Vault — todos os secrets aqui, nunca em appsettings

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());

// Secrets a criar no Key Vault:
// anthropic-api-key          → chave da Anthropic API
// db-connection-string       → PostgreSQL connection string
// servicebus-connection      → Azure Service Bus connection string
// azure-ad-client-secret     → Entra ID client secret
// rcbe-api-key               → IRN RCBE API key
// opencorporates-api-key     → OpenCorporates API key
```

### 7.2 GDPR — Data retention policies

```csharp
// KYC.Infrastructure/BackgroundJobs/DataRetentionJob.cs
// Executar diariamente via Hangfire ou Azure Functions Timer
// Regras de retenção (configuráveis por jurisdição):
// - Relatórios KYC aprovados: 10 anos (AMLD6 Art.40)
// - Casos rejeitados: 5 anos
// - Dados de PEPs: enquanto durar relação + 5 anos
// - Audit trail: 10 anos (imutável, nunca apagar)
// - Dados pessoais de terceiros (não clientes): 3 anos
// Após expiração: anonimizar (não apagar) — preserva estatísticas
```

### 7.3 Audit trail — Prompt auditável

```csharp
// Para cada chamada ao LLM, guardar em AuditEntry:
// - Hash SHA256 do system prompt + user prompt
// - Modelo utilizado (local vs cloud)
// - Timestamp
// - Versão do motor de scoring (semver)
// Isto é obrigatório para GDPR Art.22 (decisão automatizada explicável)
// e CRR III (fundamentação de decisões de crédito)
```

---

## 8. Testes

```csharp
// KYC.Application.Tests/
// - StartKycCaseCommandHandlerTests — happy path + NIF inválido + empresa já com caso aberto
// - RiskScoreCalculationTests — score correcto por dimensão, ponderações
// - UboGraphBuilderTests — detecção de ciclos, máximo de profundidade

// KYC.Domain.Tests/
// - KycCaseStateMachineTests — transições válidas e inválidas de status
// - RiskScoreValueObjectTests — cálculo de RiskLevel correcto

// KYC.Integration.Tests/
// - SanctionsScreeningIntegrationTests — mock das listas externas
// - LlmEngineIntegrationTests — smoke test com Ollama local
// - DatabaseIntegrationTests — usando Testcontainers (PostgreSQL)
```

---

## 9. Sequência de Implementação Recomendada

### Fase 1 — Fundação (Sprint 1–2)

```
[ ] Criar solução com os 5 projectos (Domain, Application, Infrastructure, Web, Workers)
[ ] Configurar EF Core + PostgreSQL + Migrations iniciais
[ ] Implementar entidades de domínio e repositórios
[ ] Configurar Entra ID OIDC + políticas de autorização
[ ] Configurar Azure Key Vault + DefaultAzureCredential
[ ] Scaffold do Blazor Server (layout, routing, autenticação)
[ ] Pipeline CI/CD básico (GitHub Actions → Azure Container Registry)
```

### Fase 2 — Core KYC Engine (Sprint 3–5)

```
[ ] Implementar StartKycCaseCommandHandler com MediatR
[ ] Integrar RCBE API (entity resolution)
[ ] Implementar UBO graph builder (recursivo)
[ ] Integrar OFAC + EU Sanctions (sanctions screening)
[ ] Configurar Azure Service Bus (queues + consumers)
[ ] Implementar Saga de orquestração dos scans paralelos
[ ] Integrar Qwen3.5:9b via Ollama (risk scoring local)
[ ] Implementar audit trail append-only
```

### Fase 3 — IA & Relatório (Sprint 6–7)

```
[ ] Integrar Claude Sonnet API (relatório narrativo)
[ ] Implementar lógica de roteamento LLM (local vs cloud por risk level)
[ ] Prompt engineering do relatório narrativo (8 secções)
[ ] Implementar consistency check (declarado vs. encontrado)
[ ] Score de coerência
[ ] Timeline de incidentes gerada pelo LLM
[ ] Armazenar embeddings de relatórios no pgvector
```

### Fase 4 — UI & Workflow (Sprint 8–9)

```
[ ] Dashboard com KPIs em tempo real (SignalR)
[ ] CaseDetail com progresso de scan ao vivo
[ ] UboGraph interactivo (D3.js ou Blazor component)
[ ] Workflow de aprovação/rejeição com 4-eyes principle
[ ] Export PDF do relatório
[ ] Audit log page (read-only para KYC.Auditor)
```

### Fase 5 — Fontes adicionais & Compliance (Sprint 10–12)

```
[ ] Adverse media (web scraping + NLP com Playwright)
[ ] Financial health (dados AT públicos + Z-Score)
[ ] Judicial intelligence (CITIUS)
[ ] ICIJ Offshore Leaks integration
[ ] Data retention job (GDPR)
[ ] Penetration testing + DORA compliance checklist
[ ] Documentação de conformidade regulatória
```

---

## 10. Prompts para o Cursor

### Prompt inicial (para começar do zero)

```
@BLUEPRINT.md

Cria a estrutura de solução .NET 9 para o KYC AI Platform conforme descrito neste blueprint.

Começa pela Fase 1:
1. Cria os 5 projectos com as dependências correctas (Domain, Application, Infrastructure, Web, Workers)
2. Implementa as entidades de domínio da secção 3.1 com os construtores privados e factory methods adequados
3. Configura o KycDbContext com EF Core e PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL)
4. Adiciona a extensão pgvector ao context
5. Cria as migrations iniciais

Usa C# 13, .NET 9, records imutáveis onde possível, e nullable reference types activados.
Segue estritamente as regras de dependência da secção 2.
```

### Prompt para o LLM Engine

```
@BLUEPRINT.md

Implementa o KycLlmEngine (secção 5.2) usando Microsoft Semantic Kernel 1.x.

Requisitos:
- Kernel local configurado com Ollama endpoint + modelo qwen3.5:9b
- Kernel cloud configurado com Anthropic API (usar OpenAI-compatible interface via Azure Key Vault)
- Método ComputeRiskScoreAsync: usa sempre o kernel local, retorna JSON estruturado
- Método GenerateNarrativeReportAsync: usa kernel cloud se RiskLevel >= High, caso contrário local
- Cada chamada regista no ILogger: modelo usado, tokens consumidos, duração
- Cada prompt é hashed (SHA256) e devolvido para auditoria
- Implementa retry com Polly (3x) e fallback para local se cloud falhar

Os prompts devem seguir a estrutura definida na secção 5.2.
```

### Prompt para o Blazor Dashboard

```
@BLUEPRINT.md

Implementa o Dashboard.razor (secção 6.1) com:

- 4 metric cards: Casos Abertos, Aprovados Hoje, Em Revisão, Taxa de Aprovação (%)
- Tabela de casos recentes (últimos 10): NIF, Empresa, Score, Status, Criado em
- Secção de alertas: sinais críticos detectados nas últimas 24h
- Updates em tempo real via SignalR (subscrever ao KycCaseHub)
- Filtro rápido por status e período
- Botão "Novo Caso" que navega para NewCase.razor

Usa os CSS variables do Blazor e componentes existentes (RiskScoreBadge, SignalCard).
Toda a data binding deve ser fortemente tipada com os DTOs de Application.
```

### Prompt para o Grafo UBO

```
@BLUEPRINT.md

Implementa o UboGraph.razor (secção 6.1) para visualizar o grafo de controlo corporativo.

Usa a biblioteca Blazor.Diagrams ou integra D3.js via JSInterop.

Requisitos do grafo:
- Nós coloridos por tipo: Empresa (azul), Pessoa (verde), Trust/Holding (amarelo), Offshore (vermelho)
- Espessura da seta proporcional à percentagem de participação
- Badge em cada nó: percentagem de ownership + flags (PEP, Sanctioned, Offshore)
- Click num nó abre painel lateral com EntityDetail
- Depth máximo configurável com slider (1–7 níveis)
- Export como PNG/SVG
- Legenda de cores sempre visível

Os dados vêm de GetUboGraphQuery via MediatR.
```

---

## 11. Variáveis de ambiente necessárias (local dev)

```bash
# .env.local (nunca commitar — adicionar ao .gitignore)
ASPNETCORE_ENVIRONMENT=Development
KYC_DB_CONNECTION="Host=localhost;Database=kyc_dev;Username=postgres;Password=dev123"
KYC_KEYVAULT_NAME="kyc-dev-kv"
KYC_SERVICEBUS_CONNECTION="Endpoint=sb://..."
KYC_OLLAMA_ENDPOINT="http://localhost:11434"
KYC_OLLAMA_MODEL="qwen3.5:9b"
AZURE_TENANT_ID="..."
AZURE_CLIENT_ID="..."
AZURE_CLIENT_SECRET="..."      # apenas dev — em prod usar Managed Identity
```

---

## 12. .cursorrules (colocar na raiz do repositório)

```
# KYC AI Platform — Cursor Rules

## Arquitectura
- NUNCA criar dependências de Infrastructure para Web ou vice-versa
- NUNCA injectar DbContext directamente em páginas Blazor — usar sempre MediatR
- NUNCA colocar lógica de negócio em Razor components — pertence a Use Cases
- SEMPRE usar construtores privados + factory methods nas entidades de domínio
- SEMPRE usar records para Value Objects e DTOs

## Segurança
- NUNCA colocar secrets em appsettings.json — sempre Azure Key Vault
- NUNCA loggar dados pessoais (NIF, nome, etc.) em níveis Information ou inferior
- SEMPRE sanitizar inputs de NIF (apenas dígitos, tamanho 9)
- SEMPRE validar autorização com [Authorize(Policy = "...")] nas páginas

## Audit Trail
- NUNCA adicionar Update ou Delete ao AuditEntryRepository
- SEMPRE registar acção em AuditEntry antes de qualquer decisão de risco
- SEMPRE incluir o hash do prompt LLM em entradas de auditoria geradas por IA

## GDPR
- NUNCA loggar conteúdo de relatórios KYC em logs aplicacionais
- SEMPRE aplicar políticas de retenção conforme DataRetentionJob
- SEMPRE anonimizar dados após expiração, nunca apagar registos de audit

## Código
- Usar C# 13 e .NET 9
- Nullable reference types sempre activados
- Async/await em toda a stack — sem bloqueios síncronos
- Usar CancellationToken em todos os métodos de infra
- Testes unitários para todos os Use Cases e Value Objects
```

---

*Este documento é o artefacto principal de desenvolvimento. Todas as decisões de arquitectura, stack e compliance estão aqui fundamentadas. Versão 1.0 — Maio 2026.*
