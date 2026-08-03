# Inicio rápido — Analista AML (KYC Platform)

## 1. Acceso

- URL de homologación/producción según el despliegue (`docs/DEPLOY_ONPREM.md`)
- Roles: `KYC.Analyst` (casos), `KYC.Supervisor` (escalado + 4-eyes), `KYC.Admin` (RPB, PAC)

## 2. Nuevo caso

1. **Casos → Nuevo** — NIF, importe, tipo de relación (ocasional/continuada), CAE si procede.
2. Espere al cribado automático (barra de progreso en el detalle del caso).
3. Revise las señales y confirme/descarte las coincidencias.

## 3. Conformidad (sección amarilla)

- **Identidad** — Verifique UBO/administradores (Aviso BdP 1/2022) antes de aprobar.
- **EDD** — Indique el origen de los fondos; se requiere un segundo aprobador.
- **SAR** — Si aparece el banner amarillo: comuníquelo a la UIF (≥200 caracteres) o márquelo como no aplicable.
- **RCBE** — Informe de una discrepancia si se detecta.

## 4. Aprobar o rechazar

- El botón **Aprobar** solo está activo cuando `CanApprove` no indica ningún bloqueo.
- Casos con una sanción confirmada → congelación automática de activos + estado «En revisión».

## 5. Alertas en tiempo real

- SignalR: progreso del cribado, informe listo, alertas de conformidad (SAR, identidad, congelación).
- Los supervisores reciben alertas SAR en el grupo `supervisors`.

## 6. Referencias

- E2E completo: `docs/E2E_HOMOLOGACAO.md`
- Checklist BdP: `docs/CHECKLIST_HOMOLOGACAO_BDP.md`
