# Deploy on-prem — KYC AI Platform

Deployment layout follows the same pattern as [Kortexio/ContextMemory](https://github.com/Kortexio/ContextMemory): `docker-compose.yml` (build), `docker-compose.ghcr.yml` (images), `.env.example`, `scripts/docker-run.*`.

## Pré-requisitos

- Docker e Docker Compose
- Ollama acessível a partir dos contentores (`OLLAMA_ENDPOINT`, tipicamente `http://host.docker.internal:11434`)
- Ficheiro `.env` (copiar de `.env.example`) — **nunca commitar**

## Arranque (build local)

```bash
cp .env.example .env
# Editar POSTGRES_PASSWORD, RABBITMQ_PASSWORD, KYC_ADMIN_PASSWORD

docker compose up --build -d
# ou: ./scripts/docker-run.sh --build
# ou: .\scripts\docker-run.ps1 -Build
```

## Arranque (imagens GHCR)

```bash
docker compose -f docker-compose.ghcr.yml up -d
# ou: ./scripts/docker-run.sh
```

## Migrations

Na primeira instalação ou após upgrade (no host, com connection string para Postgres):

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
- `UIF_BASE_URL` / `UIF_API_KEY` (opcional em dev)
- `BDP_ASSET_FREEZE_BASE_URL`

## Workers

O serviço `kyc-workers` descarrega listas OFAC/EU quando configurado. Confirmar volumes `Data/ofac` e `Data/eu-fsf` após arranque.

## Apenas base de dados

Para desenvolvimento com `dotnet run` no host:

```bash
docker compose -f docker-compose.db.yml up -d
```
