# Plan de Desarrollo — Conformidad BdP 100%

> **Base:** `BLUEPRINT_BdP_Compliance_Addendum.md` (sec. 13–22) + `docs/CHECKLIST_HOMOLOGACAO_BDP.md`  
> **Estado actual (mayo de 2026):** ~95% de código de producto (ver `docs/BLUEPRINT_COMPLETION_STATUS.md`); contingencias manuales ✅; pendiente la **ejecución** de homologación (E2E 10 escenarios, dossier, pen test) y las credenciales X1–X4.  
> **Objetivo:** 59 criterios de aceptación (47 técnicos + 12 de checklist) verificados en entorno de homologación.  
> **Documentación unificada:** `docs/README.md` → `DOCUMENTACAO_APLICACAO.md`, `CATALOGO_FUNCIONALIDADES.md`, `OPERACOES_E_HOMOLOGACAO.md`.

---

## 1. Definición de «listo» (Definition of Done)

Un elemento solo está **Done** cuando:

1. El código está merged en la rama de release con revisión.
2. Pasa una prueba automatizada o un script E2E documentado.
3. La configuración está documentada en `.env.example` / `appsettings` (sin secrets en el repo).
4. Se adjunta evidencia al dossier de homologación (captura, log de auditoría o exportación).
5. Compliance marca `[x]` el elemento correspondiente del checklist.

---

## 2. Inventario: hecho frente a pendiente

### 2.1 Ya implementado (no repetir trabajo)

| ID | Elemento | Ubicación principal |
|----|----------|---------------------|
| D-01 | Entidades PAC, Scoring, DPIA, RPB + migration | `20260529205723_BdpComplianceAndGtm` |
| D-02 | Campos regulatorios `KycCase` / `CaseParty` | `KycCase.cs`, `CaseParty.cs` |
| D-03 | `LegalBasisAttribute` en `DocumentFactKey` | `DocumentFactKey.cs` |
| D-04 | `DueDiligenceLevelEvaluator` | `DueDiligenceLevelEvaluator.cs` |
| D-05 | `PolicyComplianceValidator` en el **pipeline** | `KycCasePipelineRunner.cs` |
| D-06 | `CanApprove`, 4 ojos EDD, origen de fondos | `KycCase.cs` |
| D-07 | Comandos SAR + `SarEligibilityEvaluator` | `ComplianceCommandHandlers.cs`, pipeline |
| D-08 | Congelación de activos en `OverrideSignalCommand` | `ComplianceCommandHandlers.cs` |
| D-09 | `PeriodicReviewSchedulerJob` | `PeriodicReviewSchedulerJob.cs` |
| D-10 | `IAmlComplianceReportService` + exportación JSON | `AmlComplianceReportService.cs` |
| D-11 | UI compliance (SAR, identidad, EDD, badges) | `ComplianceCaseSection`, `PartyIdentityPanel`, `SarActionModals`, `EntityCard` |
| D-12 | Admin RPB / scoring / DPIA + carga | `Pages/Admin/*` |
| D-13 | LLM solo Ollama, descarga OFAC SLS | `KycLlmEngine`, Workers |
| D-14 | Health checks, docker prod, CI | `HealthCheckExtensions`, `docker-compose.prod.yml` |
| D-15 | Pruebas `PolicyComplianceValidator` | `PolicyComplianceValidatorTests.cs` |
| D-16 | UI de grafo UBO enriquecido + merge de partes | `UboGraphView`, `UboGraphViewBuilder` |
| D-17 | `ICurrentAnalystAccessor` + supervisores Entra Graph | `HttpContextAnalystAccessor`, `EntraGraphSupervisorUserDirectory` |
| D-18 | Guards de integraciones de producción | `ComplianceIntegrationOptions`, `RequireLiveIntegrations` |
| D-19 | Registro manual de ref. UIF (SAR pendiente) | `RegisterManualUifReferenceCommand` |
| D-20 | Pruebas de UBO view builder | `UboGraphViewBuilderTests.cs` |
| D-21 | Congelación BdP manual + fallo API de sanción | `RegisterManualAssetFreezeReferenceCommand` |
| D-22 | SAR urgente → Pending después de fallo UIF | `RecordSarPendingAfterApiFailure` |
| D-23 | Contingencia de identidad manual | `RecordManualIdentityVerificationCommand` |
| D-24 | Señales manuales + override UI | `AddManualRiskSignalCommand`, `SignalCard` |
| D-25 | Nombre legal + preview de inicio | `LegalCompanyName`, `GetEntityResolutionPreviewQuery` |

### 2.2 Pendiente (ámbito de este plan)

Ver **`docs/BLUEPRINT_COMPLETION_STATUS.md`** (mapa actualizado). Resumen: credenciales externas X1–X6, homologación E10 (E2E, dossier, pen test), Claude/Blob (fase 2 del blueprint principal).

### 2.3 Estado por épico — código (mayo de 2026)

> Las tablas de la sección 3 son **especificación/backlog**; esta sección refleja el **progreso real**.

| Épico | Código | Pendiente (no código) |
|-------|--------|-----------------------|
| **E1** PAC / inicio | ✅ Done | — |
| **E2** Identidad | ✅ Done (E2-09 P2 opcional) | Credenciales proveedor X3 en prod |
| **E3** SAR / UIF | ✅ Done (E3-12 P2 SignalR opcional) | API UIF real X2; evidencia homologación |
| **E4** EDD | ✅ Done (E4-06 P2) | — |
| **E5** Congelación | ✅ Done | API BdP real X4 |
| **E6** Explainability | ✅ Done | — |
| **E7** RPB | 🟡 Exportación XML interna | Plantilla oficial X1 (E7-01) |
| **E8** Admin de versiones | ✅ Done | — |
| **E9** RCBE | ✅ Done (detección pipeline E9-03 P2) | — |
| **E10** Homologación | 🟡 Pruebas auto ✅ | E2E manual, dossier, pen test 🔴 |

**Porcentaje global de código:** ~95% (adenda BdP); homologación regulatoria **0% de evidencias** hasta ejecutar `E2E_HOMOLOGACAO.md` (10 escenarios) y `dossier/`.

---

## 3. Épicos y tareas

### E1 — Fundación legal e inicio de caso (3–4 días)

| ID | Tarea | Prioridad | Criterio de aceptación |
|----|-------|-----------|------------------------|
| E1-01–02 | Inyectar y ejecutar `ICustomerAcceptancePolicyRepository` + `PolicyComplianceValidator` antes de `repository.AddAsync`; validar sectores, jurisdicciones y auto-reject PEP | P0 | Caso infractor no creado; `PolicyViolationException` |
| E1-03 | Capturar `RelationshipType` + importe en `StartKycCaseCommand` / `NewCase.razor` | P1 | DDC usa tipo de relación correcto |
| E1-04 | Propagar `LegalBasisRef` configurable por política activa | P2 | Audit muestra la base legal |
| E1-05 | Pruebas unitarias `StartKycCase` + PAC | P0 | 3+ escenarios verdes |
| E1-06 | Documentar flujo PAC en README o runbook | P2 | Compliance puede seguirlo |

**Dependencias:** ninguna. **Checklist:** Ley 83/2017 — PAC.

### E2 — Verificación de identidad (Aviso 1/2022) (5–7 días)

| ID | Tarea | Prioridad | Criterio de aceptación |
|----|-------|-----------|------------------------|
| E2-01–05 | Comando/handler de resultado, `POST /api/identity/webhook` firmado, fallback polling, configuración y `GetVerificationResultAsync` completo | P0/P1 | Parte actualizada; sesión pendiente → Verified en ≤5 min; sin stub en prod si hay URL |
| E2-06–08 | Modal de métodos UI, badge/link de sesión de parte y bloqueo de Approve con `CanApproveMessage` | P0 | El analista inicia sesión; mensaje Aviso 1/2022 visible |
| E2-09–10 | Eventos de dominio de identidad y pruebas de integración webhook/`CanApprove` | P2/P1 | Audit + SignalR |

**Dependencias:** contrato API del proveedor (DigitalSign o equivalente). **Checklist:** Aviso 1/2022 — identidad + bloqueo de aprobación.

### E3 — SAR / UIF (Ley 83/2017, Art. 52–57) (4–5 días)

| ID | Tarea | Prioridad | Criterio de aceptación |
|----|-------|-----------|------------------------|
| E3-01–04 | Exigir riesgo ≥ High o señal Critical, SAR urgente síncrono, modal de narrativa (mín. 200 chars), prellenar señales críticas/altas | P0/P1 | Caso Low rechazado sin justificación; log/respuesta inmediata |
| E3-05–08 | Acción no aplicable con justificación, banner `SuggestSar`, badge de lista `SarStatus`, historial SAR | P0/P1/P2 | `SarStatus = NotRequired` + audit |
| E3-09–12 | UIF producción con retry, consulta de estado, pruebas, evento enviado/notificación a supervisor | P0/P2 | Referencia UIF en staging |

**Dependencias:** credenciales UIF o MOU con proceso manual documentado. **Checklist:** SAR/UIF + audit trail.

### E4 — EDD y pipeline reforzado (4–5 días)

| ID | Tarea | Prioridad | Criterio de aceptación |
|----|-------|-----------|------------------------|
| E4-01–02 | Parámetro de años adverse-media (2 por defecto, 5 EDD) y requisito de verificación no-Simplified | P0/P1 | EDD usa 5 años |
| E4-03–05 | UI de origen de fondos, segundo supervisor, sección de DDC aplicada legible | P0/P1 | `SecondApproverId` guardado |
| E4-06–07 | Invocar `CanProceedWithEnhancedDd` y pruebas del evaluador | P2/P1 | Casos PEP/offshore/ocasionales probados |

**Checklist:** DDC, origen de fondos EDD, 4 ojos.

### E5 — Congelación de activos (Ley 97/2017) (3–4 días)

| ID | Tarea | Prioridad | Criterio de aceptación |
|----|-------|-----------|------------------------|
| E5-01–02 | Establecer `UnderReview` tras confirmar sanción; alerta realtime inmediata al supervisor | P0 | Sin auto-aprobación; toast/dashboard |
| E5-03–06 | API BdP real, configuración prod, indicador/ref. UI, prueba de integración | P0/P1 | Confirmación en staging; notificación + flags |

**Dependencias:** endpoint BdP o procedimiento manual con SLA firmado. **Checklist:** Ley 97/2017.

### E6 — Informe, IA y explainability (3–4 días)

| ID | Tarea | Prioridad | Criterio de aceptación |
|----|-------|-----------|------------------------|
| E6-01–02 | Secciones LLM de decisión automatizada Art. 22 y limitaciones del modelo IA | P0 | HTML contiene las secciones |
| E6-03–04 | `aiModelsJson` solo Ollama; audit de versión de modelo + hash de prompt | P1 | Sin cloud |
| E6-05 | Validar auto-approve solo Low score ≤30 sin High/Critical | P0 | Prueba de regresión |

**Checklist:** explainability RGPD, auto-approve.

### E7 — RPB Instrucción BdP 8/2024 (5–8 días)

| ID | Tarea | Prioridad | Criterio de aceptación |
|----|-------|-----------|------------------------|
| E7-01 | Obtener plantilla oficial BdP XML/JSON/XLS | P0 | Documento versionado |
| E7-02–03 | `IBdpRpbExporter`, mapeo, exportación oficial + JSON interno | P0 | Exportación válida contra schema; dos formatos |
| E7-04–05 | Métricas/gráficos `AmlReport.razor` e historial anual | P1 | |
| E7-06–08 | Envío BdP, rol `KYC.Admin`, pruebas de exportación/agregadas | P0/P1 | `BdpReferenceNumber` |

**Dependencias:** E7-01 (compliance). **Checklist:** generación RPB + exportación formateada.

### E8 — Admin: versiones PAC, scoring, DPIA (3–4 días)

| ID | Tarea | Prioridad | Criterio de aceptación |
|----|-------|-----------|------------------------|
| E8-01–03 | Comandos de creación de scoring, carga DPIA y PAC v2 versionados | P0/P1 | Inmutabilidad |
| E8-04–05 | Resumen PAC/DPIA/scoring activo y no eliminar versión activa | P1/P0 | |

**Checklist:** DPIA activa registrada.

### E9 — RCBE y datos auxiliares (2–3 días)

| ID | Tarea | Prioridad | Criterio de aceptación |
|----|-------|-----------|------------------------|
| E9-01–03 | `ReportRcbeDiscrepancyCommand`, UI de reporte IRN, detección pipeline declarado-vs-RCBE | P1/P2 | Flags de parte + audit |

### E10 — Pruebas, ops y homologación (5–7 días)

| ID | Tarea | Prioridad | Criterio de aceptación |
|----|-------|-----------|------------------------|
| E10-01–03 | Suite de integración, script E2E manual, actualización del checklist de evidencias | P0 | CI verde; 15 pasos; 12/12 `[x]` |
| E10-04–08 | Dossier PDF, Workers obligatorios en prod, revisión de reglas, formación de analistas, pen test básico | P1/P0/P2 | |

**Checklist:** operacional + todo lo regulatorio.

---

## 4. Cronograma sugerido (10 semanas)

| Semana | Épicos | Entregable |
|--------|--------|------------|
| S1 | E1, E10-05 | PAC al inicio; OFAC/Workers validados |
| S2 | E2 (E2-01–05) | Webhook + API de identidad |
| S3 | E2 (E2-06–10), E3 (E3-01–05) | UI de identidad + modal SAR |
| S4 | E3 (E3-06–12), E4 | SAR completo + pipeline EDD |
| S5 | E5, E6 | Congelación + explainability LLM |
| S6 | E7 (tras plantilla BdP) | Exportación RPB oficial |
| S7 | E7 (UI), E8 | Admin RPB + versiones |
| S8 | E9, E10-01–03 | RCBE + pruebas CI |
| S9 | E10-04–08 | Dossier homologación |
| S10 | Buffer + UAT | Sign-off compliance |

**Paralelización:** E7-01 (compliance) en S1; UI E4 en paralelo con E3 después de S2.

---

## 5. Dependencias externas (bloqueantes)

| # | Qué | Quién lo proporciona | Impacto si se retrasa |
|---|-----|----------------------|-----------------------|
| X1 | Plantilla export RPB Instr. 8/2024 | Compliance / BdP | E7 bloqueado |
| X2 | API UIF (o proceso manual + SLA) | Institución | E3 producción |
| X3 | API de identidad (DigitalSign/CMD) | Proveedor | E2 producción |
| X4 | API de notificación de congelación BdP | Institución | E5 producción |
| X5 | PAC v1 firmada internamente | Compliance | Seed ≠ producción |
| X6 | DPIA aprobada (PDF) | DPO | E8-02 |

---

## 6. Configuración mínima de homologación

```env
# Base
KYC_DB_CONNECTION=...
OLLAMA_ENDPOINT=http://host:11434

# Integraciones compliance
IdentityVerification__BaseUrl=
IdentityVerification__ApiKey=
IdentityVerification__WebhookSecret=
Uif__BaseUrl=
Uif__ApiKey=
BdpAssetFreeze__BaseUrl=

# Workers (OFAC + UE)
ExternalSources__OfacSdnDailyDownload__Enabled=true
ExternalSources__EuFsfDailyDownload__Enabled=true
```

---

## 7. Métricas de seguimiento

| Métrica | Objetivo |
|---------|----------|
| Tareas Done (sec. 3) | 62/62 |
| Checklist de homologación | 12/12 |
| Cobertura de pruebas compliance | ≥ 80% handlers críticos |
| Bugs P0 abiertos | 0 antes de go-live |
| Tiempo medio SAR urgente (E2E) | < 2 min (síncrono) |

---

## 8. Orden de implementación recomendado (sprint 0 → 6)

```
Sprint 0 (P0 rápido):  E1-01, E1-02, E1-05, E3-07, E4-03, E4-04, E5-01, E5-02
Sprint 1:              E2 completo
Sprint 2:              E3 completo
Sprint 3:              E4, E5, E6
Sprint 4:              E7 + E8
Sprint 5:              E9, E10
```

---

## 9. Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Plantilla RPB no disponible | Exportación JSON provisional + flag «draft» hasta schema oficial |
| UIF sin API | Modo manual: analista registra ref. externa + `SubmitSar` solo guarda audit |
| Ollama no disponible | Score heurístico + plantilla de informe (fallback ya existe) |
| XML OFAC grande | Workers + path compartido Web/Workers |

---

## 10. Próximo paso inmediato

1. Aprobar este plan con compliance (validar X1–X6).  
2. Abrir 62 issues en Jira/GitHub a partir de la sec. 3.  
3. Iniciar **Sprint 0** (E1 + quick wins UI SAR/lista).

---

*Documento vivo — actualizar cuando los elementos D-xx pasen a Done.*
