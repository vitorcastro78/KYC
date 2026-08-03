# Retenção de dados — RGPD (5–7 anos)

## Base legal

Lei 83/2017 (AML) + RGPD Art. 6.º(1)(c) obrigação legal + política interna de retenção.

## Configuração técnica

```json
"DataRetention": {
  "EnableHostedService": true,
  "RejectedCaseRetentionYears": 5,
  "ApprovedCaseRetentionYears": 7,
  "AnonymizeRejectedAfterRetention": true,
  "MarkApprovedCasesPastRetention": true
}
```

## Comportamento do job (`DataRetentionHostedService`)

| Tipo caso | Após período | Acção |
|-----------|--------------|-------|
| Rejeitado | > 5 anos desde `CompletedAt` | Anónimização (`CompanyName=ANON`, `Nif=000000000`) |
| Aprovado | > 7 anos | Audit `RetentionReviewDue` — revisão arquivo legal (sem apagar automaticamente) |

## Activar em produção

1. `.env`: `DataRetention__EnableHostedService=true`
2. Confirmar logs diários ~02:00 UTC
3. Validar caso teste rejeitado antigo (homologação)

## Excepções

- **Legal hold:** suspender anónimização via flag processo compliance (procedimento manual BD — documentar ticket)
- **SAR submetido:** retenção alinhada com obrigação UIF

## Evidência homologação

- [ ] Config prod activa
- [ ] Log job com contagens
- [ ] Entrada audit `RetentionReviewDue` em caso simulado

**Estado matriz 6.3:** 🟡 → ✅ após activação prod + evidência.
