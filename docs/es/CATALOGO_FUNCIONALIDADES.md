# Catálogo de Funcionalidades — KYC AI Platform

> **Uso:** base para manuales de usuario, RFP, homologación funcional y roadmaps.  
> **Leyenda de estado:** ✅ Implementado · 🟡 Parcial / depende de integración · 🔴 Planificado / no implementado · 🌐 Dependencia externa

---

## Módulo 1 — Gestión de casos KYC

| ID | Funcionalidad | Descripción | UI / API | Estado | Base legal / nota |
|----|----------------|-------------|----------|--------|-------------------|
| KYC-01 | Apertura de caso | NIF, importe, relación ocasional/continuada, CAE | `/cases/new` | ✅ | Ley 83/2017 |
| KYC-02 | Validación PAC al inicio | Rechaza CAE/jurisdicción prohibida antes de guardar | Command handler | ✅ | Art. 24 |
| KYC-03 | Lista de casos | Score, DDC, badge SAR, estado, fecha | `/cases` | ✅ | |
| KYC-04 | Detalle del caso | Hero, progreso de cribado, acciones, partes | `/cases/{id}` | ✅ | |
| KYC-05 | Aprobar caso | Bloqueo mediante `CanApproveMessage` | CaseDetail + Supervisor | ✅ | Aviso 1/2022 |
| KYC-06 | Rechazar caso | Motivo obligatorio | CaseDetail | ✅ | |
| KYC-07 | Solicitar revisión manual | Estado UnderReview | CaseDetail | ✅ | |
| KYC-08 | Auto-approve de riesgo Low | Score ≤30, sin señales graves | Pipeline | ✅ | RGPD / política |
| KYC-09 | Asignación de analista | `AssignedAnalystId` | Dominio | ✅ | |
| KYC-10 | Revisión periódica | `NextReviewDue` tras la aprobación | Dominio + conformidad | ✅ | Art. 35 |

---

## Módulo 2 — Resolución de entidades y UBO

| ID | Funcionalidad | Descripción | UI / API | Estado | Base legal / nota |
|----|----------------|-------------|----------|--------|-------------------|
| ENT-01 | Resolución GLEIF | Snapshot de empresa + partes relacionadas | GleifCompanyCard | ✅ | |
| ENT-02 | RCBE | Validación del registro de beneficiarios | Infraestructura | 🟡 | Ley 89/2017 |
| ENT-03 | Grafo UBO (backend) | `BuildUboGraphAsync` recursivo | Query | ✅ | |
| ENT-04 | Grafo UBO (UI rica) | Diseño jerárquico, zoom, inspector, tabla, flags PEP | `/cases/{id}/ubo`, embed CaseDetail | ✅ | Mayo 2026 |
| ENT-05 | Fusionar grafo + caso | Partes del caso + GLEIF, `CasePartyId` | `UboGraphViewBuilder` | ✅ | |
| ENT-06 | Añadir parte manualmente | UBO, accionista, órgano social, apoderado | Modal CaseDetail | ✅ | |
| ENT-07 | Detalle de la parte | Cribado, identidad, señales | `/cases/{id}/parties/{id}` | ✅ | |
| ENT-08 | Informar discrepancia RCBE | Botón + auditoría | PartyIdentityPanel | ✅ | IRN |

---

## Módulo 3 — Cribado y señales de riesgo

| ID | Funcionalidad | Descripción | UI / API | Estado | Base legal / nota |
|----|----------------|-------------|----------|--------|-------------------|
| SCR-01 | Pipeline automático | Sanciones, media, AT, judicial, ICIJ, scoring | Workers + MediatR | ✅ | |
| SCR-02 | Progreso en tiempo real | SignalR + barra de progreso | CaseDetail | ✅ | |
| SCR-03 | Rehacer cribado | Todas las partes, regenerar informe | CaseDetail | ✅ | |
| SCR-04 | Cribado por parte | Screen individual | EntityCard / PartyDetail | ✅ | |
| SCR-05 | Listas OFAC / EU | Descarga e índice local | Workers | ✅ | |
| SCR-06 | Confirmación de señales | El analista confirma la coincidencia | SignalCard | ✅ | |
| SCR-07 | Congelación por sanción | Notificación BdP + UnderReview | Pipeline | ✅ | Ley 97/2017 |
| SCR-08 | Scoring Ollama | 0–100 + nivel | RiskScoreBadge | ✅ | Sin Claude |
| SCR-09 | DDC automática | Simplified / Standard / Enhanced | Sección de conformidad | ✅ | Aviso 1/2022 |
| SCR-10 | Ventana adverse media | 2 años / 5 años EDD | Pipeline | ✅ | |

---

## Módulo 4 — Informes y explainability

| ID | Funcionalidad | Descripción | UI / API | Estado | Base legal / nota |
|----|----------------|-------------|----------|--------|-------------------|
| RPT-01 | Informe narrativo de 8 secciones | LLM Ollama | `/cases/{id}/report` | ✅ | |
| RPT-02 | Sección Art. 22 GDPR | Explainability en el prompt | Informe | ✅ | RGPD |
| RPT-03 | Export PDF | Puppeteer | `/api/cases/{id}/report.pdf` | ✅ | |
| RPT-04 | Embeddings pgvector | Búsqueda semántica en informe | Infra | ✅ | |
| RPT-05 | Consistencia de documentos | Checker vs GLEIF/caso | Ingesta | ✅ | |
| RPT-06 | API narrativa Claude | Enrutamiento cloud | — | 🔴 | Desviación intencionada |

---

## Módulo 5 — Ingesta de documentos

| ID | Funcionalidad | Descripción | UI / API | Estado | Base legal / nota |
|----|----------------|-------------|----------|--------|-------------------|
| DOC-01 | Carga de documentos | PDF, DOCX, imágenes | CaseDetail | ✅ | |
| DOC-02 | Pipeline asíncrono | Channel + hosted service | Background | ✅ | |
| DOC-03 | Extracción PDF/DOCX | PdfPig, OpenXML | Infra | ✅ | |
| DOC-04 | Extracción de imágenes | Visión Qwen | Infra | ✅ | |
| DOC-05 | Facts y parties en la BD | Tablas estructuradas | BD | ✅ | |
| DOC-06 | Recribado post-ingesta | Command | ✅ | |
| DOC-07 | Azure Blob Storage | Almacenamiento cloud | — | 🔴 | Blueprint fase 2 |
| DOC-08 | Azure Document Intelligence | OCR cloud | — | 🔴 | |

---

## Módulo 6 — Conformidad BdP (UI y reglas)

| ID | Funcionalidad | Descripción | UI / API | Estado | Base legal / nota |
|----|----------------|-------------|----------|--------|-------------------|
| CMP-01 | Sección de conformidad | Tarjeta amarilla en el caso | ComplianceCaseSection | ✅ | |
| CMP-02 | Badge SAR en hero | Estado de comunicación UIF | CaseDetail | ✅ | |
| CMP-03 | Banner SAR sugerido | Pipeline `SuggestSar` | CaseDetail + compliance | ✅ | |
| CMP-04 | Modal SAR | Narrativa ≥200, urgente | SarActionModals | ✅ | Arts. 52–57 |
| CMP-05 | SAR no aplicable | Justificación ≥50 | SarActionModals | ✅ | |
| CMP-06 | SAR urgente síncrono | Sin cola | Handler | ✅ | |
| CMP-07 | SAR no urgente | Cola asíncrona | Handler | ✅ | |
| CMP-08 | Registro manual UIF | Cuando la API no está disponible | Sección de conformidad | ✅ | |
| CMP-09 | Consultar estado UIF | Por referencia | Botón + query | ✅ | 🌐 API UIF |
| CMP-10 | Historial SAR | Tabla de auditoría | Sección de conformidad | ✅ | |
| CMP-11 | Verificación de identidad | Modal de 4 métodos | PartyIdentityPanel | ✅ | Aviso 1/2022 |
| CMP-12 | Badge de identidad PT | Verificado/Pendiente/… | EntityCard, badges | ✅ | |
| CMP-13 | Enlace de sesión de verificación | URL del proveedor | Panel de parte | ✅ | |
| CMP-14 | Webhook de identidad | HMAC POST | `/api/identity/webhook` | ✅ | |
| CMP-15 | Polling de identidad | Hosted service de fallback | Workers/Web | ✅ | |
| CMP-16 | Bloqueo de aprobación por identidad | `CanApproveMessage` | UI + dominio | ✅ | |
| CMP-17 | Origen de fondos EDD | Textarea + command | Conformidad | ✅ | |
| CMP-18 | EDD 4-eyes | Dropdown de supervisores Entra Graph | Diálogo de aprobación | ✅ | |
| CMP-19 | Alerta de congelación | Banner rojo | Conformidad | ✅ | Ley 97/2017 |
| CMP-20 | Integraciones live de producción | `RequireLiveIntegrations` | Config | ✅ | |
| CMP-21 | Envío UIF real | HTTP + Polly | Infra | 🟡 | 🌐 credenciales |
| CMP-22 | Notificación BdP real | HTTP freeze | Infra | 🟡 | 🌐 endpoint |

---

## Módulo 7 — Administración y gobernanza

| ID | Funcionalidad | Descripción | UI / API | Estado | Base legal / nota |
|----|----------------|-------------|----------|--------|-------------------|
| ADM-01 | PAC — versiones | Crear/activar, inmutabilidad | `/admin/settings` | ✅ | |
| ADM-02 | Motor de scoring — versiones | Hash de prompt, semver | Settings | ✅ | |
| ADM-03 | DPIA — versiones | Carga PDF, activa | `/admin/dpia` | ✅ | RGPD |
| ADM-04 | RPB anual | Generar borrador, métricas | `/admin/aml-report` | ✅ | Instr. 8/2024 |
| ADM-05 | Export RPB XML BdP | `?format=bdp` | API Admin | 🟡 | 🌐 template oficial X1 |
| ADM-06 | Enviar RPB BdP | Referencia + auditoría | UI Admin | ✅ | |
| ADM-07 | Audit log global | Buscar trail | `/admin/audit` | ✅ | |
| ADM-08 | Seed de conformidad | PAC/DPIA por defecto | Hosted seed | ✅ | |

---

## Módulo 8 — Dashboard y notificaciones

| ID | Funcionalidad | Descripción | UI / API | Estado |
|----|----------------|-------------|----------|--------|
| DSH-01 | KPIs de casos | Aprobados hoy, pendientes | `/` | ✅ |
| DSH-02 | Hub SignalR | Progreso y alertas | KycHub | ✅ |
| DSH-03 | Alertas de supervisores | SAR, conformidad | Grupo `supervisors` | ✅ |

---

## Módulo 9 — Infraestructura y operaciones

| ID | Funcionalidad | Descripción | Estado |
|----|----------------|-------------|--------|
| OPS-01 | Health check | `/health` | ✅ |
| OPS-02 | Docker on-prem | `docker-compose.prod.yml` | ✅ |
| OPS-03 | CI GitHub Actions | Build + migrate + test | ✅ |
| OPS-04 | Secrets Key Vault | Opcional | ✅ |
| OPS-05 | Abstracción de mensajería | SB / Rabbit / memory | ✅ |
| OPS-06 | Trabajo de retención de datos | Hosted opt-in | 🟡 |
| OPS-07 | Checklist de pen test | Documentado | 🔴 ejecución |

---

## Módulo 10 — Autenticación

| ID | Funcionalidad | Descripción | Estado |
|----|----------------|-------------|--------|
| AUTH-01 | Entra ID OIDC | Producción | ✅ |
| AUTH-02 | Identity local | Desarrollo | ✅ |
| AUTH-03 | Roles Analyst/Supervisor/Admin | Políticas | ✅ |
| AUTH-04 | Analyst accessor HTTP | Actor de auditoría | ✅ |

---

## Resumen por estado (Mayo 2026)

| Estado | Recuento aprox. | % |
|--------|-----------------|---|
| ✅ | ~75 funcionalidades | ~90% |
| 🟡 | ~8 | ~10% |
| 🔴 | ~4 | ~5% |

**Gaps prioritarios para go-live:** template RPB oficial (X1), credenciales UIF/BdP/identidad (X2–X4), ejecución E2E + pen test + dossier.

---

## Matriz UI → funcionalidad de conformidad

| Pantalla | Funcionalidades |
|----------|-----------------|
| CaseList | KYC-03, badge SAR |
| CaseDetail | KYC-04/05, CMP-02/03, embed ENT-04, SCR-02, DOC-01 |
| ComplianceCaseSection | CMP-01–20 |
| UboGraph | ENT-04/05 |
| CasePartyDetail | ENT-07, CMP-11/12, SCR-04 |
| Admin | ADM-01–07 |

---

## Referencias

- Documentación técnica: [DOCUMENTACAO_APLICACAO.md](DOCUMENTACAO_APLICACAO.md)
- Operaciones: [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
- Estado del Blueprint: [BLUEPRINT_COMPLETION_STATUS.md](BLUEPRINT_COMPLETION_STATUS.md)
