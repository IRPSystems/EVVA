[Setup]
AppName=ParamLimitsTest
AppVersion=1
WizardStyle=modern
DefaultDirName={autopf}\ParamLimitsTest
DefaultGroupName=ParamLimitsTest
SourceDir=C:\Projects\Evva\ParamLimitsTest\bin\Release\net6.0-windows
OutputDir=C:\Projects\Evva\ParamLimitsTest\Output
OutputBaseFilename=ParamLimitsTestSetup

[Files]
Source: "*.*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\ParamLimitsTest"; Filename: "{app}\ParamLimitsTest.exe"
Name: "{commondesktop}\ParamLimitsTest" ; Filename: "{app}\ParamLimitsTest.exe"        

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked
Name: "taskbaricon"; Description: "Add a Taskbar shortcut"; GroupDescription: "Additional options:"; Flags: unchecked
