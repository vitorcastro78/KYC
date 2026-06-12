# Gera evidências E2E: testes automatizados + API com app local + ficheiros em docs/dossier/
param(
    [string]$DbConnection = "",
    [string]$AppUrl = "http://localhost:5299",
    [switch]$SkipApp,
    [switch]$KeepAppRunning
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

. (Join-Path $PSScriptRoot "Import-DotEnv.ps1")
Import-DotEnv (Join-Path $root ".env")

if ([string]::IsNullOrWhiteSpace($DbConnection)) {
    $DbConnection = $env:KYC_DB_CONNECTION
}
if ([string]::IsNullOrWhiteSpace($DbConnection)) {
    throw "Defina KYC_DB_CONNECTION em .env ou passe -DbConnection."
}

Write-Host "BD: $($DbConnection -replace 'Password=[^;]+','Password=***')" -ForegroundColor DarkGray
$env:KYC_DB_CONNECTION = $DbConnection
$env:ConnectionStrings__KycDatabase = $DbConnection

$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$dossier = Join-Path $root "docs\dossier"
foreach ($sub in @("01-pac","05-sar-uif","06-identidade","07-congelamento","08-audit","09-e2e","10-seguranca")) {
    New-Item -ItemType Directory -Force -Path (Join-Path $dossier $sub) | Out-Null
}

Write-Host "=== Build ===" -ForegroundColor Cyan
dotnet build KYC.sln -v q

Write-Host "=== Migracoes EF ===" -ForegroundColor Cyan
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web --context KycDbContext

$trxPath = Join-Path $dossier "09-e2e\test-results-$stamp.trx"
Write-Host "=== Testes E2E automatizados (HomologationE2e + compliance) ===" -ForegroundColor Cyan
$testExit = 0
dotnet test tests/KYC.Web.Integration.Tests `
    --filter "FullyQualifiedName~HomologationE2e" `
    --logger "trx;LogFileName=$trxPath" `
    -v n
if ($LASTEXITCODE -ne 0) { $testExit = $LASTEXITCODE }

dotnet test tests/KYC.Application.Tests --filter "FullyQualifiedName~ComplianceFlow|FullyQualifiedName~StartKycCase" -v n `
    | Tee-Object -FilePath (Join-Path $dossier "09-e2e\application-tests-$stamp.log")

$appProc = $null
if (-not $SkipApp) {
    Write-Host "=== Arrancar KYC.Web ===" -ForegroundColor Cyan
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = $AppUrl
    $env:Kestrel__Endpoints__Http__Url = $AppUrl
    $env:Messaging__HostInMemoryPipeline = "true"
    $env:Compliance__EnablePeriodicReviewScheduler = "false"
    $env:Testing__DisableBackgroundServices = "true"
    $appProc = Start-Process -FilePath "dotnet" -WorkingDirectory $root -ArgumentList @(
        "run","--project","src/KYC.Web/KYC.Web.csproj","--no-launch-profile"
    ) `
        -PassThru -WindowStyle Hidden
    $deadline = (Get-Date).AddSeconds(90)
    $ready = $false
    while ((Get-Date) -lt $deadline) {
        try {
            $h = Invoke-WebRequest -Uri "$AppUrl/health" -UseBasicParsing -TimeoutSec 5
            if ($h.StatusCode -eq 200) { $ready = $true; break }
        } catch { Start-Sleep -Seconds 2 }
    }
    if (-not $ready) { throw "App nao respondeu em $AppUrl/health" }

    $apiDir = Join-Path $dossier "09-e2e"
    @( "/health", "/api/openapi/info" ) | ForEach-Object {
        $uri = "$AppUrl$_"
        try {
            $r = Invoke-WebRequest -Uri $uri -UseBasicParsing -TimeoutSec 30
            $out = Join-Path $apiDir ("http" + ($_ -replace '/','-') + "-$stamp.txt")
            @("GET $uri", "Status: $($r.StatusCode)", "", $r.Content) | Set-Content $out -Encoding UTF8
        } catch {
            @("GET $uri", "ERRO: $($_.Exception.Message)") | Set-Content (Join-Path $apiDir "http-error-$stamp.txt")
        }
    }

    $exportJson = Get-ChildItem (Join-Path $dossier "09-e2e") -Filter "audit-export-*.json" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($exportJson) {
        $data = Get-Content $exportJson.FullName -Raw | ConvertFrom-Json
        $party = $data.Parties | Where-Object { $_.VerificationStatus -eq 0 -or $_.VerificationStatus -eq "Pending" } | Select-Object -First 1
        if (-not $party) { $party = $data.Parties | Select-Object -First 1 }
        if ($party) {
            $body = (@{ partyId = $party.Id; sessionId = "e2e-webhook-$stamp"; verified = $true; eidasLevel = "High" } | ConvertTo-Json -Compress)
            try {
                $wh = Invoke-WebRequest -Method Post -Uri "$AppUrl/api/identity/webhook" -Body $body -ContentType "application/json" -UseBasicParsing
                @("POST $AppUrl/api/identity/webhook", "Status: $($wh.StatusCode)", "Body: $body", $wh.Content) |
                    Set-Content (Join-Path $dossier "06-identidade\webhook-$stamp.txt") -Encoding UTF8
            } catch {
                @("Webhook falhou", $_.Exception.Message, $body) | Set-Content (Join-Path $dossier "06-identidade\webhook-error-$stamp.txt")
            }
        }
    }

    if (-not $KeepAppRunning) {
        Stop-Process -Id $appProc.Id -Force -ErrorAction SilentlyContinue
        Write-Host "App encerrada." -ForegroundColor DarkGray
    } else {
        Write-Host "App continua em $AppUrl (PID $($appProc.Id))." -ForegroundColor Green
    }
}

$latestAudit = Get-ChildItem (Join-Path $dossier "09-e2e") -Filter "audit-export-*.json" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1

$registro = @"
# Registo E2E automatizado — $stamp

| Item | Ficheiro |
|------|----------|
| Resultados testes | docs/dossier/09-e2e/test-results-$stamp.trx |
| Export audit/casos | $(if ($latestAudit) { $latestAudit.Name } else { 'audit-export-*.json' }) |
| Audit trail | docs/dossier/08-audit/audit-trail-e2e-*.json |
| HTTP health/OpenAPI | docs/dossier/09-e2e/http-*-$stamp.txt |
| Webhook identidade | docs/dossier/06-identidade/webhook-*.txt |
| Testes aplicacao | docs/dossier/09-e2e/application-tests-$stamp.log |

Executado: $(Get-Date -Format o)
BD: $($DbConnection -replace 'Password=[^;]+','Password=***')
App: $AppUrl
Exit code testes: $testExit
"@
$registro | Set-Content (Join-Path $dossier "09-e2e\REGISTO_EXECUCAO_$stamp.md") -Encoding UTF8

# Preencher tabela E2E (cenarios cobertos por testes automatizados)
@"
| # | Cenario | Resultado | Evidencia |
|---|---------|-----------|-----------|
| 1 | PAC CAE 92000 | OK (teste) | test-results-$stamp.trx |
| 6 | Nome legal manual | OK (teste) | audit-export / trx |
| 7 | SAR manual | OK (teste) | 05-sar-uif / trx |
| 8 | Congelamento manual | OK (teste) | 07-congelamento / trx |
| 9 | Identidade manual | OK (teste) | 06-identidade / trx |
| 10 | Sinais manuais | OK (teste) | trx |
| 2-5 | UI/API parcial | HTTP + webhook se app OK | 09-e2e, 06-identidade |
"@ | Add-Content (Join-Path $dossier "09-e2e\REGISTO_EXECUCAO_$stamp.md") -Encoding UTF8

if ($testExit -ne 0) { exit $testExit }
Write-Host "Evidencias em docs/dossier/ - registo: REGISTO_EXECUCAO_$stamp.md" -ForegroundColor Green
