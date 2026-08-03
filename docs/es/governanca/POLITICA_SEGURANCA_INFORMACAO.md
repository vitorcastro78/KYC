# Política de Seguridad de la Información — KYC AI Platform

> **Versión:** 1.0 (borrador para aprobación)  
> **Clasificación:** Interno — Confidencial  
> **Aprobación:** _[CISO / Comité Ejecutivo — fecha y firma]_

## 1. Objetivo

Definir principios y controles de seguridad para la plataforma KYC (datos personales, datos financieros, comunicaciones con la UIF y audit trail regulatorio).

## 2. Ámbito

- Aplicación KYC.Web, Workers, PostgreSQL, Ollama, integraciones UIF/BdP/identidad
- Usuarios: analistas, supervisores, administradores, auditores
- Entornos: desarrollo, homologación, producción

## 3. Principios

1. **Mínimo privilegio** — roles `KYC.Analyst`, `KYC.Supervisor`, `KYC.Admin`, `KYC.Auditor`
2. **Defensa en profundidad** — red, TLS, autenticación, autorización, auditoría inmutable
3. **Segregación de entornos** — secretos distintos por entorno; sin datos de producción en desarrollo
4. **Responsabilidad** — `ICurrentAnalystAccessor` en todas las acciones de cumplimiento

## 4. Controles implementados en la plataforma

| Control | Implementación |
|----------|----------------|
| Autenticación | Microsoft Entra OIDC (producción) o Identity (desarrollo) |
| MFA | Acceso condicional de Entra (obligatorio para operadores de producción) |
| Autorización | Políticas ASP.NET Core por rol |
| Sesión | Cookies HttpOnly; caducidad de 14 días (Identity en desarrollo) |
| Secretos | `.env` / Azure Key Vault — nunca en el repositorio |
| Webhook | HMAC SHA-256 `IdentityVerification:WebhookSecret` |
| Auditoría | Trigger PostgreSQL inmutable |
| Integraciones de producción | `Compliance:RequireLiveIntegrations` |

## 5. Gestión de incidentes

1. Detección mediante logs de Application Insights / SIEM institucional
2. Clasificación P1–P4; notificación al DPO en caso de brecha de datos personales (72h RGPD)
3. Registro en ticket + entrada de auditoría si hay impacto en casos KYC

## 6. Revisión

Revisión anual o después de un incidente grave. Próxima revisión: _[fecha]_.

## 7. Aprobaciones

| Función | Nombre | Fecha | Firma |
|---------|--------|-------|-------|
| CISO | | | |
| DPO | | | |
| Responsable de Cumplimiento | | | |
