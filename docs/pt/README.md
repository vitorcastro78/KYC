# Documentação — KYC AI Platform

> Pacote de idioma: **português**. Hub: [`../README.md`](../README.md).

## Idiomas

| Idioma | Índice |
|--------|--------|
| Português (esta pasta) | [`README.md`](README.md) · [`../help-online/pt/`](../help-online/pt/) |
| English | [`../en/README.md`](../en/README.md) · [`../help-online/en/`](../help-online/en/) |
| Español | [`../es/README.md`](../es/README.md) · [`../help-online/es/`](../help-online/es/) |

## Pack canónico

| Documento | Conteúdo |
|-----------|----------|
| [**DOCUMENTACAO_APLICACAO.md**](DOCUMENTACAO_APLICACAO.md) | Arquitectura, stack, fluxos, UI, APIs, configuração |
| [**CATALOGO_FUNCIONALIDADES.md**](CATALOGO_FUNCIONALIDADES.md) | Features por módulo, estado e base legal |
| [**OPERACOES_E_HOMOLOGACAO.md**](OPERACOES_E_HOMOLOGACAO.md) | Deploy, runbooks, E2E, checklists, pen test, quick start |
| [**MATRIZ_REQUISITOS_INSTITUCIONAIS.md**](MATRIZ_REQUISITOS_INSTITUCIONAIS.md) | Requisitos COMEX/BdP §2.1–2.6 |
| [**governanca/README.md**](governanca/README.md) | Políticas PSI, criptografia, PCN, PRD, riscos, liveness |
| [../help-online/pt/](../help-online/pt/) | Manual UX (Help Center) |
| [api/README.md](api/README.md) | Swagger OpenAPI |

## Evidências e SQL

| Documento | Conteúdo |
|-----------|----------|
| [../dossier/README.md](../dossier/README.md) | Estrutura de evidências go-live |
| [../sql/audit_trail_immutable.sql](../sql/audit_trail_immutable.sql) | Trigger audit imutável |

## Arquivo

| Documento | Conteúdo |
|-----------|----------|
| [../_archive/PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md](../_archive/PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md) | Backlog histórico E1–E10 |

## Documentação oficial (.docx)

Gerada a partir de `docs/pt/` → [`../docx/`](../docx/).

```powershell
cd scripts/generate-docx-docs
npm install
npm run generate
```
