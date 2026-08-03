# Estado de finalización de los Blueprints — Plataforma KYC

> **Última actualización:** mayo de 2026 · Rama `feature/kyc-document-ingestion`  
> **Objetivo:** aplicación **lista para producción**, no un prototipo.  
> **Fuentes:** `Blueprint.md` (v1.1) + `BLUEPRINT_BdP_Compliance_Addendum.md` + `docs/PLANO_DESENVOLVIMENTO_COMPLIANCE_BDP.md`

---

## Resumen ejecutivo

| Blueprint | Código | Integraciones reales | Homologación |
|-----------|--------|----------------------|-------------|
| **Blueprint.md** (KYC core) | **~90%** | Variable por entorno | E2E manual pendiente |
| **Adenda BdP** (compliance) | **~95%** | UIF / BdP / identidad + **contingencia manual** | Dossier + pen test pendientes |
| **Global** | **~95%** código · **0%** evidencias de homologación | Configurar `.env` de producción | Ejecutar `docs/E2E_HOMOLOGACAO.md` (10 escenarios) |

**Leyenda:** ✅ Hecho · 🟡 Parcial / modo dev · 🔴 Pendiente · 🌐 Externo (compliance/BdP)

**Siguiente paso activo:** ejecutar [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) y completar `docs/dossier/`.

---

## Contingencia manual (mayo de 2026) — ✅ código

| Carencia | Implementación |
|----------|----------------|
| Congelación BdP sin API | `RegisterManualAssetFreezeReferenceCommand` + UI `NeedsManualAssetFreezeRegistration` |
| SAR urgente falla en UIF | `RecordSarPendingAfterApiFailure` → `Pending` + registro manual |
| Identidad sin proveedor | `RecordManualIdentityVerificationCommand` + botón UI |
| Cribado (medios/judicial) | `AddManualRiskSignalCommand` + confirmar/descartar en `SignalCard` |
| Nombre legal al inicio | `LegalCompanyName` + `GetEntityResolutionPreviewQuery` en Nuevo caso |

---

## Blueprint.md — por fase

### Fase 1 — Fundación
| Elemento | Estado | Notas |
|----------|--------|-------|
| 5 proyectos Clean Architecture | ✅ | Domain, Application, Infrastructure, Web, Workers |
| EF + PostgreSQL + migrations | ✅ | pgvector, trigger de auditoría |
| Entra OIDC + Identity local dev | ✅ | `AzureAd:Enabled` o Identity+PostgreSQL |
| Key Vault opcional | ✅ | `KYC_KEYVAULT_NAME` |
| Blazor + auth + CI | ✅ | `.github/workflows/ci.yml` |

### Fase 2 — Motor KYC core
| Elemento | Estado | Notas |
|----------|--------|-------|
| StartKycCase + MediatR | ✅ | PAC + nombre legal manual como fallback |
| Resolución de entidad RCBE + GLEIF | 🟡 | RCBE depende de endpoint; vista previa UI |
| Grafo UBO recursivo | ✅ | `BuildUboGraphAsync` |
| OFAC + sanciones UE | ✅ | Descarga Workers + índice local |
| Service Bus / Rabbit / in-memory | ✅ | `Messaging:Provider` |
| Pipeline de scans paralelos | ✅ | `KycCasePipelineRunner` |
| Scoring Ollama Qwen | ✅ | Sin Claude (desviación documentada) |
| Auditoría append-only | ✅ | Trigger PostgreSQL |

### Fase 3 — IA e informe
| Elemento | Estado | Notas |
|----------|--------|-------|
| API Claude Sonnet | 🔴 | Solo Ollama (BdP/RGPD) |
| Enrutamiento LLM local/cloud | 🟡 | Solo local |
| Informe de 8 secciones + explainability | ✅ | Art. 22 |
| Comprobación de coherencia documental | ✅ | |
| Embeddings pgvector | ✅ | |

### Fase 4 — UI y workflow
| Elemento | Estado | Notas |
|----------|--------|-------|
| Dashboard SignalR | ✅ | |
| Progreso de scan en CaseDetail | ✅ | |
| UI de grafo UBO | ✅ | |
| Aprobación EDD de 4 ojos | ✅ | |
| Exportación PDF de informe | ✅ | |
| Log de auditoría Admin | ✅ | |
| Señales: confirmar/descartar | ✅ | `SignalCard` + `OverrideSignal` |

### Fase 5 — Fuentes y compliance
| Elemento | Estado | Notas |
|----------|--------|-------|
| Adverse media / AT / CITIUS / ICIJ | ✅ | + señales manuales |
| Trabajo de retención de datos | 🟡 | `DataRetention:EnableHostedService` en prod |
| Pen test | 🔴 | Ejecutar checklist |

### Fase 5b — Ingesta de documentos
| Elemento | Estado | Notas |
|----------|--------|-------|
| Pipeline completo | ✅ | |
| Azure Blob / Doc Intelligence | 🔴 | Fase 2 del blueprint |

---

## Adenda BdP — épicos E1–E10

| Épico | Código | Carencia principal |
|-------|--------|-------------------|
| **E1** PAC / inicio de caso | ✅ | Homologación E2E #1, #6 |
| **E2** Identidad | ✅ | API X3 + E2E #2, #9 |
| **E3** SAR / UIF | ✅ | API X2 + E2E #3, #7 |
| **E4** EDD | ✅ | E2E #4 |
| **E5** Congelación | ✅ | API X4 + E2E #8 |
| **E6** Explainability | ✅ | — |
| **E7** RPB | 🟡 | Plantilla oficial X1 |
| **E8** Administración de versiones | ✅ | — |
| **E9** RCBE | ✅ | — |
| **E10** Homologación | 🟡 | **10 escenarios E2E** + dossier + pen test 🔴 |

Checklist de capacidad: [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md) — sección **Ejecución de homologación** por completar.

---

## Dependencias externas

| ID | Entrega | Responsable |
|----|---------|-------------|
| X1 | Plantilla RPB oficial BdP | Compliance |
| X2 | Credenciales API UIF | Institución |
| X3 | Proveedor de identidad | Proveedor |
| X4 | Endpoint de congelación BdP | Institución |
| X5 | PAC v1 firmada | Compliance |
| X6 | PDF DPIA aprobado | DPO |

---

## Configuración de producción

Consulte `.env.example` y `Compliance:RequireLiveIntegrations=true`:

```env
KYC_DB_CONNECTION=...
IdentityVerification__BaseUrl=...
Uif__BaseUrl=...
BdpAssetFreeze__BaseUrl=...
DataRetention__EnableHostedService=true
```

---

## Próximos pasos (en orden)

1. **`dotnet test`** → ejecutar [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) (escenarios 1–10).
2. Completar `docs/dossier/` y firmar la tabla E2E.
3. [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) → `dossier/10-seguranca/`.
4. Credenciales X2–X4 en staging (flujo API completo, no solo manual).
5. Firmas de gobernanza: [governanca/POLITICA_SEGURANCA_INFORMACAO.md](governanca/POLITICA_SEGURANCA_INFORMACAO.md), PCN/PRD.

---

## Desviación intencionada respecto de Blueprint.md v1.1

| Blueprint original | Implementación | Motivo |
|--------------------|---------------|--------|
| Claude Sonnet | Ollama Qwen | RGPD on-prem |
| Azure Blob | `Data/cases` local | Fase 5b |
