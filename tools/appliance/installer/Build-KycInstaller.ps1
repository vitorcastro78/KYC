#Requires -Version 5.1
<#
.SYNOPSIS
    Gera kyc-platform.zip e KycPlatform-{version}-setup.exe (Inno Setup).
.EXAMPLE
    .\Build-KycInstaller.ps1 -Version 1.0.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Version,

    [switch] $SkipPublish
)

$ErrorActionPreference = "Stop"
$Version = $Version.Trim()
$applianceRoot = Split-Path $PSScriptRoot -Parent
$dist = (Resolve-Path -LiteralPath (Join-Path $applianceRoot "dist") -ErrorAction SilentlyContinue).Path
if (-not $dist) {
    $dist = Join-Path $applianceRoot "dist"
    New-Item -ItemType Directory -Path $dist -Force | Out-Null
    $dist = (Resolve-Path $dist).Path
}
$staging = Join-Path $dist "staging"
$iss = Join-Path $PSScriptRoot "KycPlatform.iss"

if (-not $SkipPublish) {
    & (Join-Path $applianceRoot "Build-AppliancePackage.ps1") -Version $Version -OutputDir $dist
}

$zip = Join-Path $dist "kyc-platform-$Version.zip"
if (-not (Test-Path $zip)) { throw "Pacote nao encontrado: $zip" }

Write-Host "Preparar staging para Inno Setup..." -ForegroundColor Cyan
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging -Force | Out-Null
Expand-Archive $zip $staging -Force
Set-Content (Join-Path $applianceRoot "version.txt") -Value $Version -Encoding ASCII

$iscc = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    Write-Host ""
    Write-Host "Inno Setup nao encontrado. Instale: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Write-Host "Pacote ZIP pronto para instalacao manual:" -ForegroundColor Yellow
    Write-Host "  $zip" -ForegroundColor White
    Write-Host ""
    Write-Host "Instalacao manual (Admin):" -ForegroundColor Yellow
    Write-Host "  Expand-Archive '$zip' C:\Platform -Force" -ForegroundColor White
    Write-Host "  .\tools\appliance\Install-KycAppOnly.ps1 -InstallRoot C:\Platform -SourceDir C:\Platform" -ForegroundColor White
    return
}

Write-Host "Compilar instalador com Inno Setup..." -ForegroundColor Cyan
Write-Host "  Versao: $Version" -ForegroundColor Gray
Write-Host "  Output: $dist" -ForegroundColor Gray
& $iscc $iss "/DAppVersion=$Version" "/O$dist"
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup terminou com codigo $LASTEXITCODE"
}

$setupExe = Join-Path $dist "KycPlatform-$Version-setup.exe"
if (-not (Test-Path $setupExe)) {
    $setupExe = Get-ChildItem -Path $dist -Filter "KycPlatform-*-setup.exe" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1 -ExpandProperty FullName
}
if (-not $setupExe -or -not (Test-Path $setupExe)) {
    Write-Host "Conteudo de ${dist}:" -ForegroundColor Yellow
    Get-ChildItem -Path $dist -Recurse -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "  $($_.FullName)" }
    throw "Instalador nao encontrado apos compilacao (esperado: KycPlatform-$Version-setup.exe)"
}

Write-Host ""
Write-Host "Instalador criado:" -ForegroundColor Green
Write-Host "  $setupExe ($([math]::Round((Get-Item $setupExe).Length / 1MB, 1)) MB)" -ForegroundColor White
Write-Host ""
Write-Host "Distribuir este ficheiro como o OllamaSetup.exe / postgresql-installer.exe" -ForegroundColor Cyan
Write-Host "Silencioso: $(Split-Path $setupExe -Leaf) /VERYSILENT /SUPPRESSMSGBOXES" -ForegroundColor Gray
