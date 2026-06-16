; Inno Setup - gera KycPlatform-{version}-setup.exe
; Requer: https://jrsoftware.org/isinfo.php (Inno Setup 6)
; Compilar: ISCC.exe KycPlatform.iss /DAppVersion=1.0.0 /O"C:\path\to\dist"

#ifndef AppVersion
#define AppVersion "1.0.0"
#endif

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-KYCPLATFORM01}
AppName=KYC Platform
AppVersion={#AppVersion}
AppPublisher=VedAI
DefaultDirName=C:\Platform
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=admin
OutputDir=..\dist
OutputBaseFilename=KycPlatform-{#AppVersion}-setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "..\dist\staging\App\*"; DestDir: "{app}\App"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\dist\staging\Workers\*"; DestDir: "{app}\Workers"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Install-KycAppOnly.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\version.txt"; DestDir: "{app}"; Flags: ignoreversion

[Run]
Filename: "powershell.exe"; \
  Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\Install-KycAppOnly.ps1"" -InstallRoot ""{app}"" -SourceDir ""{app}"""; \
  StatusMsg: "A configurar servicos KYC..."; \
  Flags: runhidden waituntilterminated

[UninstallRun]
Filename: "powershell.exe"; \
  Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""Get-Service KycPlatformApp,KycPlatformWorkers -EA 0 | ForEach-Object {{ Stop-Service $_ -Force; sc.exe delete $_.Name }}"""; \
  RunOnceId: "RemoveKycServices"; \
  Flags: runhidden waituntilterminated

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;
