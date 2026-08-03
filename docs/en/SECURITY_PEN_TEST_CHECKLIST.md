# Checklist — Basic pen test (KYC homologation)

> Suggested tool: OWASP ZAP baseline scan or equivalent manual review.  
> Environment: **homologation** (never production without authorization).  
> **Formal report:** fill in [governanca/RELATORIO_PEN_TEST_MODELO.md](governanca/RELATORIO_PEN_TEST_MODELO.md) and archive it in `docs/dossier/10-seguranca/`.

## 1. Authentication and authorization

- [ ] `/admin/*` routes reject users without the `KYC.Admin` role
- [ ] `/api/admin/aml-reports/*` APIs require `KYC.Admin`
- [ ] Identity webhook (`POST /api/identity/webhook`) requires HMAC when `IdentityVerification:WebhookSecret` is set
- [ ] Attempted anonymous access to another user's case → 401/403

## 2. Input and injection

- [ ] SAR narrative: reject payload &lt; 200 chars (server-side)
- [ ] Document upload: MIME types and maximum size respected
- [ ] Invalid NIF in `NewCase` → validation error (no 500)

## 3. Sensitive data

- [ ] Secrets only in env / Key Vault (not in committed `appsettings`)
- [ ] Logs do not contain API keys, UIF tokens, or complete PII
- [ ] PDF/HTML report does not expose data from other cases (IDOR on `CaseId`)

## 4. Transport and headers

- [ ] HTTPS enforced in homologation/prod
- [ ] Session cookies: `HttpOnly`, `Secure` (if applicable)
- [ ] CORS restricted to the application domain

## 5. Dependencies

- [ ] `dotnet list package --vulnerable` has no open criticals (or documented exception)
- [ ] Docker image up to date (base image patch level)

## 6. Regulatory (smoke)

- [ ] Immutable audit trail (PostgreSQL trigger `tr_audit_entries_immutable`)
- [ ] Active PAC/scoring/DPIA version cannot be deleted (EF interceptor)

## Result

| Date | Executor | Tool | Critical | High | Medium | Homologation approved |
|------|----------|------|----------|------|--------|-----------------------|
| | | | 0 | | | ☐ Yes ☐ No |

**Notes / findings:**

_(fill in after scan)_
