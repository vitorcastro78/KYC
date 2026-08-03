# Development Plan — 100% BdP Compliance

> **Basis:** `BLUEPRINT_BdP_Compliance_Addendum.md` (sec. 13–22) + `docs/CHECKLIST_HOMOLOGACAO_BDP.md`  
> **Current status (May 2026):** ~95% product code (see `docs/BLUEPRINT_COMPLETION_STATUS.md`); manual contingencies ✅; pending **execution** of homologation (E2E 10 scenarios, dossier, pen test) and X1–X4 credentials.  
> **Objective:** 59 acceptance criteria (47 technical + 12 checklist) verified in the homologation environment.  
> **Unified documentation:** `docs/README.md` → `DOCUMENTACAO_APLICACAO.md`, `CATALOGO_FUNCIONALIDADES.md`, `OPERACOES_E_HOMOLOGACAO.md`.

---

## 1. Definition of Done

An item is only **Done** when:

1. Code is merged into the reviewed release branch.
2. An automated test or documented E2E script passes.
3. Configuration is documented in `.env.example` / `appsettings` (no secrets in the repository).
4. Evidence is attached to the homologation dossier (screenshot, audit log, or export).
5. The corresponding checklist item is marked `[x]` by compliance.

---

## 2. Inventory: done vs. missing

### 2.1 Already implemented (do not repeat work)

| ID | Item | Main location |
|----|------|---------------|
| D-01 | PAC, Scoring, DPIA, RPB entities + migration | `20260529205723_BdpComplianceAndGtm` |
| D-02 | Regulatory `KycCase` / `CaseParty` fields | `KycCase.cs`, `CaseParty.cs` |
| D-03 | `LegalBasisAttribute` in `DocumentFactKey` | `DocumentFactKey.cs` |
| D-04 | `DueDiligenceLevelEvaluator` | `DueDiligenceLevelEvaluator.cs` |
| D-05 | `PolicyComplianceValidator` in the **pipeline** | `KycCasePipelineRunner.cs` |
| D-06 | `CanApprove`, EDD 4-eyes, source of funds | `KycCase.cs` |
| D-07 | SAR commands + `SarEligibilityEvaluator` | `ComplianceCommandHandlers.cs`, pipeline |
| D-08 | Asset freeze in `OverrideSignalCommand` | `ComplianceCommandHandlers.cs` |
| D-09 | `PeriodicReviewSchedulerJob` | `PeriodicReviewSchedulerJob.cs` |
| D-10 | `IAmlComplianceReportService` + JSON export | `AmlComplianceReportService.cs` |
| D-11 | Compliance UI (SAR, identity, EDD, badges) | `ComplianceCaseSection`, `PartyIdentityPanel`, `SarActionModals`, `EntityCard` |
| D-12 | RPB / scoring / DPIA Admin + upload | `Pages/Admin/*` |
| D-13 | Ollama-only LLM, OFAC SLS download | `KycLlmEngine`, Workers |
| D-14 | Health checks, Docker prod, CI | `HealthCheckExtensions`, `docker-compose.prod.yml` |
| D-15 | `PolicyComplianceValidator` tests | `PolicyComplianceValidatorTests.cs` |
| D-16 | Rich UBO graph UI + case-party merge | `UboGraphView`, `UboGraphViewBuilder` |
| D-17 | `ICurrentAnalystAccessor` + Entra Graph supervisors | `HttpContextAnalystAccessor`, `EntraGraphSupervisorUserDirectory` |
| D-18 | Production-integration guards | `ComplianceIntegrationOptions`, `RequireLiveIntegrations` |
| D-19 | Manual UIF-reference record (pending SAR) | `RegisterManualUifReferenceCommand` |
| D-20 | UBO view-builder tests | `UboGraphViewBuilderTests.cs` |
| D-21 | Manual BdP freeze + sanction API failure | `RegisterManualAssetFreezeReferenceCommand` |
| D-22 | Urgent SAR → Pending after UIF failure | `RecordSarPendingAfterApiFailure` |
| D-23 | Manual identity contingency | `RecordManualIdentityVerificationCommand` |
| D-24 | Manual signals + UI override | `AddManualRiskSignalCommand`, `SignalCard` |
| D-25 | Legal name + start preview | `LegalCompanyName`, `GetEntityResolutionPreviewQuery` |

### 2.2 Missing (scope of this plan)

See **`docs/BLUEPRINT_COMPLETION_STATUS.md`** (updated map). Summary: external credentials X1–X6, E10 homologation (E2E, dossier, pen test), Claude/Blob (main blueprint phase 2).

### 2.3 Status by epic — code (May 2026)

> The tables in section 3 are **specification/backlog**; this section reflects **actual progress**.

| Epic | Code | Pending (non-code) |
|-------|------|--------------------|
| **E1** PAC / start | ✅ Done | — |
| **E2** Identity | ✅ Done (E2-09 P2 optional) | X3 provider credentials in prod |
| **E3** SAR / UIF | ✅ Done (E3-12 P2 SignalR optional) | Live X2 UIF API; homologation evidence |
| **E4** EDD | ✅ Done (E4-06 P2) | — |
| **E5** Asset freeze | ✅ Done | Live X4 BdP API |
| **E6** Explainability | ✅ Done | — |
| **E7** RPB | 🟡 Internal XML export | Official X1 template (E7-01) |
| **E8** Version administration | ✅ Done | — |
| **E9** RCBE | ✅ Done (E9-03 pipeline detection P2) | — |
| **E10** Homologation | 🟡 Automated tests ✅ | Manual E2E, dossier, pen test 🔴 |

**Overall code percentage:** ~95% (BdP addendum); regulatory homologation **0% evidence** until `E2E_HOMOLOGACAO.md` (10 scenarios) and `dossier/` are completed.

---

## 3. Epics and tasks

### E1 — Legal foundation and case start (3–4 days)

| ID | Task | Priority | Acceptance criterion |
|----|------|----------|----------------------|
| E1-01–02 | Inject and run `ICustomerAcceptancePolicyRepository` + `PolicyComplianceValidator` before `repository.AddAsync`; validate sectors, jurisdictions and PEP auto-reject | P0 | Violating case is not created; `PolicyViolationException` |
| E1-03 | Capture `RelationshipType` + amount in `StartKycCaseCommand` / `NewCase.razor` | P1 | DD uses correct relationship type |
| E1-04 | Propagate configurable `LegalBasisRef` by active policy | P2 | Audit shows legal basis |
| E1-05 | `StartKycCase` + PAC unit tests | P0 | 3+ green scenarios |
| E1-06 | Document PAC flow in README or runbook | P2 | Compliance can follow it |

**Dependencies:** none. **Checklist:** Law 83/2017 — PAC.

### E2 — Identity verification (Notice 1/2022) (5–7 days)

| ID | Task | Priority | Acceptance criterion |
|----|------|----------|----------------------|
| E2-01–05 | Result command/handler, signed `POST /api/identity/webhook`, polling fallback, integration configuration and complete `GetVerificationResultAsync` | P0/P1 | Party is updated; pending session → Verified in ≤5 minutes; no production stub when URL is set |
| E2-06–08 | UI method modal, party status badge/session link, and Approve blocked when `CanApproveMessage` exists | P0 | Analyst starts session; Notice 1/2022 message visible |
| E2-09–10 | Identity domain events and webhook/`CanApprove` integration tests | P2/P1 | Audit + SignalR |

**Dependencies:** provider API contract (DigitalSign or equivalent). **Checklist:** Notice 1/2022 — identity + approval block.

### E3 — SAR / UIF (Law 83/2017, Art. 52–57) (4–5 days)

| ID | Task | Priority | Acceptance criterion |
|----|------|----------|----------------------|
| E3-01–04 | Require risk ≥ High or Critical signal, synchronous urgent SAR, narrative modal (minimum 200 chars), prefill critical/high signals | P0/P1 | Low case rejected without justification; immediate log/response |
| E3-05–08 | Not-applicable action with justification, `SuggestSar` banner, `SarStatus` list badge, SAR history | P0/P1/P2 | `SarStatus = NotRequired` + audit |
| E3-09–12 | Production UIF retry, status lookup, tests, submitted event/supervisor notification | P0/P2 | UIF reference in staging |

**Dependencies:** UIF credentials or MOU with documented manual process. **Checklist:** SAR/UIF + audit trail.

### E4 — EDD and enhanced pipeline (4–5 days)

| ID | Task | Priority | Acceptance criterion |
|----|------|----------|----------------------|
| E4-01–02 | Adverse-media year parameter (2 default, 5 EDD) and non-Simplified verification requirement | P0/P1 | EDD uses 5 years |
| E4-03–05 | Source-of-funds UI, second supervisor, readable applied-DD section | P0/P1 | `SecondApproverId` recorded |
| E4-06–07 | Invoke `CanProceedWithEnhancedDd` and evaluator tests | P2/P1 | PEP/offshore/occasional cases tested |

**Checklist:** DD, EDD source of funds, 4-eyes.

### E5 — Asset freeze (Law 97/2017) (3–4 days)

| ID | Task | Priority | Acceptance criterion |
|----|------|----------|----------------------|
| E5-01–02 | Set `UnderReview` after sanction confirmation; immediate supervisor realtime alert | P0 | No auto-approval; toast/dashboard |
| E5-03–06 | Live BdP API, production config, UI indicator/reference, integration test | P0/P1 | Staging confirmation; notification + flags |

**Dependencies:** BdP endpoint or manual procedure with signed SLA. **Checklist:** Law 97/2017.

### E6 — Report, AI and explainability (3–4 days)

| ID | Task | Priority | Acceptance criterion |
|----|------|----------|----------------------|
| E6-01–02 | LLM report Art. 22 automated-decision and AI-model-limitation sections | P0 | HTML contains sections |
| E6-03–04 | Ollama-only `aiModelsJson`; audit model version + prompt hash | P1 | No cloud |
| E6-05 | Validate auto-approve only for Low score ≤30 without High/Critical | P0 | Regression test |

**Checklist:** GDPR explainability, auto-approve.

### E7 — RPB BdP Instruction 8/2024 (5–8 days)

| ID | Task | Priority | Acceptance criterion |
|----|------|----------|----------------------|
| E7-01 | Obtain official BdP XML/JSON/XLS template | P0 | Versioned document |
| E7-02–03 | `IBdpRpbExporter`, mapping, official + internal JSON exports | P0 | Schema-valid export; two download formats |
| E7-04–05 | `AmlReport.razor` metrics/charts and annual report history | P1 | |
| E7-06–08 | BdP submission, `KYC.Admin` role, export/aggregate tests | P0/P1 | `BdpReferenceNumber` |

**Dependencies:** E7-01 (compliance). **Checklist:** RPB generation + formatted export.

### E8 — Admin: PAC, scoring and DPIA versions (3–4 days)

| ID | Task | Priority | Acceptance criterion |
|----|------|----------|----------------------|
| E8-01–03 | Create versioned scoring, DPIA upload, and PAC v2 commands | P0/P1 | Immutability |
| E8-04–05 | Active PAC/DPIA/scoring summary and no deletion of active version | P1/P0 | |

**Checklist:** active DPIA recorded.

### E9 — RCBE and auxiliary data (2–3 days)

| ID | Task | Priority | Acceptance criterion |
|----|------|----------|----------------------|
| E9-01–03 | `ReportRcbeDiscrepancyCommand`, IRN-report UI, pipeline declared-vs-RCBE detection | P1/P2 | Party flags + audit |

### E10 — Tests, operations and homologation (5–7 days)

| ID | Task | Priority | Acceptance criterion |
|----|------|----------|----------------------|
| E10-01–03 | Integration suite, manual E2E script, evidence checklist update | P0 | Green CI; 15 steps; 12/12 `[x]` |
| E10-04–08 | PDF dossier, mandatory production Workers, rules review, analyst training, basic pen test | P1/P0/P2 | |

**Checklist:** operational + all regulatory.

---

## 4. Suggested schedule (10 weeks)

| Week | Epics | Deliverable |
|------|-------|-------------|
| W1 | E1, E10-05 | PAC at start; OFAC/Workers validated |
| W2 | E2 (E2-01–05) | Identity webhook + API |
| W3 | E2 (E2-06–10), E3 (E3-01–05) | Identity UI + SAR modal |
| W4 | E3 (E3-06–12), E4 | Full SAR + EDD pipeline |
| W5 | E5, E6 | Freeze + LLM explainability |
| W6 | E7 (after BdP template) | Official RPB export |
| W7 | E7 (UI), E8 | RPB Admin + versions |
| W8 | E9, E10-01–03 | RCBE + CI tests |
| W9 | E10-04–08 | Homologation dossier |
| W10 | Buffer + UAT | Compliance sign-off |

**Parallelisation:** E7-01 (compliance) in W1; E4 UI in parallel with E3 after W2.

---

## 5. External dependencies (blockers)

| # | What | Provider | Impact if delayed |
|---|------|----------|-------------------|
| X1 | RPB export template, Instr. 8/2024 | Compliance / BdP | E7 blocked |
| X2 | UIF API (or manual process + SLA) | Institution | E3 production |
| X3 | Identity API (DigitalSign/CMD) | Provider | E2 production |
| X4 | BdP freeze-notification API | Institution | E5 production |
| X5 | Internally signed PAC v1 | Compliance | Seed ≠ production |
| X6 | Approved DPIA (PDF) | DPO | E8-02 |

---

## 6. Minimum homologation configuration

```env
# Base
KYC_DB_CONNECTION=...
OLLAMA_ENDPOINT=http://host:11434

# Compliance integrations
IdentityVerification__BaseUrl=
IdentityVerification__ApiKey=
IdentityVerification__WebhookSecret=
Uif__BaseUrl=
Uif__ApiKey=
BdpAssetFreeze__BaseUrl=

# Workers (OFAC + EU)
ExternalSources__OfacSdnDailyDownload__Enabled=true
ExternalSources__EuFsfDailyDownload__Enabled=true
```

---

## 7. Tracking metrics

| Metric | Target |
|--------|--------|
| Done tasks (sec. 3) | 62/62 |
| Homologation checklist | 12/12 |
| Compliance-test coverage | ≥ 80% critical handlers |
| Open P0 bugs | 0 before go-live |
| Average urgent-SAR time (E2E) | < 2 min (synchronous) |

---

## 8. Recommended implementation order (sprint 0 → 6)

```
Sprint 0 (quick P0):  E1-01, E1-02, E1-05, E3-07, E4-03, E4-04, E5-01, E5-02
Sprint 1:             Full E2
Sprint 2:             Full E3
Sprint 3:             E4, E5, E6
Sprint 4:             E7 + E8
Sprint 5:             E9, E10
```

---

## 9. Risks

| Risk | Mitigation |
|------|------------|
| RPB template unavailable | Interim JSON export + “draft” flag until official schema |
| UIF without API | Manual mode: analyst records external reference + `SubmitSar` writes audit only |
| Ollama unavailable | Heuristic score + report template (fallback already exists) |
| Large OFAC XML | Workers + shared Web/Workers path |

---

## 10. Immediate next step

1. Approve this plan with compliance (validate X1–X6).  
2. Open 62 Jira/GitHub issues from section 3.  
3. Start **Sprint 0** (E1 + quick SAR/list UI wins).

---

*Living document — update when D-xx items become Done.*
