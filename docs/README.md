# Documentação — KYC AI Platform

> **Índice mestre** para gerar documentação da aplicação, catálogo de funcionalidades e materiais de homologação BdP.

## Documentos unificados (usar para documentação oficial)

| Documento | Conteúdo |
|-----------|----------|
| [**DOCUMENTACAO_APLICACAO.md**](DOCUMENTACAO_APLICACAO.md) | Visão geral, arquitectura, stack, fluxos, UI, APIs, configuração, segurança |
| [**CATALOGO_FUNCIONALIDADES.md**](CATALOGO_FUNCIONALIDADES.md) | Catálogo completo de features por módulo, estado e base legal |
| [**OPERACOES_E_HOMOLOGACAO.md**](OPERACOES_E_HOMOLOGACAO.md) | Deploy, runbooks, E2E, checklists, pen test, dossier |

## Estado e planeamento

| Documento | Conteúdo |
|-----------|----------|
| [BLUEPRINT_COMPLETION_STATUS.md](BLUEPRINT_COMPLETION_STATUS.md) | Mapa de conclusão Blueprint.md + adenda BdP |
| [PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md](PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md) | Épicos E1–E10, sprints, dependências externas |

## Especificações de origem (raiz do repositório)

| Ficheiro | Âmbito |
|----------|--------|
| [../Blueprint.md](../Blueprint.md) | Arquitectura core, modelo de dados, fases 1–5b |
| [../BLUEPRINT_BdP_Compliance_Addendum.md](../BLUEPRINT_BdP_Compliance_Addendum.md) | Requisitos regulatórios BdP (secções 13–20) |

## Documentos de apoio (detalhe / evidências)

| Documento | Quando usar |
|-----------|-------------|
| [ANALISTA_QUICK_START.md](ANALISTA_QUICK_START.md) | Formação rápida analistas AML |
| [DEPLOY_ONPREM.md](DEPLOY_ONPREM.md) | Deploy Docker on-prem |
| [HOMOLOGACAO_RUNBOOK.md](HOMOLOGACAO_RUNBOOK.md) | Passos técnicos homologação |
| [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) | Cenários E2E com registo de execução |
| [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md) | Checklist regulatório (capacidades) |
| [PAC_RUNBOOK.md](PAC_RUNBOOK.md) | Política de aceitação de clientes |
| [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) | Pen test homologação |
| [dossier/README.md](dossier/README.md) | Estrutura de evidências go-live |

## SQL

| Ficheiro | Conteúdo |
|----------|----------|
| [sql/audit_trail_immutable.sql](sql/audit_trail_immutable.sql) | Trigger audit imutável (referência) |

---

**Como gerar documentação externa (Word/PDF/Confluence):** exportar `DOCUMENTACAO_APLICACAO.md` + `CATALOGO_FUNCIONALIDADES.md` como base; anexar `OPERACOES_E_HOMOLOGACAO.md` para anexos operacionais.
