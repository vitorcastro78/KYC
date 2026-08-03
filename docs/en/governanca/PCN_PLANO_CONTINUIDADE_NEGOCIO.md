# Business Continuity Plan (BCP) — KYC Service

> **Version:** 1.0 (draft) · **BIA owner:** Compliance / IT

## 1. Critical services

| Service | Target RTO | Target RPO | Priority |
|---------|------------|------------|----------|
| KYC.Web (case triage) | 4 h | 1 h | P1 |
| PostgreSQL (cases + audit) | 4 h | 15 min | P1 |
| Ollama (scoring/report) | 8 h | N/A | P2 |
| Workers (OFAC/EU sanctions) | 24 h | 24 h | P3 |

## 2. Disruption scenarios

1. Application unavailability (crash, failed deployment)
2. Database unavailability
3. Ollama unavailability (degradation — manual triage)
4. Identity provider unavailability (in-person fallback)

## 3. Strategies

- **Application:** `docker-compose.prod.yml` restart; versioned image in registry
- **Database:** continuous backup + restore (see PRD)
- **Degradation:** analysts continue manual review; manual FIU SAR (`RegisterManualUifReferenceCommand`)

## 4. Response team

| Role | Contact | Responsibility |
|------|---------|----------------|
| Incident commander | _[IT]_ | Coordination |
| DBA | _[IT]_ | Database restore |
| Compliance lead | _[Compliance]_ | BdP/FIU communication if regulatory SLA is affected |

## 5. Communication

- Internal: institutional incident channel
- Regulator: in accordance with the obligation under Law 83/2017 if disruption exceeds the agreed SLA

## 6. BCP tests

| Date | Type | Result | Actions |
|------|------|--------|---------|
| | Tabletop exercise | | |
| | Technical simulation | | |

## 7. Approval

_Executive Committee — date and signature_
