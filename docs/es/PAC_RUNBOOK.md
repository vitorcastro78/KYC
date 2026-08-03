# Runbook — Política de aceptación de clientes (PAC)

> Ley 83/2017, Art. 24 — criterios mínimos de aceptación y rechazo.

## Versión activa

1. Admin → **Settings** — tarjeta «PAC activa» (versión, umbrales, base legal).
2. Base de datos: tabla `customer_acceptance_policies` con `IsActive = true`.
3. Seed automático: `ComplianceSeedHostedService` crea PAC `1.0.0` si no existe.

## Nueva versión (PAC v2+)

1. Admin → **Settings** → campo de versión (p. ej. `1.1.0`) → **Activar v2+**.
2. `CreateCustomerAcceptancePolicyCommand` desactiva la versión anterior y copia parámetros.
3. Los nuevos casos reciben `LegalBasisRef` = `PAC/{versão}/Lei83/2017-Art24`.

## Validación al iniciar el caso

`StartKycCaseCommandHandler` ejecuta `PolicyComplianceValidator` **antes** de guardar el caso:

| Regla | Efecto |
|-------|--------|
| CAE en `ProhibitedCaeActivitiesJson` | `PolicyViolationException` (auto-reject) |
| Jurisdicción prohibida / offshore | Auto-reject o infracción |
| PEP en la estructura | Auto-reject (configuración PAC) |

## Pruebas

- `StartKycCaseCommandHandlerTests` — CAE `92000` rechazado
- `ComplianceHandlersIntegrationTests` — PAC al inicio

## Evidencia de homologación

Captura de Settings + log de caso rechazado → `docs/dossier/01-pac/`.
