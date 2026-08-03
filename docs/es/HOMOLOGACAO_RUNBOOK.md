# Runbook — Homologación BdP

## 1. Base de datos

```powershell
$env:KYC_DB_CONNECTION="Host=195.179.193.136;Port=5433;Database=azureopsagent;Username=...;Password=..."
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

Confirme el trigger de auditoría:

```sql
SELECT tgname FROM pg_trigger WHERE tgname = 'tr_audit_entries_immutable';
```

Prueba automatizada (opcional):

```powershell
$env:KYC_DB_CONNECTION="..."
dotnet test tests/KYC.Web.Integration.Tests --filter AuditImmutability
```

## 2. Webhook de identidad (HMAC)

Configure `IdentityVerification:WebhookSecret` o `IDENTITY_VERIFICATION_WEBHOOK_SECRET`.

Ejemplo (PowerShell):

```powershell
$body = '{"partyId":"<GUID>","sessionId":"sess-abc","verified":true}'
$secret = "tu-secret"
$hash = [BitConverter]::ToString([System.Security.Cryptography.HMACSHA256]::HashData(
  [Text.Encoding]::UTF8.GetBytes($secret),
  [Text.Encoding]::UTF8.GetBytes($body))).Replace("-","").ToLower()
Invoke-RestMethod -Method Post -Uri "https://<host>/api/identity/webhook" `
  -Headers @{ "X-Webhook-Signature" = "sha256=$hash" } `
  -ContentType "application/json" -Body $body
```

## 3. Pruebas

```powershell
dotnet test
dotnet test tests/KYC.Web.Integration.Tests
```

## 4. Despliegue on-prem

Consulte `docs/DEPLOY_ONPREM.md`.

## 5. CI

Un push/PR a `main`, `develop` o `feature/*` activa `.github/workflows/ci.yml` (PostgreSQL + migraciones + `dotnet test`).

## 6. Checklist

Marque los elementos de `docs/CHECKLIST_HOMOLOGACAO_BDP.md` con evidencias (capturas, exportaciones XML RPB, logs de auditoría).
