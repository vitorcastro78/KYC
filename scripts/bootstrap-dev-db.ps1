$ErrorActionPreference = "Stop"
Set-Location (Split-Path $PSScriptRoot -Parent)

$docker = $null
foreach ($p in @(
        "${env:ProgramFiles}\Docker\Docker\resources\bin\docker.exe",
        "${env:ProgramFiles}\Docker\Docker\bin\docker.exe",
        (Get-Command docker -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source)
    )) {
    if ($p -and (Test-Path $p)) { $docker = $p; break }
}

if (-not $docker) {
    Write-Warning "Docker não encontrado. Opções:"
    Write-Host "  A) winget install Docker.DockerDesktop  (reinicie, depois: docker compose up -d)"
    Write-Host "  B) PowerShell como Administrador: .\scripts\install-pgvector-windows.ps1"
    exit 1
}

& $docker compose up -d
$deadline = (Get-Date).AddMinutes(2)
do {
    Start-Sleep -Seconds 2
    $ok = & $docker compose exec -T kyc-postgres pg_isready -U postgres -d kyc_dev 2>$null
    if ($LASTEXITCODE -eq 0) { break }
} while ((Get-Date) -lt $deadline)

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__KycDatabase = "Host=localhost;Port=5433;Database=kyc_dev;Username=postgres;Password=dev123"
dotnet ef database update --project src\KYC.Infrastructure --startup-project src\KYC.Web
Write-Host "Base pronta. Para correr a Web com esta BD: perfil launch 'docker-db' ou defina ConnectionStrings__KycDatabase com Port=5433."
