
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceHandler.ViewModels;
using DeviceSimulators.ViewModels;
using Entities.Enums;
using Evva.Models;
using Evva.Views;
using Microsoft.Win32;
using ScriptHandler.ViewModels;
using ScriptRunner.Services;
using ScriptRunner.ViewModels;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Evva.ViewModels
{
	public class TestStudioMainWindowViewModel : ObservableObject
	{

		public class MonitorType
		{
			public string Name { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}

		public class MCUError
		{
			public bool? IsErrorExit { get; set; }
		}

		#region Properties

		//public ObservableCollection<DeviceFullData> DevicesList { get; set; }
		public DevicesContainer DevicesContainer { get; set; }

		public Visibility SilentRunVisibility { get; set; }

		public SettingsViewModel AppSettings { get; set; }
		public TestsViewModel Tests { get; set; }
		public RunViewModel Run { get; set; }
		public DesignViewModel Design { get; set; }
		public ParametersViewModel RecordParam { get; set; }
		public MonitorRecParamViewModel MonitorRecParam { get; set; }
		public MonitorSecurityParamViewModel MonitorSecurityParam { get; set; }
		public FaultsMCUViewModel Faults { get; set; }
		public SwitchRelayStateViewModel SwitchRelayState { get; set; }


		public DocingViewModel Docking { get; set; }

		public List<MonitorType> MonitorTypesList { get; set; }


		public Visibility TestsVisibility { get; set; }
		public Visibility EAPSRampupEnableVisibility { get; set; }

		public string Version { get; set; }



		public CommunicationViewModel CommunicationSettings { get; set; }

		public MCUError IsMCUError { get; set; }

		public string CANMessagesScriptPath { get; set; }


		public int AcquisitionRate 
		{
			get => _acquisitionRate;
			set
			{
				_acquisitionRate = value;
				EvvaUserData.AcquisitionRate = value;

				if (DevicesContainer == null)
					return;

				foreach(DeviceFullData deviceFullData in DevicesContainer.DevicesFullDataList) 
				{ 
					deviceFullData.ParametersRepository.AcquisitionRate = AcquisitionRate;
				}
			}
		}

		public EvvaUserData EvvaUserData { get; set; }

		#endregion Properties

		#region Fields

		private int _acquisitionRate;

		

		private ReadDevicesFileService _readDevicesFile;

		private SetupSelectionViewModel _setupSelectionVM;

		private CANMessagesService _canMessagesService;

		#endregion Fields


		#region Constructor

		public TestStudioMainWindowViewModel()
		{

			SettingsCommand = new RelayCommand(Settings);
			ChangeDarkLightCommand = new RelayCommand(ChangeDarkLight);
			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
			CommunicationSettingsCommand = new RelayCommand(InitCommunicationSettings);
			LoadedCommand = new RelayCommand(Loaded);

			MonitorsDropDownMenuItemCommand = new RelayCommand<string>(MonitorsDropDownMenuItem);

			OpenDesignCommand = new RelayCommand(OpenDesign);
			OpenRunCommand = new RelayCommand(OpenRun);
			OpenRecordingCommand = new RelayCommand(OpenRecording);
			OpenTestCommand = new RelayCommand(OpenTest);

			DeviceSimulatorCommand = new RelayCommand(DeviceSimulator);
			ResetWindowsLayoutCommand = new RelayCommand(ResetWindowsLayout);

			SetupSelectionCommand = new RelayCommand(SetupSelection);


			BrowseCANMessagesScriptPathCommand = new RelayCommand(BrowseCANMessagesScriptPath);
			CANMessageSenderCommand = new RelayCommand(CANMessageSender);
			StartCANMessageSenderCommand = new RelayCommand(StartCANMessageSender);
			StopCANMessageSenderCommand = new RelayCommand(StopCANMessageSender);

			EAPSRampupEnableCommand = new RelayCommand<bool>(EAPSRampupEnable);



		}

		#endregion Constructor

		#region Methods

		private void AddMotorPowerOutputToTorqueKistler()
		{
			if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.TorqueKistler) == false)
			{
				return;
			}

			DeviceFullData deviceFullData =
				DevicesContainer.TypeToDevicesFullData[DeviceTypesEnum.TorqueKistler];

			CalculatedParam calculatedParam = new CalculatedParam();
			calculatedParam.Formula = "(A / 9.55) * B";

			calculatedParam.ParametersList = new ObservableCollection<DeviceParameterData>();
			calculatedParam.ParametersList.Add(
				deviceFullData.Device.ParemetersList.ToList().Find((p) => p.Name == "Speed"));
			calculatedParam.ParametersList.Add(
				deviceFullData.Device.ParemetersList.ToList().Find((p) => p.Name == "Torque"));

			calculatedParam.Device = deviceFullData.Device;
			calculatedParam.DeviceType = DeviceTypesEnum.TorqueKistler;

			calculatedParam.Name = "Motor Power Output";
			deviceFullData.Device.ParemetersList.Add(calculatedParam);

			SETTINGS_UPDATEDMessage e = new SETTINGS_UPDATEDMessage();
			WeakReferenceMessenger.Default.Send(e);
		}

		private void CopyUserFilesToEvvaDir()
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string newPath = Path.Combine(appDataPath, "Evva");
			string oldPath = Path.Combine(appDataPath, "IRPTestStudio");

			if (Directory.Exists(newPath))
				return;

			if (!Directory.Exists(oldPath))
				return;

			Directory.CreateDirectory(newPath);

			string[] filesList = Directory.GetFiles(oldPath);
			foreach (string file in filesList)
			{
				string fileName = Path.GetFileName(file);
				string newFilePath = Path.Combine(newPath, fileName);

				File.Copy(file, newFilePath, false);
			}
		}

		private void AddJson()
		{
			//DeviceData device = new DeviceData()
			//{
			//	Name = "Scope KeySight",
			//	DeviceType = DeviceTypesEnum.KeySight,
			//};

			//device.ParemetersList = new ObservableCollection<DeviceParameterData>()
			//{
			//	new Scope_KeySight_ParamData() { Name = "DC RMS N CYcle", Command = ":MEASure:VRMS CYCLe,DC,", DeviceType = DeviceTypesEnum.KeySight },
			//	new Scope_KeySight_ParamData() { Name = "DC RMS Full Screen", Command = ":MEASure:VRMS DISPlay,DC,", DeviceType = DeviceTypesEnum.KeySight },
			//	new Scope_KeySight_ParamData() { Name = "AC RMS N CYcle", Command = ":MEASure:VRMS CYCLe,AC,", DeviceType = DeviceTypesEnum.KeySight },
			//	new Scope_KeySight_ParamData() { Name = "AC RMS N Full Screen", Command = ":MEASure:VRMS DISPlay,AC,", DeviceType = DeviceTypesEnum.KeySight },
			//	new Scope_KeySight_ParamData() { Name = "Pk-pk", Command = ":MEASure:VPP ", DeviceType = DeviceTypesEnum.KeySight },
			//	new Scope_KeySight_ParamData() { Name = "Amplitude", Command = ":MEASure:VAMPlitude ", DeviceType = DeviceTypesEnum.KeySight },
			//	new Scope_KeySight_ParamData() { Name = "TOP", Command = ":MEASure:VTOP ", DeviceType = DeviceTypesEnum.KeySight },
			//	new Scope_KeySight_ParamData() { Name = "Average N Cycle", Command = ":MEASure:VAVerage CYCLe,", DeviceType = DeviceTypesEnum.KeySight },
			//	new Scope_KeySight_ParamData() { Name = "Average Full Screen", Command = ":MEASure:VAVerage DISPlay,", DeviceType = DeviceTypesEnum.KeySight },
			//};


			//Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
			//settings.Formatting = Newtonsoft.Json.Formatting.Indented;
			//settings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
			//var sz = Newtonsoft.Json.JsonConvert.SerializeObject(device, settings);
			//File.WriteAllText(@"C:\Projects\Evva\Evva\Data\Device Communications\Scope KeySight.json", sz);
		}

		private void Settings()
		{
			Docking.OpenSettings();
		}

		private void ChangeDarkLight()
		{

			EvvaUserData.IsLightTheme = !EvvaUserData.IsLightTheme;
			App.ChangeDarkLight(EvvaUserData.IsLightTheme);

			if (Design != null)
			{
				Design.RefreshTheme(EvvaUserData.IsLightTheme);
				Design.RefreshDiagram();
			}

			if (Docking != null)
				Docking.Refresh();

			if (Run != null)
				Run.ChangeDiagramBackground();
		}

		private void LoadEvvaUserData()
		{
			EvvaUserData = EvvaUserData.LoadEvvaUserData("Evva");


			if (EvvaUserData == null)
			{
				EvvaUserData = new EvvaUserData();
				EvvaUserData.IsLightTheme = false;
				EvvaUserData.AcquisitionRate = 5;
				ChangeDarkLight();
				return;
			}
			else
			{
				EvvaUserData.IsLightTheme = !EvvaUserData.IsLightTheme;
			}


			ChangeDarkLight();
		}

		private void SaveEvvaUserData()
		{
			EvvaUserData.SaveEvvaUserData(
				"Evva",
				EvvaUserData);
		}

		private void Closing(CancelEventArgs e)
		{
			SaveEvvaUserData();

			if (Design != null)
			{
				bool isCancel = Design.SaveIfNeeded();
				if (isCancel)
				{
					e.Cancel = true;
					return;
				}
			}

			if (MonitorRecParam != null)
				MonitorRecParam.Dispose();


			if (DevicesContainer != null)
			{
				foreach (DeviceFullData deviceFullData in DevicesContainer.DevicesFullDataList)
				{
					deviceFullData.Disconnect();

					if (deviceFullData.CheckCommunication == null)
						continue;

					deviceFullData.CheckCommunication.Dispose();
				}
			}

			if (Docking != null)
				Docking.Close();

			if (Faults != null)
				Faults.Dispose();

			if (_canMessagesService != null)
				_canMessagesService.CloseCANMessageSender();
		}

		private void InitCommunicationSettings()
		{
			Docking.OpenCommSettings();
		}

		private void Loaded()
		{
			CopyUserFilesToEvvaDir();
			AddJson();

			try
			{
				LoggerService.Init("Evva.log", Serilog.Events.LogEventLevel.Information);
				LoggerService.Inforamtion(this, "-------------------------------------- EVVA ---------------------");


				LoggerService.Inforamtion(this, "Starting C'tor of TestStudioMainWindowViewModel");

				LoadEvvaUserData();

				if (string.IsNullOrEmpty(EvvaUserData.MCUJsonPath))
					EvvaUserData.MCUJsonPath = @"Data\Device Communications\param_defaults.json";
				if (string.IsNullOrEmpty(EvvaUserData.MCUB2BJsonPath))
					EvvaUserData.MCUB2BJsonPath = @"Data\Device Communications\param_defaults.json";
				if (string.IsNullOrEmpty(EvvaUserData.DynoCommunicationPath))
					EvvaUserData.DynoCommunicationPath = @"Data\Device Communications\Dyno Communication.json";
				if (string.IsNullOrEmpty(EvvaUserData.NI6002CommunicationPath))
					EvvaUserData.NI6002CommunicationPath = @"Data\Device Communications\NI_6002.json";

				



				_readDevicesFile = new ReadDevicesFileService();
				_setupSelectionVM =
					new SetupSelectionViewModel(EvvaUserData, _readDevicesFile);
				SetupSelectionWindowView setupSelectionView = new SetupSelectionWindowView();
				setupSelectionView.SetDataContext(_setupSelectionVM);
				bool? resutl = setupSelectionView.ShowDialog();
				if (resutl != true)
				{
					Closing(null);
					Application.Current.Shutdown();
					return;
				}


				DevicesContainer = new DevicesContainer();
				DevicesContainer.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
				DevicesContainer.DevicesList = new ObservableCollection<DeviceData>();
				DevicesContainer.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();
				UpdateSetup();

				int actualAcquisitionRate = EvvaUserData.AcquisitionRate;
				AcquisitionRate = 5;
				AcquisitionRate = actualAcquisitionRate;

				CommunicationSettings = new CommunicationViewModel(DevicesContainer);

#if DEBUG
				TestsVisibility = Visibility.Visible;
				SilentRunVisibility = Visibility.Visible;
#else
				TestsVisibility = Visibility.Collapsed;
				SilentRunVisibility = Visibility.Collapsed;
#endif

				TestsVisibility = Visibility.Visible;

				_canMessagesService = new CANMessagesService();



				RecordParam = new ParametersViewModel(
					DevicesContainer);

				Run = new RunViewModel(
					RecordParam.RecordParamList.ParametersList,
					DevicesContainer,
					EvvaUserData.ScriptUserData,
					_canMessagesService);
				Run.CreateScriptLogDiagramViewEvent += Run_CreateScriptLogDiagramViewEvent;
				Run.ShowScriptLogDiagramViewEvent += Run_ShowScriptLogDiagramViewEvent;

				MonitorRecParam = new MonitorRecParamViewModel(
					DevicesContainer,
					RecordParam.RecordParamList.ParametersList);
				MonitorSecurityParam = new MonitorSecurityParamViewModel(
					DevicesContainer,
					Run.RunScript);




				Tests = new TestsViewModel(DevicesContainer);

				Design = new DesignViewModel(
					DevicesContainer,
					EvvaUserData.ScriptUserData);



				Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

				try
				{
					foreach (DeviceFullData deviceFullData in DevicesContainer.DevicesFullDataList)
					{
						deviceFullData.InitCheckConnection();
					}
				}
				catch (Exception ex)
				{
					LoggerService.Error(this, "Failed to init the communication check", ex);

				}


				AppSettings = new SettingsViewModel(EvvaUserData);
				AppSettings.SettingsUpdatedEvent += SettingsUpdated;

				Faults = new FaultsMCUViewModel(
					DevicesContainer,
					EvvaUserData);
				SwitchRelayState = new SwitchRelayStateViewModel(DevicesContainer);

				DeviceSimulatorsViewModel deviceSimulatorsViewModel =
					new DeviceSimulatorsViewModel(DevicesContainer);
				Docking = new DocingViewModel(
					AppSettings,
					Tests,
					Run,
					Design,
					RecordParam,
					MonitorRecParam,
					MonitorSecurityParam,
					Faults,
					SwitchRelayState,
					CommunicationSettings,
					_setupSelectionVM,
					deviceSimulatorsViewModel);

				Run.CreateScriptLoggerWindow();
				Tests.CreateTestParamsLimitWindow(Docking);


				MonitorTypesList = new List<MonitorType>
				{
					new MonitorType() { Name = "Recording parameters" },
					new MonitorType() { Name = "Scurity parameters" },
					new MonitorType() { Name = "Faults" },
					new MonitorType() { Name = "Switch Relay State" },
				};

				IsMCUError = new MCUError() { IsErrorExit = null };
				Faults.ErrorEvent += MCUErrorEventHandler;

				AddMotorPowerOutputToTorqueKistler();

				if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.PowerSupplyEA))
				{
					EAPSRampupEnableVisibility = Visibility.Visible;
					EAPSRampupEnable(EvvaUserData.IsEAPSRampupEnable);
				}
				else
					EAPSRampupEnableVisibility = Visibility.Collapsed;

			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to init the main window", "Startup Error", ex);
			}
		}

		private void Run_ShowScriptLogDiagramViewEvent()
		{
			Docking.OpenLogScript();
		}

		private void Run_CreateScriptLogDiagramViewEvent(ScriptLogDiagramViewModel obj)
		{
			Docking.CreateScriptLogger(obj);
		}

		private void SettingsUpdated(SETTINGS_UPDATEDMessage e)
		{
			if (e.IsMCUJsonPathChanged)
			{
				if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU))
				{
					UpdateMCUJson(
						"MCU",
						DeviceTypesEnum.MCU,
						EvvaUserData.MCUJsonPath);
				}
			}

			if (e.IsMCUB2BJsonPathChanged)
			{
				if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU_B2B))
				{
					UpdateMCUJson(
						"MCU - B2B",
						DeviceTypesEnum.MCU_B2B,
						EvvaUserData.MCUB2BJsonPath);
				}
			}

			if (e.IsDynoJsonPathChanged && DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.Dyno))
			{
				string dynoPath = Path.Combine(EvvaUserData.DynoCommunicationPath, "Dyno Communication.json");
				ObservableCollection<DeviceData> devicesList = new ObservableCollection<DeviceData>();
				_readDevicesFile.ReadFromJson(
					"Data\\Device Communications",
					dynoPath,
					devicesList);
				DeviceFullData deviceData = DevicesContainer.TypeToDevicesFullData[DeviceTypesEnum.Dyno];

				int index = DevicesContainer.DevicesList.IndexOf(deviceData.Device);
				if (index >= 0 && devicesList.Count > 0)
				{
					DevicesContainer.DevicesList[index] = devicesList[0] as DeviceData;
				}

				devicesList[0].Name = deviceData.Device.Name;
				deviceData.Device = devicesList[0] as DeviceData;

			}

			if (e.IsNI6002JsonPathChanged && DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.NI_6002))
			{
				//string ni6002JPath = Path.Combine(EvvaUserData.NI6002CommunicationPath, "NI_6002.json");
				ObservableCollection<DeviceData> devicesList = new ObservableCollection<DeviceData>();
				_readDevicesFile.ReadFromJson(
					"Data\\Device Communications",
					EvvaUserData.NI6002CommunicationPath,
					devicesList);
				DeviceFullData deviceData = DevicesContainer.TypeToDevicesFullData[DeviceTypesEnum.NI_6002];

				int index = DevicesContainer.DevicesList.IndexOf(deviceData.Device);
				if (index >= 0 && devicesList.Count > 0)
				{
					DevicesContainer.DevicesList[index] = devicesList[0] as DeviceData;
				}

				devicesList[0].Name = deviceData.Device.Name;
				deviceData.Device = devicesList[0] as DeviceData;
			}

			if (e.IsMotorCommandsPathChanged)
			{
				Run.RunScript.SelectMotor.UpdateMotorList(e.MotorCommandsPath);
			}

			if (e.IsControllerCommandsPathChanged)
			{
				Run.RunScript.SelectMotor.UpdateControllerList(e.ControllerCommandsPath);
			}

			Docking.CloseSettings();

			WeakReferenceMessenger.Default.Send(e);
		}

		private void UpdateMCUJson(
			string name,
			DeviceTypesEnum type,
			string filePath)
		{
			ObservableCollection<DeviceData> devicesList = new ObservableCollection<DeviceData>();
			_readDevicesFile.ReadFromMCUJson(
				filePath,
				devicesList,
				name,
				type);

			DeviceFullData deviceData = DevicesContainer.TypeToDevicesFullData[type];
			int index = DevicesContainer.DevicesList.IndexOf(deviceData.Device);
			if (index >= 0)
			{
				DevicesContainer.DevicesList[index] = devicesList[0] as DeviceData;
			}

			devicesList[0].Name = deviceData.Device.Name;
			deviceData.Device = devicesList[0] as DeviceData;

		}


		private void OpenDesign()
		{
			Docking.OpenDesign();
		}

		private void OpenRun()
		{
			Docking.OpenRun();
		}

		private void OpenRecording()
		{
			Docking.OpenRecording();
		}

		private void OpenTest()
		{
			Docking.OpenTest();
		}

		private void MonitorsDropDownMenuItem(string name)
		{
			switch (name)
			{
				case "Recording parameters":
					Docking.OpenMonitorRecParam();
					break;
				case "Scurity parameters":
					Docking.OpenMonitorSecurityParam();
					break;
				case "Faults":
					Docking.OpenMonitorFaults();
					break;
				case "Switch Relay State":
					Docking.OpenMonitorSwitchRelayState();
					break;
			}
		}

		private void DeviceSimulator()
		{
			Docking.OpenDeviceSimulators();
			//DeviceSimulatorsViewModel dsvm = new DeviceSimulatorsViewModel(DevicesContainter);
			//DeviceSimulatorsView dsv = new DeviceSimulatorsView() { DataContext = dsvm };
			//dsv.Show();
		}

		private void SetupSelection()
		{
			Docking.OpenSetupSelection();


			_setupSelectionVM.CloseOKEvent += SetupSelectionCloseOK;
			_setupSelectionVM.CloseCancelEvent += SetupSelectionCloseCancel;
		}

		private void SetupSelectionCloseOK()
		{
			_setupSelectionVM.CloseOKEvent -= SetupSelectionCloseOK;
			_setupSelectionVM.CloseCancelEvent -= SetupSelectionCloseCancel;

			UpdateSetup();

			try
			{
				foreach (DeviceFullData deviceFullData in DevicesContainer.DevicesFullDataList)
				{
					deviceFullData.InitCheckConnection();
				}
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to init the communication check", ex);

			}

			Docking.CloseSetupSelection();
		}

		private void SetupSelectionCloseCancel()
		{
			_setupSelectionVM.CloseOKEvent -= SetupSelectionCloseOK;
			_setupSelectionVM.CloseCancelEvent -= SetupSelectionCloseCancel;

			Docking.CloseSetupSelection();
		}

		private void UpdateSetup()
		{
			ObservableCollection<DeviceData> deviceList = _setupSelectionVM.DevicesList;


			List<DeviceData> newDevices = new List<DeviceData>();
			foreach (DeviceData deviceData in deviceList)
			{
				DeviceData existingDevice =
					DevicesContainer.DevicesList.ToList().Find((d) => d.DeviceType == deviceData.DeviceType);
				if (existingDevice == null)
					newDevices.Add(deviceData);
			}

			List<DeviceData> removedDevices = new List<DeviceData>();
			foreach (DeviceData deviceData in DevicesContainer.DevicesList)
			{
				DeviceData existingDevice =
					deviceList.ToList().Find((d) => d.DeviceType == deviceData.DeviceType);
				if (existingDevice == null)
					removedDevices.Add(deviceData);
			}




			foreach (DeviceData device in removedDevices)
			{
				DeviceFullData deviceFullData =
					DevicesContainer.DevicesFullDataList.ToList().Find((d) => d.Device.DeviceType == device.DeviceType);
				deviceFullData.Disconnect();

				DevicesContainer.DevicesFullDataList.Remove(deviceFullData);
				DevicesContainer.DevicesList.Remove(deviceFullData.Device);
				DevicesContainer.TypeToDevicesFullData.Remove(deviceFullData.Device.DeviceType);
			}



			foreach (DeviceData device in newDevices)
			{
				DeviceFullData deviceFullData = DeviceFullData.Factory(device);

				deviceFullData.Init("EVVA");

				DevicesContainer.DevicesFullDataList.Add(deviceFullData);
				DevicesContainer.DevicesList.Add(device as DeviceData);
				if (DevicesContainer.TypeToDevicesFullData.ContainsKey(device.DeviceType) == false)
					DevicesContainer.TypeToDevicesFullData.Add(device.DeviceType, deviceFullData);

				deviceFullData.Connect();
			}


			if (Faults != null && Faults.IsLoaded)
			{
				Faults.Loaded();
			}

			if (MonitorRecParam != null && MonitorRecParam.IsLoaded)
			{
				MonitorRecParam.Loaded();
			}

			if (MonitorSecurityParam != null && MonitorSecurityParam.IsLoaded)
			{
				MonitorSecurityParam.Loaded();
			}

			WeakReferenceMessenger.Default.Send(new SETUP_UPDATEDMessage());
		}

		private void MCUErrorEventHandler(bool? isMCUError)
		{
			IsMCUError.IsErrorExit = isMCUError;
			OnPropertyChanged(nameof(IsMCUError));
		}

		private void ResetWindowsLayout()
		{
			Docking.RestorWindowsLayout();
		}




		private void BrowseCANMessagesScriptPath()
		{
			string initDir = EvvaUserData.ScriptUserData.LastCANMessageScriptPath;
			if (string.IsNullOrEmpty(initDir))
				initDir = "";
			if (Directory.Exists(initDir) == false)
				initDir = "";


			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Test and Scripts Files|*.tst;*.scr";
			openFileDialog.InitialDirectory = initDir;
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			EvvaUserData.ScriptUserData.LastCANMessageScriptPath =
					Path.GetDirectoryName(openFileDialog.FileName);

			CANMessagesScriptPath = openFileDialog.FileName;
		}

		private void CANMessageSender()
		{
			_canMessagesService.OpenCANMessageSender();
		}

		private void StartCANMessageSender()
		{
			_canMessagesService.SendCANMessageScript(CANMessagesScriptPath);
		}

		private void StopCANMessageSender()
		{
			_canMessagesService.StopSendCANMessageScript();
		}

		private void EAPSRampupEnable(bool isEAPSRampupEnable)
		{
			DeviceFullData deviceFullData = 
				DevicesContainer.TypeToDevicesFullData[DeviceTypesEnum.PowerSupplyEA];

			if(deviceFullData.DeviceCommunicator is PowerSupplayEA_Communicator communicator)
				communicator.SetIsUseRampForOnOff(isEAPSRampupEnable);
		}

		#endregion Methods

		#region Commands


		public RelayCommand ChangeDarkLightCommand { get; private set; }
		public RelayCommand SettingsCommand { get; private set; }
		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }
		public RelayCommand CommunicationSettingsCommand { get; private set; }
		public RelayCommand LoadedCommand { get; private set; }


		public RelayCommand SetupSelectionCommand { get; private set; }

		public RelayCommand<string> MonitorsDropDownMenuItemCommand { get; private set; }

		public RelayCommand OpenDesignCommand { get; private set; }
		public RelayCommand OpenRunCommand { get; private set; }
		public RelayCommand OpenRecordingCommand { get; private set; }
		public RelayCommand OpenTestCommand { get; private set; }

		public RelayCommand DeviceSimulatorCommand { get; private set; }


		public RelayCommand ResetWindowsLayoutCommand { get; private set; }



		public RelayCommand BrowseCANMessagesScriptPathCommand { get; private set; }
		public RelayCommand CANMessageSenderCommand { get; private set; }
		public RelayCommand StartCANMessageSenderCommand { get; private set; }
		public RelayCommand StopCANMessageSenderCommand { get; private set; }



		public RelayCommand<bool> EAPSRampupEnableCommand { get; private set; }

		#endregion Commands
	}
}
