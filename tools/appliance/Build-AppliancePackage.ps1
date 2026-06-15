#Requires -Version 5.1
<#
.SYNOPSIS
    Publica KYC.Web + Workers (self-contained win-x64) e cria kyc-platform-{version}.zip
.EXAMPLE
    .\Build-AppliancePackage.ps1 -Version 1.0.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Version,

    [string] $OutputDir = (Join-Path $PSScriptRoot "dist"),
    [switch] $SingleFile
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$staging = Join-Path $env:TEMP "kyc-appliance-$Version"
$publishBase = @(
    "-c", "Release",
    "-r", "win-x64",
    "--self-contained", "true",
    "-p:PublishSingleFile=$($SingleFile.IsPresent)"
)

if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging -Force | Out-Null

Write-Host "Publicar KYC.Web..." -ForegroundColor Cyan
dotnet publish (Join-Path $repoRoot "src\KYC.Web\KYC.Web.csproj") @publishBase `
    -o (Join-Path $staging "App")
Remove-Item (Join-Path $staging "App\appsettings.Development.json") -ErrorAction SilentlyContinue

Write-Host "Publicar KYC.Workers..." -ForegroundColor Cyan
dotnet publish (Join-Path $repoRoot "src\KYC.Workers\KYC.Workers.csproj") @publishBase `
    -o (Join-Path $staging "Workers")

Write-Host "efbundle (migrations, opcional)..." -ForegroundColor Cyan
$efBundle = Join-Path $staging "App\efbundle.exe"
& dotnet ef migrations bundle `
    --project (Join-Path $repoRoot "src\KYC.Infrastructure") `
    --startup-project (Join-Path $repoRoot "src\KYC.Web") `
    --context KycDbContext `
    -o $efBundle `
    --self-contained -r win-x64 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Warning "efbundle ignorado. Migrations via KYC.Web.exe --migrate-only."
}

Set-Content -Path (Join-Path $staging "version.txt") -Value $Version -Encoding ASCII

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
$zipPath = Join-Path $OutputDir "kyc-platform-$Version.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $staging "*") -DestinationPath $zipPath -CompressionLevel Optimal

Remove-Item $staging -Recurse -Force
Write-Host "Pacote: $zipPath ($([math]::Round((Get-Item $zipPath).Length / 1MB, 1)) MB)" -ForegroundColor Green
