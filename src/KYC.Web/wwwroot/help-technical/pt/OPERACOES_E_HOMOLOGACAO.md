# Operações e Homologação — KYC AI Platform

> Documento unificado: deploy, runbooks, testes E2E, checklists regulatórios e segurança, dossier de evidências.
> Pacote de idioma: **português**. Hub: [`../README.md`](../README.md).

---

## 1. Deploy on-prem

Layout alinhado a [ContextMemory](https://github.com/Kortexio/ContextMemory): `docker-compose.yml` (build), `docker-compose.ghcr.yml` (imagens), `docker-compose.contextmemory.yml` (CM opcional via GHCR), `.env.example`, `scripts/docker-run.*`.

### 1.1 Pré-requisitos

- Docker e Docker Compose
- ContextMemory acessível (`CONTEXT_MEMORY_BASE_URL`, ex. `http://localhost:5100`) — self-host via [Kortexio/ContextMemory](https://github.com/Kortexio/ContextMemory) ou o overlay abaixo
- Ficheiro `.env` (copiar de `.env.example`) — **nunca commitar**

### 1.2 Arranque (build local)

```bash
cp .env.example .env
# Editar POSTGRES_PASSWORD, RABBITMQ_PASSWORD, KYC_ADMIN_PASSWORD, CONTEXT_MEMORY_*

docker compose up --build -d
# ou com ContextMemory a partir do GHCR:
# docker compose -f docker-compose.yml -f docker-compose.contextmemory.yml up --build -d
# ou: ./scripts/docker-run.sh --build
# ou: .\scripts\docker-run.ps1 -Build
```

### 1.3 Arranque (imagens GHCR)

```bash
docker compose -f docker-compose.ghcr.yml up -d
# ou: ./scripts/docker-run.sh
```

### 1.4 Apenas base de dados (dotnet run no host)

```bash
docker compose -f docker-compose.db.yml up -d
```

### 1.5 Migrations

As migrations **não** correm no arranque da app. Aplicar explicitamente:

```bash
# Host (connection string para Postgres)
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web

# Ou via container / appliance
dotnet KYC.Web.dll --migrate-only
# Docker Compose:
docker compose run --rm --no-deps kyc-web --migrate-only
```

### 1.6 Verificação pós-deploy

| Verificação | Comando / URL |
|-------------|---------------|
| UI | `http://localhost:8080` (ou `KYC_WEB_PORT`) |
| Health | `GET /health` |
| Admin | `KYC_ADMIN_EMAIL` / `KYC_ADMIN_PASSWORD` |
| Workers | Volumes `Data/ofac`, `Data/eu-fsf` após arranque |

### 1.7 Variáveis compliance críticas

```env
IDENTITY_VERIFICATION_WEBHOOK_SECRET=...
IdentityVerification__BaseUrl=...
Uif__BaseUrl=...
Uif__ApiKey=...
BdpAssetFreeze__BaseUrl=...
Compliance__RequireLiveIntegrations=true
```

---

## 2. Runbook — Homologação técnica

### 2.1 Base de dados

```powershell
$env:KYC_DB_CONNECTION="Host=...;Port=5433;Database=...;Username=...;Password=..."
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

Confirmar trigger audit:

```sql
SELECT tgname FROM pg_trigger WHERE tgname = 'tr_audit_entries_immutable';
```

```powershell
dotnet test tests/KYC.Web.Integration.Tests --filter AuditImmutability
```

### 2.2 Webhook identidade (HMAC)

Configurar `IdentityVerification:WebhookSecret` ou `IDENTITY_VERIFICATION_WEBHOOK_SECRET`.

```powershell
$body = '{"partyId":"<GUID>","sessionId":"sess-abc","verified":true}'
$secret = "seu-secret"
$hash = [BitConverter]::ToString([System.Security.Cryptography.HMACSHA256]::HashData(
  [Text.Encoding]::UTF8.GetBytes($secret),
  [Text.Encoding]::UTF8.GetBytes($body))).Replace("-","").ToLower()
Invoke-RestMethod -Method Post -Uri "https://<host>/api/identity/webhook" `
  -Headers @{ "X-Webhook-Signature" = "sha256=$hash" } `
  -ContentType "application/json" -Body $body
```

### 2.3 Testes automatizados

```powershell
dotnet test
dotnet test tests/KYC.Web.Integration.Tests
```

Cobertura: `ComplianceHandlersIntegrationTests`, `ComplianceFlowTests`, `SarEligibilityTests`, `IdentityWebhookHttpTests`, `UboGraphViewBuilderTests`.

### 2.4 CI

Push/PR para `main`, `develop` ou `feature/*` → `.github/workflows/ci.yml`.

---

## 3. Runbook — PAC (Política de Aceitação de Clientes)

**Base legal:** Lei 83/2017, Art. 24.º

### Versão activa

1. Admin → **Settings** — cartão «PAC activa»
2. BD: `customer_acceptance_policies` com `IsActive = true`
3. Seed: `ComplianceSeedHostedService` cria PAC `1.0.0` se vazia

### Nova versão

1. Admin → Settings → versão (ex. `1.1.0`) → **Activar**
2. `CreateCustomerAcceptancePolicyCommand` desactiva anterior
3. Novos casos: `LegalBasisRef` = `PAC/{versão}/Lei83/2017-Art24`

### Regras no arranque

| Regra | Efeito |
|-------|--------|
| CAE em lista proibida | `PolicyViolationException` |
| Jurisdição proibida / offshore | Auto-reject ou violação |
| PEP na estrutura | Auto-reject (config PAC) |

**Evidência:** `docs/dossier/01-pac/`

---

## 4. Cenários E2E — Homologação BdP

> Ambiente com BD migrada. Objectivo: preencher a tabela §4.12 e anexar evidências em `docs/dossier/`.

### 4.1 Pré-requisitos

| Item | Verificação |
|------|-------------|
| BD | `KYC_DB_CONNECTION` + `dotnet ef database update` |
| ContextMemory | `CONTEXT_MEMORY_BASE_URL` acessível |
| PAC | Activa (seed ou Admin → Settings) |
| Utilizadores | Analista + Supervisor + Admin |
| Testes auto | `dotnet test` — 0 falhas antes de E2E manual |

**Simular falha de API (cenários 6–9):** `Compliance:RequireLiveIntegrations=true` sem URLs UIF/BdP/identidade, URLs inválidas, ou mock desligado.

### 4.2 Cenário 1 — PAC no arranque

1. Caso CAE `92000` (jogos) → falha PAC; caso **não** criado
2. Caso válido → `InProgress` + `LegalBasisRef`
3. Evidência: print erro PAC + caso criado; audit `CaseStarted` → `docs/dossier/01-pac/`

### 4.3 Cenário 2 — Identidade (Aviso 1/2022)

1. Conformidade → «Verificar identidade» → método
2. Webhook HMAC (§2.2) ou polling → `Verified`
3. Aprovar com outro UBO pendente → botão desactivado + `CanApproveMessage`
4. Pasta: `docs/dossier/06-identidade/`

### 4.4 Cenário 3 — SAR / UIF

1. Caso alto risco → banner SAR → narrativa ≥200 → submeter
2. «Não aplicável» → justificação ≥50 → `NotRequired`
3. Lista casos → badges SAR/DDC
4. Pasta: `docs/dossier/05-sar-uif/`

### 4.5 Cenário 4 — EDD 4-eyes

1. Enhanced + origem fundos + verificação
2. Aprovar com segundo supervisor → `SecondApproverId`
3. Pasta: `docs/dossier/08-audit/`

### 4.6 Cenário 5 — RPB

1. Admin → Gerar RPB ano corrente
2. Export `?format=bdp` → XML
3. Submeter → referência BdP
4. Pasta: `docs/dossier/04-rpb/`

### 4.7 Cenário 6 — Denominação social manual (sem RCBE/GLEIF)

1. NIF sem RCBE/GLEIF (ou endpoint em falha)
2. Preview em **Novo caso** → aviso «indique denominação manual»
3. Iniciar sem nome → erro; preencher denominação manual → caso criado
4. Pasta: `docs/dossier/01-pac/` ou `09-e2e/`

### 4.8 Cenário 7 — SAR urgente → Pending → ref. UIF manual

1. API UIF indisponível
2. Submeter SAR **urgente** → `Pending` + toast
3. Registo manual UIF (ref. ≥5) → `SarSubmitted` + audit `SarManualRegistered`
4. Pasta: `docs/dossier/05-sar-uif/`

### 4.9 Cenário 8 — Congelamento BdP manual pós-sanção

1. Confirmar correspondência de sanção
2. API BdP em falha → alerta + `AssetFreezeNotificationFailed`
3. Ref. BdP manual → `AssetFreezeNotified` + `AssetFreezeManualRegistered`
4. Pasta: `docs/dossier/07-congelamento/`

### 4.10 Cenário 9 — Identidade manual (sem API)

1. Parte UBO pendente; prestador indisponível
2. **Verificado manualmente** → justificação ≥20 + ref. opcional
3. Audit `IdentityManualVerified`
4. Pasta: `docs/dossier/06-identidade/`

### 4.11 Cenário 10 — Sinais manuais + confirmar/descartar

1. Registar sinal manual (descrição ≥10)
2. Confirmar ou descartar sinal automático
3. Audit `ManualRiskSignalAdded` / `AnalystOverride`
4. Pasta: `docs/dossier/09-e2e/`

### 4.12 Registo de execução

| # | Cenário | Data | Executor | Resultado | Evidência |
|---|---------|------|----------|-----------|-----------|
| 1 | PAC arranque | | | ☐ OK ☐ Falha | `01-pac/` |
| 2 | Identidade + webhook | | | ☐ OK ☐ Falha | `06-identidade/` |
| 3 | SAR | | | ☐ OK ☐ Falha | `05-sar-uif/` |
| 4 | EDD 4-eyes | | | ☐ OK ☐ Falha | `08-audit/` |
| 5 | RPB Admin | | | ☐ OK ☐ Falha | `04-rpb/` |
| 6 | Nome legal manual | | | ☐ OK ☐ Falha | `09-e2e/` |
| 7 | SAR manual pós-falha UIF | | | ☐ OK ☐ Falha | `05-sar-uif/` |
| 8 | Congelamento manual BdP | | | ☐ OK ☐ Falha | `07-congelamento/` |
| 9 | Identidade manual | | | ☐ OK ☐ Falha | `06-identidade/` |
| 10 | Sinais manuais + override | | | ☐ OK ☐ Falha | `09-e2e/` |

**Assinatura compliance:** _________________________ Data: __________

### 4.13 Execução automatizada

```powershell
.\scripts\generate-e2e-evidence.ps1
.\scripts\run-e2e-ui-scenarios-2-5.ps1 -SkipAppStart   # KYC.Web em http://localhost:5299
```

---

## 5. Checklist regulatório — Capacidades (Lei 83/2017, BdP, RGPD)

> Estado **código** — evidência de execução em homologação é separada.

### Lei 83/2017 — AML/CFT

- [x] PAC versionada activa no arranque do caso
- [x] DDC Simplificada / Standard / Reforçada
- [x] EDD: origem de fundos antes de aprovação
- [x] Revisão periódica (`NextReviewDue`)
- [x] SAR/UIF com audit trail

### Aviso BdP 1/2022

- [x] Verificação identidade (webhook + polling + UI)
- [x] Bloqueio aprovação se UBO/admin não verificado
- [x] 4-eyes em EDD

### Lei 97/2017 — Congelamento

- [x] Notificação automática ao confirmar sanção
- [x] `AssetFreezeNotified` registado

### Instrução BdP 8/2024 — RPB

- [x] Geração anual `AmlComplianceReport`
- [x] Export JSON + XML BdP (`?format=bdp`)

### RGPD

- [x] DPIA activa (Admin)
- [x] Audit trail imutável (trigger PostgreSQL)
- [x] Auto-approve apenas Low risk
- [x] Explainability relatório (Art. 22)

### Operacional

- [x] Health `/health`
- [x] Secrets fora do repo (`.env.example` template)
- [x] Deploy on-prem documentado
- [x] CI pipeline

---

## 6. Pen test — Checklist homologação

> Ferramenta sugerida: OWASP ZAP baseline ou revisão manual. **Apenas homologação.**

### Autenticação e autorização

- [ ] `/admin/*` sem `KYC.Admin` → 403
- [ ] APIs admin AML → `KYC.Admin`
- [ ] Webhook identidade exige HMAC com secret definido
- [ ] IDOR caso alheio → 401/403

### Input e injecção

- [ ] SAR narrativa &lt; 200 chars rejeitada server-side
- [ ] Upload: MIME e tamanho máximo
- [ ] NIF inválido → validação (sem 500)

### Dados sensíveis

- [ ] Secrets só env/Key Vault
- [ ] Logs sem API keys / PII completa
- [ ] PDF sem IDOR entre casos

### Transporte

- [ ] HTTPS em homologação/prod
- [ ] Cookies HttpOnly/Secure
- [ ] CORS restrito

### Dependências

- [ ] `dotnet list package --vulnerable` sem críticos
- [ ] Imagem Docker actualizada

### Regulatório smoke

- [ ] Trigger audit imutável
- [ ] PAC/scoring/DPIA activa não apagável (interceptor EF)

### Resultado

| Data | Executor | Ferramenta | Críticos | Altos | Médios | Aprovado |
|------|----------|------------|----------|-------|--------|----------|
| | | | 0 | | | ☐ Sim ☐ Não |

**Evidência:** `docs/dossier/10-seguranca/` · modelo: [`governanca/RELATORIO_PEN_TEST_MODELO.md`](governanca/RELATORIO_PEN_TEST_MODELO.md)

---

## 7. Dossier de evidências (go-live)

```
docs/dossier/
  01-pac/  02-dpia/  03-scoring/  04-rpb/  05-sar-uif/
  06-identidade/  07-congelamento/  08-audit/  09-e2e/  10-seguranca/
```

Ver também [`../dossier/README.md`](../dossier/README.md).

### Responsáveis

| Área | Owner |
|------|--------|
| Compliance / PAC | Equipa compliance |
| RPB | `KYC.Admin` |
| Segurança | Infra + pen test |
| E2E | Analista AML + QA |

---

## 8. Quick start — Analista AML

1. **Acesso** — URL homologação; roles Analyst / Supervisor / Admin
2. **Novo caso** — Casos → Novo; aguardar triagem
3. **Conformidade** — Identidade UBO; EDD origem fundos; SAR se banner
4. **Aprovar** — Só se sem bloqueio `CanApproveMessage`
5. **Alertas** — SignalR; supervisores no grupo SAR
6. **Ajuda na app** — [`../help-online/pt/`](../help-online/pt/)

---

## 9. Dependências externas (go-live)

| ID | Entrega | Responsável | Bloqueia |
|----|---------|-------------|----------|
| X1 | Template RPB oficial BdP | Compliance | Export XML final |
| X2 | API / MOU UIF | Instituição | SAR produção |
| X3 | Contrato identidade (DigitalSign/CMD) | Prestador | Verificação prod |
| X4 | API congelamento BdP | Instituição | Notificação real |
| X5 | PAC v1 assinada | Compliance | Homologação formal |
| X6 | PDF DPIA DPO | DPO | RGPD |

---

## 10. Próximos passos operacionais

1. Executar E2E (§4) e preencher tabela §4.12
2. Preencher pen test (§6) → `dossier/10-seguranca/`
3. Credenciais X2–X4 em staging
4. Template RPB X1 → actualizar `BdpRpbExporter.cs`
5. Go-live com `Compliance:RequireLiveIntegrations=true`
