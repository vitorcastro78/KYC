# Documentación — KYC AI Platform

> Los originales en portugués se encuentran en la raíz de `docs/` y en `help-online/`. Esta carpeta contiene las traducciones al español.
>
> **Índice maestro** para generar documentación de la aplicación, catálogo de funcionalidades y materiales de homologación BdP.

## Requisitos institucionales (homologación BdP / COMEX)

| Documento | Contenido |
|-----------|-----------|
| [**MATRIZ_REQUISITOS_INSTITUCIONAIS.md**](MATRIZ_REQUISITOS_INSTITUCIONAIS.md) | Checklist §2.1–2.6 con estado ✅/🟡/🔴 y evidencias |
| [governanca/](governanca/) | Políticas PSI, cifrado, PCN, PRD, riesgos, liveness y retención |
| [api/README.md](api/README.md) | Swagger OpenAPI |

## Documentos consolidados (usar para documentación oficial)

| Documento | Contenido |
|-----------|-----------|
| [**DOCUMENTACAO_APLICACAO.md**](DOCUMENTACAO_APLICACAO.md) | Visión general, arquitectura, stack, flujos, UI, APIs, configuración, seguridad |
| [**CATALOGO_FUNCIONALIDADES.md**](CATALOGO_FUNCIONALIDADES.md) | Catálogo completo de funcionalidades por módulo, estado y base legal |
| [**OPERACOES_E_HOMOLOGACAO.md**](OPERACOES_E_HOMOLOGACAO.md) | Despliegue, runbooks, E2E, checklists, pen test, dossier |

## Estado y planificación (avance del proyecto)

| Documento | Contenido | ¿Actualizado? |
|-----------|-----------|---------------|
| [BLUEPRINT_COMPLETION_STATUS.md](BLUEPRINT_COMPLETION_STATUS.md) | **Fuente de verdad** — % de finalización por fase/épica | ✅ Mayo 2026 |
| [PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md](PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md) | Backlog E1–E10 + **§2.3 estado por épica** | ✅ §2 actualizado |
| [CATALOGO_FUNCIONALIDADES.md](CATALOGO_FUNCIONALIDADES.md) | Funcionalidades con estado ✅/🟡/🔴 | ✅ Mayo 2026 |

> **Nota:** Las tablas de tareas E1–E10 del PLANO son una especificación; el avance está en §2.3 del PLANO y en `BLUEPRINT_COMPLETION_STATUS.md`.

## Especificaciones de origen (esta carpeta de idioma)

| Archivo | Ámbito |
|---------|--------|
| [Blueprint.md](Blueprint.md) | Arquitectura core, modelo de datos, fases 1–5b |
| [BLUEPRINT_BdP_Compliance_Addendum.md](BLUEPRINT_BdP_Compliance_Addendum.md) | Requisitos regulatorios BdP (secciones 13–20) |

Ayuda en línea (español): [../help-online/es/](../help-online/es/)

## Documentos de apoyo (detalle / evidencias)

| Documento | Cuándo usarlo |
|-----------|---------------|
| [ANALISTA_QUICK_START.md](ANALISTA_QUICK_START.md) | Formación rápida para analistas AML |
| [DEPLOY_ONPREM.md](DEPLOY_ONPREM.md) | Despliegue Docker on-prem |
| [HOMOLOGACAO_RUNBOOK.md](HOMOLOGACAO_RUNBOOK.md) | Pasos técnicos de homologación |
| [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) | **10 escenarios** E2E (incl. contingencia manual) + registro de ejecución |
| [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md) | Checklist regulatorio (capacidades) |
| [PAC_RUNBOOK.md](PAC_RUNBOOK.md) | Política de aceptación de clientes |
| [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) | Pen test de homologación |
| [dossier/README.md](dossier/README.md) | Estructura de evidencias go-live |

## SQL

| Archivo | Contenido |
|---------|-----------|
| [sql/audit_trail_immutable.sql](sql/audit_trail_immutable.sql) | Trigger de auditoría inmutable (referencia) |

---

## Documentación oficial (.docx)

| Carpeta | Contenido |
|---------|-----------|
| [**docx/**](docx/) | **9 documentos Word** generados desde esta carpeta (aplicación, catálogo, operaciones, matriz, pruebas E2E, checklist, manual, dossier de evidencias, gobernanza) |

Regenerar:

```powershell
cd scripts/generate-docx-docs
npm install
npm run generate
```

**Cómo generar documentación externa (Word/PDF/Confluence):** use `docs/docx/*.docx` o exporte `DOCUMENTACAO_APLICACAO.md` + `CATALOGO_FUNCIONALIDADES.md` como base; adjunte `OPERACOES_E_HOMOLOGACAO.md` para los anexos operativos.
