# Documentation — KYC AI Platform

> Language pack: **English**. Hub: [`../README.md`](../README.md).

## Languages

| Language | Index |
|----------|--------|
| English (this folder) | [`README.md`](README.md) · [`../help-online/en/`](../help-online/en/) |
| Português | [`../pt/README.md`](../pt/README.md) · [`../help-online/pt/`](../help-online/pt/) |
| Español | [`../es/README.md`](../es/README.md) · [`../help-online/es/`](../help-online/es/) |

## Canonical pack

| Document | Content |
|----------|---------|
| [**DOCUMENTACAO_APLICACAO.md**](DOCUMENTACAO_APLICACAO.md) | Architecture, stack, flows, UI, APIs, configuration |
| [**CATALOGO_FUNCIONALIDADES.md**](CATALOGO_FUNCIONALIDADES.md) | Features by module, status, and legal basis |
| [**OPERACOES_E_HOMOLOGACAO.md**](OPERACOES_E_HOMOLOGACAO.md) | Deploy, runbooks, E2E, checklists, pen test, quick start |
| [**MATRIZ_REQUISITOS_INSTITUCIONAIS.md**](MATRIZ_REQUISITOS_INSTITUCIONAIS.md) | COMEX/BdP requirements §2.1–2.6 |
| [**governanca/README.md**](governanca/README.md) | PSI, encryption, BCP, DRP, risk, liveness policies |
| [../help-online/en/](../help-online/en/) | UX manual (Help Center) |
| [api/README.md](api/README.md) | Swagger OpenAPI |

## Evidence and SQL

| Document | Content |
|----------|---------|
| [../dossier/README.md](../dossier/README.md) | Go-live evidence layout |
| [../sql/audit_trail_immutable.sql](../sql/audit_trail_immutable.sql) | Immutable audit trigger |

## Archive

| Document | Content |
|----------|---------|
| [../_archive/PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md](../_archive/PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md) | Historical E1–E10 backlog |

## Official documentation (.docx)

Generated from `docs/pt/` → [`../docx/`](../docx/).

```powershell
cd scripts/generate-docx-docs
npm install
npm run generate
```
