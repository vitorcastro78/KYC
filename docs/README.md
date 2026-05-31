# Documentação — KYC AI Platform

> **Índice mestre** para gerar documentação da aplicação, catálogo de funcionalidades e materiais de homologação BdP.

## Requisitos institucionais (homologação BdP / COMEX)

| Documento | Conteúdo |
|-----------|----------|
| [**MATRIZ_REQUISITOS_INSTITUCIONAIS.md**](MATRIZ_REQUISITOS_INSTITUCIONAIS.md) | Checklist §2.1–2.6 com estado ✅/🟡/🔴 e evidências |
| [governanca/](governanca/) | Políticas PSI, criptografia, PCN, PRD, riscos, liveness, retenção |
| [api/README.md](api/README.md) | Swagger OpenAPI |

## Documentos unificados (usar para documentação oficial)

| Documento | Conteúdo |
|-----------|----------|
| [**DOCUMENTACAO_APLICACAO.md**](DOCUMENTACAO_APLICACAO.md) | Visão geral, arquitectura, stack, fluxos, UI, APIs, configuração, segurança |
| [**CATALOGO_FUNCIONALIDADES.md**](CATALOGO_FUNCIONALIDADES.md) | Catálogo completo de features por módulo, estado e base legal |
| [**OPERACOES_E_HOMOLOGACAO.md**](OPERACOES_E_HOMOLOGACAO.md) | Deploy, runbooks, E2E, checklists, pen test, dossier |

## Estado e planeamento (andamento do projeto)

| Documento | Conteúdo | Actualizado? |
|-----------|----------|--------------|
| [BLUEPRINT_COMPLETION_STATUS.md](BLUEPRINT_COMPLETION_STATUS.md) | **Fonte de verdade** — % conclusão por fase/épico | ✅ Maio 2026 |
| [PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md](PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md) | Backlog E1–E10 + **§2.3 estado por épico** | ✅ §2 actualizado |
| [CATALOGO_FUNCIONALIDADES.md](CATALOGO_FUNCIONALIDADES.md) | Features com estado ✅/🟡/🔴 | ✅ Maio 2026 |

> **Nota:** Tabelas de tarefas E1–E10 no PLANO são especificação; o andamento está em §2.3 do PLANO e em `BLUEPRINT_COMPLETION_STATUS.md`.

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
| [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) | **10 cenários** E2E (incl. contingência manual) + registo de execução |
| [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md) | Checklist regulatório (capacidades) |
| [PAC_RUNBOOK.md](PAC_RUNBOOK.md) | Política de aceitação de clientes |
| [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) | Pen test homologação |
| [dossier/README.md](dossier/README.md) | Estrutura de evidências go-live |

## SQL

| Ficheiro | Conteúdo |
|----------|----------|
| [sql/audit_trail_immutable.sql](sql/audit_trail_immutable.sql) | Trigger audit imutável (referência) |

---

## Documentação oficial (.docx)

| Pasta | Conteúdo |
|-------|----------|
| [**docx/**](docx/) | **9 documentos Word** gerados a partir desta pasta (aplicação, catálogo, operações, matriz, testes E2E, checklist, manual, dossier evidências, governança) |

Regenerar:

```powershell
cd scripts/generate-docx-docs
npm install
npm run generate
```

**Como gerar documentação externa (Word/PDF/Confluence):** usar `docs/docx/*.docx` ou exportar `DOCUMENTACAO_APLICACAO.md` + `CATALOGO_FUNCIONALIDADES.md` como base; anexar `OPERACOES_E_HOMOLOGACAO.md` para anexos operacionais.
