# Documentación — KYC AI Platform

> Paquete de idioma: **español**. Hub: [`../README.md`](../README.md).

## Idiomas

| Idioma | Índice |
|--------|--------|
| Español (esta carpeta) | [`README.md`](README.md) · [`../help-online/es/`](../help-online/es/) |
| Português | [`../pt/README.md`](../pt/README.md) · [`../help-online/pt/`](../help-online/pt/) |
| English | [`../en/README.md`](../en/README.md) · [`../help-online/en/`](../help-online/en/) |

## Pack canónico

| Documento | Contenido |
|-----------|-----------|
| [**DOCUMENTACAO_APLICACAO.md**](DOCUMENTACAO_APLICACAO.md) | Arquitectura, stack, flujos, UI, APIs, configuración |
| [**CATALOGO_FUNCIONALIDADES.md**](CATALOGO_FUNCIONALIDADES.md) | Features por módulo, estado y base legal |
| [**OPERACOES_E_HOMOLOGACAO.md**](OPERACOES_E_HOMOLOGACAO.md) | Deploy, runbooks, E2E, checklists, pen test, inicio rápido |
| [**MATRIZ_REQUISITOS_INSTITUCIONAIS.md**](MATRIZ_REQUISITOS_INSTITUCIONAIS.md) | Requisitos COMEX/BdP §2.1–2.6 |
| [**governanca/README.md**](governanca/README.md) | Políticas PSI, cifrado, PCN, PRD, riesgos, liveness |
| [../help-online/es/](../help-online/es/) | Manual UX (Help Center) |
| [api/README.md](api/README.md) | Swagger OpenAPI |

## Evidencias y SQL

| Documento | Contenido |
|-----------|-----------|
| [../dossier/README.md](../dossier/README.md) | Estructura de evidencias go-live |
| [../sql/audit_trail_immutable.sql](../sql/audit_trail_immutable.sql) | Trigger de auditoría inmutable |

## Archivo

| Documento | Contenido |
|-----------|-----------|
| [../_archive/PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md](../_archive/PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md) | Backlog histórico E1–E10 |

## Documentación oficial (.docx)

Generada desde `docs/pt/` → [`../docx/`](../docx/).

```powershell
cd scripts/generate-docx-docs
npm install
npm run generate
```
