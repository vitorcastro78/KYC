# Operations and UAT — KYC AI Platform

> Consolidated: deployment, runbooks, E2E tests, regulatory and security checklists, evidence dossier.
> Language pack: **English**. Hub: [`../README.md`](../README.md).

---

## 1. On-prem deployment

Layout matches [ContextMemory](https://github.com/Kortexio/ContextMemory): `docker-compose.yml` (build), `docker-compose.ghcr.yml` (images), `.env.example`, `scripts/docker-run.*`.

### 1.1 Prerequisites

- Docker and Docker Compose
- Reachable ContextMemory (`CONTEXT_MEMORY_BASE_URL`, e.g. `https://context.kortexio.io`)
- `.env` file (copy from `.env.example`) — **never commit**

### 1.2 Start (local build)

```bash
cp .env.example .env
docker compose up --build -d
# or: ./scripts/docker-run.sh --build
```

### 1.3 Start (GHCR images)

```bash
docker compose -f docker-compose.ghcr.yml up -d
```

### 1.4 Database only (dotnet run on host)

```bash
docker compose -f docker-compose.db.yml up -d
```

### 1.5 Migrations

Migrations do **not** run on app startup. Apply explicitly:

```bash
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
# or
dotnet KYC.Web.dll --migrate-only
docker compose run --rm --no-deps kyc-web --migrate-only
```

### 1.6 Post-deployment checks

| Check | Command / URL |
|-------|---------------|
| UI | `http://localhost:8080` (or `KYC_WEB_PORT`) |
| Health | `GET /health` |
| Admin | `KYC_ADMIN_EMAIL` / `KYC_ADMIN_PASSWORD` |
| Workers | `Data/ofac`, `Data/eu-fsf` volumes after start |

### 1.7 Critical compliance variables

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

```sql
SELECT tgname FROM pg_trigger WHERE tgname = 'tr_audit_entries_immutable';
```

### 2.2 Identity webhook (HMAC)

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

### 2.4 CI

Push/PR to `main`, `develop`, or `feature/*` → `.github/workflows/ci.yml`.

---

## 3. Runbook — CAP (Customer Acceptance Policy)

**Legal basis:** Law 83/2017, Art. 24.

### Active version

1. Admin → **Settings** — “Active CAP” card
2. DB: `customer_acceptance_policies` with `IsActive = true`
3. Seed creates CAP `1.0.0` if empty

### New version

1. Admin → Settings → version → **Activate**
2. Previous policy deactivated; new cases get `LegalBasisRef = PAC/{version}/Lei83/2017-Art24`

### Start rules

| Rule | Effect |
|------|--------|
| Prohibited CAE | `PolicyViolationException` |
| Prohibited / offshore jurisdiction | Auto-reject or violation |
| PEP in structure | Auto-reject (CAP config) |

**Evidence:** `docs/dossier/01-pac/`

---

## 4. E2E scenarios — BdP UAT

### 4.1 Prerequisites

| Item | Check |
|------|-------|
| DB | `KYC_DB_CONNECTION` + migrations |
| ContextMemory | Reachable `CONTEXT_MEMORY_BASE_URL` |
| CAP | Active |
| Users | Analyst + Supervisor + Admin |
| Tests | `dotnet test` — 0 failures before manual E2E |

**Simulate API failure (scenarios 6–9):** `Compliance:RequireLiveIntegrations=true` without UIF/BdP/identity URLs, invalid URLs, or mock off.

### 4.2–4.6 Scenarios 1–5 (mandatory)

1. **CAP at start** — CAE `92000` rejected; valid case → `InProgress` + `LegalBasisRef` → `01-pac/`
2. **Identity** — verify + HMAC webhook (§2.2); approve blocked if UBO pending → `06-identidade/`
3. **SAR** — narrative ≥200 submit; “not applicable” ≥50 → `05-sar-uif/`
4. **EDD 4-eyes** — funds origin + distinct second approver → `08-audit/`
5. **RPB** — generate, `?format=bdp`, submit → `04-rpb/`

### 4.7–4.11 Scenarios 6–10 (manual contingency)

6. **Manual legal name** without RCBE/GLEIF → `01-pac/` or `09-e2e/`
7. **Urgent SAR** UIF down → Pending → manual UIF ref → `05-sar-uif/`
8. **BdP freeze** API fail → manual BdP ref → `07-congelamento/`
9. **Manual identity** (no API) justification ≥20 → `06-identidade/`
10. **Manual risk signals** + confirm/dismiss → `09-e2e/`

### 4.12 Execution record

| # | Scenario | Date | Executor | Result | Evidence |
|---|----------|------|----------|--------|----------|
| 1–10 | (fill during UAT) | | | ☐ OK ☐ Fail | see folders above |

**Compliance signature:** _________________________ Date: __________

### 4.13 Automated execution

```powershell
.\scripts\generate-e2e-evidence.ps1
.\scripts\run-e2e-ui-scenarios-2-5.ps1 -SkipAppStart
```

---

## 5. Regulatory checklist — Capabilities

### Law 83/2017 — AML/CFT

- [x] Versioned CAP at case start
- [x] Simplified / Standard / Enhanced DD
- [x] EDD: source of funds before approval
- [x] Periodic review (`NextReviewDue`)
- [x] SAR/UIF with audit trail

### BdP Notice 1/2022

- [x] Identity verification (webhook + polling + UI)
- [x] Approval blocked if UBO/admin unverified
- [x] 4-eyes on EDD

### Law 97/2017 — Asset freeze

- [x] Auto notification on confirmed sanction
- [x] `AssetFreezeNotified` recorded

### BdP Instruction 8/2024 — RPB

- [x] Annual `AmlComplianceReport`
- [x] JSON + BdP XML export

### GDPR

- [x] Active DPIA (Admin)
- [x] Immutable audit trail
- [x] Auto-approve Low risk only
- [x] Report explainability (Art. 22)

### Operational

- [x] Health `/health`
- [x] Secrets outside repo
- [x] On-prem deploy documented
- [x] CI pipeline

---

## 6. Pen-test checklist (UAT only)

### AuthZ

- [ ] `/admin/*` without `KYC.Admin` → 403
- [ ] Admin AML APIs require `KYC.Admin`
- [ ] Identity webhook requires HMAC
- [ ] Cross-case IDOR → 401/403

### Input

- [ ] SAR narrative &lt; 200 rejected server-side
- [ ] Upload MIME/size limits
- [ ] Invalid NIF validated (no 500)

### Sensitive data / transport / deps

- [ ] Secrets only env/Key Vault; no keys/PII in logs
- [ ] HTTPS; HttpOnly/Secure cookies; restricted CORS
- [ ] `dotnet list package --vulnerable` clean; fresh Docker image

### Regulatory smoke

- [ ] Immutable audit trigger
- [ ] Active CAP/scoring/DPIA not deletable

| Date | Tester | Tool | Critical | High | Medium | Pass |
|------|--------|------|----------|------|--------|------|
| | | | 0 | | | ☐ Yes ☐ No |

**Evidence:** `docs/dossier/10-seguranca/` · template: [`governanca/RELATORIO_PEN_TEST_MODELO.md`](governanca/RELATORIO_PEN_TEST_MODELO.md)

---

## 7. Evidence dossier

See [`../dossier/README.md`](../dossier/README.md) (`01-pac` … `10-seguranca`).

---

## 8. Analyst quick start

1. Open UAT URL (Analyst / Supervisor / Admin)
2. **Cases → New** — wait for screening
3. Compliance: UBO identity; EDD funds; SAR if banner
4. Approve only if `CanApproveMessage` is clear
5. In-app help: [`../help-online/en/`](../help-online/en/)

---

## 9. External dependencies (go-live)

| ID | Deliverable | Blocks |
|----|-------------|--------|
| X1 | Official BdP RPB template | Final XML export |
| X2 | UIF API / MOU | Production SAR |
| X3 | Identity provider contract | Production verification |
| X4 | BdP freeze API | Real notifications |
| X5 | Signed CAP v1 | Formal UAT |
| X6 | DPIA PDF (DPO) | GDPR |

---

## 10. Next operational steps

1. Run E2E (§4) and fill §4.12
2. Complete pen test (§6) → `dossier/10-seguranca/`
3. Staging credentials X2–X4
4. RPB template X1 → `BdpRpbExporter.cs`
5. Go-live with `Compliance:RequireLiveIntegrations=true`
