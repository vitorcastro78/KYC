# E2E — Homologação BdP

> Executar em ambiente de homologação com BD migrada (`20260529205723_BdpComplianceAndGtm`).  
> **Objectivo:** preencher a tabela §Registo e anexar evidências em `docs/dossier/`.

## Pré-requisitos

| Item | Verificação |
|------|-------------|
| BD | `KYC_DB_CONNECTION` em `.env` (ex. `Host=...;Port=5433;Database=azureopsagent;...`) — **não** assumir `localhost` se a BD for remota |
| BD | `dotnet ef database update` na BD de homologação |
| Ollama | `OLLAMA_ENDPOINT` acessível |
| PAC | Activa (seed `ComplianceSeedHostedService` ou Admin → Settings) |
| Utilizadores | Analista + Supervisor + Admin (Entra ou Identity dev) |
| Testes auto | `dotnet test` — **0 falhas** antes de E2E manual |

### Simular falha de API (contingência manual)

Para cenários 6–9, usar **uma** das opções:

- `Compliance:RequireLiveIntegrations=true` sem `Uif__BaseUrl` / `BdpAssetFreeze__BaseUrl` / `IdentityVerification__BaseUrl`, **ou**
- URLs inválidas em staging, **ou**
- Desligar temporariamente o mock/serviço de integração.

---

## Cenários obrigatórios

### 1. PAC no arranque

1. **Casos → Novo** com CAE `92000` → mensagem de violação PAC; caso **não** criado.
2. Caso válido (NIF real ou teste) → `InProgress`, `LegalBasisRef` preenchido.
3. **Evidência:** print do erro PAC + print caso criado; audit `CaseStarted`.

**Pasta:** `docs/dossier/01-pac/`

---

### 2. Identidade (API + webhook)

1. Secção **Conformidade** → «Verificar identidade» (vídeo/CMD/presencial).
2. Webhook: `POST /api/identity/webhook` com body `{ "partyId", "sessionId", "verified": true }` e `X-Webhook-Signature: sha256=<hmac>` — ver [HOMOLOGACAO_RUNBOOK.md](HOMOLOGACAO_RUNBOOK.md).
3. Badge **Verificado** na parte; tentar **Aprovar** com outro UBO ainda pendente → botão desactivado + `CanApproveMessage`.

**Pasta:** `docs/dossier/06-identidade/`

---

### 3. SAR (submissão e não aplicável)

1. Caso risco alto / sinal crítico → banner SAR → narrativa ≥200 caracteres → submeter (não urgente ou urgente com API OK).
2. Caso sem obrigação → «SAR não aplicável» → justificação ≥50 caracteres → `SarStatus = NotRequired`.
3. Lista de casos: badges SAR e DDC correctos.

**Pasta:** `docs/dossier/05-sar-uif/`

---

### 4. EDD 4-eyes

1. Caso `DueDiligenceLevel = Enhanced` → guardar **origem dos fundos**.
2. Verificação presencial ou CMD nas partes obrigatórias.
3. **Aprovar** com segundo aprovador **distinto** → `SecondApproverId` na BD / audit.

**Pasta:** `docs/dossier/08-audit/` (extract caso EDD)

---

### 5. RPB (Admin)

1. Admin → Gerar RPB ano corrente.
2. Export `?format=bdp` → XML (estrutura interna Instr. 8/2024).
3. Marcar submetido → referência BdP no registo.

**Pasta:** `docs/dossier/04-rpb/`

---

## Cenários de contingência manual (APIs indisponíveis)

### 6. Denominação social no arranque (sem RCBE/GLEIF)

1. NIF sem correspondência em RCBE/GLEIF (ou ambiente sem endpoint).
2. Preview em **Novo caso** mostra aviso «indique denominação manual».
3. Tentar **Iniciar** sem nome → erro; preencher **Denominação social (manual)** → caso criado com esse nome (não `Entidade {NIF}`).
4. Parte tomador com o mesmo nome.

**Pasta:** `docs/dossier/01-pac/` ou `docs/dossier/09-e2e/`

---

### 7. SAR urgente — falha UIF → registo manual

1. API UIF indisponível (ver pré-requisitos simulação).
2. Submeter SAR **urgente** → toast de aviso; `SarStatus = Pending`.
3. Secção SAR: alerta + campo **Registo manual UIF** → introduzir ref. (≥5 chars) → `SarSubmitted` + audit `SarManualRegistered`.
4. Consultar audit `SarApiFailedPendingManual`.

**Pasta:** `docs/dossier/05-sar-uif/`

---

### 8. Congelamento BdP — falha API → registo manual

1. Caso com sinal **Sanção** → em **Sinais de risco** → **Confirmar correspondência**.
2. Com API BdP em falha: alerta vermelho «Congelamento BdP pendente»; caso `UnderReview`; audit `AssetFreezeNotificationFailed`.
3. Introduzir ref. BdP manual → `AssetFreezeNotified` + audit `AssetFreezeManualRegistered`.

**Pasta:** `docs/dossier/07-congelamento/`

---

### 9. Identidade — verificação manual (sem API)

1. Parte UBO/órgão social ainda pendente; prestador indisponível.
2. **Verificado manualmente (sem API)** → justificação ≥20 caracteres + ref. documento opcional.
3. Método `ThirdPartyReliance`, estado Verificado; audit `IdentityManualVerified`.
4. Aprovação desbloqueada para essa parte (se restantes OK).

**Pasta:** `docs/dossier/06-identidade/`

---

### 10. Sinais de triagem — manual + confirmação

1. **Registar sinal manual** (tipo, severidade, fonte, descrição ≥10) — fonte gravada como `Manual:...`.
2. Sinal automático pendente → **Confirmar** ou **Descartar** no cartão do sinal.
3. Timeline / audit com `ManualRiskSignalAdded` e `AnalystOverride`.

**Pasta:** `docs/dossier/09-e2e/`

---

## Evidências mínimas por dossier

| Pasta | Conteúdo mínimo |
|-------|-----------------|
| `01-pac/` | Print PAC activa + teste CAE 92000 rejeitado |
| `04-rpb/` | XML + JSON export + ref. submissão |
| `05-sar-uif/` | SAR submetido OU manual pós-falha API |
| `06-identidade/` | Webhook OK + verificação manual (screenshot) |
| `07-congelamento/` | Confirmação API OU ref. manual pós-sanção |
| `08-audit/` | SQL ou export `audit_entries` do caso teste |
| `09-e2e/` | Esta tabela assinada (PDF ou scan) |
| `10-seguranca/` | [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) preenchido |

---

## Registo de execução (preencher em homologação)

| # | Cenário | Data | Executor | Resultado | Evidência |
|---|---------|------|----------|-----------|-----------|
| 1 | PAC arranque | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Falha | `09-e2e/test-results-20260531-021829.trx` — [REGISTO_EXECUCAO_20260531-021829.md](dossier/09-e2e/REGISTO_EXECUCAO_20260531-021829.md) |
| 2 | Identidade + webhook | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Falha | `06-identidade/02-*-20260531-024650.png`; caso `943cb0b0-3fb3-4ca6-974f-421a06063d2a` — [REGISTO_UI_CENARIOS_2-5_20260531-024650.md](dossier/09-e2e/REGISTO_UI_CENARIOS_2-5_20260531-024650.md) |
| 3 | SAR | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Falha | `05-sar-uif/03-*-20260531-024650.png`; casos SAR `8279989f-…` + identidade (não aplicável) |
| 4 | EDD 4-eyes | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Falha | `08-audit/04-*-20260531-024650.png`; caso `58c21877-ec18-4b01-9351-22cefefe6ee9` |
| 5 | RPB Admin | 2026-05-31 | Playwright UI (`admin@kyc.local`) | ☑ OK ☐ Falha | `04-rpb/05-*-20260531-024650.png`, `05-rpb-export-bdp-20260531-024650.xml` |
| 6 | Nome legal manual (arranque) | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Falha | `09-e2e/audit-export-*.json`, trx — [REGISTO_EXECUCAO_20260531-021829.md](dossier/09-e2e/REGISTO_EXECUCAO_20260531-021829.md) |
| 7 | SAR manual pós-falha UIF | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Falha | `05-sar-uif/`, trx E2E-07 |
| 8 | Congelamento manual BdP | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Falha | `07-congelamento/`, trx E2E-08 |
| 9 | Identidade manual | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Falha | `06-identidade/`, trx E2E-09 |
| 10 | Sinais manuais + override | 2026-05-31 | `HomologationE2eAutomatedTests` | ☑ OK ☐ Falha | `09-e2e/`, trx E2E-10 |

**Ambiente:** `http://localhost:5299` · BD `195.179.193.136:5433` (`azureopsagent`) · IDs UI: [e2e-ui-cases.json](dossier/09-e2e/e2e-ui-cases.json)

**Assinatura compliance:** _________________________ Data: __________

---

## Testes automatizados (pré-requisito)

```bash
dotnet test
```

Pacotes relevantes: `ComplianceFlowTests`, `ComplianceHandlersIntegrationTests`, `StartKycCaseCommandHandlerTests`, `SarEligibilityTests`, `IdentityWebhookHttpTests`, `UboGraphViewBuilderTests`.

---

## Execução automatizada (agente / CI local)

Com `KYC_DB_CONNECTION` definida em `.env` (alinhada com `ConnectionStrings:KycDatabase` em `appsettings.json`):

```powershell
# .env: KYC_DB_CONNECTION=Host=...;Port=5433;Database=azureopsagent;...
.\scripts\generate-e2e-evidence.ps1
```

Gera: testes `HomologationE2eAutomatedTests` (7), export JSON em `docs/dossier/`, HTTP + webhook, registo `docs/dossier/09-e2e/REGISTO_EXECUCAO_*.md`.

**UI (cenários 2–5):**

```powershell
.\scripts\run-e2e-ui-scenarios-2-5.ps1 -SkipAppStart   # com KYC.Web já em http://localhost:5299
```

Prepara casos (`E2E-UI-PREP`), executa Playwright, screenshots em `04-rpb/`, `05-sar-uif/`, `06-identidade/`, `08-audit/` e registo `REGISTO_UI_CENARIOS_2-5_*.md`.

---

## Após E2E

1. Pen test: [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md) → `10-seguranca/`.
2. Credenciais reais X2–X4 em staging (validar fluxos **sem** só contingência manual).
3. Actualizar [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md) com data de homologação (secção abaixo).
