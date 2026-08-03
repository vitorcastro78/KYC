# New case and automatic screening

## Open a new case

**Menu:** Cases → **New case** (`/cases/new`)

| Field | What to enter |
|-------|---------------|
| NIF | Tax ID of the applicant (9 digits, PT) |
| Amount | Requested credit or exposure amount |
| Relationship | Occasional or ongoing |
| CAE | Economic activity code (when applicable) |
| Company name (manual) | **Required** if the system cannot resolve the name in RCBE/GLEIF |

### What happens next

1. The **Customer Acceptance Policy (PAC)** validates the CAE and jurisdiction **before** saving the case.
2. If the PAC rejects it, the case **is not created** — correct the data or contact the administrator (Settings → PAC).
3. Case created: status **In progress** and start of **automatic screening**.

> **Tip:** Generic names such as “Entity {NIF}” indicate a resolution failure. Return to the form and enter the correct company name manually.

## Monitor screening

In the **case details** (`/cases/{id}`):

- The **progress bar** shows the module that is running (sanctions, media, scoring, report, etc.).
- The percentage updates in real time (SignalR) and through database queries.
- When it reaches 100%, the KYC report becomes available.

### Rerun automatic screening

Use **Rerun automatic screening** when:

- You added parties or documents after the first screening;
- You want to regenerate signals and the report with updated data.

Confirm in the dialog — the operation may take several minutes.

## Risk signals

After screening, review the list of **signals** in the case details:

| Action | When to use |
|--------|-------------|
| **Confirm** | The match in lists/media is valid for this customer |
| **Dismiss** | False positive — the reason is implicitly recorded with the action |
| **Record manual signal** | APIs failed or there is a risk not detected automatically |

## Screening a single party

- On the party card, use **Screen this party**, or
- Open **Party details** (`/cases/{id}/parties/{partyId}`) and run individual screening.

## UBO graph

- In the case details, expand **UBO graph** to view the corporate structure.
- Dedicated screen: button to open the full graph (`/cases/{id}/ubo`).
