#Requires -RunAsAdministrator
<#
  Instala KYC + PostgreSQL + Ollama no Windows (sem Docker).
  HTTPS via IIS (porta 443) com dominio interno (ex: kyc.empresa.local).

  Outros PCs na LAN acedem: https://kyc.empresa.local
  (registo DNS A -> IP do servidor; certificado inclui dominio + IP)

.EXAMPLE
  .\Install-KYCAppliance.ps1 -ConfigPath .\install-config.json -KycInstallerPath C:\Downloads\KycPlatform-1.0.0-setup.exe

.EXAMPLE
  .\Install-FromGitHubRelease.ps1 -ConfigPath .\install-config.json
#>
[CmdletBinding()]
param(
    [string] $ConfigPath = (Join-Path $PSScriptRoot "install-config.json"),
    [string] $KycPackagePath = "",
    [string] $KycInstallerPath = "",
    [string] $InternalDomain = "",
    [string] $ServerIp = "",
    [switch] $SkipDownloads
)

$ErrorActionPreference = "Stop"

function Write-Step($m) { Write-Host "`n==> $m" -ForegroundColor Cyan }
function New-Password([int]$n=32) { -join ((48..57)+(65..90)+(97..122) | Get-Random -Count $n | ForEach-Object {[char]$_}) }

if (-not (Test-Path $ConfigPath)) {
    throw "Copie install-config.example.json para install-config.json e defina Hosting.InternalDomain"
}
$config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
$root = $config.PlatformRoot
$domain = if ($InternalDomain) { $InternalDomain } else { $config.Hosting.InternalDomain }
$serverIp = if ($ServerIp) { $ServerIp } else { $config.Hosting.ServerIp }
$httpsPort = [int]$config.Hosting.HttpsPort
$kestrelUrl = $config.Hosting.KestrelUrl
$downloads = Join-Path $root "Downloads"
$certsDir = Join-Path $root "Certs"
$iisSitePath = "C:\inetpub\kycplatform"

if ([string]::IsNullOrWhiteSpace($domain)) {
    throw "Defina Hosting.InternalDomain (ex: kyc.empresa.local)"
}

foreach ($d in @($root, $downloads, "$root\App", "$root\Workers", "$root\Models", "$root\Config", $certsDir, $iisSitePath)) {
    New-Item -ItemType Directory -Path $d -Force | Out-Null
}

function Install-IisPrereqs {
    param($Config, [string] $DownloadDir, [switch] $SkipDownloads)

    Write-Step "IIS + URL Rewrite + ARR"
    if (Get-Command Install-WindowsFeature -ErrorAction SilentlyContinue) {
        Install-WindowsFeature -Name Web-Server, Web-WebSockets, Web-Mgmt-Console -IncludeManagementTools | Out-Null
    } else {
        $features = @(
            "IIS-WebServerRole", "IIS-WebServer", "IIS-WebSockets",
            "IIS-ManagementConsole", "IIS-HttpRedirect", "IIS-StaticContent"
        )
        foreach ($f in $features) {
            Enable-WindowsOptionalFeature -Online -FeatureName $f -All -NoRestart -ErrorAction SilentlyContinue | Out-Null
        }
    }

    foreach ($item in @(
        @{ Url = $Config.Downloads.UrlRewriteMsiUrl; File = "rewrite_amd64.msi" },
        @{ Url = $Config.Downloads.ArrMsiUrl; File = "requestRouter_amd64.msi" }
    )) {
        $path = Join-Path $DownloadDir $item.File
        if (-not (Test-Path $path)) {
            if ($SkipDownloads) { throw "MSI em falta: $path" }
            Invoke-WebRequest -Uri $item.Url -OutFile $path -UseBasicParsing
        }
        if (-not (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*" -ErrorAction SilentlyContinue |
                Where-Object { $_.DisplayName -like "*$($item.File)*" })) {
            Start-Process msiexec.exe -ArgumentList "/i `"$path`" /quiet /norestart" -Wait
        }
    }

    Import-Module WebAdministration -ErrorAction Stop
    Set-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" -Filter "system.webServer/proxy" -Name "enabled" -Value "True"
}

function New-KycTlsCertificate {
    param([string] $Domain, [string] $Ip, [string] $OutDir)

    $certPassword = New-Password 24
    $sanParts = @("DNS=$Domain")
    if (-not [string]::IsNullOrWhiteSpace($Ip)) { $sanParts += "IPAddress=$Ip" }

    $cert = New-SelfSignedCertificate `
        -Subject "CN=$Domain" `
        -DnsName $Domain `
        -TextExtension @(
            "2.5.29.37={text}1.3.6.1.5.5.7.3.1",
            "2.5.29.17={text}$($sanParts -join '&')"
        ) `
        -CertStoreLocation "Cert:\LocalMachine\My" `
        -NotAfter (Get-Date).AddYears(10) `
        -KeyAlgorithm RSA -KeyLength 4096

    $pfxPath = Join-Path $OutDir "kyc-platform.pfx"
    $pwd = ConvertTo-SecureString $certPassword -AsPlainText -Force
    Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $pwd | Out-Null
    Export-Certificate -Cert $cert -FilePath (Join-Path $OutDir "kyc-platform.cer") | Out-Null

    @{ PfxPath = $pfxPath; Password = $certPassword; Thumbprint = $cert.Thumbprint } 
}

function Install-IisHttpsSite {
    param(
        [string] $SiteName,
        [string] $PhysicalPath,
        [string] $Domain,
        [int] $Port,
        [string] $CertThumbprint,
        [string] $WebConfigSource
    )

    Copy-Item $WebConfigSource -Destination $PhysicalPath -Force

    if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
        Remove-Website -Name $SiteName
    }
    if (Get-Website -Name "Default Web Site" -ErrorAction SilentlyContinue) {
        Stop-Website -Name "Default Web Site" -ErrorAction SilentlyContinue
    }

    New-Website -Name $SiteName -PhysicalPath $PhysicalPath -Port $Port -Ssl -HostHeader $Domain | Out-Null

    $binding = Get-WebBinding -Name $SiteName -Protocol "https" -HostHeader $Domain -ErrorAction SilentlyContinue
    if ($binding) {
        $binding.AddSslCertificate($CertThumbprint, "my")
    }

    # Permitir serverVariables do reverse proxy (IIS + ARR)
    Add-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" `
        -Filter "system.webServer/rewrite/allowedServerVariables" -Name "." `
        -Value @{ name = "HTTP_X_FORWARDED_PROTO" } -ErrorAction SilentlyContinue
    Add-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" `
        -Filter "system.webServer/rewrite/allowedServerVariables" -Name "." `
        -Value @{ name = "HTTP_X_FORWARDED_HOST" } -ErrorAction SilentlyContinue

    Start-Website -Name $SiteName
}

# --- PostgreSQL ---
Write-Step "PostgreSQL"
$pgRoot = @("17","16") | ForEach-Object { "C:\Program Files\PostgreSQL\$_" } | Where-Object { Test-Path "$_\bin\psql.exe" } | Select-Object -First 1
if (-not $pgRoot) {
    $pgExe = Join-Path $downloads "postgresql-installer.exe"
    if (-not $SkipDownloads) { Invoke-WebRequest $config.Downloads.PostgreSqlInstallerUrl -OutFile $pgExe -UseBasicParsing }
    $pgSuper = New-Password
    Start-Process $pgExe -ArgumentList @("--mode","unattended","--superpassword",$pgSuper,"--serverport","5432","--servicename","postgresql-x64-16","--install_runtimes","0") -Wait
    $pgRoot = "C:\Program Files\PostgreSQL\16"
    @{ PostgresSuperPassword = $pgSuper } | ConvertTo-Json | Set-Content "$root\Config\secrets.json"
}

$psql = Join-Path $pgRoot "bin\psql.exe"
$secrets = if (Test-Path "$root\Config\secrets.json") { Get-Content "$root\Config\secrets.json" | ConvertFrom-Json } else { $null }
if (-not $secrets.PostgresSuperPassword) {
    Write-Host "Password do utilizador postgres:" -ForegroundColor Yellow
    $secure = Read-Host -AsSecureString
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { $pgSuper = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr) } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
} else { $pgSuper = $secrets.PostgresSuperPassword }

$dbPass = New-Password 48
$dbName = $config.PostgreSql.DatabaseName
$dbUser = $config.PostgreSql.AppUser
$env:PGPASSWORD = $pgSuper
& $psql -h 127.0.0.1 -U postgres -c "CREATE ROLE $dbUser WITH LOGIN PASSWORD '$dbPass';" 2>$null
& $psql -h 127.0.0.1 -U postgres -c "CREATE DATABASE $dbName OWNER $dbUser;" 2>$null
$env:PGPASSWORD = $null
$pgService = (Get-Service postgresql* | Select-Object -First 1).Name

# --- Ollama ---
Write-Step "Ollama"
if (-not (Test-Path "$env:LOCALAPPDATA\Programs\Ollama\ollama.exe")) {
    $ollamaExe = Join-Path $downloads "OllamaSetup.exe"
    if (-not $SkipDownloads) { Invoke-WebRequest $config.Downloads.OllamaInstallerUrl -OutFile $ollamaExe -UseBasicParsing }
    Start-Process $ollamaExe -ArgumentList "/S" -Wait
}
[Environment]::SetEnvironmentVariable("OLLAMA_MODELS", "$root\Models", "Machine")
Set-Service OllamaService -StartupType Automatic -ErrorAction SilentlyContinue
Start-Service OllamaService -ErrorAction SilentlyContinue

# --- KYC ---
Write-Step "KYC (Web + Workers)"
$installer = $KycInstallerPath
if (-not $installer -and $config.Downloads.KycInstallerUrl) {
    $installer = Join-Path $downloads "KycPlatform-setup.exe"
    if (-not (Test-Path $installer) -and -not $SkipDownloads) {
        Invoke-WebRequest $config.Downloads.KycInstallerUrl -OutFile $installer -UseBasicParsing
    }
}
if ($installer -and (Test-Path $installer)) {
    Write-Host "Instalador: $installer (/NORUN = so ficheiros; servicos configurados abaixo)" -ForegroundColor Gray
    Start-Process $installer -ArgumentList @("/VERYSILENT", "/SUPPRESSMSGBOXES", "/DIR=$root", "/NORUN") -Wait
} else {
    $zip = $KycPackagePath
    if (-not $zip) {
        $zip = Join-Path $downloads "kyc-platform.zip"
        if (-not (Test-Path $zip) -and -not $SkipDownloads) {
            if (-not $config.Downloads.KycPackageUrl) { throw "Defina Downloads.KycInstallerUrl ou KycPackageUrl" }
            Invoke-WebRequest $config.Downloads.KycPackageUrl -OutFile $zip -UseBasicParsing
        }
    }
    if (-not (Test-Path $zip)) { throw "Pacote KYC em falta: $zip" }
    $extract = Join-Path $downloads "extract"
    Remove-Item $extract -Recurse -Force -ErrorAction SilentlyContinue
    Expand-Archive $zip $extract -Force
    Copy-Item "$extract\App" "$root\App" -Recurse -Force
    Copy-Item "$extract\Workers" "$root\Workers" -Recurse -Force
}

$conn = "Host=127.0.0.1;Port=5432;Database=$dbName;Username=$dbUser;Password=$dbPass"
$appSettings = @{
    Hosting = @{ BehindReverseProxy = $true }
    ConnectionStrings = @{ KycDatabase = $conn }
    LLM = @{ UseOllamaForScoring = $true; LocalEndpoint = "http://127.0.0.1:11434" }
    Auth = @{ AdminEmail = $config.Auth.AdminEmail; AdminPassword = $config.Auth.AdminPassword }
    DataProtection = @{ KeysPath = "$root\App\DataProtection-Keys" }
} | ConvertTo-Json -Depth 5
$appSettings | Set-Content "$root\App\appsettings.Production.json"
Copy-Item "$root\App\appsettings.Production.json" "$root\Workers\appsettings.Production.json"

function Install-WinService {
    param([string]$Name, [string]$Display, [string]$Exe, [string]$Depends, [hashtable]$Env)
    Get-Service $Name -ErrorAction SilentlyContinue | ForEach-Object {
        Stop-Service $Name -Force -ErrorAction SilentlyContinue
        sc.exe delete $Name | Out-Null
    }
    New-Service -Name $Name -DisplayName $Display -BinaryPathName "`"$Exe`"" -StartupType Automatic | Out-Null
    if ($Depends) { sc.exe config $Name depend= $Depends | Out-Null }
    $reg = "HKLM:\SYSTEM\CurrentControlSet\Services\$Name\Environment"
    New-Item $reg -Force | Out-Null
    Set-ItemProperty $reg -Name ASPNETCORE_ENVIRONMENT -Value Production
    foreach ($k in $Env.Keys) { Set-ItemProperty $reg -Name $k -Value $Env[$k] }
}

# Kestrel so localhost; IIS expoe HTTPS na LAN
Install-WinService -Name KycPlatformApp -Display "KYC Platform (Web)" `
    -Exe "$root\App\KYC.Web.exe" -Depends $pgService `
    -Env @{ ASPNETCORE_URLS = $kestrelUrl }

Install-WinService -Name KycPlatformWorkers -Display "KYC Platform (Workers)" `
    -Exe "$root\Workers\KYC.Workers.exe" -Depends $pgService -Env @{}

Start-Service KycPlatformApp
Start-Sleep 5
& "$root\App\KYC.Web.exe" --migrate-only
Start-Service KycPlatformWorkers

# --- IIS HTTPS + dominio interno ---
Write-Step "IIS HTTPS ($domain)"
Install-IisPrereqs -Config $config -DownloadDir $downloads -SkipDownloads:$SkipDownloads
$certInfo = New-KycTlsCertificate -Domain $domain -Ip $serverIp -OutDir $certsDir
$webConfigSrc = Join-Path $PSScriptRoot "iis\web.config"
Install-IisHttpsSite -SiteName "KycPlatform" -PhysicalPath $iisSitePath -Domain $domain `
    -Port $httpsPort -CertThumbprint $certInfo.Thumbprint -WebConfigSource $webConfigSrc

Get-NetFirewallRule -DisplayName "KYC Platform HTTPS" -ErrorAction SilentlyContinue | Remove-NetFirewallRule -ErrorAction SilentlyContinue
New-NetFirewallRule -DisplayName "KYC Platform HTTPS" -Direction Inbound -Protocol TCP -LocalPort $httpsPort -Action Allow | Out-Null

$httpsUrl = "https://$domain"
if ($httpsPort -ne 443) { $httpsUrl = "https://${domain}:$httpsPort" }

Write-Host ""
Write-Host "Instalacao concluida." -ForegroundColor Green
Write-Host ""
Write-Host "1. DNS interno (Active Directory ou router):" -ForegroundColor Yellow
Write-Host "     $domain  ->  $serverIp" -ForegroundColor White
Write-Host ""
Write-Host "2. De outros PCs na rede, abra:" -ForegroundColor Yellow
Write-Host "     $httpsUrl" -ForegroundColor White
Write-Host ""
Write-Host "3. Certificado (primeira vez no browser):" -ForegroundColor Yellow
Write-Host "     Importe em cada PC: $certsDir\kyc-platform.cer" -ForegroundColor White
Write-Host "     (Certificados -> Autoridades de certificacao fidedignas -> Importar)" -ForegroundColor Gray
Write-Host ""
Write-Host "Health (local): http://127.0.0.1:5000/health" -ForegroundColor Gray
Write-Host "Admin: $($config.Auth.AdminEmail) / $($config.Auth.AdminPassword)" -ForegroundColor Gray
