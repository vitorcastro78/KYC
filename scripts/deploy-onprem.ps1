# Deploy on-prem via docker compose
param(
    [string]$EnvFile = ".env"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

if (-not (Test-Path $EnvFile)) {
    Write-Error "Ficheiro $EnvFile em falta. Copie .env.example para .env e configure."
}

Write-Host "A construir imagens..."
docker compose -f docker-compose.prod.yml --env-file $EnvFile build

Write-Host "A iniciar serviços..."
docker compose -f docker-compose.prod.yml --env-file $EnvFile up -d

Write-Host "A aplicar migrations..."
docker compose -f docker-compose.prod.yml --env-file $EnvFile exec kyc-web dotnet ef database update `
    --project /app/KYC.Infrastructure.dll 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Migrations: executar manualmente se necessário (dotnet ef database update)."
}

Write-Host "Deploy concluído. Web: http://localhost:$($env:KYC_WEB_PORT ?? '8080')"
