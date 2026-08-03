# Plano de Recuperação de Desastres (PRD) — KYC AI Platform

> **Versão:** 1.0 · Complementa [PCN_PLANO_CONTINUIDADE_NEGOCIO.md](PCN_PLANO_CONTINUIDADE_NEGOCIO.md)

## 1. Objectivos de recuperação

| Componente | RTO | RPO | Procedimento |
|------------|-----|-----|--------------|
| PostgreSQL KYC | 4 h | 15 min | Restore backup + `dotnet ef database update` |
| KYC.Web + Workers | 2 h | 0 (stateless) | Redeploy imagem Docker última estável |
| Documentos `Data/cases/` | 8 h | 24 h | Restore volume backup |
| Ollama | 8 h | N/A | Reinstalar modelo Qwen |

## 2. Backups

| Dado | Frequência | Retenção | Localização |
|------|------------|----------|-------------|
| PostgreSQL full | Diário 02:00 UTC | 30 dias | _[S3/Azure Blob UE]_ |
| PostgreSQL WAL | Contínuo | 7 dias | _[idem]_ |
| Volumes Docker / Data | Diário | 30 dias | _[idem]_ |

Comando referência:

```bash
pg_dump -Fc -h <host> -U <user> azureopsagent > kyc-backup-$(date +%Y%m%d).dump
```

## 3. Procedimento de restore (resumo)

1. Provisionar host/VM de DR na região UE secundária
2. Restaurar PostgreSQL: `pg_restore -d kyc ...`
3. Aplicar migrations se necessário
4. `docker compose -f docker-compose.prod.yml up -d`
5. Validar `/health`, caso teste E2E cenário 1
6. Comunicar reativação à equipa compliance

## 4. Testes PRD

| Data | Âmbito | Duração real RTO | RPO medido | Aprovado |
|------|--------|------------------|------------|----------|
| | Restore BD homologação | | | ☐ |

**Frequência mínima:** 1×/ano.

## 5. Critérios de activação DR

- Perda total datacenter primário
- Corrupção BD irreversível sem PITR
- Ransomware com impacto em backups < 24h
