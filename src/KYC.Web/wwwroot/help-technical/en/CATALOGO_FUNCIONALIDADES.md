# Feature Catalogue — KYC AI Platform

> **Use:** basis for user manuals, RFPs, functional UAT, and roadmaps.  
> **Status legend:** ✅ Implemented · 🟡 Partial / depends on integration · 🔴 Planned / not implemented · 🌐 External dependency

---

## Module 1 — KYC case management

| ID | Feature | Description | UI / API | Status | Legal basis / note |
|----|---------|-------------|----------|--------|--------------------|
| KYC-01 | Open case | NIF, amount, occasional/ongoing relationship, CAE | `/cases/new` | ✅ | Law 83/2017 |
| KYC-02 | PAC validation at start | Rejects prohibited CAE/jurisdiction before saving | Command handler | ✅ | Art. 24 |
| KYC-03 | Case list | Score, DDC, SAR badge, status, date | `/cases` | ✅ | |
| KYC-04 | Case details | Hero, screening progress, actions, parties | `/cases/{id}` | ✅ | |
| KYC-05 | Approve case | Blocked by `CanApproveMessage` | CaseDetail + Supervisor | ✅ | Notice 1/2022 |
| KYC-06 | Reject case | Mandatory reason | CaseDetail | ✅ | |
| KYC-07 | Request manual review | UnderReview status | CaseDetail | ✅ | |
| KYC-08 | Auto-approve Low risk | Score ≤30, no severe signals | Pipeline | ✅ | GDPR / policy |
| KYC-09 | Assign analyst | `AssignedAnalystId` | Domain | ✅ | |
| KYC-10 | Periodic review | `NextReviewDue` after approval | Domain + compliance | ✅ | Art. 35 |

---

## Module 2 — Entity resolution and UBO

| ID | Feature | Description | UI / API | Status | Legal basis / note |
|----|---------|-------------|----------|--------|--------------------|
| ENT-01 | GLEIF resolution | Company snapshot + related parties | GleifCompanyCard | ✅ | |
| ENT-02 | RCBE | Beneficial-owner registry validation | Infrastructure | 🟡 | Law 89/2017 |
| ENT-03 | UBO graph (backend) | Recursive `BuildUboGraphAsync` | Query | ✅ | |
| ENT-04 | UBO graph (rich UI) | Hierarchical layout, zoom, inspector, table, PEP flags | `/cases/{id}/ubo`, CaseDetail embed | ✅ | May 2026 |
| ENT-05 | Merge graph + case | Case parties + GLEIF, `CasePartyId` | `UboGraphViewBuilder` | ✅ | |
| ENT-06 | Add party manually | UBO, shareholder, corporate body, proxy | CaseDetail modal | ✅ | |
| ENT-07 | Party details | Screening, identity, signals | `/cases/{id}/parties/{id}` | ✅ | |
| ENT-08 | Report RCBE discrepancy | Button + audit | PartyIdentityPanel | ✅ | IRN |

---

## Module 3 — Screening and risk signals

| ID | Feature | Description | UI / API | Status | Legal basis / note |
|----|---------|-------------|----------|--------|--------------------|
| SCR-01 | Automated pipeline | Sanctions, media, AT, judicial, ICIJ, scoring | Workers + MediatR | ✅ | |
| SCR-02 | Real-time progress | SignalR + progress bar | CaseDetail | ✅ | |
| SCR-03 | Re-run screening | All parties, regenerate report | CaseDetail | ✅ | |
| SCR-04 | Screening by party | Individual screen | EntityCard / PartyDetail | ✅ | |
| SCR-05 | OFAC / EU lists | Download and local index | Workers | ✅ | |
| SCR-06 | Signal confirmation | Analyst confirms match | SignalCard | ✅ | |
| SCR-07 | Sanctions asset freeze | BdP notification + UnderReview | Pipeline | ✅ | Law 97/2017 |
| SCR-08 | Ollama scoring | 0–100 + level | RiskScoreBadge | ✅ | No Claude |
| SCR-09 | Automated DDC | Simplified / Standard / Enhanced | Compliance section | ✅ | Notice 1/2022 |
| SCR-10 | Adverse media window | 2 years / 5 years EDD | Pipeline | ✅ | |

---

## Module 4 — Reports and explainability

| ID | Feature | Description | UI / API | Status | Legal basis / note |
|----|---------|-------------|----------|--------|--------------------|
| RPT-01 | Narrative report, 8 sections | Ollama LLM | `/cases/{id}/report` | ✅ | |
| RPT-02 | GDPR Art. 22 section | Explainability in prompt | Report | ✅ | GDPR |
| RPT-03 | PDF export | Puppeteer | `/api/cases/{id}/report.pdf` | ✅ | |
| RPT-04 | pgvector embeddings | Semantic report search | Infrastructure | ✅ | |
| RPT-05 | Document consistency | Checker vs GLEIF/case | Ingestion | ✅ | |
| RPT-06 | Claude narrative API | Cloud routing | — | 🔴 | Intentional deviation |

---

## Module 5 — Document ingestion

| ID | Feature | Description | UI / API | Status | Legal basis / note |
|----|---------|-------------|----------|--------|--------------------|
| DOC-01 | Document upload | PDF, DOCX, images | CaseDetail | ✅ | |
| DOC-02 | Asynchronous pipeline | Channel + hosted service | Background | ✅ | |
| DOC-03 | PDF/DOCX extraction | PdfPig, OpenXML | Infrastructure | ✅ | |
| DOC-04 | Image extraction | Qwen vision | Infrastructure | ✅ | |
| DOC-05 | Facts and parties in DB | Structured tables | DB | ✅ | |
| DOC-06 | Post-ingestion re-screening | Command | ✅ | |
| DOC-07 | Azure Blob Storage | Cloud storage | — | 🔴 | Blueprint phase 2 |
| DOC-08 | Azure Document Intelligence | Cloud OCR | — | 🔴 | |

---

## Module 6 — BdP compliance (UI and rules)

| ID | Feature | Description | UI / API | Status | Legal basis / note |
|----|---------|-------------|----------|--------|--------------------|
| CMP-01 | Compliance section | Yellow card on case | ComplianceCaseSection | ✅ | |
| CMP-02 | SAR badge in hero | UIF reporting status | CaseDetail | ✅ | |
| CMP-03 | Suggested SAR banner | `SuggestSar` pipeline | CaseDetail + compliance | ✅ | |
| CMP-04 | SAR modal | Narrative ≥200, urgent | SarActionModals | ✅ | Arts. 52–57 |
| CMP-05 | SAR not applicable | Justification ≥50 | SarActionModals | ✅ | |
| CMP-06 | Synchronous urgent SAR | No queue | Handler | ✅ | |
| CMP-07 | Non-urgent SAR | Asynchronous queue | Handler | ✅ | |
| CMP-08 | Manual UIF record | When API unavailable | Compliance section | ✅ | |
| CMP-09 | Check UIF status | By reference | Button + query | ✅ | 🌐 UIF API |
| CMP-10 | SAR history | Audit table | Compliance section | ✅ | |
| CMP-11 | Identity verification | 4-method modal | PartyIdentityPanel | ✅ | Notice 1/2022 |
| CMP-12 | PT identity badge | Verified/Pending/… | EntityCard, badges | ✅ | |
| CMP-13 | Verification session link | Provider URL | Party panel | ✅ | |
| CMP-14 | Identity webhook | HMAC POST | `/api/identity/webhook` | ✅ | |
| CMP-15 | Identity polling | Fallback hosted service | Workers/Web | ✅ | |
| CMP-16 | Identity approval block | `CanApproveMessage` | UI + domain | ✅ | |
| CMP-17 | EDD source of funds | Textarea + command | Compliance | ✅ | |
| CMP-18 | EDD 4-eyes | Entra Graph supervisor dropdown | Approve dialog | ✅ | |
| CMP-19 | Asset-freeze alert | Red banner | Compliance | ✅ | Law 97/2017 |
| CMP-20 | Live production integrations | `RequireLiveIntegrations` | Config | ✅ | |
| CMP-21 | Actual UIF submission | HTTP + Polly | Infrastructure | 🟡 | 🌐 credentials |
| CMP-22 | Actual BdP notification | HTTP freeze | Infrastructure | 🟡 | 🌐 endpoint |

---

## Module 7 — Administration and governance

| ID | Feature | Description | UI / API | Status | Legal basis / note |
|----|---------|-------------|----------|--------|--------------------|
| ADM-01 | PAC — versions | Create/activate, immutability | `/admin/settings` | ✅ | |
| ADM-02 | Scoring engine — versions | Prompt hash, semver | Settings | ✅ | |
| ADM-03 | DPIA — versions | PDF upload, active | `/admin/dpia` | ✅ | GDPR |
| ADM-04 | Annual RPB | Generate draft, metrics | `/admin/aml-report` | ✅ | Instr. 8/2024 |
| ADM-05 | BdP RPB XML export | `?format=bdp` | Admin API | 🟡 | 🌐 official X1 template |
| ADM-06 | Submit RPB to BdP | Reference + audit | Admin UI | ✅ | |
| ADM-07 | Global audit log | Search trail | `/admin/audit` | ✅ | |
| ADM-08 | Compliance seed | Default PAC/DPIA | Hosted seed | ✅ | |

---

## Module 8 — Dashboard and notifications

| ID | Feature | Description | UI / API | Status |
|----|---------|-------------|----------|--------|
| DSH-01 | Case KPIs | Approved today, pending | `/` | ✅ |
| DSH-02 | SignalR hub | Progress and alerts | KycHub | ✅ |
| DSH-03 | Supervisor alerts | SAR, compliance | `supervisors` group | ✅ |

---

## Module 9 — Infrastructure and operations

| ID | Feature | Description | Status |
|----|---------|-------------|--------|
| OPS-01 | Health check | `/health` | ✅ |
| OPS-02 | On-prem Docker | `docker-compose.prod.yml` | ✅ |
| OPS-03 | GitHub Actions CI | Build + migrate + test | ✅ |
| OPS-04 | Key Vault secrets | Optional | ✅ |
| OPS-05 | Messaging abstraction | SB / Rabbit / memory | ✅ |
| OPS-06 | Data retention job | Opt-in hosted | 🟡 |
| OPS-07 | Pen test checklist | Documented | 🔴 execution |

---

## Module 10 — Authentication

| ID | Feature | Description | Status |
|----|---------|-------------|--------|
| AUTH-01 | Entra ID OIDC | Production | ✅ |
| AUTH-02 | Local Identity | Dev | ✅ |
| AUTH-03 | Analyst/Supervisor/Admin roles | Policies | ✅ |
| AUTH-04 | HTTP analyst accessor | Audit actor | ✅ |

---

## Summary by status (May 2026)

| Status | Approx. count | % |
|--------|---------------|---|
| ✅ | ~75 features | ~90% |
| 🟡 | ~8 | ~10% |
| 🔴 | ~4 | ~5% |

**Priority go-live gaps:** official RPB template (X1), UIF/BdP/identity credentials (X2–X4), E2E execution + pen test + dossier.

---

## UI → compliance feature matrix

| Screen | Features |
|--------|----------|
| CaseList | KYC-03, SAR badge |
| CaseDetail | KYC-04/05, CMP-02/03, ENT-04 embed, SCR-02, DOC-01 |
| ComplianceCaseSection | CMP-01–20 |
| UboGraph | ENT-04/05 |
| CasePartyDetail | ENT-07, CMP-11/12, SCR-04 |
| Admin | ADM-01–07 |

---

## References

- Technical documentation: [DOCUMENTACAO_APLICACAO.md](DOCUMENTACAO_APLICACAO.md)
- Operations: [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
- Blueprint status: [BLUEPRINT_COMPLETION_STATUS.md](BLUEPRINT_COMPLETION_STATUS.md)
