# Run KYC stack from GHCR (or build locally with -Build).
# Usage:
#   .\scripts\docker-run.ps1
#   .\scripts\docker-run.ps1 -Build
#   .\scripts\docker-run.ps1 -DbOnly
param(
    [switch]$Build,
    [switch]$DbOnly,
    [string]$Tag = "latest"
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

if (-not (Test-Path ".env") -and (Test-Path ".env.example")) {
    Write-Host "==> Creating .env from .env.example (edit passwords before production use)"
    Copy-Item ".env.example" ".env"
}

if ($DbOnly) {
    Write-Host "==> Starting Postgres only (docker-compose.db.yml)"
    docker compose -f docker-compose.db.yml up -d
    Write-Host "Postgres: localhost:${env:POSTGRES_HOST_PORT ?? '5433'}"
    Write-Host "Stop:     docker compose -f docker-compose.db.yml down"
    exit 0
}

if ($Build) {
    Write-Host "==> Building and starting stack (docker compose up --build)"
    docker compose up --build -d
}
else {
    Write-Host "==> Pulling GHCR images and starting (docker-compose.ghcr.yml)"
    $env:KYC_TAG = $Tag
    docker compose -f docker-compose.ghcr.yml pull
    docker compose -f docker-compose.ghcr.yml up -d
}

$port = if ($env:KYC_WEB_PORT) { $env:KYC_WEB_PORT } else { "8080" }
Write-Host ""
Write-Host "Web:     http://localhost:$port"
Write-Host "Health:  http://localhost:$port/health"
Write-Host "RabbitMQ management: http://localhost:15672"
Write-Host "Admin:   admin@kyc.local / (KYC_ADMIN_PASSWORD in .env)"
Write-Host ""
Write-Host "Stop:    docker compose down"
Write-Host "         docker compose -f docker-compose.ghcr.yml down"
