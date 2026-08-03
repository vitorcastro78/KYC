# Infraestructura multirregión de la UE — KYC AI Platform

> **Estado:** 🟡 Diseño objetivo documentado; la implementación depende de la contratación cloud institucional.

## 1. Arquitectura objetivo (UE)

```
Región primaria (ej.: West Europe)
  ├── AKS / VM: kyc-web, kyc-workers
  ├── PostgreSQL Flexible (HA con redundancia de zona)
  ├── Blob Storage (documentos — fase 2)
  └── Ollama (nodo GPU o servicio dedicado)

Región de DR (ej.: North Europe)
  ├── Réplica de lectura PostgreSQL / geo-restore
  ├── Imágenes de contenedor replicadas (ACR geo-replication)
  └── Failover DNS (Traffic Manager / Front Door)
```

**SLA objetivo:** 99,9% (8,76 h de indisponibilidad/año).

## 2. Estado actual (on-prem / región única)

- Despliegue: `docker-compose.prod.yml` — región única
- BD: instancia PostgreSQL (p. ej., homologación `195.179.193.136`)
- Sin failover automático documentado en producción

## 3. Hoja de ruta

| Fase | Entrega | Estado |
|------|---------|--------|
| 1 | Backups off-site en la UE | 🟡 Procedimiento PRD |
| 2 | Réplica de BD asíncrona | 🔴 |
| 3 | Aplicación multi-AZ | 🔴 |
| 4 | Failover DNS automático | 🔴 |

## 4. Certificaciones del proveedor (4.2)

Adjuntar al expediente:

- ISO/IEC 27001 del proveedor cloud
- SOC 2 Type II (si aplica)
- DPA / cláusulas de subcontratación RGPD

## 5. Monitorización del SLA

- Uptime: comprobación sintética de `/health` cada 1 min
- Alertas: PagerDuty / correo TI tras 3 fallos consecutivos
