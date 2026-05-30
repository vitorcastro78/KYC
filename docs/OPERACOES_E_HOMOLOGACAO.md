# Operações e Homologação — KYC AI Platform

> Documento unificado: deploy, runbooks, testes E2E, checklists regulatórios e segurança, dossier de evidências.

---

## 1. Deploy on-prem

### 1.1 Pré-requisitos

- Docker e Docker Compose
- Ollama acessível (`OLLAMA_ENDPOINT`, ex. `http://host.docker.internal:11434`)
- Ficheiro `.env` (copiar de `.env.example`) — **nunca commitar**

### 1.2 Arranque

```bash
cp .env.example .env
# Editar passwords e KYC_DB_CONNECTION (compose: Host=kyc-postgres)

docker compose -f docker-compose.prod.yml up -d --build
```

### 1.3 Migrations

```bash
docker compose -f docker-compose.prod.yml exec kyc-web \
  dotnet ef database update --project /src/KYC.Infrastructure --startup-project /src/KYC.Web
```

No host:

```bash
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

### 1.4 Verificação pós-deploy

| Verificação | Comando / URL |
|-------------|---------------|
| UI | `http://localhost:8080` (ou `KYC_WEB_PORT`) |
| Health | `GET /health` |
| Admin dev | `KYC_ADMIN_EMAIL` / `KYC_ADMIN_PASSWORD` |
| Workers | Volumes `Data/ofac`, `Data/eu-fsf` após arranque |

### 1.5 Variáveis compliance críticas

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

Teste opcional:

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

Cobertura compliance: `ComplianceHandlersIntegrationTests`, `ComplianceFlowTests`, `SarEligibilityTests`, `IdentityWebhookHttpTests`, `UboGraphViewBuilderTests`.

### 2.4 CI

Push/PR para `main`, `develop` ou `feature/*` → `.github/workflows/ci.yml` (PostgreSQL + migrations + testes).

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

> Ambiente com BD migrada (`BdpComplianceAndGtm` + posteriores).  
> Pré-requisitos: `KYC_DB_CONNECTION`, Ollama, PAC activa.

### Cenário 1 — PAC no arranque

1. Caso CAE `92000` (jogos) → falha PAC
2. Caso válido → `InProgress` + `LegalBasisRef`

### Cenário 2 — Identidade (Aviso 1/2022)

1. Conformidade → «Verificar identidade» → método
2. Webhook HMAC ou polling → `Verified`
3. Aprovar sem UBO verificado → botão desactivado + mensagem

### Cenário 3 — SAR / UIF

1. Caso alto risco → banner SAR → narrativa ≥200 → submeter
2. «Não aplicável» → justificação ≥50 → `NotRequired`
3. Lista casos → badges SAR/DDC

### Cenário 4 — EDD 4-eyes

1. Enhanced + origem fundos + verificação
2. Aprovar com segundo supervisor → `SecondApproverId`

### Cenário 5 — RPB

1. Admin → Gerar RPB ano corrente
2. Export `?format=bdp` → XML
3. Submeter → referência BdP

### Registo de execução

| # | Cenário | Data | Executor | Resultado | Evidência |
|---|---------|------|----------|-----------|-----------|
| 1 | PAC arranque | | | ☐ OK ☐ Falha | `dossier/01-pac/` |
| 2 | Identidade + webhook | | | ☐ OK ☐ Falha | `dossier/06-identidade/` |
| 3 | SAR | | | ☐ OK ☐ Falha | `dossier/05-sar-uif/` |
| 4 | EDD 4-eyes | | | ☐ OK ☐ Falha | |
| 5 | RPB Admin | | | ☐ OK ☐ Falha | `dossier/04-rpb/` |

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

**Evidência:** `docs/dossier/10-seguranca/`

---

## 7. Dossier de evidências (go-live)

### Estrutura de pastas

```
docs/dossier/
  01-pac/           PAC activa (print Admin)
  02-dpia/          DPIA + documento
  03-scoring/       Versão scoring + hash prompt
  04-rpb/           XML BdP + JSON + ref. submissão
  05-sar-uif/       SAR + ref. UIF
  06-identidade/    Webhook + verificação party
  07-congelamento/  Notificação BdP
  08-audit/         Extract audit caso teste
  09-e2e/           Checklist E2E assinado
  10-seguranca/     Pen test preenchido
```

### Como gerar

1. Executar cenários da secção 4
2. Admin → Settings: captura PAC, scoring, DPIA
3. Admin → RPB: gerar, exportar, submeter
4. Caso com sanção: print congelamento + audit
5. Nomear ficheiros com data: `RPB-2025-20260530.xml`

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
3. **Conformidade** — Identidade UBO; EDD origem fundos; SAR se banner; RCBE
4. **Aprovar** — Só se sem bloqueio `CanApproveMessage`
5. **Alertas** — SignalR; supervisores no grupo SAR
6. **Referência** — Este documento + [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md)

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

## 10. Próximos passos operacionais (ordem)

1. Executar E2E (secção 4) e preencher tabela
2. Preencher pen test (secção 6) → `dossier/10-seguranca/`
3. Credenciais X2–X4 em staging
4. Template RPB X1 → actualizar `BdpRpbExporter.cs`
5. Go-live com `Compliance:RequireLiveIntegrations=true`

---

## Documentos fonte (detalhe histórico)

Os ficheiros abaixo permanecem no repositório; o conteúdo operacional relevante foi consolidado **neste documento**:

- [DEPLOY_ONPREM.md](DEPLOY_ONPREM.md)
- [HOMOLOGACAO_RUNBOOK.md](HOMOLOGACAO_RUNBOOK.md)
- [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md)
- [CHECKLIST_HOMOLOGACAO_BDP.md](CHECKLIST_HOMOLOGACAO_BDP.md)
- [PAC_RUNBOOK.md](PAC_RUNBOOK.md)
- [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md)
- [ANALISTA_QUICK_START.md](ANALISTA_QUICK_START.md)
- [dossier/README.md](dossier/README.md)
