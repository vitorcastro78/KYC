# Despliegue on-prem — KYC AI Platform

## Requisitos previos

- Docker y Docker Compose
- Ollama accesible desde los contenedores (`OLLAMA_ENDPOINT`, normalmente `http://host.docker.internal:11434`)
- Archivo `.env` (copiar desde `.env.example`) — **no hacer nunca commit**

## Inicio

```bash
cp .env.example .env
# Editar las contraseñas y la KYC_DB_CONNECTION interna (compose establece Host=kyc-postgres)

docker compose -f docker-compose.prod.yml up -d --build
```

## Migraciones

En la primera instalación o tras una actualización:

```bash
docker compose -f docker-compose.prod.yml exec kyc-web \
  dotnet ef database update --project /src/KYC.Infrastructure --startup-project /src/KYC.Web
```

Alternativa en el host (con `KYC_DB_CONNECTION` apuntando a Postgres):

```bash
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
```

## Verificación

- UI: `http://localhost:8080` (o `KYC_WEB_PORT`)
- Health: `GET /health`
- Admin: credenciales `KYC_ADMIN_EMAIL` / `KYC_ADMIN_PASSWORD`

## Conformidad (homologación)

Consulte `docs/HOMOLOGACAO_RUNBOOK.md` y `docs/CHECKLIST_HOMOLOGACAO_BDP.md`.

Variables críticas en `.env`:

- `IDENTITY_VERIFICATION_WEBHOOK_SECRET`
- `UIF_BASE_URL` / `UIF_API_KEY` (opcional en desarrollo — referencia local)
- `BDP_ASSET_FREEZE_BASE_URL`

## Workers

El servicio `kyc-workers` descarga listas OFAC/EU cuando se configura en `appsettings`. Compruebe los volúmenes `Data/ofac` y `Data/eu-fsf` tras el inicio.
