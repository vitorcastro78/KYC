# EU Multi-Region Infrastructure — KYC AI Platform

> **Status:** 🟡 Target design documented; implementation depends on institutional cloud procurement.

## 1. Target architecture (EU)

```
Primary region (e.g. West Europe)
  ├── AKS / VM: kyc-web, kyc-workers
  ├── PostgreSQL Flexible (zone-redundant HA)
  ├── Blob Storage (documents — phase 2)
  └── ContextMemory (GPU node or dedicated service)

DR region (e.g. North Europe)
  ├── PostgreSQL read replica / geo-restore
  ├── Replicated container images (ACR geo-replication)
  └── DNS failover (Traffic Manager / Front Door)
```

**Target SLA:** 99.9% (8.76 h downtime/year).

## 2. Current state (on-prem / single-region)

- Deployment: `docker-compose.prod.yml` — single region
- Database: PostgreSQL instance (e.g. staging `195.179.193.136`)
- No automatic failover documented in production

## 3. Roadmap

| Phase | Deliverable | Status |
|-------|-------------|--------|
| 1 | EU off-site backups | 🟡 DRP procedure |
| 2 | Async database replica | 🔴 |
| 3 | Multi-AZ app | 🔴 |
| 4 | Automatic DNS failover | 🔴 |

## 4. Supplier certifications (4.2)

Attach to the dossier:

- Cloud provider ISO/IEC 27001
- SOC 2 Type II (if applicable)
- DPA / RGPD subcontracting clauses

## 5. SLA monitoring

- Uptime: synthetic `/health` check every 1 min
- Alerts: PagerDuty / IT email after 3 consecutive failures
