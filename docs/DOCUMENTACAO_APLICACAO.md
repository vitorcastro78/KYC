# Documentação da Aplicação — KYC AI Platform

> **Versão do documento:** Maio 2026 · Branch `feature/kyc-document-ingestion`  
> **Stack:** .NET 9 · Blazor Server · PostgreSQL 16 + pgvector · Ollama (Qwen) · Workers  
> **Âmbito:** documentação unificada para equipas técnicas, compliance e geração de manuais.

---

## 1. Objectivo e âmbito

A **KYC AI Platform** automatiza o processo de Know Your Customer para crédito corporativo em Portugal:

- Resolução de entidades (RCBE, GLEIF) e grafo de beneficiários efectivos (UBO)
- Triagem paralela: sanções, PEP, adverse media, dados financeiros e judiciais, ICIJ
- Ingestão e extracção de documentos (PDF, DOCX, imagens)
- Scoring de risco (0–100) e relatório narrativo com explainability (RGPD Art. 22)
- Workflow analista/supervisor com conformidade **BdP** (Lei 83/2017, Aviso 1/2022, Instr. 8/2024, Lei 97/2017)

**Público-alvo deste documento:** desenvolvimento, arquitectura, compliance, operações, documentação formal.

---

## 2. Arquitectura

### 2.1 Clean Architecture

```
src/
├── KYC.Domain/           Entidades, enums, value objects, eventos de domínio
├── KYC.Application/      Commands/Queries MediatR, DTOs, interfaces, políticas
├── KYC.Infrastructure/   EF Core, HTTP clients, LLM, compliance, messaging
├── KYC.Web/              Blazor Server, páginas, componentes, APIs minimal
└── KYC.Workers/          Hosted services (listas OFAC/EU, retenção, etc.)

tests/
├── KYC.Domain.Tests/
├── KYC.Application.Tests/
├── KYC.Integration.Tests/
└── KYC.Web.Integration.Tests/
```

**Regras de dependência:** Domain → nada; Application → Domain; Infrastructure → Application + Domain; Web → Application (regista DI de Infrastructure em `Program.cs`).

### 2.2 Fluxo principal de um caso

```
NIF + montante + relação (ocasional/continuada)
  → Validação PAC (antes de persistir)
  → KycCase InProgress
  → Entity resolution (GLEIF / RCBE)
  → UBO graph (GLEIF Level 2 + partes do caso)
  → Pipeline paralelo: sanções, media, AT, CITIUS, ICIJ, scoring Ollama
  → RiskSignals + RiskScore
  → Relatório narrativo (8 secções + Art. 22)
  → Workflow: auto-approve (Low) | revisão | aprovação/rejeição
  → Audit trail append-only
```

### 2.3 Fluxo de ingestão de documentos

```
Upload UI/API → CaseDocument (Pending) → ficheiro em Data/cases/{caseId}/documents/
  → DocumentIngestionHostedService (channel)
  → PdfPig | OpenXML | Qwen visão
  → DocumentFieldExtractor + mapper → facts/parties na BD
  → DocumentConsistencyChecker → sinais de inconsistência
  → Opcional: re-triagem automática do caso
```

---

## 3. Stack tecnológica

| Camada | Tecnologia |
|--------|------------|
| Runtime | .NET 9 |
| UI | Blazor Server, Bootstrap, SignalR (`KycHub`) |
| API | Minimal APIs (`Program.cs`), webhook identidade |
| ORM | EF Core 9, PostgreSQL 16, pgvector |
| CQRS | MediatR |
| LLM | Semantic Kernel + Ollama (Qwen3.5) — **sem Claude em produção actual** |
| Auth | Microsoft Entra ID (OIDC) ou ASP.NET Identity (dev) |
| Secrets | `.env` / Azure Key Vault (`KYC_KEYVAULT_NAME`) |
| Messaging | Azure Service Bus, RabbitMQ ou in-memory |
| PDF relatório | Puppeteer |
| CI | GitHub Actions (`.github/workflows/ci.yml`) |
| Deploy | `docker-compose.prod.yml` on-prem |

---

## 4. Autenticação e autorização

### 4.1 Modos de autenticação

- **Produção/homologação:** `AzureAd:Enabled=true` — OIDC Entra ID
- **Desenvolvimento:** Identity + PostgreSQL (`AuthDbContext`), seed admin

### 4.2 Roles

| Role | Permissões típicas |
|------|-------------------|
| `KYC.Analyst` | Casos, triagem, conformidade, SAR (submissão), documentos |
| `KYC.Supervisor` | Aprovação 4-eyes EDD, alertas SAR SignalR |
| `KYC.Admin` | PAC, scoring, DPIA, RPB, audit log, settings |

Políticas em `Program.cs`: `Analyst`, `Supervisor`, `Admin`.

### 4.3 Identificação do analista

`ICurrentAnalystAccessor` / `HttpContextAnalystAccessor` — ID do utilizador autenticado em comandos de audit (substitui `dev-user`).

---

## 5. Modelo de domínio (resumo)

### 5.1 KycCase

Estados: `Pending`, `InProgress`, `UnderReview`, `Approved`, `Rejected`.

Campos regulatórios: `DueDiligenceLevel`, `RelationshipType`, `SarStatus`, `NextReviewDue`, `FundsOriginDescription`, `AssetFreezeNotified`, `LegalBasisRef`, `ScoringEngineVersion`, etc.

### 5.2 CaseParty (entidades do caso)

Papéis: `Target`, `Shareholder`, `Ubo`, `BoardMember`, `Proxy`.

Flags: `IsPep`, `IsSanctioned`, `IsOffshore`.

Identidade: `VerificationStatus`, `VerificationMethod`, `VerificationSessionId`, `VerificationUrl`.

RCBE: `RcbeDiscrepancyDetected`, `RcbeDiscrepancyReported`.

### 5.3 Artefactos associados

- `RiskSignal` — tipo, severidade, fonte, confirmação analista
- `KycReport` — relatório final + embeddings
- `CaseDocument` — ingestão e extracção
- `AuditEntry` — imutável (trigger PostgreSQL)
- `CustomerAcceptancePolicy`, `ScoringEngineConfig`, `DpiaRecord`, `AmlComplianceReport`

---

## 6. Camada Application — use cases principais

| Área | Commands / Queries (exemplos) |
|------|-------------------------------|
| Casos | `StartKycCaseCommand`, `GetKycCaseQuery`, `ListKycCasesQuery`, `ApproveKycCaseCommand`, `RejectKycCaseCommand` |
| Triagem | `RerunKycCaseScreeningCommand`, `ScreenCasePartyCommand` |
| Partes | `AddCasePartyCommand`, `ConfirmRiskSignalCommand` |
| Documentos | `UploadCaseDocumentCommand`, pipeline ingestão |
| UBO | `GetUboGraphQuery` → `UboGraphViewDto` |
| Compliance | `SubmitSarCommand`, `MarkSarNotRequiredCommand`, `InitiateEntityVerificationCommand`, `RecordPresentialVerificationCommand`, `SetFundsOriginCommand`, `RegisterManualUifReferenceCommand` |
| Admin | `CreateCustomerAcceptancePolicyCommand`, `GenerateAnnualReportCommand`, DPIA upload |
| Relatório | Geração PDF, export RPB |

---

## 7. Integrações externas

| Integração | Interface | Configuração | Modo dev |
|------------|-----------|--------------|----------|
| GLEIF | `IEntityResolutionService` | API pública | ✅ |
| RCBE | `IRcbeRegistryService` | URL configurável | Fallback / mock |
| OFAC / EU sanctions | Workers + índice local | `Data/ofac`, `Data/eu-fsf` | Download periódico |
| Ollama | `ILlmCompletionService` | `OLLAMA_ENDPOINT` | Local |
| Identidade | `IIdentityVerificationService` | `IdentityVerification:*` | Stub se sem URL |
| UIF (SAR) | `IUifReportingService` | `Uif:*` | Ref. sintética |
| Congelamento BdP | `IAssetFreezeNotificationService` | `BdpAssetFreeze:*` | Log only |
| Adverse media | `IAdverseMediaService` | NewsAPI, etc. | |
| AT devedores | `ITaxDebtorsService` | | |
| CITIUS | `ICitiusClient` | | |
| ICIJ | GraphQL offshore | | |
| Entra Graph | Supervisores EDD | `Compliance:SupervisorGroupObjectId` | Lista manual |

**Produção:** `Compliance:RequireLiveIntegrations=true` (default em Production) — bloqueia referências `local-` / `UIF-DEV` e exige URLs reais.

---

## 8. APIs e endpoints relevantes

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/health` | Health check |
| POST | `/api/identity/webhook` | Callback verificação (HMAC opcional) |
| GET | `/api/cases/{id}/report.pdf` | Export PDF relatório |
| GET | `/api/admin/aml-reports/...` | Export RPB (JSON/XML BdP) |

Upload documentos: multipart via Application/Web (ver handlers de upload).

---

## 9. Interface utilizador (mapa de páginas)

| Rota | Página | Função |
|------|--------|--------|
| `/` | Dashboard | KPIs, casos recentes, SignalR |
| `/cases` | CaseList | Carteira com score, DDC, SAR |
| `/cases/new` | NewCase | Abertura com PAC |
| `/cases/{id}` | CaseDetail | Hero, conformidade, UBO embed, partes, documentos, acções |
| `/cases/{id}/ubo` | UboGraph | Grafo UBO completo |
| `/cases/{id}/report` | Relatório HTML | |
| `/cases/{id}/parties/{partyId}` | CasePartyDetail | Triagem por parte + identidade |
| `/admin/settings` | Settings | PAC, scoring, DPIA |
| `/admin/aml-report` | AmlReport | RPB anual |
| `/admin/audit` | AuditLog | Trail global |
| `/admin/dpia` | DpiaRecord | Versões DPIA |

### 9.1 Componentes UI reutilizáveis

| Componente | Função |
|------------|--------|
| `ComplianceCaseSection` | Secção BdP: SAR, identidade, EDD, histórico |
| `PartyIdentityPanel` | Verificação identidade + modal métodos |
| `SarActionModals` | Modais SAR / não aplicável |
| `IdentityVerificationBadge` | Badge PT (Verificado, Pendente, …) |
| `UboGraphView` | SVG grafo UBO (zoom, inspector, tabela) |
| `EntityCard` | Cartão parte com badges risco + identidade |
| `RiskScoreBadge`, `SignalCard`, `ScanProgressBar` | Triagem e risco |

---

## 10. SignalR — tempo real

Hub `KycHub`: progresso de triagem, relatório pronto, alertas compliance (SAR, identidade, congelamento). Grupos por caso e grupo `supervisors` para SAR.

---

## 11. Configuração

### 11.1 Variáveis essenciais (`.env.example`)

```env
KYC_DB_CONNECTION=Host=...;Database=...;Username=...;Password=...
OLLAMA_ENDPOINT=http://host.docker.internal:11434
AzureAd__Enabled=true|false
IdentityVerification__BaseUrl=...
IdentityVerification__WebhookSecret=...
Uif__BaseUrl=...
Uif__ApiKey=...
BdpAssetFreeze__BaseUrl=...
Compliance__RequireLiveIntegrations=true
Compliance__SupervisorGroupObjectId=<guid-grupo-AD>
```

### 11.2 Ficheiros de configuração

- `src/KYC.Web/appsettings.json` — defaults (sem secrets)
- `src/KYC.Web/Program.cs` — DI, auth, endpoints, hosted services

---

## 12. Base de dados e migrations

- **KycDbContext** — casos, partes, sinais, documentos, compliance, RPB
- **AuthDbContext** — utilizadores Identity (dev)
- Migrations em `src/KYC.Infrastructure/Migrations/`
- **Audit imutável:** trigger `tr_audit_entries_immutable`
- **pgvector:** embeddings de relatório

Aplicar: `dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web`

---

## 13. Segurança e RGPD

- Secrets fora do repositório; Key Vault opcional
- Auto-approve apenas risco Low (score ≤30, sem High/Critical/sanções)
- DPIA activa obrigatória para processamento
- Retenção: `DataRetention:EnableHostedService` (opt-in)
- Explainability no relatório (Art. 22 GDPR)
- Webhook identidade com HMAC SHA-256 quando secret definido

---

## 14. Testes e CI

```bash
dotnet test
dotnet test tests/KYC.Web.Integration.Tests  # requer KYC_DB_CONNECTION para Postgres
```

Testes relevantes: compliance handlers, SAR eligibility, identity webhook, UBO graph builder, policy PAC, audit immutability.

CI: build, EF migrate, testes em PostgreSQL de serviço.

---

## 15. Desvios face ao Blueprint.md v1.1

| Blueprint original | Implementação | Motivo |
|--------------------|---------------|--------|
| Claude Sonnet | Ollama Qwen apenas | RGPD on-prem / BdP |
| Azure Blob documentos | `Data/cases` local | Fase 5b; Blob planeado |
| UBO UI básica | `UboGraphView` rico | Maio 2026 |

Estado detalhado: [BLUEPRINT_COMPLETION_STATUS.md](BLUEPRINT_COMPLETION_STATUS.md).

---

## 16. Glossário

| Termo | Significado |
|-------|-------------|
| **PAC** | Política de Aceitação de Clientes (Art. 24.º Lei 83/2017) |
| **DDC / EDD** | Diligência devida standard / reforçada |
| **SAR** | Suspicious Activity Report → comunicação **UIF** |
| **UBO** | Beneficiário efectivo final |
| **RPB** | Relatório de Prevenção ao Branqueamento (Instr. 8/2024) |
| **RCBE** | Registo Central do Beneficiário Efectivo |
| **GLEIF** | Global LEI Foundation (dados societários) |
| **4-eyes** | Dupla aprovação em EDD (`SecondApproverId`) |

---

## 17. Referências cruzadas

- Catálogo de features: [CATALOGO_FUNCIONALIDADES.md](CATALOGO_FUNCIONALIDADES.md)
- Operações e homologação: [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
- Especificação técnica: [../Blueprint.md](../Blueprint.md)
- Especificação BdP: [../BLUEPRINT_BdP_Compliance_Addendum.md](../BLUEPRINT_BdP_Compliance_Addendum.md)
