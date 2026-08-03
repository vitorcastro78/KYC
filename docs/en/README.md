# Documentation — KYC AI Platform

> Portuguese originals are in the `docs/` root and `help-online/`. This folder contains the English translations.
>
> **Master index** for application documentation, the feature catalogue, and BdP UAT materials.

## Institutional requirements (BdP / COMEX UAT)

| Document | Content |
|----------|---------|
| [**MATRIZ_REQUISITOS_INSTITUCIONAIS.md**](MATRIZ_REQUISITOS_INSTITUCIONAIS.md) | Checklist §2.1–2.6 with ✅/🟡/🔴 status and evidence |
| [governanca/](governanca/) | PSI, encryption, BCP, DRP, risk, liveness, and retention policies |
| [api/README.md](api/README.md) | Swagger OpenAPI |

## Consolidated documents (use for official documentation)

| Document | Content |
|----------|---------|
| [**DOCUMENTACAO_APLICACAO.md**](DOCUMENTACAO_APLICACAO.md) | Overview, architecture, stack, flows, UI, APIs, configuration, security |
| [**CATALOGO_FUNCIONALIDADES.md**](CATALOGO_FUNCIONALIDADES.md) | Complete feature catalogue by module, status, and legal basis |
| [**OPERACOES_E_HOMOLOGACAO.md**](OPERACOES_E_HOMOLOGACAO.md) | Deployment, runbooks, E2E, checklists, pen test, dossier |

## Status and planning (project progress)

| Document | Content | Updated? |
|----------|---------|----------|
| [BLUEPRINT_COMPLETION_STATUS.md](BLUEPRINT_COMPLETION_STATUS.md) | **Source of truth** — completion % by phase/epic | ✅ May 2026 |
| [PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md](PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md) | E1–E10 backlog + **§2.3 status by epic** | ✅ §2 updated |
| [CATALOGO_FUNCIONALIDADES.md](CATALOGO_FUNCIONALIDADES.md) | Features with ✅/🟡/🔴 status | ✅ May 2026 |

> **Note:** The E1–E10 task tables in PLANO are a specification; progress is in §2.3 of PLANO and in `BLUEPRINT_COMPLETION_STATUS.md`.

## Source specifications (this language folder)

| File | Scope |
|------|-------|
| [Blueprint.md](Blueprint.md) | Core architecture, data model, phases 1–5b |
| [BLUEPRINT_BdP_Compliance_Addendum.md](BLUEPRINT_BdP_Compliance_Addendum.md) | BdP regulatory requirements (sections 13–20) |

Online help (English): [../help-online/en/](../help-online/en/)

## Supporting documents (detail / evidence)

| Document | When to use |
|----------|-------------|
| [ANALISTA_QUICK_START.md](ANALISTA_QUICK_START.md) | Rapid training for AML analysts |
| [DEPLOY_ONPREM.md](DEPLOY_ONPREM.md) | On-prem Docker deployment |
| [HOMOLOGACAO_RUNBOOK.md](HOMOLOGACAO_RUNBOOK.md) | Technical UAT steps |
| [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) | **10 E2E scenarios** (incl. manual contingency) + execution record |
| [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md) | Regulatory checklist (capabilities) |
| [PAC_RUNBOOK.md](PAC_RUNBOOK.md) | Customer acceptance policy |
| [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) | UAT pen test |
| [dossier/README.md](dossier/README.md) | Go-live evidence structure |

## SQL

| File | Content |
|------|---------|
| [sql/audit_trail_immutable.sql](sql/audit_trail_immutable.sql) | Immutable audit trigger (reference) |

---

## Official documentation (.docx)

| Folder | Content |
|--------|---------|
| [**docx/**](docx/) | **9 Word documents** generated from this folder (application, catalogue, operations, matrix, E2E tests, checklist, manual, evidence dossier, governance) |

Regenerate:

```powershell
cd scripts/generate-docx-docs
npm install
npm run generate
```

**How to generate external documentation (Word/PDF/Confluence):** use `docs/docx/*.docx` or export `DOCUMENTACAO_APLICACAO.md` + `CATALOGO_FUNCIONALIDADES.md` as a basis; attach `OPERACOES_E_HOMOLOGACAO.md` for operational appendices.
