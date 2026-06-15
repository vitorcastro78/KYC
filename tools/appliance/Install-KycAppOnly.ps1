#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Instala ou actualiza apenas KYC (App + Workers) em C:\Platform.
    PostgreSQL, Ollama e IIS devem existir (use Install-KYCAppliance.ps1 na 1.ª vez).
#>
[CmdletBinding()]
param(
    [string] $InstallRoot = "C:\Platform",
    [string] $SourceDir = $PSScriptRoot,
    [string] $KestrelUrl = "http://127.0.0.1:5000"
)

$ErrorActionPreference = "Stop"

function Install-WinService {
    param([string]$Name, [string]$Display, [string]$Exe, [string]$Depends, [hashtable]$Env)
    $svc = Get-Service $Name -ErrorAction SilentlyContinue
    if ($svc -and $svc.Status -ne "Stopped") { Stop-Service $Name -Force; Start-Sleep 2 }
    if ($svc) { sc.exe delete $Name | Out-Null; Start-Sleep 1 }
    New-Service -Name $Name -DisplayName $Display -BinaryPathName "`"$Exe`"" -StartupType Automatic | Out-Null
    if ($Depends) { sc.exe config $Name depend= $Depends | Out-Null }
    $reg = "HKLM:\SYSTEM\CurrentControlSet\Services\$Name\Environment"
    New-Item $reg -Force | Out-Null
    Set-ItemProperty $reg -Name ASPNETCORE_ENVIRONMENT -Value Production
    foreach ($k in $Env.Keys) { Set-ItemProperty $reg -Name $k -Value $Env[$k] }
}

$appSrc = Join-Path $SourceDir "App"
$workersSrc = Join-Path $SourceDir "Workers"
if (-not (Test-Path $appSrc)) { throw "Pasta App nao encontrada em $SourceDir" }

foreach ($d in @($InstallRoot, "$InstallRoot\App", "$InstallRoot\Workers")) {
    New-Item -ItemType Directory -Path $d -Force | Out-Null
}

if (Test-Path "$InstallRoot\App") { Remove-Item "$InstallRoot\App\*" -Recurse -Force -ErrorAction SilentlyContinue }
if (Test-Path "$InstallRoot\Workers") { Remove-Item "$InstallRoot\Workers\*" -Recurse -Force -ErrorAction SilentlyContinue }
Copy-Item "$appSrc\*" "$InstallRoot\App" -Recurse -Force
Copy-Item "$workersSrc\*" "$InstallRoot\Workers" -Recurse -Force

$pgService = (Get-Service postgresql* -ErrorAction SilentlyContinue | Select-Object -First 1).Name
if (-not $pgService) { Write-Warning "PostgreSQL nao detectado. Instale com Install-KYCAppliance.ps1" }

Install-WinService -Name KycPlatformApp -Display "KYC Platform (Web)" `
    -Exe "$InstallRoot\App\KYC.Web.exe" -Depends $pgService `
    -Env @{ ASPNETCORE_URLS = $KestrelUrl }

Install-WinService -Name KycPlatformWorkers -Display "KYC Platform (Workers)" `
    -Exe "$InstallRoot\Workers\KYC.Workers.exe" -Depends $pgService -Env @{}

Start-Service KycPlatformApp -ErrorAction SilentlyContinue
Start-Sleep 3
& "$InstallRoot\App\KYC.Web.exe" --migrate-only
Start-Service KycPlatformWorkers -ErrorAction SilentlyContinue

Write-Host "KYC instalado em $InstallRoot" -ForegroundColor Green
