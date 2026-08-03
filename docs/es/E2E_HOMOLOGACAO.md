# E2E — Homologación BdP

> Ejecutar en entorno de homologación con BD migrada (`20260529205723_BdpComplianceAndGtm`).  
> **Objetivo:** completar la tabla §Registro y adjuntar evidencias en `docs/dossier/`.

## Requisitos previos

| Elemento | Verificación |
|----------|--------------|
| BD | `KYC_DB_CONNECTION` en `.env` (p. ej. `Host=...;Port=5433;Database=azureopsagent;...`) — **no** asumir `localhost` si la BD es remota |
| BD | `dotnet ef database update` en la BD de homologación |
| Ollama | `OLLAMA_ENDPOINT` accesible |
| PAC | Activa (seed `ComplianceSeedHostedService` o Admin → Settings) |
| Usuarios | Analista + Supervisor + Admin (Entra o Identity dev) |
| Pruebas auto | `dotnet test` — **0 fallos** antes de E2E manual |

### Simular fallo de API (contingencia manual)

Para los escenarios 6–9, usar **una** de las opciones:

- `Compliance:RequireLiveIntegrations=true` sin `Uif__BaseUrl` / `BdpAssetFreeze__BaseUrl` / `IdentityVerification__BaseUrl`, **o**
- URLs no válidas en staging, **o**
- Desactivar temporalmente el mock/servicio de integración.

---

## Escenarios obligatorios

### 1. PAC al inicio

1. **Casos → Nuevo** con CAE `92000` → mensaje de infracción PAC; caso **no** creado.
2. Caso válido (NIF real o de prueba) → `InProgress`, `LegalBasisRef` completado.
3. **Evidencia:** captura del error PAC + captura de caso creado; auditoría `CaseStarted`.

**Carpeta:** `docs/dossier/01-pac/`

---

### 2. Identidad (API + webhook)

1. Sección **Conformidad** → «Verificar identidad» (vídeo/CMD/presencial).
2. Webhook: `POST /api/identity/webhook` con body `{ "partyId", "sessionId", "verified": true }` y `X-Webhook-Signature: sha256=<hmac>` — ver [HOMOLOGACAO_RUNBOOK.md](HOMOLOGACAO_RUNBOOK.md).
3. Badge **Verificado** en la parte; intentar **Aprobar** con otro UBO aún pendiente → botón desactivado + `CanApproveMessage`.

**Carpeta:** `docs/dossier/06-identidade/`

---

### 3. SAR (envío y no aplicable)

1. Caso de riesgo alto / señal crítica → banner SAR → narrativa ≥200 caracteres → enviar (no urgente o urgente con API OK).
2. Caso sin obligación → «SAR no aplicable» → justificación ≥50 caracteres → `SarStatus = NotRequired`.
3. Lista de casos: badges SAR y DDC correctos.

**Carpeta:** `docs/dossier/05-sar-uif/`

---

### 4. EDD 4 ojos

1. Caso `DueDiligenceLevel = Enhanced` → guardar **origen de los fondos**.
2. Verificación presencial o CMD de las partes obligatorias.
3. **Aprobar** con segundo aprobador **distinto** → `SecondApproverId` en la BD / auditoría.

**Carpeta:** `docs/dossier/08-audit/` (extracto de caso EDD)

---

### 5. RPB (Admin)

1. Admin → Generar RPB del año actual.
2. Exportar `?format=bdp` → XML (estructura interna de la Instrucción 8/2024).
3. Marcar como enviado → referencia BdP en el registro.

**Carpeta:** `docs/dossier/04-rpb/`

---

## Escenarios de contingencia manual (APIs no disponibles)

### 6. Denominación social al inicio (sin RCBE/GLEIF)

1. NIF sin correspondencia en RCBE/GLEIF (o entorno sin endpoint).
2. La vista previa en **Nuevo caso** muestra aviso «indique denominación manual».
3. Intentar **Iniciar** sin nombre → error; rellenar **Denominación social (manual)** → caso creado con dicho nombre (no `Entidad {NIF}`).
4. Parte tomadora con el mismo nombre.

**Carpeta:** `docs/dossier/01-pac/` o `docs/dossier/09-e2e/`

---

### 7. SAR urgente — fallo UIF → registro manual

1. API UIF no disponible (ver requisitos de simulación).
2. Enviar SAR **urgente** → toast de aviso; `SarStatus = Pending`.
3. Sección SAR: alerta + campo **Registro manual UIF** → introducir ref. (≥5 chars) → `SarSubmitted` + auditoría `SarManualRegistered`.
4. Consultar auditoría `SarApiFailedPendingManual`.

**Carpeta:** `docs/dossier/05-sar-uif/`

---

### 8. Congelación BdP — fallo de API → registro manual

1. Caso con señal **Sanción** → en **Señales de riesgo** → **Confirmar correspondencia**.
2. Con API BdP en fallo: alerta roja «Congelación BdP pendiente»; caso `UnderReview`; auditoría `AssetFreezeNotificationFailed`.
3. Introducir ref. BdP manual → `AssetFreezeNotified` + auditoría `AssetFreezeManualRegistered`.

**Carpeta:** `docs/dossier/07-congelamento/`

---

### 9. Identidad — verificación manual (sin API)

1. Parte UBO/órgano social aún pendiente; proveedor no disponible.
2. **Verificado manualmente (sin API)** → justificación ≥20 caracteres + ref. de documento opcional.
3. Método `ThirdPartyReliance`, estado Verificado; auditoría `IdentityManualVerified`.
4. Aprobación desbloqueada para esa parte (si las restantes están OK).

**Carpeta:** `docs/dossier/06-identidade/`

---

### 10. Señales de cribado — manual + confirmación

1. **Registrar señal manual** (tipo, severidad, fuente, descripción ≥10) — fuente guardada como `Manual:...`.
2. Señal automática pendiente → **Confirmar** o **Descartar** en la tarjeta de señal.
3. Timeline / auditoría con `ManualRiskSignalAdded` y `AnalystOverride`.

**Carpeta:** `docs/dossier/09-e2e/`

---

## Evidencias mínimas por dossier

| Carpeta | Contenido mínimo |
|---------|------------------|
| `01-pac/` | Captura PAC activa + prueba CAE 92000 rechazada |
| `04-rpb/` | Exportación XML + JSON + referencia de envío |
| `05-sar-uif/` | SAR enviado O registro manual tras fallo API |
| `06-identidade/` | Webhook OK + verificación manual (captura) |
| `07-congelamento/` | Confirmación API O ref. manual tras sanción |
| `08-audit/` | SQL o exportación `audit_entries` del caso de prueba |
| `09-e2e/` | Esta tabla firmada (PDF o escaneo) |
| `10-seguranca/` | [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) completado |

---

## Registro de ejecución (completar en homologación)

| # | Escenario | Fecha | Ejecutor | Resultado | Evidencia |
|---|-----------|-------|----------|-----------|-----------|
| 1 | Inicio PAC | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Fallo | `09-e2e/test-results-20260531-021829.trx` — [REGISTO_EXECUCAO_20260531-021829.md](dossier/09-e2e/REGISTO_EXECUCAO_20260531-021829.md) |
| 2 | Identidad + webhook | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Fallo | `06-identidade/02-*-20260531-024650.png`; caso `943cb0b0-3fb3-4ca6-974f-421a06063d2a` — [REGISTO_UI_CENARIOS_2-5_20260531-024650.md](dossier/09-e2e/REGISTO_UI_CENARIOS_2-5_20260531-024650.md) |
| 3 | SAR | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Fallo | `05-sar-uif/03-*-20260531-024650.png`; casos SAR `8279989f-…` + identidad (no aplicable) |
| 4 | EDD 4 ojos | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Fallo | `08-audit/04-*-20260531-024650.png`; caso `58c21877-ec18-4b01-9351-22cefefe6ee9` |
| 5 | RPB Admin | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Fallo | `04-rpb/05-*-20260531-024650.png`, `05-rpb-export-bdp-20260531-024650.xml` |
| 6 | Nombre legal manual (inicio) | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Fallo | `09-e2e/audit-export-*.json`, trx — [REGISTO_EXECUCAO_20260531-021829.md](dossier/09-e2e/REGISTO_EXECUCAO_20260531-021829.md) |
| 7 | SAR manual tras fallo UIF | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Fallo | `05-sar-uif/`, trx E2E-07 |
| 8 | Congelación manual BdP | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Fallo | `07-congelamento/`, trx E2E-08 |
| 9 | Identidad manual | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Fallo | `06-identidade/`, trx E2E-09 |
| 10 | Señales manuales + override | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Fallo | `09-e2e/`, trx E2E-10 |

**Entorno:** `http://localhost:5299` · BD `195.179.193.136:5433` (`azureopsagent`) · IDs UI: [e2e-ui-cases.json](dossier/09-e2e/e2e-ui-cases.json)

**Firma compliance:** _________________________ Fecha: __________

---

## Pruebas automatizadas (requisito previo)

```bash
dotnet test
```

Paquetes relevantes: `ComplianceFlowTests`, `ComplianceHandlersIntegrationTests`, `StartKycCaseCommandHandlerTests`, `SarEligibilityTests`, `IdentityWebhookHttpTests`, `UboGraphViewBuilderTests`.

---

## Ejecución automatizada (agente / CI local)

Con `KYC_DB_CONNECTION` definido en `.env` (alineado con `ConnectionStrings:KycDatabase` en `appsettings.json`):

```powershell
# .env: KYC_DB_CONNECTION=Host=...;Port=5433;Database=azureopsagent;...
.\scripts\generate-e2e-evidence.ps1
```

Genera: pruebas `HomologationE2eAutomatedTests` (7), exportación JSON en `docs/dossier/`, HTTP + webhook, registro `docs/dossier/09-e2e/REGISTO_EXECUCAO_*.md`.

**UI (escenarios 2–5):**

```powershell
.\scripts\run-e2e-ui-scenarios-2-5.ps1 -SkipAppStart   # con KYC.Web ya en http://localhost:5299
```

Prepara casos (`E2E-UI-PREP`), ejecuta Playwright, guarda capturas en `04-rpb/`, `05-sar-uif/`, `06-identidade/`, `08-audit/` y genera `REGISTO_UI_CENARIOS_2-5_*.md`.

---

## Tras E2E

1. Pen test: [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) → `10-seguranca/`.
2. Credenciales reales X2–X4 en staging (validar flujos **sin** solo contingencia manual).
3. Actualizar [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md) con la fecha de homologación (sección inferior).
