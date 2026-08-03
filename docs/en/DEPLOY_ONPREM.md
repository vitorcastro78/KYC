# On-prem Deployment — KYC AI Platform

## Prerequisites

- Docker and Docker Compose
- Ollama accessible from containers (`OLLAMA_ENDPOINT`, typically `http://host.docker.internal:11434`)
- `.env` file (copy from `.env.example`) — **never commit it**

## Start-up

```bash
cp .env.example .env
# Edit passwords and the internal KYC_DB_CONNECTION (compose sets Host=kyc-postgres)

docker compose -f docker-compose.prod.yml up -d --build
```

## Migrations

On the first installation or after an upgrade:

```bash
docker compose -f docker-compose.prod.yml exec kyc-web \
  dotnet ef database update --project /src/KYC.Infrastructure --startup-project /src/KYC.Web
```

Host alternative (with `KYC_DB_CONNECTION` pointing to Postgres):

```bash
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

## Verification

- UI: `http://localhost:8080` (or `KYC_WEB_PORT`)
- Health: `GET /health`
- Admin: `KYC_ADMIN_EMAIL` / `KYC_ADMIN_PASSWORD` credentials

## Compliance (UAT)

See `docs/HOMOLOGACAO_RUNBOOK.md` and `docs/CHECKLIST_HOMOLOGACAO_BDP.md`.

Critical variables in `.env`:

- `IDENTITY_VERIFICATION_WEBHOOK_SECRET`
- `UIF_BASE_URL` / `UIF_API_KEY` (optional in dev — local reference)
- `BDP_ASSET_FREEZE_BASE_URL`

## Workers

The `kyc-workers` service downloads OFAC/EU lists when configured in `appsettings`. Check the `Data/ofac` and `Data/eu-fsf` volumes after start-up.
