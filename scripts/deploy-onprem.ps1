# Deploy on-prem via docker compose (ContextMemory-style stack)
param(
    [string]$EnvFile = ".env",
    [switch]$FromGhcr
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

if (-not (Test-Path $EnvFile)) {
    Write-Error "Ficheiro $EnvFile em falta. Copie .env.example para .env e configure."
}

$composeArgs = @("--env-file", $EnvFile)
if ($FromGhcr) {
    $composeArgs = @("-f", "docker-compose.ghcr.yml") + $composeArgs
    Write-Host "A puxar imagens GHCR..."
    docker compose @composeArgs pull
    Write-Host "A iniciar serviços..."
    docker compose @composeArgs up -d
}
else {
    Write-Host "A construir imagens..."
    docker compose @composeArgs build
    Write-Host "A iniciar serviços..."
    docker compose @composeArgs up -d --build
}

Write-Host "Deploy concluído. Web: http://localhost:$((Get-Content $EnvFile | Where-Object { $_ -match '^KYC_WEB_PORT=' }) -replace 'KYC_WEB_PORT=','' | Select-Object -First 1)"
Write-Host "Health: http://localhost:8080/health (ou KYC_WEB_PORT)"
Write-Host "Migrations: aplicar via EF se necessário (ver docs/pt/OPERACOES_E_HOMOLOGACAO.md §1)."
