# Métricas biométricas — FAR e FRR

## Definições

| Métrica | Significado |
|---------|-------------|
| **FAR** (False Accept Rate) | Impostores aceites como genuínos |
| **FRR** (False Reject Rate) | Genuínos rejeitados incorrectamente |

## API da plataforma

```http
GET /api/admin/compliance/metrics
Authorization: Bearer <token>  (roles: KYC.Admin, KYC.Auditor)
```

Resposta (`BiometricMetricsDto`):

- `Verified` / `Failed` — tentativas concluídas
- `WithLivenessScore` — sessões com score do prestador
- `AverageLivenessScore` — média quando numérico
- `FalseRejectRatePct` — operacional: `Failed / (Verified + Failed) × 100`
- `FalseAcceptRatePct` — **0** até relatório laboratorial prestador (não estimável só com dados operacionais)

## Relatório periódico (trimestral)

| Período | Tentativas | Verificados | Falhas | FRR % | FAR % (prestador) | Responsável |
|---------|------------|-------------|--------|-------|-------------------|-------------|
| Q_2026_1 | | | | | | Compliance |

Exportar JSON da API e arquivar em `docs/dossier/06-identidade/`.

## Limiares institucionais (definir)

| Métrica | Limiar máximo sugerido | Acção se excedido |
|---------|----------------------|-------------------|
| FRR operacional | _[ex: 5%]_ | Rever prestador / método |
| FAR (certificado prestador) | _[ex: 0,1%]_ | Escalar ao prestador |
