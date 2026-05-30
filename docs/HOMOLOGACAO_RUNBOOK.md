# Runbook — Homologação BdP

## 1. Base de dados

```powershell
$env:KYC_DB_CONNECTION="Host=195.179.193.136;Port=5433;Database=azureopsagent;Username=...;Password=..."
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

Confirma trigger de audit:

```sql
SELECT tgname FROM pg_trigger WHERE tgname = 'tr_audit_entries_immutable';
```

Teste automatizado (opcional):

```powershell
$env:KYC_DB_CONNECTION="..."
dotnet test tests/KYC.Web.Integration.Tests --filter AuditImmutability
```

## 2. Webhook identidade (HMAC)

Configurar `IdentityVerification:WebhookSecret` ou `IDENTITY_VERIFICATION_WEBHOOK_SECRET`.

Exemplo (PowerShell):

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

## 3. Testes

```powershell
dotnet test
dotnet test tests/KYC.Web.Integration.Tests
```

## 4. Checklist

Marcar itens em `docs/CHECKLIST_HOMOLOGACAO_BDP.md` com evidências (prints, exports XML RPB, logs audit).
