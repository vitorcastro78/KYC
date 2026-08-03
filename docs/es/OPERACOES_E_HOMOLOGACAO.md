# Operaciones y homologación: plataforma KYC AI

> Documento unificado: despliegue, runbooks, pruebas E2E, checklists regulatorios y de seguridad, dossier de evidencia.

---

## 1. Despliegue on-prem

### 1.1 Requisitos previos

- Docker y Docker Compose
- Ollama accesible (`OLLAMA_ENDPOINT`, por ejemplo, `http://host.docker.internal:11434`)
- Archivo `.env` (copia de `.env.example`) — **no incluir nunca en un commit**

### 1.2 Inicio
```bash
cp .env.example .env
# Editar passwords e KYC_DB_CONNECTION (compose: Host=kyc-postgres)

docker compose -f docker-compose.prod.yml up -d --build
```
### 1.3 Migraciones
```bash
docker compose -f docker-compose.prod.yml exec kyc-web \
  dotnet ef database update --project /src/KYC.Infrastructure --startup-project /src/KYC.Web
```
En anfitrión:
```bash
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```
### 1.4 Verificación posterior a la implementación

| Verificación | Comando/URL |
|---------------------|--------------------------|
| interfaz de usuario | `http://localhost:8080` (o `KYC_WEB_PORT`) |
| Salud | `GET /health` |
| Desarrollador de administración | `KYC_ADMIN_EMAIL` / `KYC_ADMIN_PASSWORD` |
| Trabajadores | Volúmenes `Data/ofac`, `Data/eu-fsf` después del inicio |

### 1.5 Variables críticas de cumplimiento
```env
IDENTITY_VERIFICATION_WEBHOOK_SECRET=...
IdentityVerification__BaseUrl=...
Uif__BaseUrl=...
Uif__ApiKey=...
BdpAssetFreeze__BaseUrl=...
Compliance__RequireLiveIntegrations=true
```
---

## 2. Guía operativa — Homologación técnica

### 2.1 Base de datos
```powershell
$env:KYC_DB_CONNECTION="Host=...;Port=5433;Database=...;Username=...;Password=..."
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```
Confirmar auditoría de activación:
```sql
SELECT tgname FROM pg_trigger WHERE tgname = 'tr_audit_entries_immutable';
```
Prueba opcional:
```powershell
dotnet test tests/KYC.Web.Integration.Tests --filter AuditImmutability
```
### 2.2 Webhook de identidad (HMAC)

Configure `IdentityVerification:WebhookSecret` o `IDENTITY_VERIFICATION_WEBHOOK_SECRET`.
```powershell
$body = '{"partyId":"<GUID>","sessionId":"sess-abc","verified":true}'
$secret = "seu-secret"
$hash = [BitConverter]::ToString([System.Security.Cryptography.HMACSHA256]::HashData(
  [Text.Encoding]::UTF8.GetBytes($secret),
  [Text.Encoding]::UTF8.GetBytes($body))).Replace("-","").ToLower()
Invoke-RestMethod -Method Post -Uri "https://<host>/api/identity/webhook" `
  -Headers @{ "X-Webhook-Signature" = "sha256=$hash" } `
  -ContentType "application/json" -Body $body
```
### 2.3 Pruebas automatizadas
```powershell
dotnet test
dotnet test tests/KYC.Web.Integration.Tests
```
Cobertura de cumplimiento: `ComplianceHandlersIntegrationTests`, `ComplianceFlowTests`, `SarEligibilityTests`, `IdentityWebhookHttpTests`, `UboGraphViewBuilderTests`.

### 2.4 IC

Push/PR para `main`, `develop` o `feature/*` → `.github/workflows/ci.yml` (PostgreSQL + migraciones + pruebas).

---

## 3. Runbook — PAC (Política de aceptación del cliente)

**Base jurídica:** Ley 83/2017, art. 24.º

### Versión activa

1. Administrador → **Configuración** — Tarjeta “PAC activo”
2. BD: `customer_acceptance_policies` con `IsActive = true`
3. Semilla: `ComplianceSeedHostedService` crea PAC `1.0.0` si está vacío

### Nueva versión

1. Administrador → Configuración → versión (ej. `1.1.0`) → **Activar**
2. `CreateCustomerAcceptancePolicyCommand` desactiva el anterior
3. Casos nuevos: `LegalBasisRef` = `PAC/{versão}/Lei83/2017-Art24`

### Reglas al inicio

| Regla | Efecto |
|-------|--------|
| CAE en la lista de prohibidos | `PolicyViolationException` |
| Jurisdicción prohibida/offshore | Rechazo automático o infracción |
| PEP en la estructura | Rechazo automático (configuración de PAC) |

**Evidencia:** `docs/dossier/01-pac/`

---

## 4. Escenarios E2E: aprobación de BdP

> Entorno con BD migrada (`BdpComplianceAndGtm` + posterior).  
> Prerrequisitos: `KYC_DB_CONNECTION`, Ollama, PAC activo.

### Escenario 1: PAC al inicio

1. Caso CAE `92000` (juegos) → Fallo del PAC
2. Caso válido → `InProgress` + `LegalBasisRef`

### Escenario 2 — Identidad (Aviso 1/2022)

1. Cumplimiento → «Verificar identidad» → método
2. Webhook o sondeo de HMAC → `Verified`
3. Aprobar sin UBO marcado → botón desactivado + mensaje

### Escenario 3: SAR/UIF

1. Caso de alto riesgo → banner SAR → narrativa ≥200 → enviar
2. «No aplicable» → justificación ≥50 → `NotRequired`
3. Listar casos → Insignias SAR/DDC

### Escenario 4: EDD de 4 ojos

1. Fondos mejorados + fuente + verificación
2. Aprobar con el segundo supervisor → `SecondApproverId`

### Escenario 5: RPB

1. Administrador → Generar RPB del año actual
2. Exportar `?format=bdp` → XML
3. Enviar → Referencia BdP

### Escenarios 6 a 10: contingencia manual (API no disponibles)

Detalles paso a paso en **[E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md)**:

| # | Tema |
|---|------|
| 6 | Denominación social manual (sin RCBE/GLEIF) |
| 7 | SAR Urgente → Pendiente → ref. manual UIF |
| 8 | Manual de congelación BdP post-sanción |
| 9 | Identidad manual (sin API) |
| 10 | Señales manuales + confirmar/descartar |

### Registro de ejecución

> Cuadro completo (10 líneas) y firma de cumplimiento: **[E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md)** §Registro.

| # | Escenario | Evidencia |
|---|------------|-----------|
| 1–5 | Ver arriba | `dossier/` según E2E |
| 6–10 | Contingencia manual | `01-pac/`, `05-sar-uif/`, `07-congelamento/`, `06-identidade/`, `09-e2e/` |

---

## 5. Lista de verificación regulatoria — Capacidades (Ley 83/2017, BdP, GDPR)

> Estado **código**: la evidencia de ejecución en la aprobación está separada.

### Ley 83/2017 — Lucha contra el blanqueo de capitales y la financiación del terrorismo

- [x] PAC versionado activo al inicio del caso
- [x] DDC simplificado/estándar/mejorado
- [x] EDD: fuente de fondos antes de la aprobación
- [x] Revisión periódica (`NextReviewDue`)
- [x] SAR/UIF con pista de auditoría

### Aviso BdP 1/2022

- [x] Verificación de identidad (webhook + encuesta + UI)
- [x] Bloquear aprobación si UBO/administrador no está verificado
- [x] 4 ojos en EDD

### Ley 97/2017 — Congelación

- [x] Notificación automática al confirmar sanción
- [x] `AssetFreezeNotified` registrado

### Instrucción BdP 8/2024 — RPB

- [x] Generación anual `AmlComplianceReport`
- [x] Exportar JSON + XML BdP (`?format=bdp`)

### RGPD

- [x] DPIA activa (Administrador)
- [x] Pista de auditoría inmutable (activador PostgreSQL)
- [x] Aprobación automática Solo riesgo bajo
- [x] Informe de explicabilidad (Art. 22)
### Operacional

- [x] Salud `/health`
- [x] Secretos fuera del repositorio (plantilla `.env.example`)
- [x] Implementación local documentada
- [x] tubería de CI

---

## 6. Prueba de penetración: lista de verificación de aprobación

> Herramienta sugerida: Línea base OWASP ZAP o revisión manual. **Solo aprobación.**

### Autenticación y autorización

- [ ] `/admin/*` sin `KYC.Admin` → 403
- [] API de administración de AML → `KYC.Admin`
- [] La identidad del webhook requiere HMAC con un secreto definido
- [ ] Caso tercero IDOR → 401/403

### Entrada e inyección

- [] Narrativa SAR < 200 caracteres rechazados en el lado del servidor
- [ ] Subir: MIME y tamaño máximo
- [ ] NIF no válido → validación (sin 500)

### Datos confidenciales

- [] Secretos solo env/Key Vault
- [] Registros sin claves API completas/PII
- [ ] PDF sin IDOR entre casos

### Transporte

- [] HTTPS en aprobación/producción
- [] HttpOnly/Cookies seguras
- [] CORS restringido

### Dependencias

- [] `dotnet list package --vulnerable` no crítico
- [] Imagen de Docker actualizada

### Humo regulatorio

- [] Activador de auditoría inmutable
- [] PAC/puntuación/DPIA activo no borrable (interceptor EF)

### Resultado

| Fecha | Ejecutor | Herramienta | Críticos | Máximos | Centrocampistas | Aprobado |
|------|----------|------------|----------|-------|--------|----------|
| | | | 0 | | | ☐ Sí ☐ No |

**Evidencia:** `docs/dossier/10-seguranca/`

---

## 7. Expediente de pruebas (puesta en marcha)

### Estructura de carpetas
```
docs/dossier/
  01-pac/           PAC activa (print Admin)
  02-dpia/          DPIA + documento
  03-scoring/       Versão scoring + hash prompt
  04-rpb/           XML BdP + JSON + ref. submissão
  05-sar-uif/       SAR + ref. UIF
  06-identidade/    Webhook + verificação party
  07-congelamento/  Notificação BdP
  08-audit/         Extract audit caso teste
  09-e2e/           Checklist E2E assinado
  10-seguranca/     Pen test preenchido
```
### Cómo generar

1. Ejecute escenarios de la sección 4
2. Administrador → Configuración: captura de PAC, puntuación, DPIA
3. Admin → RPB: generar, exportar, enviar
4. Caso con sanción: congelación de impresiones + auditoría
5. Nombrar archivos con fecha: `RPB-2025-20260530.xml`

### Responsable

| Área | Propietario |
|------|--------|
| Cumplimiento / PAC | Equipo de cumplimiento |
| RPB | `KYC.Admin` |
| Seguridad | Prueba de infrarrojos + pluma |
| E2E | Analista AML + Control de Calidad |

---

## 8. Inicio rápido: Analista AML

1. **Acceso**: URL de aprobación; roles Analista / Supervisor / Administrador
2. **Caso nuevo** — Casos → Nuevo; esperar la proyección
3. **Cumplimiento** — Identidad UBO; fondos de originación del EDD; SAR si pancarta; RCBE
4. **Aprobar**: solo sin bloqueo `CanApproveMessage`
5. **Alertas** — SignalR; supervisores en el grupo SAR
6. **Referencia** — Este documento + [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md)

---

## 9. Dependencias externas (puesta en marcha)

| identificación | Entrega | Responsable | Bloques |
|----|---------|-------------|----------|
| X1 | Plantilla Oficial BdP RPB | Cumplimiento | Exportar XML final |
| X2 | API/MOU UIF | Institución | Producción SAR |
| X3 | Contrato de identidad (DigitalSign/CMD) | Proveedor | Verificación de producción |
| X4 | API de congelación de BdP | Institución | Notificación real |
| X5 | PAC v1 firmado | Cumplimiento | Aprobación formal |
| X6 | PDF DPIA DPO | DPO | RGPD |

---

## 10. Próximos pasos operativos (orden)

1. Ejecute E2E (sección 4) y complete la tabla.
2. Complete la prueba de penetración (sección 6) → `dossier/10-seguranca/`
3. Credenciales X2-X4 en el entorno de staging
4. Plantilla RPB X1 → actualizar `BdpRpbExporter.cs`
5. Lanzamiento en vivo con `Compliance:RequireLiveIntegrations=true`

---

## Documentos fuente (detalle histórico)

Los archivos siguientes permanecen en el repositorio; el contenido operativo relevante se ha consolidado **en este documento**:

- [DEPLOY_ONPREM.md](DEPLOY_ONPREM.md)
- [HOMOLOGACAO_RUNBOOK.md](HOMOLOGACAO_RUNBOOK.md)
- [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md)
- [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md)
- [PAC_RUNBOOK.md](PAC_RUNBOOK.md)
- [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md)
- [ANALISTA_QUICK_START.md](ANALISTA_QUICK_START.md)
- [expediente/README.md](dossier/README.md)