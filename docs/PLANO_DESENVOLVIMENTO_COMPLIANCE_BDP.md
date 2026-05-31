# Plano de Desenvolvimento — Conformidade BdP 100%

> **Base:** `BLUEPRINT_BdP_Compliance_Addendum.md` (sec. 13–22) + `docs/CHECKLIST_HOMOLOGACAO_BDP.md`  
> **Estado actual (Maio 2026, commit `fc47caa`):** ~92% código produto (ver `docs/BLUEPRINT_COMPLETION_STATUS.md`); pendente integrações reais (UIF/BdP/identidade), template RPB oficial (X1) e **execução** de homologação (E2E, dossier, pen test).  
> **Objectivo:** 59 critérios de aceite (47 técnicos + 12 checklist) verificados em ambiente de homologação.  
> **Documentação unificada:** `docs/README.md` → `DOCUMENTACAO_APLICACAO.md`, `CATALOGO_FUNCIONALIDADES.md`, `OPERACOES_E_HOMOLOGACAO.md`.

---

## 1. Definição de “pronto” (Definition of Done)

Um item só está **Done** quando:

1. Código merged na branch de release com revisão.
2. Teste automatizado ou script E2E documentado passa.
3. Configuração documentada em `.env.example` / `appsettings` (sem secrets no repo).
4. Evidência anexada ao dossier de homologação (print, log audit, ou export).
5. Item correspondente no checklist marcado `[x]` por compliance.

---

## 2. Inventário: feito vs. em falta

### 2.1 Já implementado (não repetir trabalho)

| ID | Item | Localização principal |
|----|------|------------------------|
| D-01 | Entidades PAC, Scoring, DPIA, RPB + migration | `20260529205723_BdpComplianceAndGtm` |
| D-02 | Campos regulatórios `KycCase` / `CaseParty` | `KycCase.cs`, `CaseParty.cs` |
| D-03 | `LegalBasisAttribute` em `DocumentFactKey` | `DocumentFactKey.cs` |
| D-04 | `DueDiligenceLevelEvaluator` | `DueDiligenceLevelEvaluator.cs` |
| D-05 | `PolicyComplianceValidator` no **pipeline** | `KycCasePipelineRunner.cs` |
| D-06 | `CanApprove`, 4-eyes EDD, origem fundos | `KycCase.cs` |
| D-07 | SAR commands + `SarEligibilityEvaluator` | `ComplianceCommandHandlers.cs`, pipeline |
| D-08 | Asset freeze no `OverrideSignalCommand` | `ComplianceCommandHandlers.cs` |
| D-09 | `PeriodicReviewSchedulerJob` | `PeriodicReviewSchedulerJob.cs` |
| D-10 | `IAmlComplianceReportService` + export JSON | `AmlComplianceReportService.cs` |
| D-11 | UI compliance (SAR, identidade, EDD, badges) | `ComplianceCaseSection`, `PartyIdentityPanel`, `SarActionModals`, `EntityCard` |
| D-12 | Admin RPB / scoring / DPIA + upload | `Pages/Admin/*` |
| D-16 | Grafo UBO UI rico + merge partes caso | `UboGraphView`, `UboGraphViewBuilder` |
| D-17 | `ICurrentAnalystAccessor` + supervisores Entra Graph | `HttpContextAnalystAccessor`, `EntraGraphSupervisorUserDirectory` |
| D-18 | Guards integrações produção | `ComplianceIntegrationOptions`, `RequireLiveIntegrations` |
| D-19 | Registo manual ref. UIF (SAR pendente) | `RegisterManualUifReferenceCommand` |
| D-20 | Testes UBO view builder | `UboGraphViewBuilderTests.cs` |
| D-13 | Ollama-only LLM, OFAC SLS download | `KycLlmEngine`, Workers |
| D-14 | Health checks, docker prod, CI | `HealthCheckExtensions`, `docker-compose.prod.yml` |
| D-15 | Testes `PolicyComplianceValidator` | `PolicyComplianceValidatorTests.cs` |

### 2.2 Em falta (âmbito deste plano)

Ver **`docs/BLUEPRINT_COMPLETION_STATUS.md`** (mapa actualizado). Resumo: credenciais externas X1–X6, homologação E10 (E2E, dossier, pen test), Claude/Blob (fase 2 blueprint principal).

### 2.3 Estado por épico — código (Maio 2026)

> As tabelas da secção 3 são **especificação/backlog**; esta secção reflecte o **andamento real**.

| Épico | Código | Pendente (não código) |
|-------|--------|------------------------|
| **E1** PAC / arranque | ✅ Done | — |
| **E2** Identidade | ✅ Done (E2-09 P2 opcional) | Credenciais prestador X3 em prod |
| **E3** SAR / UIF | ✅ Done (E3-12 P2 SignalR opcional) | API UIF real X2; evidência homologação |
| **E4** EDD | ✅ Done (E4-06 P2) | — |
| **E5** Congelamento | ✅ Done | API BdP real X4 |
| **E6** Explainability | ✅ Done | — |
| **E7** RPB | 🟡 Export XML interno | Template oficial X1 (E7-01) |
| **E8** Admin versões | ✅ Done | — |
| **E9** RCBE | ✅ Done (E9-03 detecção pipeline P2) | — |
| **E10** Homologação | 🟡 Testes auto ✅ | E2E manual, dossier, pen test 🔴 |

**Percentagem global código:** ~92% (BdP addendum); homologação regulatória **0% evidências** até preencher `E2E_HOMOLOGACAO.md` e `dossier/`.

---

## 3. Épicos e tarefas

### E1 — Fundação legal e arranque de caso (3–4 dias)

| ID | Tarefa | Prioridade | Ficheiros / notas | Critério de aceite |
|----|--------|------------|-------------------|-------------------|
| E1-01 | Inject `ICustomerAcceptancePolicyRepository` + `PolicyComplianceValidator` em `StartKycCaseCommandHandler` | P0 | `StartKycCaseCommandHandler.cs` | Caso violando PAC não é criado; `PolicyViolationException` |
| E1-02 | Validar PAC **antes** de `repository.AddAsync` (sectores, jurisdições, PEP auto-reject) | P0 | idem | Teste: sector proibido → excepção |
| E1-03 | Capturar `RelationshipType` + montante no `StartKycCaseCommand` / `NewCase.razor` | P1 | `Commands.cs`, `NewCase.razor`, `KycCase.Start` | DDC usa tipo relação correcto |
| E1-04 | Propagar `LegalBasisRef` configurável por política activa | P2 | `CustomerAcceptancePolicy`, seed | Audit mostra base legal |
| E1-05 | Testes unitários `StartKycCase` + PAC | P0 | `tests/KYC.Application.Tests/` | 3+ cenários verdes |
| E1-06 | Documentar fluxo PAC no README ou runbook | P2 | `docs/` | Compliance consegue seguir |

**Dependências:** nenhuma.  
**Checklist:** Lei 83/2017 — PAC.

---

### E2 — Verificação de identidade (Aviso 1/2022) (5–7 dias)

| ID | Tarefa | Prioridade | Ficheiros / notas | Critério de aceite |
|----|--------|------------|-------------------|-------------------|
| E2-01 | `RecordVerificationResultCommand` + handler → `CaseParty.RecordVerificationResult` | P0 | `Commands.cs`, `ComplianceCommandHandlers.cs` | Estado party actualizado |
| E2-02 | Endpoint webhook `POST /api/identity/webhook` (assinatura HMAC opcional) | P0 | `KYC.Web/Program.cs`, novo controller/minimal API | Payload teste actualiza party |
| E2-03 | `IdentityVerificationPollingHostedService` (fallback se sem webhook) | P1 | `Infrastructure/BackgroundJobs/` | Sessão pendente → Verified em ≤5 min teste |
| E2-04 | Config: `IdentityVerification:WebhookSecret`, timeouts | P0 | `appsettings`, `.env.example` | |
| E2-05 | `DigitalSignIdentityVerificationService.GetVerificationResultAsync` completo | P0 | `DigitalSignIdentityVerificationService.cs` | Sem stub em produção se URL definida |
| E2-06 | UI: modal escolha método (Vídeo, CMD, Presencial, Assinatura qualificada) | P0 | `ComplianceCaseSection.razor` ou `CaseDetail.razor` | Analista inicia sessão |
| E2-07 | UI: badge por parte (Verified/Pending/Failed/Expired) + link sessão | P0 | idem | |
| E2-08 | Bloquear botão Aprovar na UI quando `CanApproveMessage` não vazio | P0 | componente aprovação | Mensagem Aviso 1/2022 visível |
| E2-09 | Domain events `EntityIdentityVerifiedEvent` / `Failed` (opcional MediatR notification) | P2 | `Domain/Events/`, handlers | Audit + SignalR |
| E2-10 | Testes integração webhook + `CanApprove` | P1 | `KYC.Integration.Tests` | |

**Dependências:** contrato API prestador (DigitalSign ou equivalente).  
**Checklist:** Aviso 1/2022 — identidade + bloqueio aprovação.

---

### E3 — SAR / UIF (Lei 83/2017, Art. 52.º–57.º) (4–5 dias)

| ID | Tarefa | Prioridade | Ficheiros / notas | Critério de aceite |
|----|--------|------------|-------------------|-------------------|
| E3-01 | `SubmitSarCommandHandler`: validar risk ≥ High ou sinal Critical | P0 | handler existente, reforçar regras | Rejeita caso Low sem justificação |
| E3-02 | SAR **urgente** = execução síncrona (sem fila) | P0 | `ComplianceCommandHandlers.cs` | Log + resposta imediata |
| E3-03 | UI modal SAR: narrativa min. 200 chars, checkbox urgente | P0 | `ComplianceCaseSection.razor` | Validação client + server |
| E3-04 | Pré-preencher narrativa com sinais Critical/High do caso | P1 | DTO + UI | |
| E3-05 | Botão «Marcar SAR não aplicável» + justificação obrigatória | P0 | UI + `MarkSarNotRequiredCommand` | `SarStatus = NotRequired` + audit |
| E3-06 | Banner amarelo quando `SuggestSar` (pipeline) | P1 | `KycCaseDetailDto`, UI | |
| E3-07 | `CaseList.razor`: coluna/badge `SarStatus` | P0 | `CaseList.razor`, `KycCaseDto` | |
| E3-08 | Página ou secção histórico SAR (ref UIF, data) | P2 | `CaseDetail` | |
| E3-09 | `UifReportingService` produção + retry Polly | P0 | `UifReportingService.cs` | Ref. UIF em staging |
| E3-10 | UI consulta `GetSubmissionStatusAsync` por ref. | P2 | Admin ou CaseDetail | |
| E3-11 | Testes: SubmitSar, MarkNotRequired, eligibility | P0 | `tests/` | |
| E3-12 | Domain event `SarSubmittedEvent` + notificação supervisor | P2 | SignalR | |

**Dependências:** credenciais UIF ou MOU com processo manual documentado.  
**Checklist:** SAR/UIF + audit trail.

---

### E4 — EDD e pipeline reforçado (4–5 dias)

| ID | Tarefa | Prioridade | Ficheiros / notas | Critério de aceite |
|----|--------|------------|-------------------|-------------------|
| E4-01 | `IAdverseMediaService`: parâmetro anos (2 default, 5 se EDD) | P0 | interface + impl + pipeline | EDD usa 5 anos |
| E4-02 | Pipeline: após DDC Enhanced, exigir verificação não-Simplified | P1 | `DueDiligenceLevelEvaluator`, `CanApprove` | eIDAS High ou presencial |
| E4-03 | UI origem fundos: obrigatório antes de aprovar EDD (disable approve) | P0 | `ComplianceCaseSection` | |
| E4-04 | UI segundo aprovador (dropdown supervisor) em EDD | P0 | approve command | `SecondApproverId` gravado |
| E4-05 | Secção «Diligência aplicada» com justificação DDC legível | P1 | `CaseDetail` / compliance section | |
| E4-06 | `CanProceedWithEnhancedDd` chamado antes de scoring pesado | P2 | pipeline | |
| E4-07 | Testes `DueDiligenceLevelEvaluator` (PEP, offshore, ocasional) | P1 | `tests/` | |

**Checklist:** DDC, EDD origem fundos, 4-eyes.

---

### E5 — Congelamento de ativos (Lei 97/2017) (3–4 dias)

| ID | Tarefa | Prioridade | Ficheiros / notas | Critério de aceite |
|----|--------|------------|-------------------|-------------------|
| E5-01 | Após confirmar sanção: `kyc.SetStatus(UnderReview)` | P0 | `OverrideSignalCommandHandler` | Não auto-aprova |
| E5-02 | `IKycCaseRealtimeNotifier` alerta supervisor imediato | P0 | handler + hub | Toast/dashboard |
| E5-03 | `AssetFreezeNotificationService` API BdP real | P0 | `AssetFreezeNotificationService.cs` | Confirmação em staging |
| E5-04 | Config `BdpAssetFreeze:*` em prod example | P0 | `.env.example` | |
| E5-05 | UI: indicador «Congelamento notificado» + ref. | P1 | `ComplianceCaseSection` | |
| E5-06 | Teste integração: confirm sanction → notify + flags | P0 | `tests/` | |

**Dependências:** endpoint BdP ou procedimento manual com SLA assinado.  
**Checklist:** Lei 97/2017.

---

### E6 — Relatório, IA e explainability (3–4 dias)

| ID | Tarefa | Prioridade | Ficheiros / notas | Critério de aceite |
|----|--------|------------|-------------------|-------------------|
| E6-01 | Prompt LLM: secção 10 «Declaração de Decisão Automatizada (RGPD Art. 22.º)» | P0 | `KycLlmEngine.cs` | HTML contém secção |
| E6-02 | Prompt/composer: «Limitações do Modelo de IA» | P0 | `KycStructuredReportComposer` ou LLM | Dimensões n/d listadas |
| E6-03 | `AmlComplianceReportService`: `aiModelsJson` só Ollama | P1 | `AmlComplianceReportService.cs` | Sem cloud |
| E6-04 | Audit LLM: `ModelVersion` + prompt hash em todas decisões IA | P1 | pipeline, `AuditEntry` | |
| E6-05 | Validar auto-approve só Low + score ≤30 sem High/Critical | P0 | `KycCase` / pipeline | Teste regressão |

**Checklist:** RGPD explainability, auto-approve.

---

### E7 — RPB Instrução BdP 8/2024 (5–8 dias)

| ID | Tarefa | Prioridade | Ficheiros / notas | Critério de aceite |
|----|--------|------------|-------------------|-------------------|
| E7-01 | Obter template oficial BdP (XML/JSON/XLS) junto compliance | P0 | **Bloqueante externo** | Documento versionado |
| E7-02 | `IBdpRpbExporter` + implementação mapeamento `AmlComplianceReport` → template | P0 | `Infrastructure/Compliance/` | Export valida contra schema |
| E7-03 | Substituir/aliás `ExportRpbAsync` JSON por export oficial + JSON interno | P0 | `AmlComplianceReportService` | Dois formatos download |
| E7-04 | `AmlReport.razor`: tabelas métricas, gráficos simples | P1 | UI | |
| E7-05 | Histórico relatórios (lista por ano, status Draft/Submitted) | P1 | repo + UI | |
| E7-06 | Botão «Submeter ao BdP» → `SubmitToBdpAsync` + confirmação | P0 | UI Admin | `BdpReferenceNumber` |
| E7-07 | Role `KYC.Admin` apenas para submissão | P1 | policies | |
| E7-08 | Testes export + métricas agregadas | P1 | `tests/` | |

**Dependências:** E7-01 (compliance).  
**Checklist:** RPB geração + export formatado.

---

### E8 — Admin: versões PAC, scoring, DPIA (3–4 dias)

| ID | Tarefa | Prioridade | Ficheiros / notas | Critério de aceite |
|----|--------|------------|-------------------|-------------------|
| E8-01 | `CreateScoringEngineConfigCommand`: nova versão, desactiva anterior | P0 | Application + Admin UI | Imutabilidade |
| E8-02 | `CreateDpiaRecordCommand`: upload path, nova versão | P0 | Admin `DpiaRecord.razor` | |
| E8-03 | `CreateCustomerAcceptancePolicyCommand` (PAC v2) | P1 | Domain + Admin | |
| E8-04 | `Settings.razor`: resumo PAC/DPIA/scoring activos | P1 | já parcial | |
| E8-05 | Validação: não apagar versão activa | P0 | domain rules | |

**Checklist:** DPIA activa registada.

---

### E9 — RCBE e dados auxiliares (2–3 dias)

| ID | Tarefa | Prioridade | Ficheiros / notas | Critério de aceite |
|----|--------|------------|-------------------|-------------------|
| E9-01 | `ReportRcbeDiscrepancyCommand` + handler | P1 | `CaseParty.ReportRcbeDiscrepancy` | Flags + audit |
| E9-02 | UI botão «Reportar discrepância RCBE ao IRN» | P1 | CaseDetail | |
| E9-03 | Pipeline: detectar discrepância declarado vs RCBE → flag party | P2 | entity resolution | |

---

### E10 — Testes, ops e homologação (5–7 dias)

| ID | Tarefa | Prioridade | Ficheiros / notas | Critério de aceite |
|----|--------|------------|-------------------|-------------------|
| E10-01 | Suite integração: SAR, asset freeze, periodic review, PAC start | P0 | `KYC.Integration.Tests` | CI verde |
| E10-02 | Teste E2E manual script (`docs/E2E_HOMOLOGACAO.md`) | P0 | novo doc | 15 passos |
| E10-03 | Actualizar `CHECKLIST_HOMOLOGACAO_BDP.md` com evidências | P0 | checklist | 12/12 [x] |
| E10-04 | Dossier PDF: prints + versões scoring/DPIA/PAC | P1 | `docs/dossier/` | |
| E10-05 | Workers obrigatório em prod: OFAC + EU FSF download | P0 | deploy doc | XML presente |
| E10-06 | Rever `cursorrules.md` secção BdP vs §21 adenda | P2 | `cursorrules.md` | |
| E10-07 | Pen test básico (OWASP ZAP ou checklist interno) | P1 | relatório | |
| E10-08 | Formação analistas (1 página quick start) | P2 | `docs/` | |

**Checklist:** operacional + todos os regulatórios.

---

## 4. Cronograma sugerido (10 semanas)

| Semana | Épicos | Entregável |
|--------|--------|------------|
| S1 | E1, E10-05 | PAC no start; OFAC/Workers validados |
| S2 | E2 (E2-01–05) | Webhook + API identidade |
| S3 | E2 (E2-06–10), E3 (E3-01–05) | UI identidade + SAR modal |
| S4 | E3 (E3-06–12), E4 | SAR completo + EDD pipeline |
| S5 | E5, E6 | Congelamento + explainability LLM |
| S6 | E7 (após template BdP) | Export RPB oficial |
| S7 | E7 (UI), E8 | Admin RPB + versões |
| S8 | E9, E10-01–03 | RCBE + testes CI |
| S9 | E10-04–08 | Dossier homologação |
| S10 | Buffer + UAT | Sign-off compliance |

**Paralelização:** E7-01 (compliance) em S1; E4 UI em paralelo com E3 após S2.

---

## 5. Dependências externas (bloqueantes)

| # | O quê | Quem fornece | Impacto se atrasar |
|---|--------|--------------|-------------------|
| X1 | Template export RPB Instr. 8/2024 | Compliance / BdP | E7 bloqueado |
| X2 | API UIF (ou processo manual + SLA) | Instituição | E3 produção |
| X3 | API identidade (DigitalSign/CMD) | Prestador | E2 produção |
| X4 | API notificação congelamento BdP | Instituição | E5 produção |
| X5 | PAC v1 assinada internamente | Compliance | Seed ≠ produção |
| X6 | DPIA aprovada (documento PDF) | DPO | E8-02 |

---

## 6. Configuração mínima homologação

```env
# Base
KYC_DB_CONNECTION=...
OLLAMA_ENDPOINT=http://host:11434

# Compliance integrations
IdentityVerification__BaseUrl=
IdentityVerification__ApiKey=
IdentityVerification__WebhookSecret=
Uif__BaseUrl=
Uif__ApiKey=
BdpAssetFreeze__BaseUrl=

# Workers (OFAC + EU)
ExternalSources__OfacSdnDailyDownload__Enabled=true
ExternalSources__EuFsfDailyDownload__Enabled=true
```

---

## 7. Métricas de acompanhamento

| Métrica | Meta |
|---------|------|
| Tarefas Done (sec. 3) | 62/62 |
| Checklist homologação | 12/12 |
| Cobertura testes compliance | ≥ 80% handlers críticos |
| Bugs P0 abertos | 0 antes de go-live |
| Tempo médio SAR urgente (E2E) | < 2 min (síncrono) |

---

## 8. Ordem de implementação recomendada (sprint 0 → 6)

```
Sprint 0 (P0 rápido):  E1-01, E1-02, E1-05, E3-07, E4-03, E4-04, E5-01, E5-02
Sprint 1:             E2 completo
Sprint 2:             E3 completo
Sprint 3:             E4, E5, E6
Sprint 4:             E7 + E8
Sprint 5:             E9, E10
```

---

## 9. Riscos

| Risco | Mitigação |
|-------|-----------|
| Template RPB indisponível | Export JSON interino + flag «draft» até schema oficial |
| UIF sem API | Modo manual: analista regista ref. externa + `SubmitSar` grava audit only |
| Ollama indisponível | Score heurístico + relatório template (já existe fallback) |
| OFAC XML grande | Workers + path partilhado Web/Workers |

---

## 10. Próximo passo imediato

1. Aprovar este plano com compliance (validar X1–X6).  
2. Abrir 62 issues no Jira/GitHub a partir da sec. 3.  
3. Iniciar **Sprint 0** (E1 + quick wins UI SAR/lista).

---

*Documento vivo — actualizar quando itens D-xx passarem a Done.*
