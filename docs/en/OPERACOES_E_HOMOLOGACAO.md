# Operations and UAT — KYC AI Platform

> Consolidated document: deployment, runbooks, E2E tests, regulatory and security checklists, evidence dossier.

---

## 1. On-prem deployment

### 1.1 Prerequisites
- Docker and Docker Compose
- Accessible Ollama (`OLLAMA_ENDPOINT`, e.g. `http://host.docker.internal:11434`)
- `.env` file (copy `.env.example`) — **never commit**

### 1.2 Start-up
```bash
cp .env.example .env
# Edit passwords and KYC_DB_CONNECTION (compose: Host=kyc-postgres)
docker compose -f docker-compose.prod.yml up -d --build
```

### 1.3 Migrations
```bash
docker compose -f docker-compose.prod.yml exec kyc-web \
  dotnet ef database update --project /src/KYC.Infrastructure --startup-project /src/KYC.Web
```
On the host:
```bash
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

### 1.4 Post-deployment verification
| Check | Command / URL |
|-------|---------------|
| UI | `http://localhost:8080` (or `KYC_WEB_PORT`) |
| Health | `GET /health` |
| Dev admin | `KYC_ADMIN_EMAIL` / `KYC_ADMIN_PASSWORD` |
| Workers | `Data/ofac`, `Data/eu-fsf` volumes after start-up |

### 1.5 Critical compliance variables
```env
IDENTITY_VERIFICATION_WEBHOOK_SECRET=...
IdentityVerification__BaseUrl=...
Uif__BaseUrl=...
Uif__ApiKey=...
BdpAssetFreeze__BaseUrl=...
Compliance__RequireLiveIntegrations=true
```

---

## 2. Runbook — Technical UAT

### 2.1 Database
```powershell
$env:KYC_DB_CONNECTION="Host=...;Port=5433;Database=...;Username=...;Password=..."
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```
Confirm the audit trigger:
```sql
SELECT tgname FROM pg_trigger WHERE tgname = 'tr_audit_entries_immutable';
```
Optional test:
```powershell
dotnet test tests/KYC.Web.Integration.Tests --filter AuditImmutability
```

### 2.2 Identity webhook (HMAC)
Configure `IdentityVerification:WebhookSecret` or `IDENTITY_VERIFICATION_WEBHOOK_SECRET`.
```powershell
$body = '{"partyId":"<GUID>","sessionId":"sess-abc","verified":true}'
$secret = "your-secret"
$hash = [BitConverter]::ToString([System.Security.Cryptography.HMACSHA256]::HashData(
  [Text.Encoding]::UTF8.GetBytes($secret),
  [Text.Encoding]::UTF8.GetBytes($body))).Replace("-","").ToLower()
Invoke-RestMethod -Method Post -Uri "https://<host>/api/identity/webhook" `
  -Headers @{ "X-Webhook-Signature" = "sha256=$hash" } `
  -ContentType "application/json" -Body $body
```

### 2.3 Automated tests
```powershell
dotnet test
dotnet test tests/KYC.Web.Integration.Tests
```
Compliance coverage: `ComplianceHandlersIntegrationTests`, `ComplianceFlowTests`, `SarEligibilityTests`, `IdentityWebhookHttpTests`, `UboGraphViewBuilderTests`.

### 2.4 CI
Push/PR to `main`, `develop`, or `feature/*` → `.github/workflows/ci.yml` (PostgreSQL + migrations + tests).

---

## 3. Runbook — PAC (Customer Acceptance Policy)

**Legal basis:** Law 83/2017, Art. 24.

### Active version
1. Admin → **Settings** — “Active PAC” card
2. DB: `customer_acceptance_policies` with `IsActive = true`
3. Seed: `ComplianceSeedHostedService` creates PAC `1.0.0` if empty

### New version
1. Admin → Settings → version (e.g. `1.1.0`) → **Activate**
2. `CreateCustomerAcceptancePolicyCommand` deactivates the previous version
3. New cases: `LegalBasisRef` = `PAC/{version}/Lei83/2017-Art24`

### Rules at start-up
| Rule | Effect |
|------|--------|
| CAE in prohibited list | `PolicyViolationException` |
| Prohibited / offshore jurisdiction | Auto-reject or violation |
| PEP in the structure | Auto-reject (PAC configuration) |

**Evidence:** `docs/dossier/01-pac/`

---

## 4. E2E scenarios — BdP UAT

> Environment with migrated DB (`BdpComplianceAndGtm` + subsequent migrations).  
> Prerequisites: `KYC_DB_CONNECTION`, Ollama, active PAC.

### Scenario 1 — PAC at start-up
1. CAE case `92000` (gambling) → PAC failure
2. Valid case → `InProgress` + `LegalBasisRef`

### Scenario 2 — Identity (Notice 1/2022)
1. Compliance → “Verify identity” → method
2. HMAC webhook or polling → `Verified`
3. Approve without a verified UBO → disabled button + message

### Scenario 3 — SAR / UIF
1. High-risk case → SAR banner → narrative ≥200 → submit
2. “Not applicable” → justification ≥50 → `NotRequired`
3. Case list → SAR/DDC badges

### Scenario 4 — EDD 4-eyes
1. Enhanced + source of funds + verification
2. Approve with second supervisor → `SecondApproverId`

### Scenario 5 — RPB
1. Admin → Generate current-year RPB
2. Export `?format=bdp` → XML
3. Submit → BdP reference

### Scenarios 6–10 — Manual contingency (APIs unavailable)
Step-by-step details in **[E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md)**:
| # | Subject |
|---|---------|
| 6 | Manual company name (without RCBE/GLEIF) |
| 7 | Urgent SAR → Pending → manual UIF reference |
| 8 | Manual BdP asset freeze after sanction |
| 9 | Manual identity (without API) |
| 10 | Manual signals + confirm/discard |

### Execution record
> Complete table (10 rows) and compliance signature: **[E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md)** §Record.
| # | Scenario | Evidence |
|---|----------|----------|
| 1–5 | See above | `dossier/` according to E2E |
| 6–10 | Manual contingency | `01-pac/`, `05-sar-uif/`, `07-congelamento/`, `06-identidade/`, `09-e2e/` |

---

## 5. Regulatory checklist — Capabilities (Law 83/2017, BdP, GDPR)

> **Code** status — UAT execution evidence is separate.

### Law 83/2017 — AML/CFT
- [x] Versioned PAC active at case start
- [x] Simplified / Standard / Enhanced DDC
- [x] EDD: source of funds before approval
- [x] Periodic review (`NextReviewDue`)
- [x] SAR/UIF with audit trail

### BdP Notice 1/2022
- [x] Identity verification (webhook + polling + UI)
- [x] Approval blocked if UBO/director is unverified
- [x] EDD 4-eyes

### Law 97/2017 — Asset freeze
- [x] Automatic notification when sanction is confirmed
- [x] `AssetFreezeNotified` recorded

### BdP Instruction 8/2024 — RPB
- [x] Annual `AmlComplianceReport` generation
- [x] JSON + BdP XML export (`?format=bdp`)

### GDPR
- [x] Active DPIA (Admin)
- [x] Immutable audit trail (PostgreSQL trigger)
- [x] Auto-approve only for Low risk
- [x] Report explainability (Art. 22)

### Operations
- [x] Health `/health`
- [x] Secrets outside the repo (`.env.example` template)
- [x] Documented on-prem deployment
- [x] CI pipeline

---

## 6. Pen test — UAT checklist

> Suggested tool: OWASP ZAP baseline or manual review. **UAT only.**

### Authentication and authorization
- [ ] `/admin/*` without `KYC.Admin` → 403
- [ ] Admin AML APIs → `KYC.Admin`
- [ ] Identity webhook requires HMAC when a secret is set
- [ ] Other user's case IDOR → 401/403

### Input and injection
- [ ] SAR narrative &lt; 200 chars rejected server-side
- [ ] Upload: MIME and maximum size
- [ ] Invalid NIF → validation (no 500)

### Sensitive data
- [ ] Secrets only in env/Key Vault
- [ ] Logs without API keys / complete PII
- [ ] PDF without IDOR between cases

### Transport
- [ ] HTTPS in UAT/prod
- [ ] HttpOnly/Secure cookies
- [ ] Restricted CORS

### Dependencies
- [ ] `dotnet list package --vulnerable` with no criticals
- [ ] Updated Docker image

### Regulatory smoke
- [ ] Immutable audit trigger
- [ ] Active PAC/scoring/DPIA not deletable (EF interceptor)

### Result
| Date | Executor | Tool | Critical | High | Medium | Approved |
|------|----------|------|----------|------|--------|----------|
| | | | 0 | | | ☐ Yes ☐ No |

**Evidence:** `docs/dossier/10-seguranca/`

---

## 7. Evidence dossier (go-live)

### Folder structure
```
docs/dossier/
  01-pac/           Active PAC (Admin screenshot)
  02-dpia/          DPIA + document
  03-scoring/       Scoring version + prompt hash
  04-rpb/           BdP XML + JSON + submission reference
  05-sar-uif/       SAR + UIF reference
  06-identidade/    Webhook + party verification
  07-congelamento/  BdP notification
  08-audit/         Test-case audit extract
  09-e2e/           Signed E2E checklist
  10-seguranca/     Completed pen test
```

### How to generate
1. Run the scenarios in section 4
2. Admin → Settings: capture PAC, scoring, DPIA
3. Admin → RPB: generate, export, submit
4. Case with sanction: asset-freeze screenshot + audit
5. Name files with the date: `RPB-2025-20260530.xml`

### Owners
| Area | Owner |
|------|-------|
| Compliance / PAC | Compliance team |
| RPB | `KYC.Admin` |
| Security | Infrastructure + pen test |
| E2E | AML analyst + QA |

---

## 8. Quick start — AML analyst
1. **Access** — UAT URL; Analyst / Supervisor / Admin roles
2. **New case** — Cases → New; wait for screening
3. **Compliance** — UBO identity; EDD source of funds; SAR if banner; RCBE
4. **Approve** — Only when no `CanApproveMessage` block
5. **Alerts** — SignalR; supervisors in SAR group
6. **Reference** — This document + [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md)

---

## 9. External dependencies (go-live)
| ID | Delivery | Owner | Blocks |
|----|----------|-------|--------|
| X1 | Official BdP RPB template | Compliance | Final XML export |
| X2 | UIF API / MOU | Institution | Production SAR |
| X3 | Identity contract (DigitalSign/CMD) | Provider | Production verification |
| X4 | BdP asset-freeze API | Institution | Actual notification |
| X5 | Signed PAC v1 | Compliance | Formal UAT |
| X6 | DPO DPIA PDF | DPO | GDPR |

---

## 10. Next operational steps (order)
1. Run E2E (section 4) and complete the table
2. Complete pen test (section 6) → `dossier/10-seguranca/`
3. X2–X4 credentials in staging
4. X1 RPB template → update `BdpRpbExporter.cs`
5. Go live with `Compliance:RequireLiveIntegrations=true`

---

## Source documents (historical detail)
The files below remain in the repository; the relevant operational content was consolidated **in this document**:
- [DEPLOY_ONPREM.md](DEPLOY_ONPREM.md)
- [HOMOLOGACAO_RUNBOOK.md](HOMOLOGACAO_RUNBOOK.md)
- [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md)
- [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md)
- [PAC_RUNBOOK.md](PAC_RUNBOOK.md)
- [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md)
- [ANALISTA_QUICK_START.md](ANALISTA_QUICK_START.md)
- [dossier/README.md](dossier/README.md)
