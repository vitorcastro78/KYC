# Decisions, report and documents

## Case statuses

| Status | Typical meaning |
|--------|-----------------|
| Pending | Case created, screening not yet completed |
| In progress | Screening or analysis in progress |
| Under review | Requires human intervention (sanctions, SAR, high scoring, etc.) |
| Approved | Positive decision recorded |
| Rejected | Negative decision with a reason |

## Approve a case

1. Confirm that there is **no blocking message** next to the Approve button.
2. Frequent blockers: unverified UBO, missing source of funds (EDD), critical signals awaiting confirmation.
3. In **EDD**, choose the second approver in the modal.
4. Confirm the approval.

**Low-risk** cases (score ≤ 30, without serious signals) can be **automatically approved** by the engine — check the status after screening.

## Reject or request review

- **Reject** — reason mandatory; use when the business relationship should not continue.
- **Request manual review** — routes the case to the review queue without rejecting it permanently.

## KYC report

| Action | Where |
|--------|-------|
| View HTML report | **View report** in the case details (`/cases/{id}/report`) |
| Export PDF | **PDF** button in the case details or report |

The report includes an executive summary, parties, signals, scoring, recommendation and transparency notes (GDPR Art. 22).

> **Tip:** If the PDF fails, first try opening the HTML report. If the error persists, see [Troubleshooting](07-resolucao-problemas.md).

## Document upload

In the case details:

1. **Upload document** — PDF, DOCX or image (up to ~25 MB).
2. Optionally associate it with a party and a type (identification, statements, UBO, etc.).
3. Processing is **asynchronous** — status: Pending → Processing → Completed / Failed.
4. After completion, you can trigger **re-screening** to incorporate extracted facts.

## Add a party manually

**Add party** — shareholder, UBO, corporate body, attorney-in-fact, with the option of immediate screening after saving.
