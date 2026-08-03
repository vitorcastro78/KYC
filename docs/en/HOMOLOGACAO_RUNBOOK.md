# Runbook — BdP UAT

## 1. Database

```powershell
$env:KYC_DB_CONNECTION="Host=195.179.193.136;Port=5433;Database=azureopsagent;Username=...;Password=..."
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

Confirm the audit trigger:

```sql
SELECT tgname FROM pg_trigger WHERE tgname = 'tr_audit_entries_immutable';
```

Automated test (optional):

```powershell
$env:KYC_DB_CONNECTION="..."
dotnet test tests/KYC.Web.Integration.Tests --filter AuditImmutability
```

## 2. Identity webhook (HMAC)

Configure `IdentityVerification:WebhookSecret` or `IDENTITY_VERIFICATION_WEBHOOK_SECRET`.

Example (PowerShell):

```powershell
$body = '{"partyId":"<GUID>","sessionId":"sess-abc","verified":true}'
$secret = "your-secret"
$hash = [BitConverter]::ToString([System.Security.Cryptography.HMACSHA256]::HashData(
  [Text.Encoding]::UTF8.GetBytes($secret),
  [Text.Encoding]::UTF8.GetBytes($body))).Replace("-","").ToLower()
Invoke-RestMethod -Method Post -Uri "https://<host>/api/identity/webhook" `
  -Headers @{ "X-Webhook-Signature" = "sha256=$hash" } `
  -ContentType "application/json" -Body $body
```

## 3. Tests

```powershell
dotnet test
dotnet test tests/KYC.Web.Integration.Tests
```

## 4. On-prem deployment

See `docs/DEPLOY_ONPREM.md`.

## 5. CI

Push/PR to `main`, `develop`, or `feature/*` triggers `.github/workflows/ci.yml` (PostgreSQL + migrations + `dotnet test`).

## 6. Checklist

Tick items in `docs/CHECKLIST_HOMOLOGACAO_BDP.md` and include evidence (screenshots, RPB XML exports, audit logs).
