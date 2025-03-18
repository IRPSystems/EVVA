[Setup]
AppName=EVVA
AppVersion=1
WizardStyle=modern
DefaultDirName={autopf}\EVVA
DefaultGroupName=EVVA

SourceDir=C:\Projects\Evva_1.4.6.0\Evva\bin\Release\net6.0-windows
OutputDir=C:\Projects\Evva_1.4.6.0\Evva\Output
OutputBaseFilename=EvvaSetup

[Files]
Source: "*.*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\EVVA"; Filename: "{app}\Evva.exe"
Name: "{commondesktop}\EVVA" ; Filename: "{app}\Evva.exe"

[Code]

procedure InitializeWizard;
 Begin
 DelTree(ExpandConstant('{autopf}') + '\EVVA\Data', True, True, True) ;
 
End;