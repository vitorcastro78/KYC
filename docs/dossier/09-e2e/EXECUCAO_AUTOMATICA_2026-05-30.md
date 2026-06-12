# Execução E2E automatizada — 2026-05-30

> Ambiente: máquina de desenvolvimento Cursor/agente.  
> **UI Blazor (cenários com cliques):** não executada nesta sessão.

## Infraestrutura

| Recurso | Estado |
|---------|--------|
| Docker / `docker compose` | **Indisponível** no PATH desta sessão |
| PostgreSQL `localhost:5433` | **Conexão recusada** — BD de homologação é **remota** (`195.179.193.136:5433`, `azureopsagent`) via `KYC_DB_CONNECTION` em `.env` |
| Testes `HomologationE2eAutomatedTests` (6) | **Falharam** — dependem de BD |

## Testes executados com sucesso (equivalente técnico)

### `KYC.Application.Tests` — 14/14 aprovados

| Cenário E2E doc | Teste | Resultado |
|-----------------|-------|-----------|
| #1 PAC CAE 92000 | `Policy_validator_rejects_prohibited_cae_at_start` | OK |
| #1 PAC arranque | `Creates_case_and_publishes_bus` | OK |
| #6 Nome legal manual | `Uses_manual_legal_name_when_resolution_is_fallback` | OK |
| #6 Sem nome → erro | `Throws_when_fallback_without_manual_legal_name` | OK |
| #7 SAR urgente falha | `Urgent_sar_api_failure_sets_pending_for_manual_registration` | OK |
| #3 SAR sucesso / fila | `Submit_sar_*` (3 testes) | OK |
| #8 Congelamento manual | `Register_manual_asset_freeze_after_confirmed_sanction` | OK |
| #8 Falha API BdP | `Override_sanction_freeze_failure_allows_manual_registration` | OK |
| #8 Sanção + freeze OK | `Override_sanction_confirm_notifies_freeze_and_under_review` | OK |

### `KYC.Integration.Tests` — 19/19 aprovados (compliance handlers)

| Cenário | Teste | Resultado |
|---------|-------|-----------|
| #1 PAC | `Start_case_with_prohibited_cae_throws_policy_violation` | OK |
| #3 SAR urgente | `Submit_sar_urgent_records_urgent_audit_entry` | OK |
| #5 RPB métricas | `Rpb_metrics_builder_aggregates_case_counts` | OK parcial |
| #2 Webhook HMAC | `Webhook_hmac_roundtrip_matches_validator` | OK |

### `KYC.Web.Integration.Tests` — 4/4 webhook HTTP

| Cenário | Teste | Resultado |
|---------|-------|-----------|
| #2 Identidade webhook | `Webhook_accepts_valid_hmac_and_verifies_party` | OK |
| #2 | `Webhook_verification_unblocks_can_approve_for_standard_dd` | OK |

## Não executado nesta sessão

| Cenário | Motivo | Como completar |
|---------|--------|----------------|
| #4 EDD 4-eyes | Sem teste E2E dedicado + UI | Manual em homologação ou novo teste Postgres |
| #5 RPB Admin export | Requer UI Admin + BD | Homologação manual |
| #6–#10 Postgres | BD offline | Ver secção abaixo |
| UI (todos) | Sem browser automation | Analista + prints → `dossier/` |

## Como executar E2E completo na sua máquina

1. Definir `.env` com a mesma `KYC_DB_CONNECTION` que `appsettings.json` / homologação:
   `Host=195.179.193.136;Port=5433;Database=azureopsagent;Username=kycdb;Password=...`
2. Na raiz do repo:
   ```powershell
   .\scripts\run-e2e-homologation.ps1
   # ou evidências completas:
   .\scripts\generate-e2e-evidence.ps1
   ```
3. Subir a app (`dotnet run --project src/KYC.Web`) e percorrer [E2E_HOMOLOGACAO.md](../../E2E_HOMOLOGACAO.md) cenários 1–10 na UI.
4. Preencher tabela de assinatura no E2E e anexar screenshots.

## Ficheiros adicionados para automação

- `tests/KYC.Web.Integration.Tests/HomologationE2eAutomatedTests.cs` — cenários #1, #6–#10 com PostgreSQL
- `scripts/run-e2e-homologation.ps1`

**Executor:** agente Cursor (sessão automática)  
**Próximo passo humano:** arrancar PostgreSQL + UI + assinatura compliance
