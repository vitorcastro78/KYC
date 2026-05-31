# Executa E2E de homologação (docs/E2E_HOMOLOGACAO.md)
# Pré-requisito: KYC_DB_CONNECTION em .env (ex.: PostgreSQL homologação remoto :5433)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

. (Join-Path $PSScriptRoot "Import-DotEnv.ps1")
Import-DotEnv (Join-Path $root ".env")

if ([string]::IsNullOrWhiteSpace($env:KYC_DB_CONNECTION)) {
    throw "Defina KYC_DB_CONNECTION em .env (ver .env.example)."
}
$env:ConnectionStrings__KycDatabase = $env:KYC_DB_CONNECTION
Write-Host "BD: $($env:KYC_DB_CONNECTION -replace 'Password=[^;]+','Password=***')" -ForegroundColor DarkGray

Write-Host "=== 1/3 Testes em memória (contingência + compliance) ===" -ForegroundColor Cyan
dotnet test tests/KYC.Application.Tests --filter "FullyQualifiedName~ComplianceFlow|FullyQualifiedName~StartKycCase" -v n
dotnet test tests/KYC.Integration.Tests --filter "FullyQualifiedName~ComplianceHandlers" -v n
dotnet test tests/KYC.Web.Integration.Tests --filter "FullyQualifiedName~IdentityWebhook" -v n

Write-Host "=== 2/3 Testes PostgreSQL (HomologationE2e) ===" -ForegroundColor Cyan
dotnet test tests/KYC.Web.Integration.Tests --filter "FullyQualifiedName~HomologationE2e" -v n

Write-Host "=== 3/3 Suite completa (opcional) ===" -ForegroundColor Cyan
dotnet test -v n

Write-Host "Concluído. Actualize docs/dossier/09-e2e/ com o relatório." -ForegroundColor Green
