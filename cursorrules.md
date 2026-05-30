# KYC AI Platform — Cursor Rules

## Arquitectura
- NUNCA criar dependências de Infrastructure para Web ou vice-versa
- NUNCA injectar DbContext directamente em páginas Blazor — usar sempre MediatR
- NUNCA colocar lógica de negócio em Razor components — pertence a Use Cases
- SEMPRE usar construtores privados + factory methods nas entidades de domínio
- SEMPRE usar records para Value Objects e DTOs

## Segurança
- NUNCA colocar secrets em appsettings.json — sempre Azure Key Vault
- NUNCA loggar dados pessoais (NIF, nome, etc.) em níveis Information ou inferior
- SEMPRE sanitizar inputs de NIF (apenas dígitos, tamanho 9)
- SEMPRE validar autorização com [Authorize(Policy = "...")] nas páginas

## Audit Trail
- NUNCA adicionar Update ou Delete ao AuditEntryRepository
- SEMPRE registar acção em AuditEntry antes de qualquer decisão de risco
- SEMPRE incluir o hash do prompt LLM em entradas de auditoria geradas por IA

## GDPR
- NUNCA loggar conteúdo de relatórios KYC em logs aplicacionais
- SEMPRE aplicar políticas de retenção conforme DataRetentionJob
- SEMPRE anonimizar dados após expiração, nunca apagar registos de audit

## Código
- Usar C# 13 e .NET 9
- Nullable reference types sempre activados
- Async/await em toda a stack — sem bloqueios síncronos
- Usar CancellationToken em todos os métodos de infra
- Testes unitários para todos os Use Cases e Value Objects

## Conformidade BdP (`BLUEPRINT_BdP_Compliance_Addendum.md` §13–22)

### Identificação e diligência (Aviso 1/2022)
- SEMPRE verificar DueDiligenceLevelEvaluator após entity resolution no pipeline
- NUNCA aprovar caso com CaseParty.VerificationStatus != Verified para UBOs/BoardMembers/Proxy
- SEMPRE registar LegalBasisRef em KycCase (`PAC/{versão}/Lei83/2017-Art24`)
- EDD: verificação presencial, CMD ou assinatura qualificada — não só videoconferência
- Publicar `EntityIdentityVerifiedNotification` / `Failed` após webhook ou polling

### SAR e notificações (Lei 83/2017)
- NUNCA silenciar SarEligibilityEvaluator.ShouldSuggestSar sem audit `SarSuggested`
- SEMPRE chamar IAssetFreezeNotificationService quando sanção confirmada
- SAR urgente: audit `SarUrgentSubmitted` + `SarSubmittedNotification` → supervisores SignalR
- Consultar estado UIF via `GetUifSubmissionStatusQuery` quando ref. disponível

### Revisão periódica
- SEMPRE `ScheduleNextReview()` em `ApproveKycCaseCommandHandler`
- `IPeriodicReviewScheduler` publica re-triagem para casos com `NextReviewDue` vencido

### Pipeline EDD
- Se `DueDiligenceLevel == Enhanced` e `CanProceedWithEnhancedDd()` falhar: **omitir scoring LLM**

### Reproducibilidade e IA
- SEMPRE snapshot scoring (`SetScoringEngineSnapshot`) antes do LLM
- SEMPRE `LlmPromptHash` em `AuditEntry` para acções `LlmRiskScored` / `LlmReportGenerated`
- RPB: apenas modelos Ollama locais em `aiModelsJson`

### RGPD / explainability
- NUNCA auto-approve fora de Low + score ≤30 sem High/Critical/sanções
- Relatório: secções Art. 22 e limitações do modelo (`KycStructuredReportComposer`)

### Documentação operacional
- PAC: `docs/PAC_RUNBOOK.md`
- Analistas: `docs/ANALISTA_QUICK_START.md`
- Homologação: `docs/E2E_HOMOLOGACAO.md`, `docs/dossier/`