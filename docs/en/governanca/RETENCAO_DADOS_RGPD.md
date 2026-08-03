# Data Retention — RGPD (5–7 years)

## Legal basis

Law 83/2017 (AML) + RGPD Art. 6(1)(c), legal obligation + internal retention policy.

## Technical configuration

```json
"DataRetention": {
  "EnableHostedService": true,
  "RejectedCaseRetentionYears": 5,
  "ApprovedCaseRetentionYears": 7,
  "AnonymizeRejectedAfterRetention": true,
  "MarkApprovedCasesPastRetention": true
}
```

## Job behaviour (`DataRetentionHostedService`)

| Case type | After period | Action |
|-----------|--------------|--------|
| Rejected | > 5 years from `CompletedAt` | Anonymisation (`CompanyName=ANON`, `Nif=000000000`) |
| Approved | > 7 years | `RetentionReviewDue` audit — legal archive review (not automatically deleted) |

## Enable in production

1. `.env`: `DataRetention__EnableHostedService=true`
2. Confirm daily logs at approximately 02:00 UTC
3. Validate an old rejected test case (staging)

## Exceptions

- **Legal hold:** suspend anonymisation through the compliance process flag (manual database procedure — document the ticket)
- **SAR submitted:** retention aligned with the FIU obligation

## Staging evidence

- [ ] Production configuration active
- [ ] Job log with counts
- [ ] `RetentionReviewDue` audit entry in a simulated case

**Matrix status 6.3:** 🟡 → ✅ after production activation + evidence.
