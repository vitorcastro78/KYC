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
    Write-Host "  A) winget install Docker.DockerDesktop  (reinicie, depois: docker compose -f docker-compose.db.yml up -d)"
    Write-Host "  B) PowerShell como Administrador: .\scripts\install-pgvector-windows.ps1"
    exit 1
}

& $docker compose -f docker-compose.db.yml up -d
$deadline = (Get-Date).AddMinutes(2)
$user = if ($env:POSTGRES_USER) { $env:POSTGRES_USER } else { "postgres" }
$db = if ($env:POSTGRES_DB) { $env:POSTGRES_DB } else { "kyc_dev" }
$pwd = if ($env:POSTGRES_PASSWORD) { $env:POSTGRES_PASSWORD } else { "CHANGE_ME" }
do {
    Start-Sleep -Seconds 2
    $ok = & $docker compose -f docker-compose.db.yml exec -T kyc-postgres pg_isready -U $user -d $db 2>$null
    if ($LASTEXITCODE -eq 0) { break }
} while ((Get-Date) -lt $deadline)

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__KycDatabase = "Host=localhost;Port=5433;Database=$db;Username=$user;Password=$pwd"
dotnet ef database update --project src\KYC.Infrastructure --startup-project src\KYC.Web
Write-Host "Base pronta. Para correr a Web: dotnet run --project src/KYC.Web (ConnectionStrings__KycDatabase Port=5433)."
