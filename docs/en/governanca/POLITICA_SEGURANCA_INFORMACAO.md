# Information Security Policy — KYC AI Platform

> **Version:** 1.0 (draft for approval)  
> **Classification:** Internal — Confidential  
> **Approval:** _[CISO / Executive Committee — date and signature]_

## 1. Purpose

Define security principles and controls for the KYC platform (personal data, financial data, FIU communications and regulatory audit trail).

## 2. Scope

- KYC.Web application, Workers, PostgreSQL, Ollama, FIU/BdP/identity integrations
- Users: analysts, supervisors, administrators, auditors
- Environments: development, staging, production

## 3. Principles

1. **Least privilege** — roles `KYC.Analyst`, `KYC.Supervisor`, `KYC.Admin`, `KYC.Auditor`
2. **Defence in depth** — network, TLS, authentication, authorisation, immutable audit
3. **Environment segregation** — separate secrets per environment; no production data in development
4. **Accountability** — `ICurrentAnalystAccessor` in all compliance actions

## 4. Controls implemented on the platform

| Control | Implementation |
|---------|----------------|
| Authentication | Microsoft Entra OIDC (production) or Identity (development) |
| MFA | Entra Conditional Access (mandatory for production operators) |
| Authorisation | Role-based ASP.NET Core policies |
| Session | HttpOnly cookies; 14-day expiry (development Identity) |
| Secrets | `.env` / Azure Key Vault — never in the repository |
| Webhook | HMAC SHA-256 `IdentityVerification:WebhookSecret` |
| Audit | Immutable PostgreSQL trigger |
| Production integrations | `Compliance:RequireLiveIntegrations` |

## 5. Incident management

1. Detection through Application Insights logs / institutional SIEM
2. P1–P4 classification; notify the DPO in case of a personal-data breach (72h RGPD)
3. Record in a ticket + audit entry if KYC cases are impacted

## 6. Review

Annual review or after a serious incident. Next review: _[date]_.

## 7. Approvals

| Role | Name | Date | Signature |
|------|------|------|-----------|
| CISO | | | |
| DPO | | | |
| Head of Compliance | | | |
