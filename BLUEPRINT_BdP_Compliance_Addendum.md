# KYC AI Platform — Adenda de Conformidade Regulatória BdP

> **Complemento ao:** BLUEPRINT.md v1.1 (Maio 2026)  
> **Âmbito:** Requisitos de homologação Banco de Portugal — AML/KYC  
> **Base legal principal:** Lei n.º 83/2017 · Aviso BdP n.º 1/2022 · Instrução BdP n.º 8/2024 · Lei n.º 89/2017 · Lei n.º 97/2017 · RGPD  
> **Versão:** 1.0 · Maio 2026

---

## Como integrar esta adenda

Este documento adiciona **secções 13 a 20** ao blueprint principal.  
As alterações ao modelo de dados existente (secção 3) estão identificadas com o prefixo `[ALTERAÇÃO §3]`.  
Novas interfaces de Application Layer (secção 4) estão identificadas com `[NOVO §4]`.  
Todos os exemplos de código seguem os mesmos padrões de C# 13, .NET 9 e Clean Architecture do blueprint original.

---

## §3 — Alterações ao Modelo de Dados Existente

### [ALTERAÇÃO §3] — KycCase — Campos regulatórios adicionais

```csharp
// KYC.Domain/Entities/KycCase.cs — campos a adicionar
public class KycCase
{
    // ... campos existentes mantidos ...

    // § Regime de diligência aplicado (Lei 83/2017, Art. 35.º; Aviso 1/2022)
    public DueDiligenceLevel DueDiligenceLevel { get; private set; }   // Simplified | Standard | Enhanced
    public string? DueDiligenceJustification { get; private set; }     // Razão documentada da escalation/redução

    // § Tipo de relação de negócio (limiar €12.500 / €15.000)
    public RelationshipType RelationshipType { get; private set; }     // Occasional | Ongoing
    public decimal? CreditAmountRequested { get; private set; }        // Para aplicar threshold DDC

    // § Revisão periódica (Lei 83/2017, Art. 35.º, n.º 6)
    public DateTime? NextReviewDue { get; private set; }               // Calculado após aprovação
    public DateTime? LastReviewedAt { get; private set; }

    // § SAR / comunicação UIF
    public SarStatus SarStatus { get; private set; }                   // None | Pending | Submitted | NotRequired
    public string? SarReferenceNumber { get; private set; }            // Nº de referência atribuído pela UIF
    public DateTime? SarSubmittedAt { get; private set; }

    // § Congelamento de ativos (Lei 97/2017)
    public bool AssetFreezeNotified { get; private set; }
    public DateTime? AssetFreezeNotifiedAt { get; private set; }

    // § Versioning do motor de scoring (para reproducibilidade em auditoria)
    public string? ScoringEngineVersion { get; private set; }          // semver, ex: "2.1.0"
    public string? ScoringModelSnapshot { get; private set; }          // JSON snapshot das configs activas

    // § Base legal de tratamento (RGPD Art. 6.º)
    public string LegalBasisRef { get; private set; }                  // Ex: "Lei83/2017-Art24" 
}

public enum DueDiligenceLevel { Simplified, Standard, Enhanced }
public enum RelationshipType { Occasional, Ongoing }
public enum SarStatus { None, Pending, Submitted, NotRequired }
```

### [ALTERAÇÃO §3] — Entity — Verificação de identidade e base legal

```csharp
// KYC.Domain/Entities/Entity.cs — campos a adicionar
public class Entity
{
    // ... campos existentes mantidos ...

    // § Método de verificação de identidade (Aviso 1/2022, Art. 31.º)
    public IdentityVerificationMethod VerificationMethod { get; private set; }
    // Presential | VideoConference | QualifiedSignature | CMD | ThirdPartyReliance | NotYetVerified
    public DateTime? VerifiedAt { get; private set; }
    public string? VerificationSessionId { get; private set; }         // Ref. do prestador (ex: DigitalSign)
    public IdentityVerificationStatus VerificationStatus { get; private set; }
    // Pending | Verified | Failed | Expired

    // § Validação RCBE (Lei 89/2017)
    public DateTime? RcbeVerifiedAt { get; private set; }
    public bool RcbeDiscrepancyDetected { get; private set; }          // Divergência declarado vs RCBE
    public bool RcbeDiscrepancyReported { get; private set; }          // Notificação ao IRN enviada
    public DateTime? RcbeDiscrepancyReportedAt { get; private set; }

    // § Base legal de recolha de dados por entidade (RGPD Art. 6.º)
    public string DataCollectionBasis { get; private set; }            // Ex: "Lei83/2017-Art24-n1"
}

public enum IdentityVerificationMethod
{
    Presential,
    VideoConference,      // Aviso 1/2022, Art. 31.º, n.º 2, al. a)
    QualifiedSignature,   // eIDAS nível Substantial ou High
    CMD,                  // Chave Móvel Digital
    ThirdPartyReliance,   // Art. 41.º Lei 83/2017
    NotYetVerified
}

public enum IdentityVerificationStatus { Pending, Verified, Failed, Expired }
```

### [ALTERAÇÃO §3] — DocumentFactKey — Mapeamento para base legal

```csharp
// KYC.Domain/Enums/DocumentFactKey.cs — extensão com anotação legal
// Cada valor do enum deve ter um atributo de base legal para audit trail
[AttributeUsage(AttributeTargets.Field)]
public class LegalBasisAttribute(string legalRef) : Attribute
{
    public string LegalRef { get; } = legalRef;
}

public enum DocumentFactKey
{
    [LegalBasis("Lei83/2017-Art24-n1-a")] CompanyName,
    [LegalBasis("Lei83/2017-Art24-n1-b")] Nif,
    [LegalBasis("Lei83/2017-Art24-n1-c")] Address,
    [LegalBasis("Lei83/2017-Art25")]       Cae,
    [LegalBasis("Lei83/2017-Art35")]       Iban,
    [LegalBasis("Lei83/2017-Art35")]       Revenue,
    [LegalBasis("Lei83/2017-Art35")]       Equity,
    [LegalBasis("Lei83/2017-Art24")]       DocumentDate,
    [LegalBasis("Lei83/2017-Art35")]       Summary
}
```

---

## 13. Verificação de Identidade Remota — Aviso BdP n.º 1/2022, Art. 31.º

### 13.1 Contexto regulatório

O Aviso n.º 1/2022 do Banco de Portugal permite a identificação não presencial de clientes desde que cumpra requisitos técnicos específicos. Aplicam-se a todos os **UBOs, administradores e representantes** identificados no grafo de controlo que não sejam verificados presencialmente.

**Métodos aceites:**
- Videoconferência em tempo real (sem interrupções, com liveness detection)
- Assinatura qualificada eIDAS (nível Substantial ou High)
- Chave Móvel Digital (CMD) — específico PT
- Delegação a terceiro (Art. 41.º Lei 83/2017) com acordo de reciprocidade

### 13.2 Interfaces de Application Layer

```csharp
// [NOVO §4] KYC.Application/Interfaces/IIdentityVerificationService.cs
public interface IIdentityVerificationService
{
    /// <summary>
    /// Inicia sessão de videoconferência ou CMD para verificação de UBO/administrador.
    /// Retorna SessionId do prestador para posterior polling de resultado.
    /// </summary>
    Task<IdentityVerificationSession> InitiateVerificationAsync(
        Guid entityId,
        IdentityVerificationMethod method,
        string entityName,
        string? email,
        CancellationToken ct = default);

    /// <summary>
    /// Consulta resultado da sessão — chamado pelo webhook ou polling.
    /// </summary>
    Task<IdentityVerificationResult> GetVerificationResultAsync(
        string sessionId,
        CancellationToken ct = default);

    /// <summary>
    /// Regista verificação presencial (feita por analista, sem sistema externo).
    /// </summary>
    Task RecordPresentialVerificationAsync(
        Guid entityId,
        string analystId,
        string documentReference,
        CancellationToken ct = default);
}

public record IdentityVerificationSession(
    string SessionId,
    string VerificationUrl,
    IdentityVerificationMethod Method,
    DateTime ExpiresAt);

public record IdentityVerificationResult(
    string SessionId,
    bool IsVerified,
    string? FailureReason,
    DateTime VerifiedAt,
    string? LivenessScore,   // Para videoconferência
    string? EidasLevel);     // "Substantial" | "High" | null
```

### 13.3 Comandos e eventos

```csharp
// KYC.Application/UseCases/Identity/
public record InitiateEntityVerificationCommand(
    Guid CaseId,
    Guid EntityId,
    IdentityVerificationMethod Method,
    string RequestedBy) : IRequest<IdentityVerificationSession>;

public record RecordVerificationResultCommand(
    Guid EntityId,
    string SessionId,
    bool IsVerified,
    string? FailureReason,
    string? EidasLevel) : IRequest<Unit>;

// Domain Event — emitido quando entidade verificada
public record EntityIdentityVerifiedEvent(
    Guid EntityId,
    Guid KycCaseId,
    IdentityVerificationMethod Method,
    DateTime VerifiedAt) : IDomainEvent;

// Domain Event — verificação falhou → bloqueia aprovação do caso
public record EntityIdentityVerificationFailedEvent(
    Guid EntityId,
    Guid KycCaseId,
    string FailureReason) : IDomainEvent;
```

### 13.4 Regra de negócio — bloqueio de aprovação

```csharp
// KYC.Domain/Entities/KycCase.cs — método de validação
public Result CanApprove()
{
    // Todos os UBOs e administradores devem ter identidade verificada
    var unverifiedEntities = EntitiesAnalysed
        .Where(e => e.Role is EntityRole.UBO or EntityRole.BoardMember or EntityRole.Proxy)
        .Where(e => e.VerificationStatus != IdentityVerificationStatus.Verified)
        .ToList();

    if (unverifiedEntities.Any())
        return Result.Failure(
            $"Aprovação bloqueada: {unverifiedEntities.Count} entidade(s) com identidade não verificada " +
            $"(Aviso BdP 1/2022, Art. 31.º): {string.Join(", ", unverifiedEntities.Select(e => e.Name))}");

    return Result.Success();
}
```

### 13.5 UI — CaseDetail.razor (adições)

```
// Secção "Verificação de Identidade" em CaseDetail.razor:
// - Por cada Entity com Role UBO/BoardMember/Proxy:
//   - Badge de estado: Verificado / Pendente / Falhado / Expirado
//   - Botão "Iniciar Verificação" (abre modal com escolha de método)
//   - Data e método de verificação (se verificado)
//   - Link para sessão do prestador (audit)
// - Aprovação bloqueada com aviso explícito se alguma entidade Pendente
```

---

## 14. Comunicação à UIF — Suspicious Activity Reports (SAR)

### 14.1 Contexto regulatório

A Lei n.º 83/2017 (Art. 52.º a 57.º) estabelece a obrigação de comunicação imediata à **Unidade de Informação Financeira (UIF)** do Banco de Portugal de operações suspeitas. O prazo máximo é **imediato** para casos urgentes. A plataforma deve suportar este fluxo de forma auditável.

### 14.2 Interfaces de Application Layer

```csharp
// [NOVO §4] KYC.Application/Interfaces/IUifReportingService.cs
public interface IUifReportingService
{
    /// <summary>
    /// Submete comunicação de operação suspeita à UIF.
    /// Retorna número de referência atribuído pela UIF.
    /// </summary>
    Task<UifSubmissionResult> SubmitSuspiciousActivityReportAsync(
        SuspiciousActivityReport report,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica estado de uma comunicação já submetida.
    /// </summary>
    Task<UifSubmissionStatus> GetSubmissionStatusAsync(
        string referenceNumber,
        CancellationToken ct = default);
}

public record SuspiciousActivityReport(
    Guid KycCaseId,
    string Nif,
    string CompanyName,
    string SuspicionDescription,           // Narrativa da suspeita
    IReadOnlyList<string> SignalSources,   // Fontes que originaram a suspeita
    decimal? AmountInvolved,
    string SubmittedByAnalystId,
    string SubmittedByAnalystName,
    DateTime DetectedAt);

public record UifSubmissionResult(
    bool IsSuccess,
    string? ReferenceNumber,
    string? ErrorMessage,
    DateTime SubmittedAt);
```

### 14.3 Comandos e fluxo

```csharp
// KYC.Application/UseCases/Sar/
public record SubmitSarCommand(
    Guid CaseId,
    string SuspicionDescription,
    string AnalystId,
    bool IsUrgent) : IRequest<UifSubmissionResult>;

public record MarkSarNotRequiredCommand(
    Guid CaseId,
    string AnalystId,
    string Justification) : IRequest<Unit>;

// Handler — SubmitSarCommandHandler
// 1. Valida que caso tem RiskLevel >= High ou sinal Critical não resolvido
// 2. Constrói SuspiciousActivityReport a partir do KycCase
// 3. Chama IUifReportingService.SubmitSuspiciousActivityReportAsync
// 4. Actualiza KycCase: SarStatus = Submitted, SarReferenceNumber, SarSubmittedAt
// 5. Regista AuditEntry: "SarSubmitted" com ActorId = analystId
// 6. Publica domain event SarSubmittedEvent (notifica supervisor)

// NOTA: Se IsUrgent = true, o handler deve executar sincrona e não via Service Bus.
// Prazo legal: comunicação imediata (Art. 54.º, Lei 83/2017).
```

### 14.4 Alertas automáticos — quando sugerir SAR

```csharp
// KYC.Application/Services/SarEligibilityEvaluator.cs
public class SarEligibilityEvaluator
{
    // Triggers automáticos para sugestão de SAR ao analista:
    public bool ShouldSuggestSar(KycCase kycCase) =>
        kycCase.Score?.Level >= RiskLevel.High
        && kycCase.RiskSignals.Any(s =>
            s.Severity == SignalSeverity.Critical
            && !s.IsConfirmed)
        || kycCase.EntitiesAnalysed.Any(e => e.IsSanctioned)
        || kycCase.EntitiesAnalysed.Any(e => e.IsOffshore
            && e.OwnershipPercentage >= 25m);
}
```

### 14.5 UI — fluxo SAR em CaseDetail.razor

```
// Quando SarEligibilityEvaluator.ShouldSuggestSar = true:
// - Aviso amarelo: "Este caso reúne condições para comunicação à UIF"
// - Botão "Comunicar à UIF" → abre modal com:
//   - Campo de narrativa de suspeita (obrigatório, mín. 200 chars)
//   - Checkbox "Urgente" (comunicação imediata vs. prazo normal)
//   - Pré-preenchimento automático de sinais críticos detectados
// - Após submissão: badge "SAR Submetido — Ref. {número}"
// - Botão "Marcar como Não Aplicável" (com justificação obrigatória)
// - SarStatus visível no topo do caso e no CaseList.razor
```

---

## 15. Diligência Reforçada (EDD) — PEPs, Alto Risco e Não Presencial

### 15.1 Contexto regulatório

A Lei n.º 83/2017 (Art. 36.º a 40.º) e o Aviso n.º 1/2022 (Anexo III) obrigam a **medidas de diligência reforçada (EDD)** em situações específicas. O sistema deve aplicar e registar o regime de DDC de forma auditável.

### 15.2 Motor de escalation DDC

```csharp
// KYC.Application/Services/DueDiligenceLevelEvaluator.cs
public class DueDiligenceLevelEvaluator
{
    private readonly ICustomerAcceptancePolicyRepository _policyRepo;

    public async Task<DueDiligenceLevelDecision> EvaluateAsync(
        string nif,
        IReadOnlyList<Entity> entities,
        decimal? creditAmount,
        RelationshipType relationshipType,
        CancellationToken ct = default)
    {
        var policy = await _policyRepo.GetActiveAsync(ct);

        // Diligência Simplificada — Art. 33.º Lei 83/2017
        if (relationshipType == RelationshipType.Occasional
            && creditAmount < policy.OccasionalThreshold   // €12.500
            && !entities.Any(e => e.IsPep)
            && !entities.Any(e => e.IsOffshore))
        {
            return new DueDiligenceLevelDecision(
                DueDiligenceLevel.Simplified,
                "Transação ocasional abaixo de limiar sem factores de risco (Art. 33.º Lei 83/2017)");
        }

        // Diligência Reforçada obrigatória — Art. 36.º Lei 83/2017
        var eddReasons = new List<string>();

        if (entities.Any(e => e.IsPep))
            eddReasons.Add("Presença de PEP (Art. 36.º, n.º 1, al. c) Lei 83/2017)");

        if (entities.Any(e => e.IsOffshore
            && policy.HighRiskJurisdictions.Contains(e.OffshoreJurisdiction)))
            eddReasons.Add("Jurisdição de alto risco FATF/UE (Art. 36.º, n.º 1, al. a))");

        if (!entities.Any(e => e.VerificationMethod == IdentityVerificationMethod.Presential))
            eddReasons.Add("Estabelecimento sem presença física (Art. 36.º, n.º 1, al. b))");

        if (eddReasons.Any())
            return new DueDiligenceLevelDecision(
                DueDiligenceLevel.Enhanced,
                string.Join("; ", eddReasons));

        return new DueDiligenceLevelDecision(
            DueDiligenceLevel.Standard,
            "Sem factores de escalation identificados");
    }
}

public record DueDiligenceLevelDecision(
    DueDiligenceLevel Level,
    string Justification);
```

### 15.3 Medidas adicionais de EDD implementadas

```csharp
// Quando DueDiligenceLevel = Enhanced, o pipeline deve adicionalmente:
// 1. Obter informação sobre origem dos fundos (campo FundsOriginDescription obrigatório)
// 2. Obter aprovação de supervisor antes de continuar (4-eyes obrigatório em EDD)
// 3. Monitorização reforçada pós-aprovação (NextReviewDue = 90 dias em vez de 365)
// 4. Verificação de identidade presencial ou eIDAS High (não aceita Simplified)
// 5. Pesquisa alargada de adverse media (últimos 5 anos em vez de 2)

// KYC.Domain/Entities/KycCase.cs — campos adicionais para EDD
public string? FundsOriginDescription { get; private set; }   // Obrigatório em EDD
public bool FundsOriginVerified { get; private set; }
public string? FundsOriginDocumentId { get; private set; }    // Ref. ao CaseDocument

// Regra de negócio:
public Result CanProceedWithEnhancedDd()
{
    if (DueDiligenceLevel != DueDiligenceLevel.Enhanced)
        return Result.Success();

    if (string.IsNullOrWhiteSpace(FundsOriginDescription))
        return Result.Failure("EDD requer declaração de origem de fundos (Art. 37.º Lei 83/2017)");

    return Result.Success();
}
```

---

## 16. Política de Aceitação de Clientes (PAC)

### 16.1 Contexto regulatório

O Aviso n.º 1/2022 (Art. 7.º e ss.) exige que a entidade documente e aplique uma **Política de Aceitação de Clientes** formal. A PAC define critérios de aceitação, recusa e escalation — e o sistema deve aplicá-la de forma auditável.

### 16.2 Modelo de dados

```csharp
// KYC.Domain/Entities/CustomerAcceptancePolicy.cs
public class CustomerAcceptancePolicy
{
    public Guid Id { get; private set; }
    public string Version { get; private set; }             // semver, ex: "2.0.0"
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public string ApprovedBy { get; private set; }          // UserId do Compliance Officer
    public bool IsActive { get; private set; }

    // Limiares financeiros
    public decimal OccasionalThreshold { get; private set; }    // €12.500 (instituições financeiras)
    public decimal EnhancedDdThreshold { get; private set; }    // Valor acima do qual EDD é obrigatório

    // Jurisdições
    public IReadOnlyList<string> HighRiskJurisdictions { get; private set; }    // FATF + lista UE
    public IReadOnlyList<string> ProhibitedJurisdictions { get; private set; }  // Embargos ONU/UE

    // Categorias de recusa automática
    public IReadOnlyList<string> ProhibitedCaeActivities { get; private set; }  // CAEs proibidos
    public bool BlockShellCompanies { get; private set; }
    public bool BlockOffshoreAboveThreshold { get; private set; }
    public decimal OffshoreBlockThreshold { get; private set; } // % de UBOs offshore

    // Calendário de revisão
    public int ReviewDaysLowRisk { get; private set; }      // Default: 365
    public int ReviewDaysMediumRisk { get; private set; }   // Default: 180
    public int ReviewDaysHighRisk { get; private set; }     // Default: 90
    public int ReviewDaysCriticalRisk { get; private set; } // Default: 30
}

// KYC.Infrastructure/Persistence/ — tabela: customer_acceptance_policies
// Imutável após aprovação — sem Update; nova versão = novo registo
```

### 16.3 Serviço de validação PAC

```csharp
// KYC.Application/Services/PolicyComplianceValidator.cs
public class PolicyComplianceValidator
{
    public PolicyValidationResult Validate(
        string nif,
        IReadOnlyList<Entity> entities,
        string? caeCode,
        CustomerAcceptancePolicy policy)
    {
        var violations = new List<string>();
        var autoRejected = false;

        // Jurisdições proibidas (embargo ONU/UE — Lei 97/2017)
        var prohibitedEntities = entities
            .Where(e => policy.ProhibitedJurisdictions.Contains(e.OffshoreJurisdiction))
            .ToList();
        if (prohibitedEntities.Any())
        {
            violations.Add($"Entidade em jurisdição sob embargo ONU/UE: " +
                $"{string.Join(", ", prohibitedEntities.Select(e => e.Name))}");
            autoRejected = true;
        }

        // CAE proibido
        if (caeCode != null && policy.ProhibitedCaeActivities.Contains(caeCode))
        {
            violations.Add($"Actividade económica proibida pela PAC: CAE {caeCode}");
            autoRejected = true;
        }

        // Shell companies
        if (policy.BlockShellCompanies
            && entities.All(e => e.Type == EntityType.Company)
            && !entities.Any(e => e.Role == EntityRole.UBO && e.Type == EntityType.Individual))
        {
            violations.Add("Estrutura sem beneficiário efectivo individual identificado (shell company)");
            autoRejected = true;
        }

        return new PolicyValidationResult(
            IsCompliant: !violations.Any(),
            AutoRejected: autoRejected,
            Violations: violations,
            PolicyVersion: policy.Version);
    }
}

public record PolicyValidationResult(
    bool IsCompliant,
    bool AutoRejected,
    IReadOnlyList<string> Violations,
    string PolicyVersion);
```

### 16.4 Integração no pipeline

```csharp
// Em StartKycCaseCommandHandler, antes de iniciar scan:
var policyResult = _policyValidator.Validate(nif, entities, cae, activePolicy);

if (policyResult.AutoRejected)
{
    // Rejeição imediata sem scan (regista no audit trail)
    await _auditService.RecordAsync(new AuditEntry(
        KycCaseId: caseId,
        Action: "AutoRejectedByPolicy",
        ActorId: "System",
        ActorType: "Agent",
        Details: JsonSerializer.Serialize(policyResult.Violations)));

    throw new PolicyViolationException(policyResult.Violations);
}
```

---

## 17. Revisão Periódica de Clientes — Lei 83/2017, Art. 35.º, n.º 6

### 17.1 Contexto regulatório

As entidades obrigadas devem **rever periodicamente** os perfis de risco dos clientes com relação de negócio activa. A frequência depende do nível de risco atribuído.

### 17.2 Job de revisão periódica

```csharp
// KYC.Infrastructure/BackgroundJobs/PeriodicReviewSchedulerJob.cs
// Executar diariamente às 07:00 UTC via Azure Functions Timer Trigger ou Hangfire

public class PeriodicReviewSchedulerJob(
    IKycCaseRepository caseRepo,
    IPublisher publisher,
    ILogger<PeriodicReviewSchedulerJob> logger)
{
    public async Task RunAsync(CancellationToken ct)
    {
        // Casos aprovados com revisão vencida ou a vencer em 14 dias
        var casesForReview = await caseRepo.GetCasesDueForReviewAsync(
            dueBy: DateTime.UtcNow.AddDays(14), ct);

        foreach (var kycCase in casesForReview)
        {
            await publisher.Publish(
                new TriggerPeriodicReviewNotification(kycCase.Id, kycCase.AssignedAnalystId),
                ct);

            logger.LogInformation(
                "Revisão periódica agendada para caso {CaseId} — risco {Level}, " +
                "vence em {DueDate:yyyy-MM-dd}",
                kycCase.Id, kycCase.Score?.Level, kycCase.NextReviewDue);
        }
    }
}
```

### 17.3 Comando e cálculo de NextReviewDue

```csharp
// KYC.Application/UseCases/Cases/TriggerPeriodicReviewCommand.cs
public record TriggerPeriodicReviewCommand(
    Guid CaseId,
    string InitiatedBy,    // "System" (scheduler) ou AnalystId (manual)
    string? ReviewNotes) : IRequest<Unit>;

// KYC.Domain/Entities/KycCase.cs — método de domínio
public void ScheduleNextReview(CustomerAcceptancePolicy policy)
{
    var daysUntilReview = Score?.Level switch
    {
        RiskLevel.Low      => policy.ReviewDaysLowRisk,      // 365
        RiskLevel.Medium   => policy.ReviewDaysMediumRisk,   // 180
        RiskLevel.High     => policy.ReviewDaysHighRisk,     // 90
        RiskLevel.Critical => policy.ReviewDaysCriticalRisk, // 30
        _                  => policy.ReviewDaysLowRisk
    };

    NextReviewDue = DateTime.UtcNow.AddDays(daysUntilReview);
    LastReviewedAt = DateTime.UtcNow;
}
```

### 17.4 Fluxo de re-triagem

```
Scheduler detecta caso vencido
  → TriggerPeriodicReviewNotification publicado
  → Notificação Blazor + email ao analista assignado
  → Analista inicia revisão: RerunKycCaseScreeningCommand (já implementado)
  → Pipeline executa novo scan completo
  → Novo score e relatório gerados
  → Se score escalou → EscalateToSupervisorCommand automático
  → ScheduleNextReview() chamado após aprovação da revisão
  → AuditEntry: "PeriodicReviewCompleted"
```

---

## 18. Reporte Anual ao BdP — Instrução n.º 8/2024 (RPB)

### 18.1 Contexto regulatório

A Instrução n.º 8/2024 do Banco de Portugal define o modelo de **Relatório de Prevenção do Branqueamento (RPB)** a enviar anualmente. Este relatório agrega métricas do sistema KYC para supervisão do BdP.

### 18.2 Modelo de dados agregados

```csharp
// KYC.Domain/Entities/AmlComplianceReport.cs
public class AmlComplianceReport
{
    public Guid Id { get; private set; }
    public int ReportingYear { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public string GeneratedBy { get; private set; }           // UserId Compliance Officer
    public AmlReportStatus Status { get; private set; }       // Draft | UnderReview | Submitted
    public string? BdpReferenceNumber { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    // Secção 1 — Estrutura organizacional AML
    public int TotalAmlAnalysts { get; private set; }
    public int TotalCasesProcessed { get; private set; }
    public int TotalCasesApproved { get; private set; }
    public int TotalCasesRejected { get; private set; }
    public int TotalCasesUnderReview { get; private set; }

    // Secção 2 — Distribuição de risco
    public int CasesLowRisk { get; private set; }
    public int CasesMediumRisk { get; private set; }
    public int CasesHighRisk { get; private set; }
    public int CasesCriticalRisk { get; private set; }

    // Secção 3 — Sinais e comunicações
    public int TotalRiskSignalsDetected { get; private set; }
    public int SanctionMatches { get; private set; }
    public int PepMatches { get; private set; }
    public int SarsSubmitted { get; private set; }            // Comunicações à UIF
    public int AssetFreezeNotifications { get; private set; }

    // Secção 4 — Diligência
    public int CasesSimplifiedDd { get; private set; }
    public int CasesStandardDd { get; private set; }
    public int CasesEnhancedDd { get; private set; }
    public int PeriodicReviewsCompleted { get; private set; }
    public int PeriodicReviewsOverdue { get; private set; }

    // Secção 5 — Tecnologia e IA
    public string PlatformVersion { get; private set; }
    public string AiModelsUsed { get; private set; }          // JSON: lista de modelos activos
    public int AutoApprovedCases { get; private set; }
    public int AiAssistedDecisions { get; private set; }
    public int HumanOverriddenDecisions { get; private set; }
}

public enum AmlReportStatus { Draft, UnderReview, Submitted }
```

### 18.3 Serviço de geração do RPB

```csharp
// [NOVO §4] KYC.Application/Interfaces/IAmlComplianceReportService.cs
public interface IAmlComplianceReportService
{
    /// <summary>
    /// Gera rascunho do RPB anual a partir dos dados do período.
    /// </summary>
    Task<AmlComplianceReport> GenerateAnnualReportAsync(
        int year,
        string requestedBy,
        CancellationToken ct = default);

    /// <summary>
    /// Exporta relatório no formato exigido pela Instrução BdP 8/2024.
    /// </summary>
    Task<Stream> ExportRpbAsync(
        Guid reportId,
        CancellationToken ct = default);

    /// <summary>
    /// Submete relatório ao BdP (via BPnet ou canal definido).
    /// </summary>
    Task<string> SubmitToBdpAsync(
        Guid reportId,
        string submittedBy,
        CancellationToken ct = default);
}

// Comando para geração
public record GenerateAmlReportCommand(int Year, string RequestedBy) : IRequest<Guid>;

// Página Admin/AmlReport.razor:
// - Botão "Gerar RPB {ano}" → chama GenerateAmlReportCommand
// - Visualização das métricas em tabelas e gráficos
// - Botão "Exportar" → download do formato BdP
// - Botão "Submeter ao BdP" (apenas KYC.Admin)
// - Histórico de relatórios anteriores com status
```

---

## 19. Congelamento de Ativos — Lei n.º 97/2017

### 19.1 Contexto regulatório

A Lei n.º 97/2017 regula a aplicação de medidas restritivas da ONU e UE. Quando um **match de sanção é confirmado**, a entidade obrigada deve notificar imediatamente o Banco de Portugal e proceder ao congelamento de ativos.

### 19.2 Fluxo de notificação obrigatória

```csharp
// [NOVO §4] KYC.Application/Interfaces/IAssetFreezeNotificationService.cs
public interface IAssetFreezeNotificationService
{
    /// <summary>
    /// Notifica BdP/AT de match de sanção confirmado.
    /// Prazo legal: imediato após confirmação (Lei 97/2017, Art. 8.º).
    /// </summary>
    Task<AssetFreezeNotificationResult> NotifyAsync(
        Guid kycCaseId,
        Guid entityId,
        string sanctionListSource,    // "OFAC" | "EU" | "UN" | "Portugal"
        string matchReference,
        string notifiedBy,
        CancellationToken ct = default);
}

public record AssetFreezeNotificationResult(
    bool IsSuccess,
    string? ConfirmationNumber,
    string? ErrorMessage,
    DateTime NotifiedAt);

// Trigger — em OverrideSignalCommandHandler:
// Quando analista confirma (IsConfirmed = true) sinal do tipo Sanction:
//   → Chamar IAssetFreezeNotificationService.NotifyAsync imediatamente
//   → Actualizar KycCase: AssetFreezeNotified = true, AssetFreezeNotifiedAt
//   → AuditEntry: "AssetFreezeNotificationSent" com ConfirmationNumber
//   → Status do caso → UnderReview (não pode auto-aprovar com sanção confirmada)
//   → Notificação imediata ao supervisor via SignalR
```

---

## 20. DPIA e Versioning do Motor de Scoring

### 20.1 DPIA — Avaliação de Impacto sobre Proteção de Dados (RGPD Art. 35.º)

A plataforma KYC realiza **decisões automatizadas com efeitos significativos** (decisões de crédito), o que torna a DPIA obrigatória nos termos do RGPD.

```csharp
// KYC.Domain/Entities/DpiaRecord.cs
public class DpiaRecord
{
    public Guid Id { get; private set; }
    public string Version { get; private set; }             // Ex: "1.2"
    public DateTime ApprovedAt { get; private set; }
    public string ApprovedBy { get; private set; }          // DPO ou responsável equivalente
    public DateTime NextReviewDue { get; private set; }     // Anual, no mínimo
    public string DocumentStoragePath { get; private set; } // Ref. ao documento DPIA completo
    public IReadOnlyList<string> ProcessingActivitiesCovered { get; private set; }
    // Ex: ["KYC_screening", "UBO_graph_analysis", "LLM_risk_scoring", "document_ingestion"]
    public IReadOnlyList<string> MitigationMeasures { get; private set; }
    public bool IsActive { get; private set; }
}
// Tabela: dpia_records — imutável após aprovação
// Admin/Settings.razor → secção "DPIA" com versão activa e data de próxima revisão
```

### 20.2 Versioning do Motor de Scoring

Para garantir **reproducibilidade auditável** de decisões passadas (exigência do BdP em inspeções), o sistema deve guardar um snapshot completo da configuração do scoring usada em cada caso.

```csharp
// KYC.Domain/Entities/ScoringEngineConfig.cs
public class ScoringEngineConfig
{
    public Guid Id { get; private set; }
    public string Version { get; private set; }             // semver: "2.1.0"
    public DateTime ActiveFrom { get; private set; }
    public DateTime? ActiveTo { get; private set; }
    public bool IsActive { get; private set; }

    // Configuração completa do motor nesta versão (JSON)
    public string LocalModelName { get; private set; }      // "qwen3.5:9b"
    public string LocalModelVersion { get; private set; }   // Hash/tag do modelo Ollama
    public string CloudModelName { get; private set; }      // "claude-sonnet-4-20250514"
    public string SystemPromptHash { get; private set; }    // SHA256 do system prompt de scoring
    public string WeightsJson { get; private set; }         // Ponderações por dimensão de score

    public string ApprovedBy { get; private set; }
    public DateTime ApprovedAt { get; private set; }
}
// Tabela: scoring_engine_configs — imutável após activação

// Em KycCase: ScoringEngineVersion guarda o Version da ScoringEngineConfig usada
// Em KycLlmEngine: injectar IScoringEngineConfigRepository para carregar config activa
// Em auditoria: dado o ScoringEngineVersion de um caso, reconstruir exactamente
//               os parâmetros usados — sem depender de modelo activo actual
```

### 20.3 Explainability — RGPD Art. 22.º e CRR III

O RGPD Art. 22.º proíbe decisões **exclusivamente automatizadas** com efeitos jurídicos significativos sem supervisão humana. Para crédito corporativo, o CRR III reforça a necessidade de fundamentação.

```csharp
// Regras de negócio implementadas:
// 1. Auto-Approve só é possível para RiskLevel.Low + Score < 30
//    → Todos os outros casos exigem aprovação humana (KYC.Supervisor)
// 2. AuditEntry regista sempre: "DecisionMadeBy: Human | System"
// 3. KycReport inclui secção "Limitações do Modelo de IA" gerada pelo LLM
// 4. O relatório final deve incluir (já previsto no §5.2, secção 9):
//    "9. Fontes Consultadas e Limitações"
//    → O LLM deve indicar explicitamente quais dimensões têm dados incompletos
// 5. Analista pode exercer direito de revisão (RGPD Art. 22.º, n.º 3):
//    → OverrideSignalCommand já implementado
//    → ApproveKycCaseCommand com notas obrigatórias
//
// ADICIONAR ao relatório narrativo (prompt §5.2):
//    "10. Declaração de Decisão Automatizada (RGPD Art. 22.º)
//         Esta análise foi gerada por sistema de IA e requer revisão humana
//         antes de qualquer decisão de crédito. O score é um instrumento
//         de apoio à decisão — não constitui decisão autónoma."
```

---

## 21. Alterações ao .cursorrules

Adicionar ao ficheiro `.cursorrules` existente (secção 12 do blueprint principal):

```
## Conformidade BdP (adicionado v1.1-compliance)

# Identificação e diligência
- SEMPRE verificar DueDiligenceLevelEvaluator antes de iniciar scan completo
- NUNCA aprovar caso com Entity.VerificationStatus != Verified para UBOs/BoardMembers
- SEMPRE registar LegalBasisRef em KycCase e DataCollectionBasis em Entity

# SAR e notificações obrigatórias
- NUNCA silenciar SarEligibilityEvaluator.ShouldSuggestSar = true sem registo em audit
- SEMPRE chamar IAssetFreezeNotificationService quando sinal Sanction é confirmado
- Prazo de notificação de congelamento de ativos: IMEDIATO (não via Service Bus)

# Revisão periódica
- SEMPRE chamar KycCase.ScheduleNextReview() após ApproveKycCaseCommand
- NUNCA omitir NextReviewDue em casos aprovados com RelationshipType = Ongoing

# Reproducibilidade e auditoria
- SEMPRE guardar ScoringEngineVersion no KycCase antes de iniciar scoring
- SEMPRE incluir ModelVersion no AuditEntry de decisões LLM
- NUNCA alterar ScoringEngineConfig após activação — criar nova versão

# DPIA e RGPD
- NUNCA implementar Auto-Approve para RiskLevel > Low sem aprovação humana
- SEMPRE incluir secção de explainability no relatório narrativo
- SEMPRE validar PolicyComplianceValidator antes de StartKycCaseCommand
```

---

## 22. Sequência de Implementação — Fases Regulatórias

Integrar nas fases existentes do blueprint:

### Fase 1 (Fundação) — adicionar:
```
[ ] Entidade CustomerAcceptancePolicy + migration
[ ] Entidade ScoringEngineConfig + migration  
[ ] Entidade DpiaRecord + migration
[ ] Entidade AmlComplianceReport + migration
[ ] Campos regulatórios adicionais em KycCase e Entity (ver §3 desta adenda)
[ ] LegalBasisAttribute nos enums de DocumentFactKey
[ ] DueDiligenceLevelEvaluator (sem dependências externas)
[ ] PolicyComplianceValidator
```

### Fase 2 (Core KYC Engine) — adicionar:
```
[ ] IIdentityVerificationService + implementação (integrar prestador PT: ex. DigitalSign)
[ ] Webhook/polling de resultado de verificação remota
[ ] InitiateEntityVerificationCommand + RecordVerificationResultCommand
[ ] Bloqueio de aprovação se entidade não verificada (KycCase.CanApprove)
[ ] DueDiligenceLevelEvaluator integrado no pipeline (após entity resolution)
[ ] PolicyComplianceValidator integrado em StartKycCaseCommandHandler
[ ] Campo FundsOriginDescription obrigatório para EDD
```

### Fase 3 (IA & Relatório) — adicionar:
```
[ ] IUifReportingService + SubmitSarCommand + MarkSarNotRequiredCommand
[ ] SarEligibilityEvaluator integrado em KycCasePipelineRunner
[ ] IAssetFreezeNotificationService + trigger em OverrideSignalCommandHandler
[ ] Secção 10 "Declaração de Decisão Automatizada" no prompt do relatório narrativo
[ ] ScoringEngineVersion gravado em cada KycCase antes de scoring
```

### Fase 4 (UI & Workflow) — adicionar:
```
[ ] UI de verificação de identidade em CaseDetail.razor (badge + botão por entidade)
[ ] Modal SAR com narrativa obrigatória
[ ] Badge SarStatus em CaseList.razor
[ ] Secção "Diligência Aplicada" em CaseDetail com justificação DDC
[ ] Página Admin/AmlReport.razor (geração RPB)
[ ] Página Admin/ScoringEngineConfig.razor (gestão de versões do motor)
```

### Fase 5 (Compliance) — adicionar:
```
[ ] PeriodicReviewSchedulerJob
[ ] IAmlComplianceReportService + GenerateAnnualReportAsync
[ ] Export no formato Instrução BdP 8/2024
[ ] Página Admin/DpiaRecord.razor (gestão de versões DPIA)
[ ] Testes de integração SAR, asset freeze, periodic review
[ ] Checklist de conformidade final: Lei 83/2017 + Aviso 1/2022 + Instrução 8/2024
```

---

*Adenda regulatória ao BLUEPRINT.md v1.1. Deve ser mantida em sincronismo com o documento principal. Versão 1.0 — Maio 2026.*
