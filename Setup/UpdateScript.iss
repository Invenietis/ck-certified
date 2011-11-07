; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define CKVersion ReadIni(SourcePath + "\\BuildInfo.ini","Info","Version","0");
#define ApplicationName ReadIni(SourcePath + "\\BuildInfo.ini","Info","ApplicationName","0");
#define PackageName ReadIni(SourcePath + "\\BuildInfo.ini","Info","PackageName","0");
#define UpdateServerUrl ReadIni(SourcePath + "\\BuildInfo.ini","Update","UpdateServerUrl","0");
#define UpdaterRunningKey ReadIni(SourcePath + "\\BuildInfo.ini","Update","UpdaterRunningKey","0");
#define OverridePreviousContext ReadIni(SourcePath + "\\BuildInfo.ini","Update","OverridePreviousContext","false");;

; Read the previuos build number. If there is none take 0 instead.
;#define BuildNum String(ReadIni(SourcePath	+ "\\BuildInfo.ini","Info","Build","0"))
; Increment the build number by one.
;#expr BuildNum = BuildNum + 1
; Store the number in the ini file for the next build.
;#expr WriteIni(SourcePath + "\\BuildInfo.ini","Info","Build", BuildNum)

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{79CA0471-5C7D-4E7E-949D-5B29BBB4E960}}
AppName={#PackageName}
AppVersion={#CKVersion}
AppPublisher=Invenietis
AppPublisherURL=http://www.invenietis.com/
DefaultDirName={pf}\CiviKey\
DefaultGroupName=CiviKey
AllowNoIcons=yes
OutputDir=..\Setup\
OutputBaseFilename={#PackageName}-{#CKVersion}
Compression=lzma
SolidCompression=yes
SetupIconFile=CiviKey.ico
UninstallDisplayIcon={app}\resources\CiviKey.ico
AppMutex=CiviKeyMutex
VersionInfoVersion={#CKVersion}
VersionInfoProductName={#PackageName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Files]
Source: "..\Output\Release\*"; DestDir: "{app}\binaries"; Excludes: "*.pdb, *.xml, *.ck, *.vshost.exe, *.manifest, *.iss, \Setup, \Tests"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "CiviKey.ico"; DestDir: "{app}\resources"
Source: "CiviKeyPostInstallScript.exe"; DestDir: "{tmp}"; Flags: ignoreversion
Source: System.config.ck; DestDir: {commonappdata}\{#ApplicationName}\2.5\; Flags: onlyifdoesntexist; Permissions: everyone-full; 
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\CiviKey"; Filename: "{app}\binaries\CiviKey.exe"; WorkingDir: "{app}"; IconFileName:"{app}\resources\CiviKey.ico";
Name: "{commondesktop}\CiviKey"; Filename: "{app}\binaries\CiviKey.exe"; WorkingDir: "{app}"; IconFileName:"{app}\resources\CiviKey.ico"

[Run]
Filename: "{tmp}\CiviKeyPostInstallScript.exe"; Parameters: """{#CKVersion}"" ""{#ApplicationName}"" ""{#PackageName}"" ""{#UpdateServerUrl}"" ""{#UpdaterRunningKey}"" ""{#OverridePreviousContext}"" "

[Code]
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, serviceCount: cardinal;
    success: boolean;
begin
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;
    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;
    // .NET 4.0 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;
    result := success and (install = 1) and (serviceCount >= service);
end;

function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v3.5', 1) then begin
        MsgBox('CiviKey requires Microsoft .NET Framework 3.5 SP1.'#13#13
            'Please use Windows Update to install this version,'#13
            'and then re-run the CiviKey setup program.', mbInformation, MB_OK);
        result := false;
    end else
        result := true;
end;

