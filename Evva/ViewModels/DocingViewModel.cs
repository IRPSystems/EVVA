using Controls.ViewModels;
using DeviceHandler.Faults;
using DeviceHandler.ViewModels;
using DeviceHandler.Views;
using DeviceSimulators.ViewModels;
using DeviceSimulators.Views;
using Evva.Views;
using ScriptHandler.ViewModels;
using ScriptHandler.Views;
using ScriptRunner.ViewModels;
using ScriptRunner.Views;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Evva.ViewModels
{
	public class DocingViewModel: DocingBaseViewModel
	{
		#region Fields

		private ContentControl _appSettings;
		private ContentControl _communicationSettings;
		private ContentControl _deviceSimulatorsViewModel;
		private ContentControl _canMessageSenderViewModel;
		private ContentControl _mainScriptLogger;

		private ContentControl _monitorRecParam;
		private ContentControl _monitorSecurityParam;
		private ContentControl _faultsMCU;
		private ContentControl _switchRelayState;

		private ContentControl _setupSelection;


		private ContentControl _design;
		private ContentControl _run;
		private ContentControl _recording;
		private ContentControl _tests;
		private ContentControl _logger;

		#endregion Fields

		#region Constructor

		public DocingViewModel(
			SettingsViewModel appSettings,
			TestsViewModel tests,
			RunViewModel run,
			DesignViewModel design,
			ParametersViewModel recordParam,
			MonitorRecParamViewModel monitorRecParam,
			DeviceHandler.Faults.FaultsMCUViewModel faults,
			SwitchRelayStateViewModel switchRelayState,
			CommunicationViewModel communicationSettings,
			SetupSelectionViewModel setupSelectionVM,
			DeviceSimulatorsViewModel deviceSimulatorsViewModel,
			CANMessageSenderViewModel canMessageSenderViewModel) :
			base("DockingMain")
		{

			CreateWindows(
				appSettings,
				tests,
				run,
				design,
				recordParam,
				monitorRecParam,
				faults,
				switchRelayState,
				communicationSettings,
				setupSelectionVM,
				deviceSimulatorsViewModel,
				canMessageSenderViewModel);
		}

		#endregion Constructor

		#region Methods

		private void CreateWindows(
			SettingsViewModel appSettings,
			TestsViewModel tests,
			RunViewModel run,
			DesignViewModel design,
			ParametersViewModel parameters,
			MonitorRecParamViewModel monitorRecParam,
			DeviceHandler.Faults.FaultsMCUViewModel faultsMCU,
			SwitchRelayStateViewModel switchRelayState,
			CommunicationViewModel communicationSettings,
			SetupSelectionViewModel setupSelectionVM,
			DeviceSimulatorsViewModel deviceSimulatorsViewModel,
			CANMessageSenderViewModel canMessageSenderViewModel)
		{
			DockFill = true;


			_appSettings = new ContentControl();
			SettingsView settings = new SettingsView() { DataContext = appSettings };
			_appSettings.Content = settings;
			SetHeader(_appSettings, "Settings");
			SetFloatWindow(_appSettings);
			Children.Add(_appSettings);


			_communicationSettings = new ContentControl();
			CommunicationView communication = new CommunicationView() { DataContext = communicationSettings };
			_communicationSettings.Content = communication;
			SetHeader(_communicationSettings, "Communication Settings");
			SetFloatWindow(_communicationSettings);
			Children.Add(_communicationSettings);

			_deviceSimulatorsViewModel = new ContentControl();
			DeviceSimulatorsView deviceSimulators = new DeviceSimulatorsView() { DataContext = deviceSimulatorsViewModel };
			_deviceSimulatorsViewModel.Content = deviceSimulators;
			SetHeader(_deviceSimulatorsViewModel, "Device Simulators");
			SetFloatWindow(_deviceSimulatorsViewModel);
			Children.Add(_deviceSimulatorsViewModel);

			_canMessageSenderViewModel = new ContentControl();
			CANMessageSenderView _canMessageSenderView = new CANMessageSenderView() { DataContext = canMessageSenderViewModel };
			_canMessageSenderViewModel.Content = _canMessageSenderView;
			SetHeader(_canMessageSenderViewModel, "CAN Message Sender");
			//SetFloatWindow(_canMessageSenderViewModel);
			Children.Add(_canMessageSenderViewModel);
			SetState(_canMessageSenderViewModel, DockState.Hidden);
			SetSizetoContentInFloat(_canMessageSenderViewModel, false);
			SetDesiredWidthInFloatingMode(_canMessageSenderViewModel, 900);
			SetDesiredHeightInFloatingMode(_canMessageSenderViewModel, 500);



			DesignView designView = new DesignView() { DataContext = design };
			CreateTabbedWindow(designView, "Design", string.Empty, out _design);
			SetDesiredWidthInDockedMode(_design, 1200);

//#if DEBUG
			TestsView testsView = new TestsView() { DataContext = tests };
			CreateTabbedWindow(testsView, "Tests", "Design", out _tests);
			//#endif

			Evva.Views.ParametersView paramView = new Evva.Views.ParametersView() { DataContext = parameters };
			CreateTabbedWindow(paramView, "Parameters", "Design", out _recording);

			RunView runView = new RunView() { DataContext = run };
			CreateTabbedWindow(runView, "Run", "Design", out _run);


			_logger = new ContentControl();
			ScriptLoggerView loggerView = new ScriptLoggerView() { DataContext = run.RunScript.MainScriptLogger };
			_logger.Content = loggerView;
			SetHeader(_logger, "Script Logger");
			SetState(_logger, DockState.Hidden);
			Children.Add(_logger);




			_monitorRecParam = new ContentControl();
			MonitorView monitorView = new MonitorView() { DataContext = monitorRecParam };
			_monitorRecParam.Content = monitorView;
			SetHeader(_monitorRecParam, "Monitor - Record Param");
			SetState(_monitorRecParam, DockState.Hidden);
			Children.Add(_monitorRecParam);

			_faultsMCU = new ContentControl();
			FaultsMCUDataView faultsMCUView = new FaultsMCUDataView() { DataContext = faultsMCU };
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
			SetFloatWindow(_setupSelection);
			Children.Add(_setupSelection);

			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, "Evva");
			path = Path.Combine(path, "Default.txt");
			SaveDockState(path);
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

		private void SetFloatWindow(ContentControl control)
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

		public void OpenDeviceSimulators()
		{
			SetState(_deviceSimulatorsViewModel, DockState.Float);
		}

		public void OpenCANMessageSender()
		{
			SetState(_canMessageSenderViewModel, DockState.Float);
		}

		public void OpenLogScript()
		{
			SetState(_mainScriptLogger, DockState.Dock);
		}


		public void OpenSetupSelection()
		{
			SetState(_setupSelection, DockState.Float);
		}

		public void CloseSetupSelection()
		{
			SetState(_setupSelection, DockState.Hidden);
		}

		public void OpenScriptLoggerParam()
		{
			SetState(_logger, DockState.Dock);
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

		protected override void LoadEventHandler_Specific()
		{
			SetRecMonitorIsOpened(_monitorRecParam);
		}

		protected override void DocingBaseViewModel_DockStateChanged_Specific(FrameworkElement sender, DockStateEventArgs e)
		{
			if (!(sender is ContentControl contentControl))
				return;

			SetRecMonitorIsOpened(contentControl);
		}

		protected void SetRecMonitorIsOpened(ContentControl contentControl)
		{
			if (!(contentControl.Content is MonitorView view))
				return;

			if (!(view.DataContext is MonitorRecParamViewModel viewModel))
				return;

			DockState state = GetState(_monitorRecParam);
			if (state == DockState.Hidden)
				viewModel.IsOpened = false;
			else
				viewModel.IsOpened = true;
		}

		#endregion Methods
	}
}
