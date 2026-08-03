# Plano de Continuidade de Negócio (PCN) — Serviço KYC

> **Versão:** 1.0 (rascunho) · **BIA owner:** Compliance / TI

## 1. Serviços críticos

| Serviço | RTO alvo | RPO alvo | Prioridade |
|---------|----------|----------|------------|
| KYC.Web (triagem casos) | 4 h | 1 h | P1 |
| PostgreSQL (casos + audit) | 4 h | 15 min | P1 |
| Ollama (scoring/relatório) | 8 h | N/A | P2 |
| Workers (sanções OFAC/EU) | 24 h | 24 h | P3 |

## 2. Cenários de interrupção

1. Indisponibilidade aplicação (crash, deploy falhado)
2. Indisponibilidade BD
3. Indisponibilidade Ollama (degradação — triagem manual)
4. Indisponibilidade prestador identidade (fallback presencial)

## 3. Estratégias

- **Aplicação:** `docker-compose.prod.yml` restart; imagem versionada em registry
- **BD:** backup contínuo + restore (ver PRD)
- **Degradação:** analistas continuam revisão manual; SAR manual UIF (`RegisterManualUifReferenceCommand`)

## 4. Equipa de resposta

| Papel | Contacto | Responsabilidade |
|-------|----------|------------------|
| Incident commander | _[TI]_ | Coordenação |
| DBA | _[TI]_ | Restore BD |
| Compliance lead | _[Compliance]_ | Comunicação BdP/UIF se SLA regulatório |

## 5. Comunicação

- Interna: canal incidente institucional
- Regulador: conforme obrigação Lei 83/2017 se interrupção > SLA acordado

## 6. Testes PCN

| Data | Tipo | Resultado | Acções |
|------|------|-----------|--------|
| | Mesa redonda | | |
| | Simulação técnica | | |

## 7. Aprovação

_COMEX — data e assinatura_
