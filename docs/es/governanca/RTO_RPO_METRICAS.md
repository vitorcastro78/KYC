# RTO y RPO — Métricas y registro de simulaciones

## Objetivos acordados

| ID | Servicio | RTO (horas) | RPO (minutos) | Método de medición |
|----|----------|-------------|---------------|--------------------|
| S1 | KYC.Web | 4 | 60 | Tiempo desde el incidente hasta `/health` OK |
| S2 | PostgreSQL | 4 | 15 | Tiempo de restauración + integridad del audit trail |
| S3 | Documentos | 8 | 1440 | Restauración de volumen + checksum de muestra |
| S4 | ContextMemory | 8 | — | Tiempo hasta disponer de scoring |

## Registro de simulaciones (cumplimentar en homologación/producción)

| # | Fecha | Escenario | RTO medido | RPO medido | Objetivo cumplido | Evidencia |
|---|-------|-----------|------------|------------|-------------------|-----------|
| 1 | | Restauración de backup BD D-1 | | | ☐ Sí ☐ No | `dossier/09-e2e/` |
| 2 | | Failover de aplicación (redespliegue) | | | ☐ Sí ☐ No | |
| 3 | | Pérdida de ContextMemory — modo degradado | | | ☐ Sí ☐ No | |

## Estado actual

**🔴 Pendiente** — objetivos definidos; simulaciones no ejecutadas. Tras la primera simulación, actualizar [MATRIZ_REQUISITOS_INSTITUCIONAIS.md](../MATRIZ_REQUISITOS_INSTITUCIONAIS.md) §3.3 a 🟡/✅.
