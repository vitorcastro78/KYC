# KYC AI Platform

[![.NET](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Automate corporate KYC in Portugal — entity resolution, sanctions/PEP/adverse media, document ingestion, and risk scoring with a local LLM.**

KYC AI Platform is an on-prem Know Your Customer stack for corporate credit: Blazor UI + workers, PostgreSQL + pgvector, and Ollama for scoring and narrative reports. Designed with **BdP** compliance workflows in mind (analyst/supervisor review, audit trail, PAC gates).

**Contents:** [Why](#why-it-exists) · [Quick start — Docker](#quick-start--self-host) · [dotnet run](#prerequisites-dotnet-run) · [Architecture](#architecture-in-30-seconds) · [Features](#features) · [Configuration](#configuration) · [Docs](#documentation) · [Tests](#tests) · [License](#licensing)

---

## Why it exists

| Problem | KYC AI Platform solution |
| -------- | ------------------------ |
| Manual KYC is slow and inconsistent | Automated case pipeline with parallel screening |
| Sanctions / PEP / media live in silos | Unified triage (OFAC, EU FSF, OpenSanctions, NewsAPI, ICIJ, …) |
| Documents arrive as PDFs/scans | Ingestion + OCR/LLM field extraction into the case |
| Risk scores without explainability | Narrative report + Art. 22-oriented explainability |
| Regulators need auditability | Append-only audit trail, human review gates, retention |

---

## Quick start — self-host

Run the full stack yourself — **on-prem or fully local**. You point the apps at **your own Ollama** (or disable LLM features). Secrets stay in `.env`.

### Fastest: Docker Compose (build from source)

Requires [Docker](https://docs.docker.com/get-docker/) and Ollama on the host.

```bash
git clone https://github.com/vitorcastro78/KYC.git
cd KYC

cp .env.example .env
# edit POSTGRES_PASSWORD, RABBITMQ_PASSWORD, KYC_ADMIN_PASSWORD

docker compose up --build -d
```

Or the helper scripts (same as [ContextMemory](https://github.com/Kortexio/ContextMemory)):

```bash
./scripts/docker-run.sh --build
```

```powershell
.\scripts\docker-run.ps1 -Build
```

Then open **http://localhost:8080** — Health: **http://localhost:8080/health**

| Service | URL / value |
| -------- | ----------- |
| Web UI | http://localhost:8080 |
| Health | http://localhost:8080/health |
| RabbitMQ management | http://localhost:15672 |
| Postgres (host) | `localhost:5433` |
| Admin seed | `admin@kyc.local` / `ChangeMe@1234` (override via `.env`) |

**Ollama on the host**

```bash
ollama pull qwen3.5:9b
# Compose default: LLM__LocalEndpoint=http://host.docker.internal:11434
```

**Useful Compose env vars** (see [`.env.example`](.env.example)):

| Variable | Default | Meaning |
| -------- | ------- | ------- |
| `KYC_WEB_PORT` | `8080` | Host port for the Web UI |
| `POSTGRES_HOST_PORT` | `5433` | Host port for Postgres |
| `OLLAMA_ENDPOINT` | `http://host.docker.internal:11434` | LLM from inside containers |
| `DEFAULT_LLM_MODEL` | `qwen3.5:9b` | Ollama model id |
| `KYC_ADMIN_PASSWORD` | `ChangeMe@1234` | Seed admin password |
| `OPENSANCTIONS_API_KEY` / `NEWSAPI_KEY` | _(empty)_ | Optional integrations |

Stop with `docker compose down`.

### From GHCR (no local build)

When images are published:

```bash
docker compose -f docker-compose.ghcr.yml up -d
# or
./scripts/docker-run.sh
.\scripts\docker-run.ps1
```

| Image | Package |
| ----- | ------- |
| `ghcr.io/vitorcastro78/kyc` | Web |
| `ghcr.io/vitorcastro78/kyc-workers` | Workers |

### Database only (for `dotnet run`)

```bash
docker compose -f docker-compose.db.yml up -d
# or: ./scripts/docker-run.sh --db-only
```

---

## Prerequisites (dotnet run)

- .NET 9 SDK
- PostgreSQL 16+ with pgvector (or `docker-compose.db.yml`)
- Ollama (optional, for scoring / narrative)

### 1. Configure

```bash
cp .env.example .env
# set KYC_DB_CONNECTION / ConnectionStrings__KycDatabase
```

### 2. Start

```bash
docker compose -f docker-compose.db.yml up -d
dotnet restore KYC.sln
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
dotnet run --project src/KYC.Web
# http://localhost:5272 (Development launch profile)
```

Workers (optional second terminal):

```bash
dotnet run --project src/KYC.Workers
```

### 3. Login

- Email: `admin@kyc.local`
- Password: value of `Auth:AdminPassword` / `KYC_ADMIN_PASSWORD` (default `ChangeMe@1234`)

---

## Architecture in 30 seconds

```
src/
  KYC.Domain/           Entities, value objects, domain events
  KYC.Application/      MediatR commands/queries, policies
  KYC.Infrastructure/   EF Core, HTTP clients, LLM, messaging
  KYC.Web/              Blazor Server + Minimal APIs
  KYC.Workers/          Hosted services (lists, retention, …)
```

Case flow: intake → PAC → entity resolution / UBO → parallel screening → risk score + narrative → human review → audit.

Deploy layout matches the open-source style used in [Kortexio/ContextMemory](https://github.com/Kortexio/ContextMemory): `docker-compose.yml` (build), `docker-compose.ghcr.yml` (images), `.env.example`, `scripts/docker-run.*`.

---

## Features

- Entity resolution (GLEIF / RCBE) and UBO graph
- Sanctions & lists: OFAC SDN, EU FSF, OpenSanctions
- Adverse media (NewsAPI), ICIJ, optional CITIUS / AT debtors
- Document upload + extraction (PDF/DOCX/images)
- LLM scoring & narrative via **local Ollama** (no cloud required)
- Analyst / supervisor workflow and append-only audit
- Data retention hooks (RGPD-oriented)

---

## Configuration

**Never commit secrets.** Use `.env` (gitignored), environment variables, or Azure Key Vault (`KYC_KEYVAULT_NAME`).

`appsettings*.json` in the repo uses placeholders only (`CHANGE_ME`, empty API keys).

---

## Documentation

Index: [`docs/README.md`](docs/README.md)

| Document | Purpose |
| -------- | ------- |
| [`docs/DOCUMENTACAO_APLICACAO.md`](docs/DOCUMENTACAO_APLICACAO.md) | Architecture & stack |
| [`docs/OPERACOES_E_HOMOLOGACAO.md`](docs/OPERACOES_E_HOMOLOGACAO.md) | Ops & homologation |
| [`docs/DEPLOY_ONPREM.md`](docs/DEPLOY_ONPREM.md) | On-prem deploy |
| [`docs/CATALOGO_FUNCIONALIDADES.md`](docs/CATALOGO_FUNCIONALIDADES.md) | Feature catalogue |

---

## Tests

```bash
dotnet test KYC.sln
```

---

## Repository structure

```
docker/                 Dockerfile.web, Dockerfile.workers
docker-compose.yml      Full stack (build)
docker-compose.ghcr.yml Full stack (GHCR images)
docker-compose.db.yml   Postgres only
scripts/docker-run.*    One-command helpers
docs/                   Homologation & product docs
src/                    Application code
tests/                  Unit / integration / E2E
```

---

## Licensing

MIT — see [LICENSE](LICENSE).

## Support

Issues and PRs welcome on GitHub. For compliance dossiers and BdP checklists, start in `docs/`.
