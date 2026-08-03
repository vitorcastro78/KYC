# BdP Homologation Checklist — KYC AI Platform

## Law 83/2017 — AML/CFT
- [x] Active versioned PAC (`customer_acceptance_policies`) — validation in `StartKycCase`
- [x] Simplified/Standard/Enhanced DD calculated per case
- [x] EDD: source of funds required before approval
- [x] Periodic review (`NextReviewDue`) after approval
- [x] SAR/UIF with audit trail (`SarSubmitted`, UIF reference)
- [x] Pending SAR + manual UIF record (`SarApiFailedPendingManual`, `SarManualRegistered`)
- [x] Manual legal name if RCBE/GLEIF fails (`LegalCompanyName` at start)
- [x] Manual risk signals + analyst confirmation (`AddManualRiskSignalCommand`, `OverrideSignal`)

## BdP Notice 1/2022
- [x] Identity verification (webhook + polling + method UI)
- [x] Manual contingency verification (`RecordManualIdentityVerificationCommand`)
- [x] Approval blocked if UBO/administrator is not verified
- [x] 4-eyes in EDD (`SecondApproverId`)

## Law 97/2017 — Asset freeze
- [x] Automatic notification when sanction is confirmed
- [x] `AssetFreezeNotified` recorded
- [x] Manual BdP reference record if API fails (`RegisterManualAssetFreezeReferenceCommand`)

## BdP Instruction 8/2024 — RPB
- [x] Annual `AmlComplianceReport` generation
- [x] Internal JSON export + BdP XML (`?format=bdp`)

## GDPR
- [x] Active DPIA recorded (Admin creates version)
- [x] Immutable audit trail (trigger `tr_audit_entries_immutable` in the BdP migration)
- [x] Auto-approve only for Low risk (score ≤30, no High/Critical/sanctions)
- [x] Explainability section in the report (Art. 22)

## Homologation execution (evidence)

- [x] E2E scenarios 1–10 executed (automated tests + UI 2–5) — see [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) §Record (2026-05-31)
- [x] Dossier completed in `docs/dossier/` (partial: `10-seguranca/` pen test missing)
- [x] PAC screenshots in `docs/dossier/01-pac/` — [REGISTO_UI_PAC_20260531-181205.md](dossier/09-e2e/REGISTO_UI_PAC_20260531-181205.md)
- [ ] Pen test — [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md)

_Homologation date:_ 2026-05-31 _Owner:_ technical homologation (automated + Playwright UI)

## Operational
- [x] Health check `/health`
- [x] Secrets outside the repository (`.env` in `.gitignore`, use `.env.example`)
- [x] On-prem deployment documented (`docker-compose.prod.yml`, `docs/DEPLOY_ONPREM.md`)
- [x] CI pipeline (`/.github/workflows/ci.yml` — build, migrate, test)
