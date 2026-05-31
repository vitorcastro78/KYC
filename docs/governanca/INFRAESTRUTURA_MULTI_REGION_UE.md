# Infraestrutura multi-região UE — KYC AI Platform

> **Estado:** 🟡 Desenho alvo documentado; implementação depende de contratação cloud institucional.

## 1. Arquitectura alvo (UE)

```
Região primária (ex: West Europe)
  ├── AKS / VM: kyc-web, kyc-workers
  ├── PostgreSQL Flexible (HA zone-redundant)
  ├── Blob Storage (documentos — fase 2)
  └── Ollama (GPU node ou serviço dedicado)

Região DR (ex: North Europe)
  ├── PostgreSQL read replica / geo-restore
  ├── Container images replicadas (ACR geo-replication)
  └── DNS failover (Traffic Manager / Front Door)
```

**SLA alvo:** 99.9% (8,76 h indisponibilidade/ano).

## 2. Estado actual (on-prem / single-region)

- Deploy: `docker-compose.prod.yml` — região única
- BD: instância PostgreSQL (ex. homologação `195.179.193.136`)
- Sem failover automático documentado em produção

## 3. Roadmap

| Fase | Entrega | Estado |
|------|---------|--------|
| 1 | Backups off-site UE | 🟡 Procedimento PRD |
| 2 | Réplica BD async | 🔴 |
| 3 | App multi-AZ | 🔴 |
| 4 | Failover DNS automático | 🔴 |

## 4. Certificações fornecedor (4.2)

Anexar ao dossier:

- ISO/IEC 27001 do provider cloud
- SOC 2 Type II (se aplicável)
- DPA / cláusulas subcontratação RGPD

## 5. Monitorização SLA

- Uptime: synthetic check `/health` a cada 1 min
- Alertas: PagerDuty / email TI se 3 falhas consecutivas
