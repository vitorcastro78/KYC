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

## Conformidade BdP (adenda v1.0)

### Identificação e diligência
- SEMPRE verificar DueDiligenceLevelEvaluator após entity resolution no pipeline
- NUNCA aprovar caso com CaseParty.VerificationStatus != Verified para UBOs/BoardMembers
- SEMPRE registar LegalBasisRef em KycCase

### SAR e notificações obrigatórias
- NUNCA silenciar SarEligibilityEvaluator.ShouldSuggestSar sem registo em audit
- SEMPRE chamar IAssetFreezeNotificationService quando sinal Sanction é confirmado
- Prazo de notificação de congelamento: IMEDIATO (síncrono)

### Revisão periódica
- SEMPRE chamar ScheduleNextReview() após ApproveKycCaseCommand
- NUNCA omitir NextReviewDue em casos aprovados Ongoing

### Reproducibilidade
- SEMPRE guardar ScoringEngineVersion no KycCase antes de scoring
- NUNCA alterar ScoringEngineConfig após activação — nova versão

### RGPD
- NUNCA Auto-Approve para RiskLevel > Low sem revisão humana
- SEMPRE validar PolicyComplianceValidator no pipeline após UBO sync