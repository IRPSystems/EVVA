
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using DeviceSimulators.ViewModels;
using DeviceSimulators.Views;
using Entities.Enums;
using Entities.Models;
using Evva.Models;
using Evva.Services;
using Evva.Views;
using Microsoft.Win32;
using ScriptHandler.Models;
using ScriptHandler.ViewModels;
using ScriptRunner.Services;
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
	public class TestStudioMainWindowViewModel: ObservableObject
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
		public DevicesContainer DevicesContainter { get; set; }

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

		public string Version { get; set; }



		public CommunicationViewModel CommunicationSettings { get; set; }

		public MCUError IsMCUError { get; set; }

		public string CANMessagesScriptPath { get; set; }

		#endregion Properties

		#region Fields



		public EvvaUserData EvvaUserData;
		
		private ReadDevicesFileService _readDevicesFile;

		private SetupSelectionViewModel _setupSelectionVM;

		private CANMessagesService _canMessagesService;

		#endregion Fields


		#region Constructor

		public TestStudioMainWindowViewModel()
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


				DevicesContainter = new DevicesContainer();
				DevicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
				DevicesContainter.DevicesList = new ObservableCollection<DeviceData>();
				DevicesContainter.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();
				UpdateSetup();


				CommunicationSettings = new CommunicationViewModel(DevicesContainter);

				ReadingMotorSettingsService readingMotorSettings = new ReadingMotorSettingsService();
				List<MotorSettingsData> motorSettingsList = readingMotorSettings.GetMotorSettings(
					@"Data\Motor Security Command Parameters.xlsx",
					@"Data\Motor Security Status Parameters.xlsx");

				ReadingControllerSettingsService readingControllerSettings = new ReadingControllerSettingsService();
				List<ControllerSettingsData> controllerSettingsList = 
					readingControllerSettings.GetMotorSettings(
						@"Data\Controller Security Command Parameters.xlsx",
						@"Data\Controller Security Status Parameters.xlsx");

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
					DevicesContainter);

				Run = new RunViewModel(
					RecordParam.RecordParamList.LogParametersList,
					DevicesContainter,
					motorSettingsList,
					controllerSettingsList,
					EvvaUserData,
					_canMessagesService);

				MonitorRecParam = new MonitorRecParamViewModel(
					DevicesContainter,
					RecordParam.RecordParamList.LogParametersList);
				MonitorSecurityParam = new MonitorSecurityParamViewModel(
					DevicesContainter,
					motorSettingsList,
					controllerSettingsList);



				ObservableCollection<MotorSettingsData> motor = null;
				if(motorSettingsList != null)
					motor = new ObservableCollection<MotorSettingsData>(motorSettingsList);

				ObservableCollection<ControllerSettingsData> controller = null;
				if(controllerSettingsList != null)
					controller = new ObservableCollection<ControllerSettingsData>(controllerSettingsList);

				Tests = new TestsViewModel(
					DevicesContainter,
					motor,
					controller);

				Design = new DesignViewModel(
					DevicesContainter, 
					EvvaUserData.ScriptUserData);

				

				Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

				try
				{
					foreach(DeviceFullData deviceFullData in DevicesContainter.DevicesFullDataList)
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
					DevicesContainter,
					EvvaUserData);
				SwitchRelayState = new SwitchRelayStateViewModel(DevicesContainter);
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
					_setupSelectionVM);

				Run.CreateScriptLoggerWindow(Docking);
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

			}
			catch(Exception ex)
			{
				LoggerService.Error(this, "Failed to init the main window", "Startup Error", ex);
			}
		}

		#endregion Constructor

		#region Methods

		private void AddMotorPowerOutputToTorqueKistler()
		{
			if(DevicesContainter.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.TorqueKistler) == false)
			{
				return;
			}

			DeviceFullData deviceFullData =
				DevicesContainter.TypeToDevicesFullData[DeviceTypesEnum.TorqueKistler];

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
			foreach(string file in filesList)
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
			//	Name = "Torque Kistler",
			//	DeviceType = DeviceTypesEnum.TorqueKistler,
			//};

			//device.ParemetersList = new ObservableCollection<DeviceParameterData>()
			//{
			//	new TorqueKistler_ParamData() { Name = "Torque", Command = "MEAS:TORQ", Units = "Nm", DeviceType = DeviceTypesEnum.TorqueKistler },
			//	new TorqueKistler_ParamData() { Name = "Speed", Command = "MEAS:SPE", Units = "RPM", DeviceType = DeviceTypesEnum.TorqueKistler },
			//	new TorqueKistler_ParamData() { Name = "All", Command = "MEAS:ALL", Units = "", DeviceType = DeviceTypesEnum.TorqueKistler },
			//	new TorqueKistler_ParamData() { Name = "Torque filter Freq", Command = "OUTP:TORQ:FILT:FREQ", Units = "Hz", DeviceType = DeviceTypesEnum.TorqueKistler },
			//	new TorqueKistler_ParamData() { Name = "Speed filter Freq", Command = "OUTP:SPE:FILT:FREQ", Units = "Hz", DeviceType = DeviceTypesEnum.TorqueKistler },
			//	new TorqueKistler_ParamData() { Name = "Calibrate Zero offset", Command = "OUTP:TARE:AUTO", Units = "", DeviceType = DeviceTypesEnum.TorqueKistler },
			//};


			//JsonSerializerSettings settings = new JsonSerializerSettings();
			//settings.Formatting = Formatting.Indented;
			//settings.TypeNameHandling = TypeNameHandling.All;
			//var sz = JsonConvert.SerializeObject(device, settings);
			//File.WriteAllText(@"C:\Projects\Infrastructure\Evva\Data\Device Communications\Torque Kistler.json", sz);
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

			if(Run != null)
				Run.ChangeDiagramBackground();
		}

		private void LoadEvvaUserData()
		{
			EvvaUserData = EvvaUserData.LoadEvvaUserData("Evva");

			if (EvvaUserData == null)
			{
				EvvaUserData = new EvvaUserData();
				EvvaUserData.IsLightTheme = false;
				ChangeDarkLight();
				return;
			}
			else
				EvvaUserData.IsLightTheme = !EvvaUserData.IsLightTheme;


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
				if(isCancel) 
				{
					e.Cancel = true;
					return;
				}
			}

			if (MonitorRecParam != null)
				MonitorRecParam.Dispose();


			if(DevicesContainter != null)
			{
				foreach (DeviceFullData deviceFullData in DevicesContainter.DevicesFullDataList)
				{
					deviceFullData.Disconnect();

					if (deviceFullData.CheckCommunication == null)
						continue;

					deviceFullData.CheckCommunication.Dispose();
				}
			}

			if(Docking != null)
				Docking.Close();

			if(Faults != null)
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
			//if(Docking != null)
			//	Docking.Load();
		}

		private void SettingsUpdated(SETTINGS_UPDATEDMessage e)
		{
			if (e.IsMCUJsonPathChanged)
			{
				if (DevicesContainter.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU))
				{
					UpdateMCUJson(
						"MCU",
						DeviceTypesEnum.MCU,
						EvvaUserData.MCUJsonPath);
				}
			}

			if (e.IsMCUB2BJsonPathChanged)
			{
				if (DevicesContainter.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU_B2B))
				{
					UpdateMCUJson(
						"MCU - B2B",
						DeviceTypesEnum.MCU_B2B,
						EvvaUserData.MCUB2BJsonPath);
				}
			}

			if (e.IsDynoJsonPathChanged && DevicesContainter.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.Dyno))
			{
				string dynoPath = Path.Combine(EvvaUserData.DynoCommunicationPath, "Dyno Communication.json");
				ObservableCollection<DeviceBase> devicesList = new ObservableCollection<DeviceBase>();
				_readDevicesFile.ReadFromJson(
					dynoPath,
					devicesList);
				DeviceFullData deviceData = DevicesContainter.TypeToDevicesFullData[DeviceTypesEnum.Dyno];

				int index = DevicesContainter.DevicesList.IndexOf(deviceData.Device);
				if (index >= 0 && devicesList.Count > 0)
				{
					DevicesContainter.DevicesList[index] = devicesList[0] as DeviceData;
				}

				devicesList[0].Name = deviceData.Device.Name;
				deviceData.Device = devicesList[0] as DeviceData;



				((Dyno_Communicator)(deviceData.DeviceCommunicator)).InitMessageDict(
							deviceData.Device);
			}

			if (e.IsNI6002JsonPathChanged && DevicesContainter.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.NI_6002))
			{
				string ni6002JPath = Path.Combine(EvvaUserData.NI6002CommunicationPath, "NI_6002.json");
				ObservableCollection<DeviceBase> devicesList = new ObservableCollection<DeviceBase>();
				_readDevicesFile.ReadFromJson(
					ni6002JPath,
					devicesList);
				DeviceFullData deviceData = DevicesContainter.TypeToDevicesFullData[DeviceTypesEnum.NI_6002];

				int index = DevicesContainter.DevicesList.IndexOf(deviceData.Device);
				if (index >= 0 && devicesList.Count > 0)
				{
					DevicesContainter.DevicesList[index] = devicesList[0] as DeviceData;
				}

				devicesList[0].Name = deviceData.Device.Name;
				deviceData.Device = devicesList[0] as DeviceData;
			}

			Docking.CloseSettings();

			WeakReferenceMessenger.Default.Send(e);
		}

		private void UpdateMCUJson(
			string name,
			DeviceTypesEnum type,
			string filePath)
		{
			ObservableCollection<DeviceBase> devicesList = new ObservableCollection<DeviceBase>();
			_readDevicesFile.ReadFromMCUJson(
				filePath,
				devicesList,
				name,
				type);

			DeviceFullData deviceData = DevicesContainter.TypeToDevicesFullData[type];
			int index = DevicesContainter.DevicesList.IndexOf(deviceData.Device);
			if (index >= 0)
			{
				DevicesContainter.DevicesList[index] = devicesList[0] as DeviceData;
			}

			devicesList[0].Name = deviceData.Device.Name;
			deviceData.Device = devicesList[0] as DeviceData;

			((MCU_Communicator)(deviceData.DeviceCommunicator)).InitMessageDict(
						deviceData.Device);
			
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
			DeviceSimulatorsViewModel dsvm = new DeviceSimulatorsViewModel(DevicesContainter);
			DeviceSimulatorsView dsv = new DeviceSimulatorsView() { DataContext = dsvm };
			dsv.Show();
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
				foreach (DeviceFullData deviceFullData in DevicesContainter.DevicesFullDataList)
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
			ObservableCollection<DeviceBase> deviceList = _setupSelectionVM.DevicesList;

			foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
			{
				device.Disconnect();
			}

			DevicesContainter.DevicesFullDataList.Clear();
			DevicesContainter.DevicesList.Clear();
			DevicesContainter.TypeToDevicesFullData.Clear();


			foreach (DeviceBase device in deviceList)
			{
				DeviceFullData deviceFullData = new DeviceFullData(device as DeviceData);				

				deviceFullData.Init();

				if (device.DeviceType == DeviceTypesEnum.MCU)
				{
					((MCU_Communicator)(deviceFullData.DeviceCommunicator)).InitMessageDict(
						device as DeviceData);
				}
				else if(device.DeviceType == DeviceTypesEnum.Dyno)
				{
					((Dyno_Communicator)(deviceFullData.DeviceCommunicator)).InitMessageDict(
								deviceFullData.Device);
				}

				DevicesContainter.DevicesFullDataList.Add(deviceFullData);
				DevicesContainter.DevicesList.Add(device as DeviceData);
				if(DevicesContainter.TypeToDevicesFullData.ContainsKey(device.DeviceType) == false)
					DevicesContainter.TypeToDevicesFullData.Add(device.DeviceType, deviceFullData);
			}

			foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
				device.Connect();

			if(Faults != null && Faults.IsLoaded) 
			{
				Faults.Loaded();
			}

			if(MonitorRecParam != null && MonitorRecParam.IsLoaded)
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

		#endregion Commands
	}
}
