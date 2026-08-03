# Manual de usuario y resolución de problemas — KYC AI Platform

> Analistas, supervisores y administradores.

## 1. Acceso

| Entorno | URL | Autenticación |
|---------|-----|---------------|
| Homologación | _[URL institucional]_ | Entra ID (MFA) |
| Desarrollo local | `http://localhost:8080` | `admin@kyc.local` (consulte `.env`) |

Roles: `KYC.Analyst`, `KYC.Supervisor`, `KYC.Admin`, `KYC.Auditor`.

## 2. Flujos principales

### Nuevo caso
1. **Casos → Nuevo** — NIF, importe, relación, CAE
2. Si RCBE/GLEIF no resuelven la entidad, introduzca la **denominación social (manual)** (obligatorio)
3. Espere a la barra de progreso (cribado automático)
4. Revise las señales → confirme o descarte; use **Registrar señal manual** si fallan las APIs de cribado

### Conformidad (tarjeta amarilla)
- **Identidad** — Verifique UBO/órgano social; enlace al portal si está pendiente; **Verificado manualmente (sin API)** si el proveedor no está disponible
- **SAR** — Narrativa ≥200 caracteres o «no aplicable» ≥50; si una comunicación urgente falla en UIF, el estado queda **Pendiente** con registro manual de la referencia
- **Congelación BdP** — Tras confirmar una sanción; si falla la API, registre manualmente la referencia BdP en la alerta roja
- **EDD** — Origen de fondos + segundo aprobador al aprobar

### Aprobar
- El botón **Aprobar** solo está activo si no hay mensaje de bloqueo
- Supervisor: segundo aprobador obligatorio en EDD

## 3. Resolución de problemas

| Síntoma | Causa probable | Acción |
|---------|----------------|--------|
| El caso no inicia (PAC) | CAE/jurisdicción prohibida | Consulte Settings → PAC; corrija los datos |
| Aprobar desactivado | UBO sin verificar / fondos EDD | Sección de conformidad |
| Cribado detenido en % | Ollama no disponible | Compruebe `OLLAMA_ENDPOINT`; reinicie Ollama |
| Webhook de identidad 401 | HMAC incorrecto | Alinee `IdentityVerification:WebhookSecret` |
| Error de PDF del informe | Puppeteer/Chromium | Logs de `kyc-web`; reinstale las dependencias Docker |
| SAR falla en producción | Falta URL de UIF | Configure `Uif:BaseUrl` o registre manualmente en la sección SAR (estado Pendiente) |
| Falló la congelación BdP | `BdpAssetFreeze:BaseUrl` | Registre la referencia manualmente tras confirmar la sanción |
| Nombre «Entidad {NIF}» | Sin RCBE/GLEIF | Corrija al inicio (denominación manual) o añada partes manuales |
| Lista de casos vacía | Base de datos / migraciones | `dotnet ef database update` |
| SignalR sin actualizaciones | Proxy WebSocket | nginx: cabeceras `Upgrade` para `/hubs/` |

## 4. Logs y soporte

- Logs de aplicación: stdout Docker de `kyc-web`
- Auditoría: Admin → Audit log o consulta `audit_entries`
- Health: `GET /health`

## 5. APIs (equipo técnico)

Consulte [api/README.md](api/README.md) y Swagger `/swagger`.

## 6. Documentación relacionada

- [ANALISTA_QUICK_START.md](ANALISTA_QUICK_START.md)
- [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
- [MATRIZ_REQUISITOS_INSTITUCIONAIS.md](MATRIZ_REQUISITOS_INSTITUCIONAIS.md)
