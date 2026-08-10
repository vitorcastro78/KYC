# Dossier de homologação BdP — KYC Platform

Pasta para evidências do go-live regulatório (Instrução BdP 8/2024, Lei 83/2017, Aviso 1/2022).

## Estrutura

| Pasta | Cenários E2E | Evidências mínimas |
|-------|--------------|-------------------|
| `01-pac/` | #1, #6 | PAC activa; rejeição CAE 92000; nome legal manual |
| `02-dpia/` | — | DPIA activa + PDF |
| `03-scoring/` | — | Versão scoring + hash prompt |
| `04-rpb/` | #5 | XML BdP + JSON + ref. submissão |
| `05-sar-uif/` | #3, #7 | SAR submetido; SAR manual pós-falha API |
| `06-identidade/` | #2, #9 | Webhook HMAC; verificação manual |
| `07-congelamento/` | #8 | Ref. API ou manual pós-sanção |
| `08-audit/` | #4 | Export audit caso EDD |
| `09-e2e/` | #10 | Tabela E2E assinada; sinais manuais |
| `10-seguranca/` | — | Pen test preenchido |

## Como executar

1. `dotnet test` (0 falhas).
2. Seguir [OPERACOES_E_HOMOLOGACAO.md](../pt/OPERACOES_E_HOMOLOGACAO.md) §4 — **10 cenários** (inclui contingência manual).
3. Guardar ficheiros com data: `RPB-2025-20260530.xml`, `sar-manual-20260530.png`, etc.
4. Marcar checklist em [OPERACOES_E_HOMOLOGACAO.md](../pt/OPERACOES_E_HOMOLOGACAO.md) §5 e registo E2E §4.12.

## Simular APIs em falha

Homologação dos cenários 6–9: `Compliance:RequireLiveIntegrations=true` sem URLs UIF/BdP/identidade, ou endpoints inválidos em staging.

## Responsáveis

| Área | Owner |
|------|--------|
| E2E + dossier | Analista AML + QA |
| RPB | Admin `KYC.Admin` |
| Pen test | Infra + auditor |
| PAC / compliance | Equipa compliance |
