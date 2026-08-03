# Retención de datos — RGPD (5–7 años)

## Base jurídica

Ley 83/2017 (AML) + RGPD Art. 6.º(1)(c), obligación legal + política interna de retención.

## Configuración técnica

```json
"DataRetention": {
  "EnableHostedService": true,
  "RejectedCaseRetentionYears": 5,
  "ApprovedCaseRetentionYears": 7,
  "AnonymizeRejectedAfterRetention": true,
  "MarkApprovedCasesPastRetention": true
}
```

## Comportamiento del job (`DataRetentionHostedService`)

| Tipo de caso | Tras el período | Acción |
|--------------|-----------------|--------|
| Rechazado | > 5 años desde `CompletedAt` | Anonimización (`CompanyName=ANON`, `Nif=000000000`) |
| Aprobado | > 7 años | Auditoría `RetentionReviewDue` — revisión del archivo legal (sin borrado automático) |

## Activar en producción

1. `.env`: `DataRetention__EnableHostedService=true`
2. Confirmar logs diarios alrededor de las 02:00 UTC
3. Validar un caso de prueba rechazado antiguo (homologación)

## Excepciones

- **Legal hold:** suspender la anonimización mediante la flag del proceso de cumplimiento (procedimiento manual de BD — documentar el ticket)
- **SAR enviado:** retención alineada con la obligación de la UIF

## Evidencia de homologación

- [ ] Configuración de producción activa
- [ ] Log del job con recuentos
- [ ] Entrada de auditoría `RetentionReviewDue` en caso simulado

**Estado de matriz 6.3:** 🟡 → ✅ tras activación de producción + evidencia.
