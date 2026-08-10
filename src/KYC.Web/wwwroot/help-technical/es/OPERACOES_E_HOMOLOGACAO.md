# Operaciones y homologación — KYC AI Platform

> Documento unificado: despliegue, runbooks, pruebas E2E, checklists regulatorios y de seguridad, dossier de evidencias.
> Paquete de idioma: **español**. Hub: [`../README.md`](../README.md).

---

## 1. Despliegue on-prem

Misma disposición que [ContextMemory](https://github.com/Kortexio/ContextMemory): `docker-compose.yml` (build), `docker-compose.ghcr.yml` (imágenes), `.env.example`, `scripts/docker-run.*`.

### 1.1 Requisitos previos

- Docker y Docker Compose
- ContextMemory accesible (`CONTEXT_MEMORY_BASE_URL`, p. ej. `https://context.kortexio.io`)
- Fichero `.env` (copiar de `.env.example`) — **nunca hacer commit**

### 1.2 Arranque (build local)

```bash
cp .env.example .env
docker compose up --build -d
```

### 1.3 Arranque (imágenes GHCR)

```bash
docker compose -f docker-compose.ghcr.yml up -d
```

### 1.4 Solo base de datos

```bash
docker compose -f docker-compose.db.yml up -d
```

### 1.5 Migraciones

```bash
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

### 1.6 Verificación

| Comprobación | URL / comando |
|--------------|---------------|
| UI | `http://localhost:8080` |
| Health | `GET /health` |
| Admin | `KYC_ADMIN_EMAIL` / `KYC_ADMIN_PASSWORD` |

### 1.7 Variables de cumplimiento críticas

```env
IDENTITY_VERIFICATION_WEBHOOK_SECRET=...
IdentityVerification__BaseUrl=...
Uif__BaseUrl=...
Uif__ApiKey=...
BdpAssetFreeze__BaseUrl=...
Compliance__RequireLiveIntegrations=true
```

---

## 2. Runbook — Homologación técnica

### 2.1 Base de datos

```powershell
$env:KYC_DB_CONNECTION="Host=...;Port=5433;Database=...;Username=...;Password=..."
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

```sql
SELECT tgname FROM pg_trigger WHERE tgname = 'tr_audit_entries_immutable';
```

### 2.2 Webhook de identidad (HMAC)

Igual al paquete PT/EN (§2.2): `POST /api/identity/webhook` con `X-Webhook-Signature: sha256=<hmac>`.

### 2.3 Pruebas automatizadas

```powershell
dotnet test
dotnet test tests/KYC.Web.Integration.Tests
```

### 2.4 CI

Push/PR a `main`, `develop` o `feature/*` → `.github/workflows/ci.yml`.

---

## 3. Runbook — PAC (Política de aceptación de clientes)

**Base legal:** Ley 83/2017, art. 24.º

1. Admin → **Settings** — PAC activa
2. Nueva versión desactiva la anterior; `LegalBasisRef = PAC/{versión}/Lei83/2017-Art24`
3. CAE prohibido / jurisdicción / PEP según reglas PAC

**Evidencia:** `docs/dossier/01-pac/`

---

## 4. Escenarios E2E — Homologación BdP

### 4.1 Requisitos previos

BD migrada, ContextMemory, PAC activa, usuarios Analyst/Supervisor/Admin, `dotnet test` en verde.

**Simular fallo de API (6–9):** `Compliance:RequireLiveIntegrations=true` sin URLs UIF/BdP/identidad.

### 4.2–4.11 Escenarios

1. **PAC al inicio** — CAE `92000` rechazado; caso válido → `InProgress` → `01-pac/`
2. **Identidad** — verificar + webhook HMAC; bloqueo si UBO pendiente → `06-identidade/`
3. **SAR** — narrativa ≥200; «no aplicable» ≥50 → `05-sar-uif/`
4. **EDD 4 ojos** — origen de fondos + segundo aprobador → `08-audit/`
5. **RPB** — generar, export `?format=bdp`, enviar → `04-rpb/`
6. **Denominación manual** sin RCBE/GLEIF → `09-e2e/`
7. **SAR urgente** UIF caída → Pending → ref. manual → `05-sar-uif/`
8. **Congelación BdP** API caída → ref. manual → `07-congelamento/`
9. **Identidad manual** justificación ≥20 → `06-identidade/`
10. **Señales manuales** + confirmar/descartar → `09-e2e/`

### 4.12 Registro de ejecución

| # | Escenario | Fecha | Ejecutor | Resultado | Evidencia |
|---|-----------|-------|----------|-----------|-----------|
| 1–10 | (rellenar en homologación) | | | ☐ OK ☐ Fallo | carpetas arriba |

**Firma compliance:** _________________________ Fecha: __________

### 4.13 Ejecución automatizada

```powershell
.\scripts\generate-e2e-evidence.ps1
.\scripts\run-e2e-ui-scenarios-2-5.ps1 -SkipAppStart
```

---

## 5. Checklist regulatorio — Capacidades

- [x] PAC versionada; DDC S/S/R; EDD fondos; revisión periódica; SAR/UIF
- [x] Identidad (Aviso BdP 1/2022); 4 ojos EDD
- [x] Congelación (Ley 97/2017); RPB Instr. 8/2024
- [x] DPIA; audit inmutable; auto-approve Low; Art. 22
- [x] `/health`; secretos fuera del repo; deploy documentado; CI

---

## 6. Pen test — Checklist (solo homologación)

- [ ] Admin sin rol → 403; webhook HMAC; sin IDOR
- [ ] SAR &lt; 200 rechazada; límites de upload; NIF inválido
- [ ] Secretos en env/Key Vault; HTTPS; cookies seguros
- [ ] Paquetes vulnerables; imagen Docker actualizada
- [ ] Trigger audit; PAC/scoring/DPIA no borrables

| Fecha | Ejecutor | Herramienta | Críticos | Altos | Medios | Aprobado |
|-------|----------|-------------|----------|-------|--------|----------|
| | | | 0 | | | ☐ Sí ☐ No |

**Evidencia:** `docs/dossier/10-seguranca/` · modelo: [`governanca/RELATORIO_PEN_TEST_MODELO.md`](governanca/RELATORIO_PEN_TEST_MODELO.md)

---

## 7. Dossier de evidencias

Ver [`../dossier/README.md`](../dossier/README.md).

---

## 8. Inicio rápido — Analista AML

1. URL de homologación (Analyst / Supervisor / Admin)
2. **Casos → Nuevo** — esperar cribado
3. Cumplimiento: identidad UBO; EDD; SAR si hay banner
4. Aprobar solo sin bloqueo `CanApproveMessage`
5. Ayuda en app: [`../help-online/es/`](../help-online/es/)

---

## 9. Dependencias externas (go-live)

| ID | Entrega | Bloquea |
|----|---------|---------|
| X1–X6 | Plantilla RPB, UIF, identidad, congelación, PAC firmada, DPIA | Producción / homologación formal |

---

## 10. Próximos pasos

1. Ejecutar E2E (§4) y rellenar §4.12
2. Completar pen test (§6)
3. Credenciales X2–X4 en staging
4. Go-live con `Compliance:RequireLiveIntegrations=true`
