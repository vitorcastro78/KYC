# Disaster Recovery Plan (DRP) — KYC AI Platform

> **Version:** 1.0 · Complements [PCN_PLANO_CONTINUIDADE_NEGOCIO.md](PCN_PLANO_CONTINUIDADE_NEGOCIO.md)

## 1. Recovery objectives

| Component | RTO | RPO | Procedure |
|-----------|-----|-----|-----------|
| KYC PostgreSQL | 4 h | 15 min | Backup restore + `dotnet ef database update` |
| KYC.Web + Workers | 2 h | 0 (stateless) | Redeploy latest stable Docker image |
| Documents `Data/cases/` | 8 h | 24 h | Restore backup volume |
| Ollama | 8 h | N/A | Reinstall Qwen model |

## 2. Backups

| Data | Frequency | Retention | Location |
|------|-----------|-----------|----------|
| PostgreSQL full | Daily 02:00 UTC | 30 days | _[S3/Azure Blob EU]_ |
| PostgreSQL WAL | Continuous | 7 days | _[same]_ |
| Docker / Data volumes | Daily | 30 days | _[same]_ |

Reference command:

```bash
pg_dump -Fc -h <host> -U <user> azureopsagent > kyc-backup-$(date +%Y%m%d).dump
```

## 3. Restore procedure (summary)

1. Provision a DR host/VM in the secondary EU region
2. Restore PostgreSQL: `pg_restore -d kyc ...`
3. Apply migrations if necessary
4. `docker compose -f docker-compose.prod.yml up -d`
5. Validate `/health`, E2E test case scenario 1
6. Communicate reactivation to the compliance team

## 4. DRP tests

| Date | Scope | Actual RTO duration | Measured RPO | Approved |
|------|-------|---------------------|--------------|----------|
| | Database restore in staging | | | ☐ |

**Minimum frequency:** once/year.

## 5. DR activation criteria

- Total loss of primary data centre
- Irreversible database corruption without PITR
- Ransomware affecting backups < 24h
