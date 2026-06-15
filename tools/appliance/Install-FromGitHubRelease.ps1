#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Instala o KYC a partir de um release GitHub (instalador .exe gerado pelo CI).

.EXAMPLE
    # Setup completo (Postgres + Ollama + IIS + KYC do GitHub)
    .\Install-FromGitHubRelease.ps1 -ConfigPath .\install-config.json

.EXAMPLE
    # So actualizar KYC (Postgres/Ollama ja instalados)
    .\Install-FromGitHubRelease.ps1 -UpdateOnly -Tag v1.0.0

.EXAMPLE
    # URL directa do asset (sem API GitHub)
    .\Install-FromGitHubRelease.ps1 -UpdateOnly -DirectUrl "https://github.com/org/KYC/releases/download/v1.0.0/KycPlatform-1.0.0-setup.exe"
#>
[CmdletBinding(DefaultParameterSetName = "FullAppliance")]
param(
    [string] $ConfigPath = (Join-Path $PSScriptRoot "install-config.json"),

    [Parameter(ParameterSetName = "FullAppliance")]
    [switch] $FullAppliance,

    [Parameter(ParameterSetName = "UpdateOnly")]
    [switch] $UpdateOnly,

    [string] $Owner = "",
    [string] $Repo = "",
    [string] $Tag = "",
    [string] $AssetPattern = "KycPlatform-*-setup.exe",
    [string] $DirectUrl = "",
    [string] $Token = $env:GITHUB_TOKEN,
    [switch] $SkipDownloads
)

$ErrorActionPreference = "Stop"

if (-not $UpdateOnly) { $FullAppliance = $true }

$config = $null
if (Test-Path $ConfigPath) {
    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
}

$gh = $null
if ($config -and $config.Downloads) {
    $gh = $config.Downloads.GitHub
    if (-not $DirectUrl -and $config.Downloads.KycInstallerUrl) { $DirectUrl = $config.Downloads.KycInstallerUrl }
}

if (-not $Owner -and $gh) { $Owner = $gh.Owner }
if (-not $Repo -and $gh) { $Repo = $gh.Repo }
if (-not $Tag -and $gh) { $Tag = $gh.Tag }
if ($gh -and $gh.AssetPattern) { $AssetPattern = $gh.AssetPattern }

if (-not $Tag) { $Tag = "latest" }

if (-not $DirectUrl -and (-not $Owner -or -not $Repo)) {
    throw "Defina Downloads.GitHub (Owner/Repo) ou Downloads.KycInstallerUrl em install-config.json, ou passe -DirectUrl"
}

$downloadsDir = if ($config -and $config.PlatformRoot) {
    Join-Path $config.PlatformRoot "Downloads"
} else {
    Join-Path $env:TEMP "kyc-downloads"
}
New-Item -ItemType Directory -Path $downloadsDir -Force | Out-Null

$installerPath = & (Join-Path $PSScriptRoot "Get-GitHubReleaseAsset.ps1") `
    -Owner $Owner -Repo $Repo -Tag $Tag -AssetPattern $AssetPattern `
    -DirectUrl $DirectUrl -Token $Token `
    -OutFile (Join-Path $downloadsDir "KycPlatform-setup.exe")

if ($UpdateOnly) {
    Write-Host "Instalar/atualizar KYC (silencioso)..." -ForegroundColor Cyan
    $installRoot = if ($config -and $config.PlatformRoot) { $config.PlatformRoot } else { "C:\Platform" }
    $args = @("/VERYSILENT", "/SUPPRESSMSGBOXES", "/DIR=$installRoot")
    Start-Process $installerPath -ArgumentList $args -Wait
    Write-Host "KYC actualizado em $installRoot" -ForegroundColor Green
    return
}

& (Join-Path $PSScriptRoot "Install-KYCAppliance.ps1") `
    -ConfigPath $ConfigPath `
    -KycInstallerPath $installerPath `
    -SkipDownloads:$SkipDownloads
