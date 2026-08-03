# E2E — BdP Homologation

> Run in the homologation environment with database migration `20260529205723_BdpComplianceAndGtm`.  
> **Objective:** fill in the §Record table and attach evidence in `docs/dossier/`.

## Prerequisites

| Item | Check |
|------|-------|
| DB | `KYC_DB_CONNECTION` in `.env` (e.g. `Host=...;Port=5433;Database=azureopsagent;...`) — do **not** assume `localhost` if the database is remote |
| DB | `dotnet ef database update` in the homologation database |
| Ollama | Reachable `OLLAMA_ENDPOINT` |
| PAC | Active (seed `ComplianceSeedHostedService` or Admin → Settings) |
| Users | Analyst + Supervisor + Admin (Entra or dev Identity) |
| Automated tests | `dotnet test` — **0 failures** before manual E2E |

### Simulate API failure (manual contingency)

For scenarios 6–9, use **one** of:

- `Compliance:RequireLiveIntegrations=true` without `Uif__BaseUrl` / `BdpAssetFreeze__BaseUrl` / `IdentityVerification__BaseUrl`, **or**
- Invalid URLs in staging, **or**
- Temporarily turn off the mock/integration service.

---

## Mandatory scenarios

### 1. PAC at case start

1. **Cases → New** with CAE `92000` → PAC violation message; case **not** created.
2. Valid case (real or test NIF) → `InProgress`, `LegalBasisRef` populated.
3. **Evidence:** PAC-error screenshot + created-case screenshot; `CaseStarted` audit.

**Folder:** `docs/dossier/01-pac/`

---

### 2. Identity (API + webhook)

1. **Compliance** section → “Verify identity” (video/CMD/in person).
2. Webhook: `POST /api/identity/webhook` with body `{ "partyId", "sessionId", "verified": true }` and `X-Webhook-Signature: sha256=<hmac>` — see [HOMOLOGACAO_RUNBOOK.md](HOMOLOGACAO_RUNBOOK.md).
3. **Verified** badge on the party; attempt **Approve** with another UBO still pending → disabled button + `CanApproveMessage`.

**Folder:** `docs/dossier/06-identidade/`

---

### 3. SAR (submission and not applicable)

1. High-risk case / critical signal → SAR banner → narrative ≥200 characters → submit (non-urgent or urgent with API OK).
2. Case with no obligation → “SAR not applicable” → justification ≥50 characters → `SarStatus = NotRequired`.
3. Case list: correct SAR and DD badges.

**Folder:** `docs/dossier/05-sar-uif/`

---

### 4. EDD 4-eyes

1. Case with `DueDiligenceLevel = Enhanced` → save **source of funds**.
2. In-person or CMD verification for required parties.
3. **Approve** with a **different** second approver → `SecondApproverId` in database / audit.

**Folder:** `docs/dossier/08-audit/` (EDD case extract)

---

### 5. RPB (Admin)

1. Admin → Generate current-year RPB.
2. Export `?format=bdp` → XML (internal structure of Instruction 8/2024).
3. Mark submitted → BdP reference in record.

**Folder:** `docs/dossier/04-rpb/`

---

## Manual-contingency scenarios (APIs unavailable)

### 6. Legal name at start (without RCBE/GLEIF)

1. NIF without a RCBE/GLEIF match (or environment without endpoint).
2. Preview in **New case** shows “provide a manual legal name” notice.
3. Attempt **Start** without name → error; fill in **Legal name (manual)** → case created with this name (not `Entity {NIF}`).
4. Applicant party with the same name.

**Folder:** `docs/dossier/01-pac/` or `docs/dossier/09-e2e/`

---

### 7. Urgent SAR — UIF failure → manual record

1. UIF API unavailable (see simulation prerequisites).
2. Submit **urgent** SAR → warning toast; `SarStatus = Pending`.
3. SAR section: alert + **Manual UIF record** field → enter reference (≥5 chars) → `SarSubmitted` + `SarManualRegistered` audit.
4. Check `SarApiFailedPendingManual` audit.

**Folder:** `docs/dossier/05-sar-uif/`

---

### 8. BdP asset freeze — API failure → manual record

1. Case with **Sanction** signal → **Risk signals** → **Confirm match**.
2. With BdP API failure: red “BdP asset freeze pending” alert; case `UnderReview`; `AssetFreezeNotificationFailed` audit.
3. Enter manual BdP reference → `AssetFreezeNotified` + `AssetFreezeManualRegistered` audit.

**Folder:** `docs/dossier/07-congelamento/`

---

### 9. Identity — manual verification (without API)

1. UBO/corporate-body party still pending; provider unavailable.
2. **Manually verified (without API)** → justification ≥20 characters + optional document reference.
3. `ThirdPartyReliance` method, Verified status; `IdentityManualVerified` audit.
4. Approval unlocked for that party (if the remaining parties are OK).

**Folder:** `docs/dossier/06-identidade/`

---

### 10. Screening signals — manual + confirmation

1. **Record manual signal** (type, severity, source, description ≥10) — source stored as `Manual:...`.
2. Pending automatic signal → **Confirm** or **Discard** on signal card.
3. Timeline / audit with `ManualRiskSignalAdded` and `AnalystOverride`.

**Folder:** `docs/dossier/09-e2e/`

---

## Minimum dossier evidence

| Folder | Minimum content |
|--------|-----------------|
| `01-pac/` | Active PAC screenshot + rejected CAE 92000 test |
| `04-rpb/` | XML + JSON export + submission reference |
| `05-sar-uif/` | Submitted SAR OR manual record after API failure |
| `06-identidade/` | Webhook OK + manual verification (screenshot) |
| `07-congelamento/` | API confirmation OR manual reference after sanction |
| `08-audit/` | SQL or `audit_entries` export from test case |
| `09-e2e/` | This signed table (PDF or scan) |
| `10-seguranca/` | Completed [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) |

---

## Execution record (complete in homologation)

| # | Scenario | Date | Executor | Result | Evidence |
|---|----------|------|----------|--------|----------|
| 1 | PAC start | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Failed | `09-e2e/test-results-20260531-021829.trx` — [REGISTO_EXECUCAO_20260531-021829.md](dossier/09-e2e/REGISTO_EXECUCAO_20260531-021829.md) |
| 2 | Identity + webhook | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Failed | `06-identidade/02-*-20260531-024650.png`; case `943cb0b0-3fb3-4ca6-974f-421a06063d2a` — [REGISTO_UI_CENARIOS_2-5_20260531-024650.md](dossier/09-e2e/REGISTO_UI_CENARIOS_2-5_20260531-024650.md) |
| 3 | SAR | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Failed | `05-sar-uif/03-*-20260531-024650.png`; SAR cases `8279989f-…` + identity (not applicable) |
| 4 | EDD 4-eyes | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Failed | `08-audit/04-*-20260531-024650.png`; case `58c21877-ec18-4b01-9351-22cefefe6ee9` |
| 5 | RPB Admin | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Failed | `04-rpb/05-*-20260531-024650.png`, `05-rpb-export-bdp-20260531-024650.xml` |
| 6 | Manual legal name (start) | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Failed | `09-e2e/audit-export-*.json`, trx — [REGISTO_EXECUCAO_20260531-021829.md](dossier/09-e2e/REGISTO_EXECUCAO_20260531-021829.md) |
| 7 | Manual SAR after UIF failure | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Failed | `05-sar-uif/`, trx E2E-07 |
| 8 | Manual BdP freeze | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Failed | `07-congelamento/`, trx E2E-08 |
| 9 | Manual identity | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Failed | `06-identidade/`, trx E2E-09 |
| 10 | Manual signals + override | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Failed | `09-e2e/`, trx E2E-10 |

**Environment:** `http://localhost:5299` · DB `195.179.193.136:5433` (`azureopsagent`) · UI IDs: [e2e-ui-cases.json](dossier/09-e2e/e2e-ui-cases.json)

**Compliance signature:** _________________________ Date: __________

---

## Automated tests (prerequisite)

```bash
dotnet test
```

Relevant packages: `ComplianceFlowTests`, `ComplianceHandlersIntegrationTests`, `StartKycCaseCommandHandlerTests`, `SarEligibilityTests`, `IdentityWebhookHttpTests`, `UboGraphViewBuilderTests`.

---

## Automated execution (agent / local CI)

With `KYC_DB_CONNECTION` set in `.env` (aligned with `ConnectionStrings:KycDatabase` in `appsettings.json`):

```powershell
# .env: KYC_DB_CONNECTION=Host=...;Port=5433;Database=azureopsagent;...
.\scripts\generate-e2e-evidence.ps1
```

Generates: `HomologationE2eAutomatedTests` (7), JSON export in `docs/dossier/`, HTTP + webhook, and `docs/dossier/09-e2e/REGISTO_EXECUCAO_*.md`.

**UI (scenarios 2–5):**

```powershell
.\scripts\run-e2e-ui-scenarios-2-5.ps1 -SkipAppStart   # with KYC.Web already at http://localhost:5299
```

Prepares cases (`E2E-UI-PREP`), runs Playwright, saves screenshots in `04-rpb/`, `05-sar-uif/`, `06-identidade/`, `08-audit/`, and creates `REGISTO_UI_CENARIOS_2-5_*.md`.

---

## After E2E

1. Pen test: [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) → `10-seguranca/`.
2. Real X2–X4 credentials in staging (validate flows **without** manual contingency only).
3. Update [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md) with the homologation date (section below).
