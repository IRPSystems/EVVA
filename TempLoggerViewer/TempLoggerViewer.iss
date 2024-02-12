[Setup]
AppName=TempLoggerViewer
AppVersion=1
WizardStyle=modern
DefaultDirName={autopf}\TempLoggerViewer
DefaultGroupName=TempLoggerViewer
SourceDir=C:\Projects\Evva\TempLoggerViewer\bin\Release\net6.0-windows
OutputDir=C:\Projects\Evva\TempLoggerViewer\Output
OutputBaseFilename=TempLoggerViewerSetup

[Files]
Source: "*.*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\TempLoggerViewer"; Filename: "{app}\TempLoggerViewer.exe"
Name: "{commondesktop}\TempLoggerViewer" ; Filename: "{app}\TempLoggerViewer.exe"

[Code]

procedure InitializeWizard;
 Begin
 DelTree(ExpandConstant('{autopf}') + '\TempLoggerViewer\Data', True, True, True) ;
 
End;