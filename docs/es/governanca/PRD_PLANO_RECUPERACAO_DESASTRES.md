# Plan de Recuperación ante Desastres (PRD) — KYC AI Platform

> **Versión:** 1.0 · Complementa [PCN_PLANO_CONTINUIDADE_NEGOCIO.md](PCN_PLANO_CONTINUIDADE_NEGOCIO.md)

## 1. Objetivos de recuperación

| Componente | RTO | RPO | Procedimiento |
|------------|-----|-----|---------------|
| PostgreSQL KYC | 4 h | 15 min | Restauración de backup + `dotnet ef database update` |
| KYC.Web + Workers | 2 h | 0 (sin estado) | Redesplegar la última imagen Docker estable |
| Documentos `Data/cases/` | 8 h | 24 h | Restaurar volumen de backup |
| ContextMemory | 8 h | N/A | Reconfigurar modelo no ContextMemory |

## 2. Copias de seguridad

| Dato | Frecuencia | Retención | Ubicación |
|------|------------|-----------|-----------|
| PostgreSQL completo | Diario 02:00 UTC | 30 días | _[S3/Azure Blob UE]_ |
| PostgreSQL WAL | Continuo | 7 días | _[ídem]_ |
| Volúmenes Docker / Data | Diario | 30 días | _[ídem]_ |

Comando de referencia:

```bash
pg_dump -Fc -h <host> -U <user> azureopsagent > kyc-backup-$(date +%Y%m%d).dump
```

## 3. Procedimiento de restauración (resumen)

1. Aprovisionar host/VM de DR en la región secundaria de la UE
2. Restaurar PostgreSQL: `pg_restore -d kyc ...`
3. Aplicar migrations si es necesario
4. `docker compose -f docker-compose.prod.yml up -d`
5. Validar `/health`, caso de prueba E2E escenario 1
6. Comunicar la reactivación al equipo de cumplimiento

## 4. Pruebas del PRD

| Fecha | Ámbito | Duración real del RTO | RPO medido | Aprobado |
|------|--------|-----------------------|------------|----------|
| | Restauración BD en homologación | | | ☐ |

**Frecuencia mínima:** 1×/año.

## 5. Criterios de activación de DR

- Pérdida total del centro de datos primario
- Corrupción irreversible de la BD sin PITR
- Ransomware con impacto en backups < 24h
