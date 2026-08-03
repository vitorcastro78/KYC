# Blueprint completion status — KYC Platform

> **Last updated:** May 2026 · Branch `feature/kyc-document-ingestion`  
> **Objective:** a **production-ready** application, not a prototype.  
> **Sources:** `Blueprint.md` (v1.1) + `BLUEPRINT_BdP_Compliance_Addendum.md` + `docs/PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md`

---

## Executive summary

| Blueprint | Code | Live integrations | Homologation |
|-----------|------|-------------------|-------------|
| **Blueprint.md** (core KYC) | **~90%** | Environment-dependent | Manual E2E pending |
| **BdP Addendum** (compliance) | **~95%** | UIF / BdP / identity + **manual contingency** | Dossier + pen test pending |
| **Global** | **~95%** code · **0%** homologation evidence | Configure production `.env` | Run `docs/E2E_HOMOLOGACAO.md` (10 scenarios) |

**Legend:** ✅ Done · 🟡 Partial / dev mode · 🔴 Pending · 🌐 External (compliance/BdP)

**Current next step:** run [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) and fill in `docs/dossier/`.

---

## Manual contingency (May 2026) — ✅ code

| Gap | Implementation |
|-----|----------------|
| BdP asset freeze without API | `RegisterManualAssetFreezeReferenceCommand` + `NeedsManualAssetFreezeRegistration` UI |
| Urgent SAR fails at UIF | `RecordSarPendingAfterApiFailure` → `Pending` + manual record |
| Identity without provider | `RecordManualIdentityVerificationCommand` + UI button |
| Screening (media/judicial) | `AddManualRiskSignalCommand` + confirm/discard in `SignalCard` |
| Legal name on start | `LegalCompanyName` + `GetEntityResolutionPreviewQuery` in New case |

---

## Blueprint.md — by phase

### Phase 1 — Foundation
| Item | Status | Notes |
|------|--------|-------|
| 5 Clean Architecture projects | ✅ | Domain, Application, Infrastructure, Web, Workers |
| EF + PostgreSQL + migrations | ✅ | pgvector, audit trigger |
| Entra OIDC + local-dev Identity | ✅ | `AzureAd:Enabled` or Identity+PostgreSQL |
| Optional Key Vault | ✅ | `KYC_KEYVAULT_NAME` |
| Blazor + auth + CI | ✅ | `.github/workflows/ci.yml` |

### Phase 2 — Core KYC Engine
| Item | Status | Notes |
|------|--------|-------|
| StartKycCase + MediatR | ✅ | PAC + manual legal name in fallback |
| RCBE + GLEIF entity resolution | 🟡 | RCBE depends on endpoint; UI preview |
| Recursive UBO graph | ✅ | `BuildUboGraphAsync` |
| OFAC + EU Sanctions | ✅ | Workers download + local index |
| Service Bus / Rabbit / in-memory | ✅ | `Messaging:Provider` |
| Parallel scan pipeline | ✅ | `KycCasePipelineRunner` |
| Ollama Qwen scoring | ✅ | No Claude (documented deviation) |
| Append-only audit | ✅ | PostgreSQL trigger |

### Phase 3 — AI & Report
| Item | Status | Notes |
|------|--------|-------|
| Claude Sonnet API | 🔴 | Ollama-only (BdP/GDPR) |
| Local/cloud LLM routing | 🟡 | Local only |
| 8-section report + explainability | ✅ | Art. 22 |
| Document consistency check | ✅ | |
| pgvector embeddings | ✅ | |

### Phase 4 — UI & Workflow
| Item | Status | Notes |
|------|--------|-------|
| SignalR dashboard | ✅ | |
| CaseDetail scan progress | ✅ | |
| UBO graph UI | ✅ | |
| EDD 4-eyes approval | ✅ | |
| Report PDF export | ✅ | |
| Audit log Admin | ✅ | |
| Signals: confirm/discard | ✅ | `SignalCard` + `OverrideSignal` |

### Phase 5 — Sources & compliance
| Item | Status | Notes |
|------|--------|-------|
| Adverse media / AT / CITIUS / ICIJ | ✅ | + manual signals |
| Data retention job | 🟡 | `DataRetention:EnableHostedService` in prod |
| Pen test | 🔴 | Run checklist |

### Phase 5b — Document ingestion
| Item | Status | Notes |
|------|--------|-------|
| Full pipeline | ✅ | |
| Azure Blob / Doc Intelligence | 🔴 | Blueprint phase 2 |

---

## BdP Addendum — epics E1–E10

| Epic | Code | Main gap |
|------|------|----------|
| **E1** PAC / case start | ✅ | E2E homologation #1, #6 |
| **E2** Identity | ✅ | X3 API + E2E #2, #9 |
| **E3** SAR / UIF | ✅ | X2 API + E2E #3, #7 |
| **E4** EDD | ✅ | E2E #4 |
| **E5** Freeze | ✅ | X4 API + E2E #8 |
| **E6** Explainability | ✅ | — |
| **E7** RPB | 🟡 | X1 official template |
| **E8** Version administration | ✅ | — |
| **E9** RCBE | ✅ | — |
| **E10** Homologation | 🟡 | **10 E2E scenarios** + dossier + pen test 🔴 |

Capacity checklist: [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md) — **Homologation execution** section to be completed.

---

## External dependencies

| ID | Deliverable | Owner |
|----|-------------|-------|
| X1 | Official BdP RPB template | Compliance |
| X2 | UIF API credentials | Institution |
| X3 | Identity provider | Provider |
| X4 | BdP freeze endpoint | Institution |
| X5 | Signed PAC v1 | Compliance |
| X6 | Approved DPIA PDF | DPO |

---

## Production configuration

See `.env.example` and `Compliance:RequireLiveIntegrations=true`:

```env
KYC_DB_CONNECTION=...
IdentityVerification__BaseUrl=...
Uif__BaseUrl=...
BdpAssetFreeze__BaseUrl=...
DataRetention__EnableHostedService=true
```

---

## Next steps (in order)

1. **`dotnet test`** → run [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) (scenarios 1–10).
2. Fill in `docs/dossier/` and sign the E2E table.
3. [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) → `dossier/10-seguranca/`.
4. X2–X4 staging credentials (full API flow, not manual only).
5. Governance sign-offs: [governanca/POLITICA_SEGURANCA_INFORMACAO.md](governanca/POLITICA_SEGURANCA_INFORMACAO.md), BCP/DRP.

---

## Intentional deviation from Blueprint.md v1.1

| Original blueprint | Implementation | Reason |
|--------------------|---------------|--------|
| Claude Sonnet | Ollama Qwen | On-prem GDPR |
| Azure Blob | Local `Data/cases` | Phase 5b |
