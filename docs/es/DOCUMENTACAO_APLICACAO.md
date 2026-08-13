# Documentación de la aplicación: plataforma KYC AI

> **Versión del documento:** Mayo 2026 · Rama `feature/kyc-document-ingestion`  
> **Pila:** .NET 9 · Servidor Blazor · PostgreSQL 16 · ContextMemory · Trabajadores  
> **Alcance:** documentación unificada para equipos técnicos, cumplimiento y generación de manuales.

---

## 1. Objetivo y alcance

La **Plataforma KYC AI** automatiza el proceso Conozca a su cliente para crédito corporativo en Portugal:

- Resolución de entidades (RCBE, GLEIF) y gráfico de beneficiarios reales (UBO)
- Cribado paralelo: sanciones, PEP, medios adversos, datos financieros y judiciales, ICIJ
- Ingestión y extracción de documentos (PDF, DOCX, imágenes)
- Puntuación de riesgo (0-100) e informe narrativo con explicabilidad (RGPD Art. 22)
- Flujo de trabajo de analista/supervisor con cumplimiento **BdP** (Ley 83/2017, Aviso 1/2022, Instr. 8/2024, Ley 97/2017)

**Público objetivo de este documento:** desarrollo, arquitectura, cumplimiento, operaciones, documentación formal.

---

## 2. Arquitectura

### 2.1 Arquitectura limpia
```
src/
├── KYC.Domain/           Entidades, enums, value objects, eventos de domínio
├── KYC.Application/      Commands/Queries MediatR, DTOs, interfaces, políticas
├── KYC.Infrastructure/   EF Core, HTTP clients, LLM, compliance, messaging
├── KYC.Web/              Blazor Server, páginas, componentes, APIs minimal
└── KYC.Workers/          Hosted services (listas OFAC/EU, retenção, etc.)

tests/
├── KYC.Domain.Tests/
├── KYC.Application.Tests/
├── KYC.Integration.Tests/
└── KYC.Web.Integration.Tests/
```
**Reglas de dependencia:** Dominio → nada; Aplicación → Dominio; Infraestructura → Aplicación + Dominio; Web → Aplicación (registra Infraestructura DI en `Program.cs`).

### 2.2 Flujo principal de un caso
```
NIF + montante + relação (ocasional/continuada)
  → Validação PAC (antes de persistir)
  → KycCase InProgress
  → Entity resolution (GLEIF / RCBE)
  → UBO graph (GLEIF Level 2 + partes do caso)
  → Pipeline paralelo: sanções, media, AT, CITIUS, ICIJ, Scoring ContextMemory
  → RiskSignals + RiskScore
  → Relatório narrativo (8 secções + Art. 22)
  → Workflow: auto-approve (Low) | revisão | aprovação/rejeição
  → Audit trail append-only
```
### 2.3 Flujo de ingesta de documentos
```
Upload UI/API → CaseDocument (Pending) → ficheiro em Data/cases/{caseId}/documents/
  → DocumentIngestionHostedService (channel)
  → PdfPig | OpenXML | Qwen visão
  → DocumentFieldExtractor + mapper → facts/parties na BD
  → DocumentConsistencyChecker → sinais de inconsistência
  → Opcional: re-triagem automática do caso
```
---

## 3. Pila tecnológica

| Capa | Tecnología |
|--------|------------|
| Tiempo de ejecución | .NET 9 |
| interfaz de usuario | Servidor Blazor, Bootstrap, SignalR (`KycHub`) |
| API | API mínimas (`Program.cs`), webhook de identidad |
| ORM | EF Core 9, PostgreSQL 16 |
| CQRS | MediatR |
| LLM | Gateway ContextMemory — OpenAI-compatible `POST /v1/chat/completions` + Global Wiki |
| Autenticación | Microsoft Entra ID (OIDC) o identidad ASP.NET (dev) |
| Secretos | `.env` / Bóveda de claves de Azure (`KYC_KEYVAULT_NAME`) |
| Mensajería | Azure Service Bus, RabbitMQ o en memoria |
| Informe en PDF | Puppeteer |
| CI | Acciones de GitHub (`.github/workflows/ci.yml`) |
| Implementar | `docker-compose.prod.yml` local |

---

## 4. Autenticación y autorización

### 4.1 Modos de autenticación

- **Producción/aprobación:** `AzureAd:Enabled=true` — OIDC Introduzca ID
- **Desarrollo:** Identidad + PostgreSQL (`AuthDbContext`), administrador semilla

### 4.2 Funciones

| Desplazarse | Permisos típicos |
|------|-------------------|
| `KYC.Analyst` | Casos, Selección, Cumplimiento, SAR (Presentación), Documentos |
| `KYC.Supervisor` | Aprobación EDD de 4 ojos, alertas SignalR SAR |
| `KYC.Admin` | PAC, puntuación, DPIA, RPB, registro de auditoría, configuración |

Políticas en `Program.cs`: `Analyst`, `Supervisor`, `Admin`.

### 4.3 Identificación del analista

`ICurrentAnalystAccessor` / `HttpContextAnalystAccessor`: ID de usuario autenticado en comandos de auditoría (reemplaza a `dev-user`).

---

## 5. Modelo de dominio (resumen)

### 5.1 Caso Kyc

Estados: `Pending`, `InProgress`, `UnderReview`, `Approved`, `Rejected`.

Campos regulatorios: `DueDiligenceLevel`, `RelationshipType`, `SarStatus`, `NextReviewDue`, `FundsOriginDescription`, `AssetFreezeNotified`, `LegalBasisRef`, `ScoringEngineVersion`, etc.

### 5.2 CaseParty (entidades de caso)

Roles: `Target`, `Shareholder`, `Ubo`, `BoardMember`, `Proxy`.

Banderas: `IsPep`, `IsSanctioned`, `IsOffshore`.

Identidad: `VerificationStatus`, `VerificationMethod`, `VerificationSessionId`, `VerificationUrl`.

RCBE: `RcbeDiscrepancyDetected`, `RcbeDiscrepancyReported`.

### 5.3 Artefactos asociados

- `RiskSignal`: tipo, gravedad, fuente, confirmación del analista
- `KycReport` — informe final + incrustaciones
- `CaseDocument` — ingestión y extracción
- `AuditEntry` — inmutable (disparador PostgreSQL)
- `CustomerAcceptancePolicy`, `ScoringEngineConfig`, `DpiaRecord`, `AmlComplianceReport`

---

## 6. Capa de aplicación: utilice casos principales

| Área | Comandos/Consultas (ejemplos) |
|------|------------------------------|
| Casos | `StartKycCaseCommand`, `GetKycCaseQuery`, `ListKycCasesQuery`, `ApproveKycCaseCommand`, `RejectKycCaseCommand` |
| Proyección | `RerunKycCaseScreeningCommand`, `ScreenCasePartyCommand` |
| Fiestas | `AddCasePartyCommand`, `ConfirmRiskSignalCommand` |
| Documentos | `UploadCaseDocumentCommand`, tubería de admisión |
| UBO | `GetUboGraphQuery` → `UboGraphViewDto` |
| Cumplimiento | `SubmitSarCommand`, `MarkSarNotRequiredCommand`, `InitiateEntityVerificationCommand`, `RecordPresentialVerificationCommand`, `SetFundsOriginCommand`, `RegisterManualUifReferenceCommand` |
| Administrador | `CreateCustomerAcceptancePolicyCommand`, `GenerateAnnualReportCommand`, carga DPIA |
| Informe | Generación de PDF, exportación RPB |

---

## 7. Integraciones externas

| Integración | Interfaz | Configuración | Modo de desarrollo |
|------------|-----------|--------------|----------|
| GLEIF | `IEntityResolutionService` | API pública | ✅ |
| RCBE | `IRcbeRegistryService` | URL configurable | Respaldo/simulacro |
| Sanciones OFAC/UE | Trabajadores + índice local | `Data/ofac`, `Data/eu-fsf` | Descarga periódica |
| ContextMemory | `IKycLlmEngine` / `IContextMemoryWikiClient` | `ContextMemory:*` | Gateway |
| Identidad | `IIdentityVerificationService` | `IdentityVerification:*` | Código auxiliar si no hay URL |
| UIF (SAR) | `IUifReportingService` | `Uif:*` | Ref. sintética. |
| Congelación BdP | `IAssetFreezeNotificationService` | `BdpAssetFreeze:*` | Sólo iniciar sesión |
| Medios adversos | `IAdverseMediaService` | API de noticias, etc. | |
| En deudores | `ITaxDebtorsService` | | |
| CICIO | `ICitiusClient` | | |
| ICIJ | GraphQL costa afuera | | |
| Ingresar Gráfico | Supervisores del EDD | `Compliance:SupervisorGroupObjectId` | Lista de manuales |

**Producción:** `Compliance:RequireLiveIntegrations=true` (predeterminado en Producción): bloquea las referencias de `local-` / `UIF-DEV` y requiere URL reales.

---

## 8. API y puntos finales relevantes

| Método | Ruta | Descripción |
|-----------|------|-----------|
| OBTENER | `/health` | Control de salud |
| PUBLICAR | `/api/identity/webhook` | Verificación de devolución de llamada (HMAC opcional) |
| OBTENER | `/api/cases/{id}/report.pdf` | Exportar informe PDF |
| OBTENER | `/api/admin/aml-reports/...` | Exportar RPB (JSON/XML BdP) |

Cargar documentos: multiparte a través de Aplicación/Web (ver controladores de carga).

---

## 9. Interfaz de usuario (mapa de página)

| Ruta | Página | Función |
|------|--------|--------|
| `/` | Panel de control | KPI, casos recientes, SignalR |
| `/cases` | Lista de casos | Portafolio con puntaje, DDC, SAR |
| `/cases/new` | Nuevo caso | Inauguración con PAC |
| `/cases/{id}` | Detalle del caso | Hero, cumplimiento, inserción UBO, acciones, documentos, acciones |
| `/cases/{id}/ubo` | UboGraph | Gráfico completo de la UBO |
| `/cases/{id}/report` | Informe HTML | |
| `/cases/{id}/parties/{partyId}` | Detalle de la fiesta del caso | Cribado por partido + identidad |
| `/admin/settings` | Configuración | PAC, puntuación, EIPD |
| `/admin/aml-report` | Informe Aml | RPB anual |
| `/admin/audit` | Registro de auditoría | Sendero global |
| `/admin/dpia` | Registro de dpia | Versiones EIPD |

### 9.1 Componentes de interfaz de usuario reutilizables

| Componente | Función |
|------------|-----------|
| `ComplianceCaseSection` | Sección BdP: SAR, identidad, EDD, historia |
| `PartyIdentityPanel` | Verificación de identidad + métodos modales |
| `SarActionModals` | Modos SAR / no aplicable |
| `IdentityVerificationBadge` | Insignia PT (verificada, pendiente,…) |
| `UboGraphView` | Gráfico SVG UBO (zoom, inspector, tabla) |
| `EntityCard` | Parte de la tarjeta con riesgo + tarjetas de identidad |
| `RiskScoreBadge`, `SignalCard`, `ScanProgressBar` | Detección y riesgo |

---

## 10. SignalR: en tiempo real

`KycHub` Hub: progreso de detección, informe listo, alertas de cumplimiento (SAR, identidad, congelación). Grupos por caso y grupo `supervisors` para SAR.

---

## 11. Configuración

### 11.1 Variables esenciales (`.env.example`)
```env
KYC_DB_CONNECTION=Host=...;Database=...;Username=...;Password=...
CONTEXT_MEMORY_BASE_URL=http://host.docker.internal:11434
AzureAd__Enabled=true|false
IdentityVerification__BaseUrl=...
IdentityVerification__WebhookSecret=...
Uif__BaseUrl=...
Uif__ApiKey=...
BdpAssetFreeze__BaseUrl=...
Compliance__RequireLiveIntegrations=true
Compliance__SupervisorGroupObjectId=<guid-grupo-AD>
```
### 11.2 Archivos de configuración

- `src/KYC.Web/appsettings.json` — valores predeterminados (sin secretos)
- `src/KYC.Web/Program.cs`: DI, autenticación, puntos finales, servicios alojados

---

## 12. Base de datos y migraciones

- **KycDbContext** — casos, partes, señales, documentos, cumplimiento, RPB
- **AuthDbContext** — Usuarios de identidad (desarrollador)
- Migraciones en `src/KYC.Infrastructure/Migrations/`
- **Auditoría inmutable:** activa `tr_audit_entries_immutable`
- **ContextMemory Global Wiki:** conocimiento de informes (sin pgvector local)

Aplicar: `dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web`

---

## 13. Seguridad y RGPD

- Secretos fuera del repositorio; Bóveda de claves opcional
- Aprobación automática solo riesgo bajo (puntuación ≤30, sin sanciones altas/críticas)
- EIPD activa obligatoria para su tramitación
- Retención: `DataRetention:EnableHostedService` (optar por participar)
- Explicabilidad en el informe (Art. 22 RGPD)
- Identidad del webhook con HMAC SHA-256 cuando se define el secreto

---

## 14. Pruebas y CI
```bash
dotnet test
dotnet test tests/KYC.Web.Integration.Tests  # requer KYC_DB_CONNECTION para Postgres
```
Pruebas relevantes: controladores de cumplimiento, elegibilidad de SAR, webhook de identidad, creador de gráficos UBO, política PAC, inmutabilidad de auditoría.

CI: compilación, migración EF, pruebas en servicio PostgreSQL.

---

## 15. Glosario

| Término | Significado |
|-------|-------------|
| **PAC** | Política de Aceptación del Cliente (Art. 24 Ley 83/2017) |
| **DDC/EDD** | Debida diligencia estándar/mejorada |
| **SAR** | Informe de actividad sospechosa → Comunicación **UIF** |
| **UBO** | Titular real final |
| **RPB** | Informe de Prevención del Blanqueo de Capitales (Instr. 8/2024) |
| **RCBE** | Registro central de beneficiarios reales |
| **GLEIF** | Fundación Global LEI (datos corporativos) |
| **4 ojos** | Doble aprobación en EDD (`SecondApproverId`) |

---

## 16. Referencias cruzadas

- Catálogo de características: [CATALOGO_FUNCIONALIDADES.md](CATALOGO_FUNCIONALIDADES.md)
- Operaciones y homologación: [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
