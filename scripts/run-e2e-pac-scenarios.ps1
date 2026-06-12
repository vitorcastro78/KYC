# Executa cenários E2E PAC na UI (Playwright) — evidências em docs/dossier/01-pac/
param(
    [string]$AppUrl = "http://localhost:5100",
    [switch]$SkipAppStart,
    [switch]$KeepAppRunning
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $root "scripts\Import-DotEnv.ps1")
Import-DotEnv (Join-Path $root ".env")

$conn = $env:KYC_DB_CONNECTION
if (-not $conn) {
    $conn = "Host=195.179.193.136;Port=5433;Database=azureopsagent;Username=kycdb;Password=lara2308"
}

$uiDir = Join-Path $root "scripts\e2e-ui"
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$env:E2E_STAMP = $stamp
$env:KYC_APP_URL = $AppUrl
$env:ConnectionStrings__KycDatabase = $conn

function Test-AppReady {
    try {
        $r = Invoke-WebRequest -Uri "$AppUrl/health" -UseBasicParsing -TimeoutSec 5
        return $r.StatusCode -eq 200
    } catch { return $false }
}

$appProc = $null
if (-not $SkipAppStart -and -not (Test-AppReady)) {
    Write-Host "A arrancar KYC.Web em $AppUrl ..." -ForegroundColor Cyan
    $env:Kestrel__Endpoints__Http__Url = $AppUrl.Replace("localhost", "0.0.0.0")
    $appProc = Start-Process -FilePath "dotnet" -WorkingDirectory $root -ArgumentList @(
        "run", "--project", "src/KYC.Web/KYC.Web.csproj", "--no-launch-profile", "--urls", $AppUrl
    ) -PassThru -WindowStyle Hidden
    $deadline = (Get-Date).AddSeconds(180)
    while ((Get-Date) -lt $deadline) {
        if (Test-AppReady) { break }
        Start-Sleep -Seconds 3
    }
    if (-not (Test-AppReady)) {
        if ($appProc) { Stop-Process -Id $appProc.Id -Force -ErrorAction SilentlyContinue }
        throw "App não respondeu em $AppUrl"
    }
    Write-Host "App OK." -ForegroundColor Green
} elseif (Test-AppReady) {
    Write-Host "App já disponível em $AppUrl" -ForegroundColor Green
} else {
    throw "App indisponível e -SkipAppStart activo."
}

Push-Location $uiDir
try {
    if (-not (Test-Path "node_modules/playwright")) {
        npm install --no-fund --no-audit 2>&1 | Write-Host
        npx playwright install chromium 2>&1 | Write-Host
    }
    node scenarios-pac.mjs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
} finally {
    Pop-Location
    if ($appProc -and -not $KeepAppRunning) {
        Stop-Process -Id $appProc.Id -Force -ErrorAction SilentlyContinue
        Write-Host "App encerrada." -ForegroundColor DarkGray
    } elseif ($appProc -and $KeepAppRunning) {
        Write-Host "App continua (PID $($appProc.Id))." -ForegroundColor Green
    }
}

Write-Host "Concluído. Ver docs/dossier/01-pac/ e docs/dossier/09-e2e/REGISTO_UI_PAC_$stamp.md" -ForegroundColor Green
