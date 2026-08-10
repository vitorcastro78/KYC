# Plan de Continuidad de Negocio (PCN) — Servicio KYC

> **Versión:** 1.0 (borrador) · **Propietario del BIA:** Cumplimiento / TI

## 1. Servicios críticos

| Servicio | RTO objetivo | RPO objetivo | Prioridad |
|----------|--------------|--------------|-----------|
| KYC.Web (triaje de casos) | 4 h | 1 h | P1 |
| PostgreSQL (casos + auditoría) | 4 h | 15 min | P1 |
| ContextMemory (scoring/informe) | 8 h | N/A | P2 |
| Workers (sanciones OFAC/UE) | 24 h | 24 h | P3 |

## 2. Escenarios de interrupción

1. Indisponibilidad de la aplicación (caída, despliegue fallido)
2. Indisponibilidad de la BD
3. Indisponibilidad de ContextMemory (degradación — triaje manual)
4. Indisponibilidad del proveedor de identidad (alternativa presencial)

## 3. Estrategias

- **Aplicación:** reinicio de `docker-compose.prod.yml`; imagen versionada en registry
- **BD:** backup continuo + restauración (véase PRD)
- **Degradación:** los analistas continúan la revisión manual; SAR manual a la UIF (`RegisterManualUifReferenceCommand`)

## 4. Equipo de respuesta

| Papel | Contacto | Responsabilidad |
|-------|----------|-----------------|
| Incident commander | _[TI]_ | Coordinación |
| DBA | _[TI]_ | Restauración de BD |
| Responsable de Cumplimiento | _[Cumplimiento]_ | Comunicación BdP/UIF si se afecta el SLA regulatorio |

## 5. Comunicación

- Interna: canal institucional de incidentes
- Regulador: conforme a la obligación de la Ley 83/2017 si la interrupción supera el SLA acordado

## 6. Pruebas del PCN

| Fecha | Tipo | Resultado | Acciones |
|-------|------|-----------|----------|
| | Ejercicio de mesa | | |
| | Simulación técnica | | |

## 7. Aprobación

_Comité Ejecutivo — fecha y firma_
