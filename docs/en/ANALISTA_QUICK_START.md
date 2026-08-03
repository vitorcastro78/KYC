# Quick Start — AML Analyst (KYC Platform)

## 1. Access

- UAT/production URL as deployed (`docs/DEPLOY_ONPREM.md`)
- Roles: `KYC.Analyst` (cases), `KYC.Supervisor` (escalation + 4-eyes), `KYC.Admin` (RPB, PAC)

## 2. New case

1. **Cases → New** — NIF, amount, relationship type (occasional/ongoing), CAE where applicable.
2. Wait for automated screening (progress bar in the case details).
3. Review signals and confirm/discard matches.

## 3. Compliance (yellow section)

- **Identity** — Verify UBOs/directors (BdP Notice 1/2022) before approval.
- **EDD** — Provide source of funds; a second approver is mandatory.
- **SAR** — If the yellow banner appears: report to UIF (≥200 characters) or mark as not applicable.
- **RCBE** — Report a discrepancy if detected.

## 4. Approve or reject

- The **Approve** button is active only when `CanApprove` indicates no blocking condition.
- Cases with a confirmed sanction → automatic asset freeze + “Under review” status.

## 5. Real-time alerts

- SignalR: screening progress, report ready, compliance alerts (SAR, identity, asset freeze).
- Supervisors receive SAR alerts in the `supervisors` group.

## 6. References

- Complete E2E: `docs/E2E_HOMOLOGACAO.md`
- BdP checklist: `docs/CHECKLIST_HOMOLOGACAO_BDP.md`
