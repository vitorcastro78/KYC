# RTO e RPO — Métricas e registo de simulações

## Objectivos acordados

| ID | Serviço | RTO (horas) | RPO (minutos) | Método de medição |
|----|---------|-------------|---------------|-------------------|
| S1 | KYC.Web | 4 | 60 | Tempo desde incidente até `/health` OK |
| S2 | PostgreSQL | 4 | 15 | Tempo restore + integridade audit trail |
| S3 | Documentos | 8 | 1440 | Restore volume + checksum amostra |
| S4 | Ollama | 8 | — | Tempo até scoring disponível |

## Registo de simulações (preencher em homologação/prod)

| # | Data | Cenário | RTO medido | RPO medido | Objectivo cumprido | Evidência |
|---|------|---------|------------|------------|-------------------|-----------|
| 1 | | Restore BD backup D-1 | | | ☐ Sim ☐ Não | `dossier/09-e2e/` |
| 2 | | Failover app (redeploy) | | | ☐ Sim ☐ Não | |
| 3 | | Perda Ollama — modo degradado | | | ☐ Sim ☐ Não | |

## Estado actual

**🔴 Pendente** — objectivos definidos; simulações não executadas. Após 1.ª simulação, actualizar [MATRIZ_REQUISITOS_INSTITUCIONAIS.md](../MATRIZ_REQUISITOS_INSTITUCIONAIS.md) §3.3 para 🟡/✅.
