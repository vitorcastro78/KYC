#Requires -RunAsAdministrator
<#
  Instala ficheiros da extensão pgvector (build não oficial) em PostgreSQL 17 EDB.
  Fonte: https://github.com/andreiramani/pgvector_pgsql_windows/releases
  Depois: reinicie o serviço postgresql-x64-17 e execute CREATE EXTENSION vector na base kyc_dev.
#>
$ErrorActionPreference = "Stop"
$pgRoot = "${env:ProgramFiles}\PostgreSQL\17"
if (-not (Test-Path "$pgRoot\bin\psql.exe")) {
    Write-Error "PostgreSQL 17 não encontrado em $pgRoot"
}
$zip = Join-Path $env:TEMP "vector.v0.8.2-pg17.zip"
$uri = "https://github.com/andreiramani/pgvector_pgsql_windows/releases/download/0.8.2_17.6/vector.v0.8.2-pg17.zip"
Write-Host "A descarregar $uri ..."
Invoke-WebRequest -Uri $uri -OutFile $zip -UseBasicParsing
$dest = Join-Path $env:TEMP "pgvector-win-install"
if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
Expand-Archive -Path $zip -DestinationPath $dest -Force
Copy-Item "$dest\lib\vector.dll" "$pgRoot\lib\" -Force
Copy-Item "$dest\share\extension\*" "$pgRoot\share\extension\" -Force
Write-Host "pgvector copiado para $pgRoot. Reinicie o serviço: Restart-Service postgresql-x64-17"
