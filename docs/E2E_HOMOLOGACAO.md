# E2E — Homologação BdP

> Executar em ambiente de homologação com BD migrada (`20260529205723_BdpComplianceAndGtm`).

## Pré-requisitos

- `KYC_DB_CONNECTION` apontando para a BD de homologação
- Ollama acessível (`OLLAMA_ENDPOINT`)
- PAC activa (seed `ComplianceSeedHostedService` ou Admin)

## Cenários

### 1. PAC no arranque

1. Criar caso com CAE `92000` (jogos) → deve falhar com violação PAC
2. Criar caso válido → caso `InProgress` com `LegalBasisRef` preenchido

### 2. Identidade

1. No caso, secção Conformidade → «Verificar identidade» → escolher método
2. Webhook: `POST /api/identity/webhook` com `{ partyId, sessionId, verified: true }` e header `X-Webhook-Signature: sha256=<hmac>` (ver `docs/HOMOLOGACAO_RUNBOOK.md`)
3. Tentar aprovar sem UBO verificado → botão Aprovar desactivado + mensagem

### 3. SAR

1. Caso com risco alto → banner SAR → narrativa ≥200 chars → submeter
2. «Não aplicável» → justificação ≥50 chars → `SarStatus = NotRequired`
3. Lista de casos mostra badge SAR/DDC

### 4. EDD 4-eyes

1. Caso Enhanced + origem fundos + verificação presencial/CMD
2. Aprovar com segundo aprovador distinto → `SecondApproverId` gravado

### 5. RPB

1. Admin → Gerar RPB ano corrente
2. Export `?format=bdp` → XML Instr. 8/2024 (estrutura interna)
3. Marcar submetido → referência BdP no registo

## Evidências

Anexar ao dossier (`docs/dossier/`): prints UI, export XML RPB, entradas `AuditTrail` (SAR, IdentityVerified, RcbeDiscrepancyReported).

## Registo de execução (preencher em homologação)

| # | Cenário | Data | Executor | Resultado | Evidência (ficheiro) |
|---|---------|------|----------|-----------|----------------------|
| 1 | PAC arranque | | | ☐ OK ☐ Falha | `docs/dossier/01-pac/` |
| 2 | Identidade + webhook | | | ☐ OK ☐ Falha | `docs/dossier/06-identidade/` |
| 3 | SAR | | | ☐ OK ☐ Falha | `docs/dossier/05-sar-uif/` |
| 4 | EDD 4-eyes | | | ☐ OK ☐ Falha | |
| 5 | RPB Admin | | | ☐ OK ☐ Falha | `docs/dossier/04-rpb/` |

## Testes automatizados (pré-requisito)

```bash
dotnet test
```

Cobertura compliance relevante: `ComplianceHandlersIntegrationTests`, `ComplianceFlowTests`, `SarEligibilityTests`, `IdentityWebhookHttpTests`.
