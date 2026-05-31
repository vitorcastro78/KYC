# Estado de conclusão dos Blueprints — KYC Platform

> **Última actualização:** Maio 2026 · Branch `feature/kyc-document-ingestion` · Commit `fc47caa`  
> **Objectivo:** aplicação **production-ready**, não protótipo.  
> **Fontes:** `Blueprint.md` (v1.1) + `BLUEPRINT_BdP_Compliance_Addendum.md` + `docs/PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md`

---

## Resumo executivo

| Blueprint | Código | Integrações reais | Homologação |
|-----------|--------|-------------------|-------------|
| **Blueprint.md** (core KYC) | **~90%** | Variável por ambiente | E2E manual pendente |
| **BdP Addendum** (compliance) | **~92%** | UIF / BdP freeze / identidade requerem URL + credenciais | Dossier + pen test pendentes |
| **Global** | **~92%** código · **0%** evidências homologação | Configurar `.env` produção | Executar `docs/OPERACOES_E_HOMOLOGACAO.md` §4–7 |

**Legenda:** ✅ Done · 🟡 Parcial / modo dev · 🔴 Pendente · 🌐 Externo (compliance/BdP)

---

## Blueprint.md — por fase

### Fase 1 — Fundação
| Item | Estado | Notas |
|------|--------|-------|
| 5 projectos Clean Architecture | ✅ | Domain, Application, Infrastructure, Web, Workers |
| EF + PostgreSQL + migrations | ✅ | pgvector, audit trigger |
| Entra OIDC + Identity local dev | ✅ | `AzureAd:Enabled` ou Identity+PostgreSQL |
| Key Vault opcional | ✅ | `KYC_KEYVAULT_NAME` |
| Blazor + auth + CI | ✅ | `.github/workflows/ci.yml` |

### Fase 2 — Core KYC Engine
| Item | Estado | Notas |
|------|--------|-------|
| StartKycCase + MediatR | ✅ | PAC validada antes de persistir |
| RCBE + GLEIF entity resolution | 🟡 | RCBE depende de endpoint; fallback GLEIF |
| UBO graph recursivo | ✅ | `BuildUboGraphAsync` |
| OFAC + EU Sanctions | ✅ | Workers download + índice local |
| Service Bus / Rabbit / in-memory | ✅ | `Messaging:Provider` |
| Pipeline scans paralelos | ✅ | `KycCasePipelineRunner` |
| Ollama Qwen scoring | ✅ | Sem Claude (desvio documentado: só Ollama em compliance) |
| Audit append-only | ✅ | Trigger PostgreSQL |

### Fase 3 — IA & Relatório
| Item | Estado | Notas |
|------|--------|-------|
| Claude Sonnet API | 🔴 | **Não implementado** — stack actual = Ollama-only (BdP/RGPD) |
| Roteamento LLM local/cloud | 🟡 | Só local; suficiente para on-prem BdP |
| Relatório 8 secções + explainability | ✅ | Secção Art. 22 no prompt |
| Consistency check documentos | ✅ | `DocumentConsistencyChecker` |
| Embeddings pgvector | ✅ | `ReportEmbeddingWriter` |

### Fase 4 — UI & Workflow
| Item | Estado | Notas |
|------|--------|-------|
| Dashboard SignalR | ✅ | |
| CaseDetail scan progress | ✅ | |
| UBO graph UI | ✅ | `UboGraphView`, layout hierárquico, embed CaseDetail |
| Aprovação 4-eyes EDD | ✅ | `SecondApproverId` |
| Export PDF relatório | ✅ | Puppeteer |
| Audit log Admin | ✅ | `AuditLog.razor` |

### Fase 5 — Fontes & compliance
| Item | Estado | Notas |
|------|--------|-------|
| Adverse media | ✅ | NewsAPI + fallbacks |
| AT devedores | ✅ | |
| CITIUS judicial | ✅ | `CitiusClient` |
| ICIJ offshore | ✅ | GraphQL |
| Data retention job | 🟡 | Opt-in `DataRetention:EnableHostedService` |
| Pen test | 🔴 | Checklist em `docs/SECURITY_PEN_TEST_CHECKLIST.md` |

### Fase 5b — Ingestão documentos
| Item | Estado | Notas |
|------|--------|-------|
| Pipeline completo | ✅ | Ver Blueprint §5b |
| Azure Blob / Doc Intelligence | 🔴 | Fase 2 explícita no blueprint |

---

## BdP Addendum — épicos E1–E10

| Épico | Código | Gap principal |
|-------|--------|---------------|
| **E1** PAC / start caso | ✅ | — |
| **E2** Identidade | ✅ | Produção: `IdentityVerification:BaseUrl` obrigatório se `RequireLiveIntegrations` |
| **E3** SAR / UIF | ✅ | API UIF real + registo manual opcional; fila não-urgente |
| **E4** EDD | ✅ | Supervisores Entra: Graph se grupo configurado |
| **E5** Congelamento | ✅ | API BdP real em prod |
| **E6** Explainability | ✅ | — |
| **E7** RPB | 🟡 | XML interno; **E7-01** template oficial 🌐 |
| **E8** Admin versões | ✅ | DPIA upload PDF |
| **E9** RCBE | ✅ | Detecção + report UI |
| **E10** Testes / homologação | 🟡 | Testes auto ✅; E2E + dossier + pen test 🔴 |

Checklist regulatório (`docs/CHECKLIST_HOMOLOGACAO_BDP.md`): **12/12 capacidade** — falta **evidência** de execução.

---

## Dependências externas (não são código)

| ID | Entrega | Responsável |
|----|---------|-------------|
| X1 | Template RPB oficial BdP | Compliance |
| X2 | Credenciais / MOU API UIF | Instituição |
| X3 | Contrato prestador identidade (DigitalSign/CMD) | Prestador |
| X4 | Endpoint notificação congelamento BdP | Instituição |
| X5 | PAC v1 assinada | Compliance |
| X6 | PDF DPIA aprovado pelo DPO | DPO |

---

## Configuração produção (obrigatória)

Ver `.env.example` e `Compliance:RequireLiveIntegrations=true` (default em Production):

```env
KYC_DB_CONNECTION=...
IdentityVerification__BaseUrl=...
IdentityVerification__ApiKey=...
IdentityVerification__WebhookSecret=...
Uif__BaseUrl=...
Uif__ApiKey=...
BdpAssetFreeze__BaseUrl=...
# Entra supervisores (opcional)
Compliance__SupervisorGroupObjectId=<guid-grupo-AD>
AzureAd__ClientSecret=...   # app-only Graph
```

---

## Próximos passos recomendados (ordem)

1. **Homologação:** executar `docs/E2E_HOMOLOGACAO.md` + preencher `docs/dossier/`.
2. **Pen test:** `docs/SECURITY_PEN_TEST_CHECKLIST.md`.
3. **Credenciais** X2–X4 em staging; validar sem refs `local-` / `UIF-` sintéticas.
4. **Template RPB** X1 → actualizar `BdpRpbExporter.cs`.
5. **Opcional produto:** Claude API (se instituição exigir cloud LLM fora do âmbito BdP actual).

---

## Desvio intencional vs. Blueprint.md v1.1

| Blueprint original | Implementação actual | Motivo |
|--------------------|--------------------|--------|
| Claude Sonnet narrativa | Ollama Qwen apenas | RGPD on-prem / BdP addendum |
| Azure Blob documentos | Storage local `Data/cases` | Fase 5b; Blob = fase 2 |

Este documento deve ser actualizado quando cada linha 🔴 passar a ✅.
