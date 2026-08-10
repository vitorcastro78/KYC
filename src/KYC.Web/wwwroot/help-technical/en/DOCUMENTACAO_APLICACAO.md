# Application Documentation — KYC AI Platform

> **Document version:** May 2026 · Branch `feature/kyc-document-ingestion`  
> **Stack:** .NET 9 · Blazor Server · PostgreSQL 16 · ContextMemory · Workers  
> **Scope:** consolidated documentation for technical, compliance, and manual-generation teams.

---

## 1. Purpose and scope
The **KYC AI Platform** automates the Know Your Customer process for corporate lending in Portugal:
- Entity resolution (RCBE, GLEIF) and ultimate beneficial owner (UBO) graph
- Parallel screening: sanctions, PEP, adverse media, financial and judicial data, ICIJ
- Document ingestion and extraction (PDF, DOCX, images)
- Risk scoring (0–100) and narrative report with explainability (GDPR Art. 22)
- Analyst/supervisor workflow compliant with **BdP** (Law 83/2017, Notice 1/2022, Instr. 8/2024, Law 97/2017)

**Target audience:** development, architecture, compliance, operations, formal documentation.

---

## 2. Architecture

### 2.1 Clean Architecture
```
src/
├── KYC.Domain/           Entities, enums, value objects, domain events
├── KYC.Application/      MediatR Commands/Queries, DTOs, interfaces, policies
├── KYC.Infrastructure/   EF Core, HTTP clients, LLM, compliance, messaging
├── KYC.Web/              Blazor Server, pages, components, minimal APIs
└── KYC.Workers/          Hosted services (OFAC/EU lists, retention, etc.)

tests/
├── KYC.Domain.Tests/
├── KYC.Application.Tests/
├── KYC.Integration.Tests/
└── KYC.Web.Integration.Tests/
```
**Dependency rules:** Domain → none; Application → Domain; Infrastructure → Application + Domain; Web → Application (registers Infrastructure DI in `Program.cs`).

### 2.2 Main case flow
```
NIF + amount + relationship (occasional/ongoing)
  → PAC validation (before persisting)
  → KycCase InProgress
  → Entity resolution (GLEIF / RCBE)
  → UBO graph (GLEIF Level 2 + case parties)
  → Parallel pipeline: sanctions, media, AT, CITIUS, ICIJ, ContextMemory scoring
  → RiskSignals + RiskScore
  → Narrative report (8 sections + Art. 22)
  → Workflow: auto-approve (Low) | review | approve/reject
  → Append-only audit trail
```

### 2.3 Document-ingestion flow
```
Upload UI/API → CaseDocument (Pending) → file in Data/cases/{caseId}/documents/
  → DocumentIngestionHostedService (channel)
  → PdfPig | OpenXML | Qwen vision
  → DocumentFieldExtractor + mapper → facts/parties in DB
  → DocumentConsistencyChecker → inconsistency signals
  → Optional: automatic case re-screening
```

---

## 3. Technology stack
| Layer | Technology |
|-------|------------|
| Runtime | .NET 9 |
| UI | Blazor Server, Bootstrap, SignalR (`KycHub`) |
| API | Minimal APIs (`Program.cs`), identity webhook |
| ORM | EF Core 9, PostgreSQL 16 |
| CQRS | MediatR |
| LLM | ContextMemory gateway (chat/scoring + Global Wiki) |
| Auth | Microsoft Entra ID (OIDC) or ASP.NET Identity (dev) |
| Secrets | `.env` / Azure Key Vault (`KYC_KEYVAULT_NAME`) |
| Messaging | Azure Service Bus, RabbitMQ, or in-memory |
| Report PDF | Puppeteer |
| CI | GitHub Actions (`.github/workflows/ci.yml`) |
| Deploy | `docker-compose.prod.yml` on-prem |

---

## 4. Authentication and authorization
### 4.1 Authentication modes
- **Production/UAT:** `AzureAd:Enabled=true` — Entra ID OIDC
- **Development:** Identity + PostgreSQL (`AuthDbContext`), admin seed

### 4.2 Roles
| Role | Typical permissions |
|------|---------------------|
| `KYC.Analyst` | Cases, screening, compliance, SAR (submission), documents |
| `KYC.Supervisor` | EDD 4-eyes approval, SAR SignalR alerts |
| `KYC.Admin` | PAC, scoring, DPIA, RPB, audit log, settings |
Policies in `Program.cs`: `Analyst`, `Supervisor`, `Admin`.

### 4.3 Analyst identity
`ICurrentAnalystAccessor` / `HttpContextAnalystAccessor` — authenticated user ID in audit commands (replaces `dev-user`).

---

## 5. Domain model (summary)
### 5.1 KycCase
Statuses: `Pending`, `InProgress`, `UnderReview`, `Approved`, `Rejected`. Regulatory fields: `DueDiligenceLevel`, `RelationshipType`, `SarStatus`, `NextReviewDue`, `FundsOriginDescription`, `AssetFreezeNotified`, `LegalBasisRef`, `ScoringEngineVersion`, etc.
### 5.2 CaseParty (case entities)
Roles: `Target`, `Shareholder`, `Ubo`, `BoardMember`, `Proxy`. Flags: `IsPep`, `IsSanctioned`, `IsOffshore`. Identity: `VerificationStatus`, `VerificationMethod`, `VerificationSessionId`, `VerificationUrl`. RCBE: `RcbeDiscrepancyDetected`, `RcbeDiscrepancyReported`.
### 5.3 Associated artefacts
- `RiskSignal` — type, severity, source, analyst confirmation
- `KycReport` — final report + embeddings
- `CaseDocument` — ingestion and extraction
- `AuditEntry` — immutable (PostgreSQL trigger)
- `CustomerAcceptancePolicy`, `ScoringEngineConfig`, `DpiaRecord`, `AmlComplianceReport`

---

## 6. Application layer — main use cases
| Area | Commands / Queries (examples) |
|------|--------------------------------|
| Cases | `StartKycCaseCommand`, `GetKycCaseQuery`, `ListKycCasesQuery`, `ApproveKycCaseCommand`, `RejectKycCaseCommand` |
| Screening | `RerunKycCaseScreeningCommand`, `ScreenCasePartyCommand` |
| Parties | `AddCasePartyCommand`, `ConfirmRiskSignalCommand` |
| Documents | `UploadCaseDocumentCommand`, ingestion pipeline |
| UBO | `GetUboGraphQuery` → `UboGraphViewDto` |
| Compliance | `SubmitSarCommand`, `MarkSarNotRequiredCommand`, `InitiateEntityVerificationCommand`, `RecordPresentialVerificationCommand`, `SetFundsOriginCommand`, `RegisterManualUifReferenceCommand` |
| Admin | `CreateCustomerAcceptancePolicyCommand`, `GenerateAnnualReportCommand`, DPIA upload |
| Report | PDF generation, RPB export |

---

## 7. External integrations
| Integration | Interface | Configuration | Dev mode |
|-------------|-----------|---------------|----------|
| GLEIF | `IEntityResolutionService` | Public API | ✅ |
| RCBE | `IRcbeRegistryService` | Configurable URL | Fallback / mock |
| OFAC / EU sanctions | Workers + local index | `Data/ofac`, `Data/eu-fsf` | Periodic download |
| ContextMemory | `IKycLlmEngine` / `IContextMemoryWikiClient` | `ContextMemory:*` | Gateway |
| Identity | `IIdentityVerificationService` | `IdentityVerification:*` | Stub without URL |
| UIF (SAR) | `IUifReportingService` | `Uif:*` | Synthetic reference |
| BdP asset freeze | `IAssetFreezeNotificationService` | `BdpAssetFreeze:*` | Log only |
| Adverse media | `IAdverseMediaService` | NewsAPI, etc. | |
| AT debtors | `ITaxDebtorsService` | | |
| CITIUS | `ICitiusClient` | | |
| ICIJ | GraphQL offshore | | |
| Entra Graph | EDD supervisors | `Compliance:SupervisorGroupObjectId` | Manual list |
**Production:** `Compliance:RequireLiveIntegrations=true` (default in Production) blocks `local-` / `UIF-DEV` references and requires real URLs.

---

## 8. Relevant APIs and endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/health` | Health check |
| POST | `/api/identity/webhook` | Verification callback (optional HMAC) |
| GET | `/api/cases/{id}/report.pdf` | Report PDF export |
| GET | `/api/admin/aml-reports/...` | RPB export (BdP JSON/XML) |
Document upload: multipart through Application/Web (see upload handlers).

## 9. User interface (page map)
| Route | Page | Function |
|-------|------|----------|
| `/` | Dashboard | KPIs, recent cases, SignalR |
| `/cases` | CaseList | Portfolio with score, DDC, SAR |
| `/cases/new` | NewCase | Opening with PAC |
| `/cases/{id}` | CaseDetail | Hero, compliance, UBO embed, parties, documents, actions |
| `/cases/{id}/ubo` | UboGraph | Full UBO graph |
| `/cases/{id}/report` | HTML report | |
| `/cases/{id}/parties/{partyId}` | CasePartyDetail | Party screening + identity |
| `/admin/settings` | Settings | PAC, scoring, DPIA |
| `/admin/aml-report` | AmlReport | Annual RPB |
| `/admin/audit` | AuditLog | Global trail |
| `/admin/dpia` | DpiaRecord | DPIA versions |

### 9.1 Reusable UI components
| Component | Function |
|-----------|----------|
| `ComplianceCaseSection` | BdP section: SAR, identity, EDD, history |
| `PartyIdentityPanel` | Identity verification + methods modal |
| `SarActionModals` | SAR / not-applicable modals |
| `IdentityVerificationBadge` | PT badge (Verified, Pending, …) |
| `UboGraphView` | SVG UBO graph (zoom, inspector, table) |
| `EntityCard` | Party card with risk + identity badges |
| `RiskScoreBadge`, `SignalCard`, `ScanProgressBar` | Screening and risk |

## 10. SignalR — real time
`KycHub`: screening progress, report ready, compliance alerts (SAR, identity, asset freeze). Case-specific groups and `supervisors` group for SAR.

## 11. Configuration
### 11.1 Essential variables (`.env.example`)
```env
KYC_DB_CONNECTION=Host=...;Database=...;Username=...;Password=...
CONTEXT_MEMORY_BASE_URL=http://host.docker.internal:11434
AzureAd__Enabled=true|false
IdentityVerification__BaseUrl=...
IdentityVerification__WebhookSecret=...
Uif__BaseUrl=...
Uif__ApiKey=...
BdpAssetFreeze__BaseUrl=...
Compliance__RequireLiveIntegrations=true
Compliance__SupervisorGroupObjectId=<guid-grupo-AD>
```
### 11.2 Configuration files
- `src/KYC.Web/appsettings.json` — defaults (without secrets)
- `src/KYC.Web/Program.cs` — DI, auth, endpoints, hosted services

## 12. Database and migrations
- **KycDbContext** — cases, parties, signals, documents, compliance, RPB
- **AuthDbContext** — Identity users (dev)
- Migrations in `src/KYC.Infrastructure/Migrations/`
- **Immutable audit:** `tr_audit_entries_immutable` trigger
- **ContextMemory Global Wiki:** report knowledge
Apply: `dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web`

## 13. Security and GDPR
- Secrets outside the repository; optional Key Vault
- Auto-approve only Low risk (score ≤30, no High/Critical/sanctions)
- Active DPIA mandatory for processing
- Retention: `DataRetention:EnableHostedService` (opt-in)
- Explainability in the report (GDPR Art. 22)
- Identity webhook using HMAC SHA-256 when a secret is defined

## 14. Tests and CI
```bash
dotnet test
dotnet test tests/KYC.Web.Integration.Tests  # requires KYC_DB_CONNECTION for Postgres
```
Relevant tests: compliance handlers, SAR eligibility, identity webhook, UBO graph builder, PAC policy, audit immutability. CI: build, EF migrate, tests using service PostgreSQL.

## 15. Glossary
| Term | Meaning |
|------|---------|
| **PAC** | Customer Acceptance Policy (Art. 24 Law 83/2017) |
| **DDC / EDD** | Standard / enhanced due diligence |
| **SAR** | Suspicious Activity Report → **UIF** report |
| **UBO** | Ultimate beneficial owner |
| **RPB** | Anti-Money-Laundering Prevention Report (Instr. 8/2024) |
| **RCBE** | Central Register of Beneficial Ownership |
| **GLEIF** | Global LEI Foundation (company data) |
| **4-eyes** | Dual EDD approval (`SecondApproverId`) |

## 16. Cross-references
- Feature catalogue: [CATALOGO_FUNCIONALIDADES.md](CATALOGO_FUNCIONALIDADES.md)
- Operations and UAT: [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
