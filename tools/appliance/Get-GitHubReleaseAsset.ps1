#Requires -Version 5.1
<#
.SYNOPSIS
    Descarrega um asset de GitHub Releases (API ou URL directa).
.EXAMPLE
    .\Get-GitHubReleaseAsset.ps1 -Owner vedai -Repo KYC -Tag v1.0.0 -AssetPattern "KycPlatform-*-setup.exe"
#>
[CmdletBinding()]
param(
    [string] $Owner = "",
    [string] $Repo = "",
    [string] $Tag = "latest",
    [string] $AssetPattern = "KycPlatform-*-setup.exe",
    [string] $DirectUrl = "",
    [string] $OutFile = "",
    [string] $Token = $env:GITHUB_TOKEN
)

$ErrorActionPreference = "Stop"

function Test-AssetMatch {
    param([string] $Name, [string] $Pattern)
    if ($Pattern -match '[\*\?]') {
        return ($Name -like $Pattern)
    }
    return ($Name -eq $Pattern)
}

if ($DirectUrl) {
    $downloadUrl = $DirectUrl
    if (-not $OutFile) {
        $OutFile = Join-Path $env:TEMP ([IO.Path]::GetFileName(($DirectUrl -split '\?')[0]))
    }
} else {
    if (-not $Owner -or -not $Repo) { throw "Defina Owner/Repo ou DirectUrl" }

    $headers = @{
        Accept        = "application/vnd.github+json"
        "User-Agent"  = "KYC-Appliance-Installer"
        "X-GitHub-Api-Version" = "2022-11-28"
    }
    if ($Token) { $headers.Authorization = "Bearer $Token" }

    $apiBase = "https://api.github.com/repos/$Owner/$Repo/releases"
    $releaseUrl = if ($Tag -eq "latest") { "$apiBase/latest" } else { "$apiBase/tags/$Tag" }

    Write-Host "GitHub Release: $releaseUrl" -ForegroundColor Cyan
    $release = Invoke-RestMethod -Uri $releaseUrl -Headers $headers
    $asset = $release.assets | Where-Object { Test-AssetMatch $_.name $AssetPattern } | Select-Object -First 1
    if (-not $asset) {
        $names = ($release.assets | ForEach-Object { $_.name }) -join ", "
        throw "Asset '$AssetPattern' nao encontrado em $($release.tag_name). Disponiveis: $names"
    }

    $downloadUrl = $asset.browser_download_url
    if (-not $OutFile) { $OutFile = Join-Path $env:TEMP $asset.name }
    Write-Host "Asset: $($asset.name) ($([math]::Round($asset.size / 1MB, 1)) MB)" -ForegroundColor Gray
}

if (Test-Path $OutFile) { Remove-Item $OutFile -Force }
$dlHeaders = @{ "User-Agent" = "KYC-Appliance-Installer" }
if ($Token) { $dlHeaders.Authorization = "Bearer $Token" }
Invoke-WebRequest -Uri $downloadUrl -OutFile $OutFile -Headers $dlHeaders -UseBasicParsing

Write-Host "Descarregado: $OutFile" -ForegroundColor Green
$OutFile
