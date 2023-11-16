using Controls.ViewModels;
using Evva.Views;
using ScriptHandler.Services;
using ScriptHandler.ViewModels;
using ScriptHandler.Views;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.IO;
using System.Windows.Controls;

namespace Evva.ViewModels
{
	public class DocingViewModel: DocingBaseViewModel
	{
		#region Fields

		private ContentControl _appSettings;
		private ContentControl _communicationSettings;
		private ContentControl _mainScriptLogger;
		private ContentControl _testParamsLimit;

		private ContentControl _monitorRecParam;
		private ContentControl _monitorSecurityParam;
		private ContentControl _faultsMCU;
		private ContentControl _switchRelayState;

		private ContentControl _setupSelection;


		private ContentControl _design;
		private ContentControl _run;
		private ContentControl _recording;
		private ContentControl _tests;

		#endregion Fields

		#region Constructor

		public DocingViewModel(
			SettingsViewModel appSettings,
			TestsViewModel tests,
			RunViewModel run,
			DesignViewModel design,
			RecordParamViewModel recordParam,
			MonitorRecParamViewModel monitorRecParam,
			MonitorSecurityParamViewModel monitorSecurityParam,
			FaultsMCUViewModel faults,
			SwitchRelayStateViewModel switchRelayState,
			CommunicationViewModel communicationSettings,
			SetupSelectionViewModel setupSelectionVM) :
			base("DockingMain")
		{
			CreateWindows(
				appSettings,
				tests,
				run,
				design,
				recordParam,
				monitorRecParam,
				monitorSecurityParam,
				faults,
				switchRelayState,
				communicationSettings,
				setupSelectionVM);
		}

		#endregion Constructor

		#region Methods

		private void CreateWindows(
			SettingsViewModel appSettings,
			TestsViewModel tests,
			RunViewModel run,
			DesignViewModel design,
			RecordParamViewModel recordParam,
			MonitorRecParamViewModel monitorRecParam,
			MonitorSecurityParamViewModel monitorSecurityParam,
			FaultsMCUViewModel faultsMCU,
			SwitchRelayStateViewModel switchRelayState,
			CommunicationViewModel communicationSettings,
			SetupSelectionViewModel setupSelectionVM)
		{
			DockFill = true;


			_appSettings = new ContentControl();
			SettingsView settings = new SettingsView() { DataContext = appSettings };
			_appSettings.Content = settings;
			SetHeader(_appSettings, "Settings");
			SetFloatParams(_appSettings);
			Children.Add(_appSettings);


			_communicationSettings = new ContentControl();
			CommunicationView communication = new CommunicationView() { DataContext = communicationSettings };
			_communicationSettings.Content = communication;
			SetHeader(_communicationSettings, "Communication Settings");
			SetFloatParams(_communicationSettings);
			Children.Add(_communicationSettings);




			DesignView designView = new DesignView() { DataContext = design };
			CreateTabbedWindow(designView, "Design", string.Empty, out _design);
			SetDesiredWidthInDockedMode(_design, 1200);

//#if DEBUG
			TestsView testsView = new TestsView() { DataContext = tests };
			CreateTabbedWindow(testsView, "Tests", "Design", out _tests);
//#endif

			RecordParamView paramView = new RecordParamView() { DataContext = recordParam };
			CreateTabbedWindow(paramView, "Record", "Design", out _recording);

			RunView runView = new RunView() { DataContext = run };
			CreateTabbedWindow(runView, "Run", "Design", out _run);



			_monitorRecParam = new ContentControl();
			MonitorView monitorView = new MonitorView() { DataContext = monitorRecParam };
			_monitorRecParam.Content = monitorView;
			SetHeader(_monitorRecParam, "Monitor - Record Param");
			SetState(_monitorRecParam, DockState.Hidden);
			Children.Add(_monitorRecParam);

			_monitorSecurityParam = new ContentControl();
			monitorView = new MonitorView() { DataContext = monitorSecurityParam };
			_monitorSecurityParam.Content = monitorView;
			SetHeader(_monitorSecurityParam, "Monitor - Security Param");
			SetState(_monitorSecurityParam, DockState.Hidden);
			Children.Add(_monitorSecurityParam);

			_faultsMCU = new ContentControl();
			FaultsMCUView faultsMCUView = new FaultsMCUView() { DataContext = faultsMCU };
			_faultsMCU.Content = faultsMCUView;
			SetHeader(_faultsMCU, "Faults");
			SetState(_faultsMCU, DockState.Hidden);
			Children.Add(_faultsMCU);

			_switchRelayState = new ContentControl();
			SwitchRelayStateView switchRelayStateView = new SwitchRelayStateView() { DataContext = switchRelayState };
			_switchRelayState.Content = switchRelayStateView;
			SetHeader(_switchRelayState, "Switch Relay State");
			SetState(_switchRelayState, DockState.Hidden);
			Children.Add(_switchRelayState);

			_setupSelection = new ContentControl();
			SetupSelectionView setupSelectionView = new SetupSelectionView() { DataContext = setupSelectionVM };
			_setupSelection.Content = setupSelectionView;
			SetHeader(_setupSelection, "Setup Selection");
			SetState(_setupSelection, DockState.Hidden);
			SetFloatParams(_setupSelection);
			Children.Add(_setupSelection);

			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, "Evva");
			path = Path.Combine(path, "Default.txt");
			SaveDockState(path);
		}

		private void CreateTabbedWindow(
			UserControl userControl,
			string name,
			string targetname,
			out ContentControl window)
		{
			window = new ContentControl();
			window.Name = name;
			window.Content = userControl;

			if (targetname != string.Empty)
			{
				SetTargetNameInDockedMode(window, targetname);
				SetSideInDockedMode(window, DockSide.Tabbed);
			}

			SetHeader(window, name);
			Children.Add(window);
		}


		public void CreateScriptLogger(
			ScriptDiagramViewModel mainScriptLogger)
		{
			_mainScriptLogger = new ContentControl();
			ScriptLogDiagramView scriptLog = new ScriptLogDiagramView() { DataContext = mainScriptLogger };
			_mainScriptLogger.Content = scriptLog;
			SetHeader(_mainScriptLogger, "Script Run Diagram");
			SetState(_mainScriptLogger, DockState.Hidden);
			SetSideInDockedMode(_mainScriptLogger, DockSide.Right);
			Children.Add(_mainScriptLogger);
		}

		public void CreateParamsLimitTester(
			TestParamsLimitViewModel testParamsLimitViewModel)
		{
			_testParamsLimit = new ContentControl();
			TestParamsLimitView scriptLog = new TestParamsLimitView() { DataContext = testParamsLimitViewModel };
			_testParamsLimit.Content = scriptLog;
			SetHeader(_testParamsLimit, "Test Params Limit");
			SetState(_testParamsLimit, DockState.Hidden);
			SetSideInDockedMode(_testParamsLimit, DockSide.Right);
			Children.Add(_testParamsLimit);
		}

		private void SetFloatParams(ContentControl control)
		{
			SetSizetoContentInDock(control, true);
			SetSizetoContentInFloat(control, true);
			SetState(control, DockState.Hidden);
		}




		public void OpenSettings()
		{
			SetState(_appSettings, DockState.Float);
		}

		public void CloseSettings()
		{
			SetState(_appSettings, DockState.Hidden);
		}

		public void OpenCommSettings()
		{
			SetState(_communicationSettings, DockState.Float);
		}

		public void OpenLogScript()
		{
			SetState(_mainScriptLogger, DockState.Dock);
		}

		public void OpenTestParamsLimit()
		{
			SetState(_testParamsLimit, DockState.Dock);
		}

		public void OpenSetupSelection()
		{
			SetState(_setupSelection, DockState.Float);
		}

		public void CloseSetupSelection()
		{
			SetState(_setupSelection, DockState.Hidden);
		}


		public void OpenMonitorRecParam()
		{
			SetState(_monitorRecParam, DockState.Dock);
		}

		public void OpenMonitorSecurityParam()
		{
			SetState(_monitorSecurityParam, DockState.Dock);
		}

		public void OpenMonitorFaults()
		{
			SetState(_faultsMCU, DockState.Dock);
		}

		public void OpenMonitorSwitchRelayState()
		{
			SetState(_switchRelayState, DockState.Dock);
		}



		public void OpenDesign()
		{
			SetState(_design, DockState.Dock);
		}

		public void OpenRun()
		{
			SetState(_run, DockState.Dock);
		}

		public void OpenRecording()
		{
			SetState(_recording, DockState.Dock);
		}

		public void OpenTest()
		{
			SetState(_tests, DockState.Dock);
		}



		public void RestorWindowsLayout()
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, "Evva");
			path = Path.Combine(path, "Default.txt");
			if (System.IO.File.Exists(path))
				LoadDockState(path);
		}

#endregion Methods
	}
}
