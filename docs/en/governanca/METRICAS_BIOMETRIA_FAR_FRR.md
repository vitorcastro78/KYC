# Biometric Metrics — FAR and FRR

## Definitions

| Metric | Meaning |
|--------|---------|
| **FAR** (False Accept Rate) | Impostors accepted as genuine |
| **FRR** (False Reject Rate) | Genuine users incorrectly rejected |

## Platform API

```http
GET /api/admin/compliance/metrics
Authorization: Bearer <token>  (roles: KYC.Admin, KYC.Auditor)
```

Response (`BiometricMetricsDto`):

- `Verified` / `Failed` — completed attempts
- `WithLivenessScore` — sessions with a provider score
- `AverageLivenessScore` — average when numeric
- `FalseRejectRatePct` — operational: `Failed / (Verified + Failed) × 100`
- `FalseAcceptRatePct` — **0** until the provider laboratory report is available (not estimable from operational data alone)

## Periodic report (quarterly)

| Period | Attempts | Verified | Failures | FRR % | FAR % (provider) | Owner |
|--------|----------|----------|----------|-------|------------------|-------|
| Q_2026_1 | | | | | | Compliance |

Export the API JSON and archive it in `docs/dossier/06-identidade/`.

## Institutional thresholds (to define)

| Metric | Suggested maximum threshold | Action if exceeded |
|--------|-----------------------------|--------------------|
| Operational FRR | _[e.g. 5%]_ | Review provider / method |
| FAR (provider certificate) | _[e.g. 0.1%]_ | Escalate to provider |
