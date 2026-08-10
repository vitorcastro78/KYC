# Contributing

## Language

- Prefer **clear English or Portuguese** in docs; keep public README sections scannable in English where possible (same approach as [ContextMemory](https://github.com/Kortexio/ContextMemory)).
- Do not commit secrets, `.env`, or local overrides.

## Development setup

```bash
dotnet restore KYC.sln
dotnet build KYC.sln
dotnet test KYC.sln
```

### Run Web + DB

```bash
# Terminal 1 — Postgres
docker compose -f docker-compose.db.yml up -d

# Terminal 2 — Web
dotnet run --project src/KYC.Web
```

Local secrets — prefer `.env`, User Secrets, or environment variables (see [`.env.example`](.env.example)).

### Full stack (Docker)

```bash
cp .env.example .env
./scripts/docker-run.sh --build
# or: .\scripts\docker-run.ps1 -Build
```

## Pull requests

1. Keep changes focused; match existing naming and DI patterns.
2. Run tests before opening a PR.
3. Do not commit secrets, `Data/cases/`, or downloaded OFAC/EU list XML blobs.

## Releases

Use conventional commits on `main`. See [`docs/RELEASE.md`](docs/RELEASE.md) for release-please, GHCR, PyPI, npm, and social announcements.
