# BdP compliance (yellow section)

In the **case details**, the **BdP Compliance** card brings together regulatory obligations before approval.

## Identity verification (UBO and corporate bodies)

**Basis:** BdP Notice 1/2022 — identify and verify beneficial owners and directors.

| Status on screen | Meaning | What to do |
|------------------|---------|------------|
| Pending | Not yet verified | Open **Verify identity** on the party |
| Verified | Process completed | You may proceed with approval (if the remaining requirements are OK) |
| Manually verified | No provider API | Use when the external portal is unavailable — justification ≥ 20 characters |

**Methods available in the modal:**

- In-person verification (document reference)
- Remote session (portal link, when configured)
- **Manually verified (without API)** — documented contingency

> **Attention:** As long as a UBO or corporate body remains **pending**, the **Approve** button remains blocked with an explanatory message.

## Enhanced due diligence (EDD)

When the CDD level is **EDD**:

1. Fill in **Source of funds** in the compliance section.
2. During approval, select the **second approver** (supervisor) — mandatory.

## Report to the UIF (SAR)

| Situation | Action |
|-----------|--------|
| Suspicious transaction to report | **Report to UIF** — narrative of **at least 200 characters** |
| SAR not applicable | **SAR not applicable** — justification of **at least 50 characters** |
| Urgent | Mark “Urgent” for immediate submission (when the integration is active) |
| UIF API unavailable | Record the **manual reference** in the SAR section field; status remains **Pending** |

Supervisors receive real-time SAR alerts in the supervision group.

## Asset freezing (BdP)

After **confirming** a sanctions signal:

- The system can automatically notify the BdP and place the case **Under review**.
- If the API fails, a red alert appears — record the **manual reference** for BdP confirmation.

## RCBE discrepancy

On a party’s identity record, use **Report RCBE discrepancy** when registry data does not match the documentation — it is recorded in the audit trail.

## Acceptance policy (PAC)

The PAC is validated **when the case is created**. Prohibited CAEs or jurisdictions prevent it from being opened. Administrators manage versions in **Administration → Settings**.
