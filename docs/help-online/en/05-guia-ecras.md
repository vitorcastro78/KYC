# Screen-by-screen guide

Quick reference: what to find in each area of the application.

## Dashboard (`/dashboard`)

- Total cases, cases in progress, approved today, under human review.
- List of the most recent cases.
- **New case** shortcut.
- Real-time updates when cases change status.

## Case list (`/cases`)

| Column / element | Description |
|------------------|-------------|
| Score | Risk score from 0–100 |
| CDD | Simplified / Standard / Enhanced |
| SAR badge | Status of communication to the UIF |
| Status | Pending, In progress, Under review, Approved, Rejected |

Click a row to open the details.

## Case details (`/cases/{id}`)

| Area | Content |
|------|---------|
| Hero | Name, NIF, score, risk level, status, CDD |
| Progress bar | Automatic screening in progress |
| Actions | Approve, Reject, Review, Rerun screening, Report, PDF |
| BdP compliance | Yellow card — identity, SAR, freezing, funds |
| Parties | Cards with screening and identity |
| Signals | List with confirm / dismiss |
| Documents | Upload and ingestion status |
| Timeline | Risk history and summarized audit trail |
| UBO graph | Preview and link to full screen |

## New case (`/cases/new`)

Opening form + entity resolution preview by NIF.

## Report (`/cases/{id}/report`)

Complete HTML report for reading and printing.

## UBO graph (`/cases/{id}/ubo`)

Hierarchical visualization, table and PEP/sanctions indicators per node.

## Party details (`/cases/{id}/parties/{partyId}`)

Focus on one party: identity, filtered signals, individual screening.

## Administration (Admin role)

| Screen | Function |
|--------|----------|
| Users | Account management (when applicable) |
| BdP RPB | Annual anti-money laundering prevention report |
| Scoring | Versions of the scoring engine |
| DPIA | Impact assessment record |
| Settings | Active PAC, global parameters |
| Audit log | Audit trail (also Auditor) |
