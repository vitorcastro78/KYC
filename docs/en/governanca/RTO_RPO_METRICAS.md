# RTO and RPO — Metrics and Simulation Record

## Agreed objectives

| ID | Service | RTO (hours) | RPO (minutes) | Measurement method |
|----|---------|-------------|---------------|--------------------|
| S1 | KYC.Web | 4 | 60 | Time from incident to `/health` OK |
| S2 | PostgreSQL | 4 | 15 | Restore time + audit-trail integrity |
| S3 | Documents | 8 | 1440 | Volume restore + sample checksum |
| S4 | ContextMemory | 8 | — | Time until scoring is available |

## Simulation record (complete in staging/production)

| # | Date | Scenario | Measured RTO | Measured RPO | Objective met | Evidence |
|---|------|----------|--------------|--------------|---------------|----------|
| 1 | | D-1 database backup restore | | | ☐ Yes ☐ No | `dossier/09-e2e/` |
| 2 | | App failover (redeployment) | | | ☐ Yes ☐ No | |
| 3 | | ContextMemory loss — degraded mode | | | ☐ Yes ☐ No | |

## Current status

**🔴 Pending** — objectives defined; simulations not run. After the first simulation, update [MATRIZ_REQUISITOS_INSTITUCIONAIS.md](../MATRIZ_REQUISITOS_INSTITUCIONAIS.md) §3.3 to 🟡/✅.
