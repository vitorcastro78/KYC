# Checklist — Pen test básico (homologação KYC)

> Ferramenta sugerida: OWASP ZAP baseline scan ou revisão manual equivalente.  
> Ambiente: **homologação** (nunca produção sem autorização).

## 1. Autenticação e autorização

- [ ] Rotas `/admin/*` rejeitam utilizador sem role `KYC.Admin`
- [ ] APIs `/api/admin/aml-reports/*` exigem `KYC.Admin`
- [ ] Webhook identidade (`POST /api/identity/webhook`) exige HMAC quando `IdentityVerification:WebhookSecret` definido
- [ ] Tentativa de acesso anónimo a caso alheio → 401/403

## 2. Input e injecção

- [ ] SAR narrativa: rejeitar payload &lt; 200 chars (server-side)
- [ ] Upload documentos: tipos MIME e tamanho máximo respeitados
- [ ] NIF inválido no `NewCase` → erro de validação (sem 500)

## 3. Dados sensíveis

- [ ] Secrets apenas em env / Key Vault (não em `appsettings` commitado)
- [ ] Logs não contêm API keys, tokens UIF, ou PII completa
- [ ] Relatório PDF/HTML não expõe dados de outros casos (IDOR no `CaseId`)

## 4. Transporte e headers

- [ ] HTTPS forçado em homologação/prod
- [ ] Cookies de sessão: `HttpOnly`, `Secure` (se aplicável)
- [ ] CORS restrito ao domínio da aplicação

## 5. Dependências

- [ ] `dotnet list package --vulnerable` sem críticos abertos (ou excepção documentada)
- [ ] Imagem Docker actualizada (base image patch level)

## 6. Regulatório (smoke)

- [ ] Audit trail imutável (trigger PostgreSQL `tr_audit_entries_immutable`)
- [ ] Versão PAC/scoring/DPIA activa não apagável (interceptor EF)

## Resultado

| Data | Executor | Ferramenta | Críticos | Altos | Médios | Aprovado homologação |
|------|----------|------------|----------|-------|--------|----------------------|
| | | | 0 | | | ☐ Sim ☐ Não |

**Notas / findings:**

_(preencher após scan)_
