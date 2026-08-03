# Lista de verificación — Pen test básico (homologación KYC)

> Herramienta sugerida: escaneo baseline de OWASP ZAP o revisión manual equivalente.  
> Entorno: **homologación** (nunca producción sin autorización).  
> **Informe formal:** complete [governanca/RELATORIO_PEN_TEST_MODELO.md](governanca/RELATORIO_PEN_TEST_MODELO.md) y archívelo en `docs/dossier/10-seguranca/`.

## 1. Autenticación y autorización

- [ ] Las rutas `/admin/*` rechazan a usuarios sin el rol `KYC.Admin`
- [ ] Las APIs `/api/admin/aml-reports/*` exigen `KYC.Admin`
- [ ] El webhook de identidad (`POST /api/identity/webhook`) exige HMAC cuando se define `IdentityVerification:WebhookSecret`
- [ ] Intento de acceso anónimo a un caso ajeno → 401/403

## 2. Entrada e inyección

- [ ] Narrativa SAR: rechazar payload &lt; 200 chars (server-side)
- [ ] Carga de documentos: se respetan tipos MIME y tamaño máximo
- [ ] NIF no válido en `NewCase` → error de validación (sin 500)

## 3. Datos sensibles

- [ ] Secrets solo en env / Key Vault (no en `appsettings` versionado)
- [ ] Los logs no contienen API keys, tokens UIF ni PII completa
- [ ] El informe PDF/HTML no expone datos de otros casos (IDOR en `CaseId`)

## 4. Transporte y headers

- [ ] HTTPS forzado en homologación/prod
- [ ] Cookies de sesión: `HttpOnly`, `Secure` (si aplica)
- [ ] CORS restringido al dominio de la aplicación

## 5. Dependencias

- [ ] `dotnet list package --vulnerable` sin críticos abiertos (o excepción documentada)
- [ ] Imagen Docker actualizada (nivel de parche de imagen base)

## 6. Regulatorio (smoke)

- [ ] Audit trail inmutable (trigger PostgreSQL `tr_audit_entries_immutable`)
- [ ] La versión activa PAC/scoring/DPIA no se puede eliminar (interceptor EF)

## Resultado

| Fecha | Ejecutor | Herramienta | Críticos | Altos | Medios | Homologación aprobada |
|-------|----------|-------------|----------|-------|--------|-----------------------|
| | | | 0 | | | ☐ Sí ☐ No |

**Notas / hallazgos:**

_(completar tras el escaneo)_
