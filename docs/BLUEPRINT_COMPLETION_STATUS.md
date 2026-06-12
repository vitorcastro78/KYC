# Estado de conclusão dos Blueprints — KYC Platform

> **Última actualização:** Maio 2026 · Branch `feature/kyc-document-ingestion`  
> **Objectivo:** aplicação **production-ready**, não protótipo.  
> **Fontes:** `Blueprint.md` (v1.1) + `BLUEPRINT_BdP_Compliance_Addendum.md` + `docs/PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md`

---

## Resumo executivo

| Blueprint | Código | Integrações reais | Homologação |
|-----------|--------|-------------------|-------------|
| **Blueprint.md** (core KYC) | **~90%** | Variável por ambiente | E2E manual pendente |
| **BdP Addendum** (compliance) | **~95%** | UIF / BdP / identidade + **contingência manual** | Dossier + pen test pendentes |
| **Global** | **~95%** código · **0%** evidências homologação | Configurar `.env` produção | Executar `docs/E2E_HOMOLOGACAO.md` (10 cenários) |

**Legenda:** ✅ Done · 🟡 Parcial / modo dev · 🔴 Pendente · 🌐 Externo (compliance/BdP)

**Próximo passo activo:** executar [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) e preencher `docs/dossier/`.

---

## Contingência manual (Maio 2026) — ✅ código

| Lacuna | Implementação |
|--------|----------------|
| Congelamento BdP sem API | `RegisterManualAssetFreezeReferenceCommand` + UI `NeedsManualAssetFreezeRegistration` |
| SAR urgente falha UIF | `RecordSarPendingAfterApiFailure` → `Pending` + registo manual |
| Identidade sem prestador | `RecordManualIdentityVerificationCommand` + botão UI |
| Triagem (media/judicial) | `AddManualRiskSignalCommand` + confirmar/descartar em `SignalCard` |
| Nome legal no arranque | `LegalCompanyName` + `GetEntityResolutionPreviewQuery` em Novo caso |

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
| StartKycCase + MediatR | ✅ | PAC + nome legal manual em fallback |
| RCBE + GLEIF entity resolution | 🟡 | RCBE depende de endpoint; preview UI |
| UBO graph recursivo | ✅ | `BuildUboGraphAsync` |
| OFAC + EU Sanctions | ✅ | Workers download + índice local |
| Service Bus / Rabbit / in-memory | ✅ | `Messaging:Provider` |
| Pipeline scans paralelos | ✅ | `KycCasePipelineRunner` |
| Ollama Qwen scoring | ✅ | Sem Claude (desvio documentado) |
| Audit append-only | ✅ | Trigger PostgreSQL |

### Fase 3 — IA & Relatório
| Item | Estado | Notas |
|------|--------|-------|
| Claude Sonnet API | 🔴 | Ollama-only (BdP/RGPD) |
| Roteamento LLM local/cloud | 🟡 | Só local |
| Relatório 8 secções + explainability | ✅ | Art. 22 |
| Consistency check documentos | ✅ | |
| Embeddings pgvector | ✅ | |

### Fase 4 — UI & Workflow
| Item | Estado | Notas |
|------|--------|-------|
| Dashboard SignalR | ✅ | |
| CaseDetail scan progress | ✅ | |
| UBO graph UI | ✅ | |
| Aprovação 4-eyes EDD | ✅ | |
| Export PDF relatório | ✅ | |
| Audit log Admin | ✅ | |
| Sinais: confirmar/descartar | ✅ | `SignalCard` + `OverrideSignal` |

### Fase 5 — Fontes & compliance
| Item | Estado | Notas |
|------|--------|-------|
| Adverse media / AT / CITIUS / ICIJ | ✅ | + sinais manuais |
| Data retention job | 🟡 | `DataRetention:EnableHostedService` em prod |
| Pen test | 🔴 | Executar checklist |

### Fase 5b — Ingestão documentos
| Item | Estado | Notas |
|------|--------|-------|
| Pipeline completo | ✅ | |
| Azure Blob / Doc Intelligence | 🔴 | Fase 2 blueprint |

---

## BdP Addendum — épicos E1–E10

| Épico | Código | Gap principal |
|-------|--------|---------------|
| **E1** PAC / start caso | ✅ | Homologação E2E #1, #6 |
| **E2** Identidade | ✅ | API X3 + E2E #2, #9 |
| **E3** SAR / UIF | ✅ | API X2 + E2E #3, #7 |
| **E4** EDD | ✅ | E2E #4 |
| **E5** Congelamento | ✅ | API X4 + E2E #8 |
| **E6** Explainability | ✅ | — |
| **E7** RPB | 🟡 | Template oficial X1 |
| **E8** Admin versões | ✅ | — |
| **E9** RCBE | ✅ | — |
| **E10** Homologação | 🟡 | **10 cenários E2E** + dossier + pen test 🔴 |

Checklist capacidade: [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md) — secção **Execução homologação** por preencher.

---

## Dependências externas

| ID | Entrega | Responsável |
|----|---------|-------------|
| X1 | Template RPB oficial BdP | Compliance |
| X2 | Credenciais API UIF | Instituição |
| X3 | Prestador identidade | Prestador |
| X4 | Endpoint congelamento BdP | Instituição |
| X5 | PAC v1 assinada | Compliance |
| X6 | PDF DPIA aprovado DPO | DPO |

---

## Configuração produção

Ver `.env.example` e `Compliance:RequireLiveIntegrations=true`:

```env
KYC_DB_CONNECTION=...
IdentityVerification__BaseUrl=...
Uif__BaseUrl=...
BdpAssetFreeze__BaseUrl=...
DataRetention__EnableHostedService=true
```

---

## Próximos passos (ordem)

1. **`dotnet test`** → executar [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) (cenários 1–10).
2. Preencher `docs/dossier/` e assinar tabela E2E.
3. [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) → `dossier/10-seguranca/`.
4. Credenciais X2–X4 em staging (fluxo API completo, não só manual).
5. Assinaturas governança: [governanca/POLITICA_SEGURANCA_INFORMACAO.md](governanca/POLITICA_SEGURANCA_INFORMACAO.md), PCN/PRD.

---

## Desvio intencional vs. Blueprint.md v1.1

| Blueprint original | Implementação | Motivo |
|--------------------|---------------|--------|
| Claude Sonnet | Ollama Qwen | RGPD on-prem |
| Azure Blob | `Data/cases` local | Fase 5b |
