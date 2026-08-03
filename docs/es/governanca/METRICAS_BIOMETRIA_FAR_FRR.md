# Métricas biométricas — FAR y FRR

## Definiciones

| Métrica | Significado |
|---------|-------------|
| **FAR** (False Accept Rate) | Impostores aceptados como genuinos |
| **FRR** (False Reject Rate) | Usuarios genuinos rechazados incorrectamente |

## API de la plataforma

```http
GET /api/admin/compliance/metrics
Authorization: Bearer <token>  (roles: KYC.Admin, KYC.Auditor)
```

Respuesta (`BiometricMetricsDto`):

- `Verified` / `Failed` — intentos completados
- `WithLivenessScore` — sesiones con score del proveedor
- `AverageLivenessScore` — media cuando es numérico
- `FalseRejectRatePct` — operativo: `Failed / (Verified + Failed) × 100`
- `FalseAcceptRatePct` — **0** hasta el informe de laboratorio del proveedor (no estimable solo con datos operativos)

## Informe periódico (trimestral)

| Período | Intentos | Verificados | Fallos | FRR % | FAR % (proveedor) | Responsable |
|---------|----------|-------------|--------|-------|-------------------|-------------|
| Q_2026_1 | | | | | | Cumplimiento |

Exportar el JSON de la API y archivarlo en `docs/dossier/06-identidade/`.

## Umbrales institucionales (por definir)

| Métrica | Umbral máximo sugerido | Acción si se supera |
|---------|------------------------|---------------------|
| FRR operativo | _[ej.: 5%]_ | Revisar proveedor / método |
| FAR (certificado del proveedor) | _[ej.: 0,1%]_ | Escalar al proveedor |
