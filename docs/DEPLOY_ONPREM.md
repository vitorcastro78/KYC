# Deploy on-prem — KYC AI Platform

## Pré-requisitos

- Docker e Docker Compose
- Ollama acessível a partir dos contentores (`OLLAMA_ENDPOINT`, tipicamente `http://host.docker.internal:11434`)
- Ficheiro `.env` (copiar de `.env.example`) — **nunca commitar**

## Arranque

```bash
cp .env.example .env
# Editar passwords e KYC_DB_CONNECTION interno (compose define Host=kyc-postgres)

docker compose -f docker-compose.prod.yml up -d --build
```

## Migrations

Na primeira instalação ou após upgrade:

```bash
docker compose -f docker-compose.prod.yml exec kyc-web \
  dotnet ef database update --project /src/KYC.Infrastructure --startup-project /src/KYC.Web
```

Alternativa no host (com `KYC_DB_CONNECTION` apontando para Postgres):

```bash
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

## Verificação

- UI: `http://localhost:8080` (ou `KYC_WEB_PORT`)
- Health: `GET /health`
- Admin: credenciais `KYC_ADMIN_EMAIL` / `KYC_ADMIN_PASSWORD`

## Compliance (homologação)

Ver `docs/HOMOLOGACAO_RUNBOOK.md` e `docs/CHECKLIST_HOMOLOGACAO_BDP.md`.

Variáveis críticas no `.env`:

- `IDENTITY_VERIFICATION_WEBHOOK_SECRET`
- `UIF_BASE_URL` / `UIF_API_KEY` (opcional em dev — referência local)
- `BDP_ASSET_FREEZE_BASE_URL`

## Workers

O serviço `kyc-workers` descarrega listas OFAC/EU quando configurado em `appsettings`. Confirmar volumes `Data/ofac` e `Data/eu-fsf` após arranque.
