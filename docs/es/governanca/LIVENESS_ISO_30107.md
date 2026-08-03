# Prueba de vida (Liveness) — ISO/IEC 30107-3

## Requisito regulatorio

Aviso BdP 1/2022 — verificación remota con detección de ataque de presentación (PAD).

## Implementación en la plataforma

| Componente | Descripción |
|------------|-------------|
| Proveedor | DigitalSign / API configurada en `IdentityVerification:BaseUrl` |
| Métodos | Videoconferencia, CMD, presencial, firma cualificada |
| Campo `LivenessScore` | Persistido en `case_parties` tras webhook/polling |
| Auditoría | Entrada `IdentityVerified` con `liveness:{score}` |

## Conformidad ISO/IEC 30107-3

| Nivel | Responsable | Evidencia |
|-------|-------------|-----------|
| Certificación del algoritmo PAD | **Proveedor de identidad** | Certificado o informe de laboratorio acreditado |
| Integración técnica | Plataforma KYC | Webhook + polling + almacenamiento de score |
| Operación | Institución | Elección de método adecuado al riesgo (EDD → no simplificado) |

**Estado:** 🟡 Parcial — integración técnica ✅; certificado del proveedor 🌐 pendiente en el expediente.

## Checklist de homologación

- [ ] El contrato del proveedor hace referencia a ISO 30107-3 o equivalente
- [ ] Prueba de videoconferencia con score de liveness > umbral institucional
- [ ] Impresión de audit trail con liveness registrado
- [ ] Anexo del certificado PDF en `docs/dossier/06-identidade/`
