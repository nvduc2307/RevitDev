
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "DRTool"
#define MyAppCondition "2.0"
#define MyAppVersion "1.0"
#define MyAppPublisher "MF TECNICA"
#define AppGUID "{88ee6571-52b8-4153-8232-3179ac80c518}"


[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{2E136E69-3EB7-4A94-92E0-3E7F3AA3BF1F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
//AppPublisherURL={#MyAppURL}
//AppSupportURL={#MyAppURL}
//AppUpdatesURL={#MyAppURL}
DefaultDirName="{userappdata}"
DisableDirPage=yes
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
PrivilegesRequired=lowest
; OutputBaseFilename={#MyAppName} {#MyAppCondition} {#MyAppVersion} 
OutputBaseFilename={#MyAppName} Installer
; OutputBaseFilename={#MyAppName}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
//UninstallDisplayIcon={app}\icon.ico
DisableWelcomePage=no
WizardImageFile=banner.bmp
SetupIconFile=icon.ico
OutputDir="bin\"
CloseApplications=force
//LicenseFile=license.rtf

[Components]
;Name: "Revit2020"; Description: "Add-in Revit 2020"; Types: full
Name: "Revit2021"; Description: "Add-in Revit 2021"; Types: full
Name: "Revit2022"; Description: "Add-in Revit 2022"; Types: full
Name: "Revit2023"; Description: "Add-in Revit 2023"; Types: full
Name: "Revit2024"; Description: "Add-in Revit 2024"; Types: full

[Messages]
WelcomeLabel2=Hello!

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"  
//Name: "japanese"; MessagesFile: "compiler:Languages/Japanese.isl"

[Code]
// Import IsISPackageInstalled() function from UninsIS.dll at setup time
function DLLIsISPackageInstalled(AppId: string;
  Is64BitInstallMode, IsAdminInstallMode: DWORD): DWORD;
  external 'IsISPackageInstalled@files:UninsIS.dll stdcall setuponly';

// Import CompareISPackageVersion() function from UninsIS.dll at setup time
function DLLCompareISPackageVersion(AppId, InstallingVersion: string;
  Is64BitInstallMode, IsAdminInstallMode: DWORD): longint;
  external 'CompareISPackageVersion@files:UninsIS.dll stdcall setuponly';

// Import UninstallISPackage() function from UninsIS.dll at setup time
function DLLUninstallISPackage(AppId: string;
  Is64BitInstallMode, IsAdminInstallMode: DWORD): DWORD;
  external 'UninstallISPackage@files:UninsIS.dll stdcall setuponly';


// Wrapper for UninsIS.dll IsISPackageInstalled() function
// Returns true if package is detected as installed, or false otherwise
function IsISPackageInstalled(): boolean;
  begin
  result := DLLIsISPackageInstalled(
    '{#AppGUID}',                     // AppId
    DWORD(Is64BitInstallMode()),      // Is64BitInstallMode
    DWORD(IsAdminInstallMode())       // IsAdminInstallMode
  ) = 1;
  if result then
    Log('UninsIS.dll - Package detected as installed')
  else
    Log('UninsIS.dll - Package not detected as installed');
  end;

// Wrapper for UninsIS.dll CompareISPackageVersion() function
// Returns:
// < 0 if version we are installing is < installed version
// 0   if version we are installing is = installed version
// > 0 if version we are installing is > installed version
function CompareISPackageVersion(): longint;
  begin
  result := DLLCompareISPackageVersion(
    '{#AppGUID}',                        // AppId
    '{#MyAppVersion}',                     // InstallingVersion
    DWORD(Is64BitInstallMode()),         // Is64BitInstallMode
    DWORD(IsAdminInstallMode())          // IsAdminInstallMode
  );
  if result < 0 then
    Log('UninsIS.dll - This version {#MyAppVersion} older than installed version')
  else if result = 0 then
    Log('UninsIS.dll - This version {#MyAppVersion} same as installed version')
  else
    Log('UninsIS.dll - This version {#MyAppVersion} newer than installed version');
  end;

// Wrapper for UninsIS.dll UninstallISPackage() function
// Returns 0 for success, non-zero for failure
function UninstallISPackage(): DWORD;
  begin
  result := DLLUninstallISPackage(
    '{#AppGUID}',                   // AppId
    DWORD(Is64BitInstallMode()),    // Is64BitInstallMode
    DWORD(IsAdminInstallMode())     // IsAdminInstallMode
  );
  if result = 0 then
    Log('UninsIS.dll - Installed package uninstall completed successfully')
  else
    Log('UninsIS.dll - installed package uninstall did not complete successfully');
  end;


function PrepareToInstall(var NeedsRestart: boolean): string;
  begin
  result := '';
  // If package installed, uninstall it automatically if the version we are
  // installing does not match the installed version; If you want to
  // automatically uninstall only...
  // ...when downgrading: change <> to <
  // ...when upgrading:   change <> to >
  if IsISPackageInstalled() and (CompareISPackageVersion() <> 0) then
    UninstallISPackage();
  end;

[Files] 
Source: "UninsIS.dll"; Flags: dontcopy
;Source: "Content\2020\*"; DestDir: "{app}\Autodesk\Revit\Addins\2020"; Components: Revit2020; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Content\2021\*"; DestDir: "{app}\Autodesk\Revit\Addins\2021"; Components: Revit2021; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Content\2022\*"; DestDir: "{app}\Autodesk\Revit\Addins\2022"; Components: Revit2022; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Content\2023\*"; DestDir: "{app}\Autodesk\Revit\Addins\2023"; Components: Revit2023; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Content\2024\*"; DestDir: "{app}\Autodesk\Revit\Addins\2024"; Components: Revit2024; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: "Content\Manual\*"; DestDir: "{app}\Autodesk\Revit\RIM-T"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
