
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.NI_6002;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceHandler.ViewModels;
using DeviceHandler.Views;
using DeviceSimulators.ViewModels;
using Entities.Enums;
using Evva.Models;
using Evva.Views;
using ScriptHandler.Services;
using ScriptHandler.ViewModels;
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
using System.Xml.Linq;
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
			public ActiveErrorLevelEnum SafetyOfficerErrorLevel { get; set; }
		}

		#region Properties

		//public ObservableCollection<DeviceFullData> DevicesList { get; set; }
		public DevicesContainer DevicesContainer { get; set; }

		public Visibility DebugControlVisibility { get; set; }

		public SettingsViewModel AppSettings { get; set; }
		public TestsViewModel Tests { get; set; }
		public RunViewModel Run { get; set; }
		public DesignViewModel Design { get; set; }
		public ParametersViewModel RecordParam { get; set; }
		public MonitorRecParamViewModel MonitorRecParam { get; set; }
		public DeviceHandler.Faults.FaultsMCUViewModel Faults { get; set; }
		public SwitchRelayStateViewModel SwitchRelayState { get; set; }
		public CommunicationViewModel CommunicationSettings { get; set; }





		public DocingViewModel Docking { get; set; }

		public List<MonitorType> MonitorTypesList { get; set; }


		public Visibility EAPSRampupEnableVisibility { get; set; }

		public string Version { get; set; }


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


				if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU))
				{
					DeviceFullData deviceFullData = DevicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];
					deviceFullData.ParametersRepository.AcquisitionRate = AcquisitionRate;
				}

				if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU_2))
				{
					DeviceFullData deviceFullData = DevicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU_2];
					deviceFullData.ParametersRepository.AcquisitionRate = AcquisitionRate;
				}

				if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU_B2B))
				{
					DeviceFullData deviceFullData = DevicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU_B2B];
					deviceFullData.ParametersRepository.AcquisitionRate = AcquisitionRate;
				}
			}
		}

		public EvvaUserData EvvaUserData { get; set; }

		public ActiveErrorLevelEnum ActiveErrorLevel { get; set; }

		#endregion Properties

		#region Fields

		private int _acquisitionRate;

		

		private ReadDevicesFileService _readDevicesFile;

		private SetupSelectionViewModel _setupSelectionVM;

		private NI6002_Init _initNI;

		private FlashingHandler _flashingHandler;

		public CANMessageSenderViewModel _canMessageSender { get; set; }

		private MCU_DeviceData _deviceData_ATE;

		private LogLineListService _logLineList;

		#endregion Fields


		#region Constructor

		public TestStudioMainWindowViewModel()
		{

			SettingsCommand = new RelayCommand(Settings);
			ChangeDarkLightCommand = new RelayCommand(ChangeDarkLight);
			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
			CommunicationSettingsCommand = new RelayCommand(InitCommunicationSettings);
			LoadedCommand = new RelayCommand(Loaded);
			FaultCommand = new RelayCommand(Fault);

			MonitorsDropDownMenuItemCommand = new RelayCommand<string>(MonitorsDropDownMenuItem);

			OpenDesignCommand = new RelayCommand(OpenDesign);
			OpenRunCommand = new RelayCommand(OpenRun);
			OpenRecordingCommand = new RelayCommand(OpenRecording);
			OpenTestCommand = new RelayCommand(OpenTest);
			OpenLoggerServiceCommand = new RelayCommand(OpenLoggerService);

			DeviceSimulatorCommand = new RelayCommand(DeviceSimulator);
			ResetWindowsLayoutCommand = new RelayCommand(ResetWindowsLayout);

			SetupSelectionCommand = new RelayCommand(SetupSelection);


			CANMessageSenderCommand = new RelayCommand(RunCANMessageSender);

			EAPSRampupEnableCommand = new RelayCommand<bool>(EAPSRampupEnable);

			

			ActiveErrorLevel = ActiveErrorLevelEnum.None;
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

			calculatedParam.DevicesList = DevicesContainer.DevicesFullDataList;

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
			//MCU_DeviceData device = new MCU_DeviceData("ATE", DeviceTypesEnum.ATE);

			//device.MCU_FullList = new ObservableCollection<DeviceParameterData>()
			//{
			//	new ATE_ParamData() { Name = "ATE Mode", Cmd = "ate_mode", DeviceType = DeviceTypesEnum.ATE, Save = true },
			//	new ATE_ParamData() { Name = "ATE Done", Cmd = "ate_done", DeviceType = DeviceTypesEnum.ATE, Save = true },
			//	new ATE_ParamData() 
			//	{ 
			//		Name = "ATE Set", Cmd = "ate_set", DeviceType = DeviceTypesEnum.ATE, Save = true,
			//		ATECommand = new List<Entities.Models.DropDownParamData>()
			//		{						
			//			new DropDownParamData() { Name = "ATE_GET_Vbus", Value = "0" },
			//			new DropDownParamData() { Name = "ATE_GET_Meas_5V",Value = "1"},
			//			new DropDownParamData() {Name = "ATE_GET_Vsw",Value = "2"},
			//			new DropDownParamData() {Name = "ATE_GET_BoardTemp1",Value = "3"},
			//			new DropDownParamData()  {Name = "ATE_GET_BoardTemp2",Value = "4"},
			//			new DropDownParamData() {Name = "ATE_GET_PhaseU",Value = "5"},
			//			new DropDownParamData()  {Name = "ATE_GET_PhaseV",Value = "6"},
			//			new DropDownParamData()  {Name = "ATE_GET_PhaseW",Value = "7"},
			//			new DropDownParamData() {Name = "ATE_GET_AqbA",Value = "8"},
			//			new DropDownParamData() {Name = "ATE_GET_AqbB",Value = "9"},
			//			new DropDownParamData()  {Name = "ATE_GET_AqbI",Value = "10"},
			//			new DropDownParamData()  {Name = "ATE_GET_Bus_Current",Value = "11"},
			//			new DropDownParamData()  {Name = "ATE_GET_CoolingTemp",Value = "12"},
			//			new DropDownParamData()   {Name = "ATE_GET_MotorTemp1",Value = "13"},
			//			new DropDownParamData()  {Name = "ATE_GET_MotorTemp2",Value = "14"},
			//			new DropDownParamData()  {Name = "ATE_GET_SerialNumber",Value = "15"},
			//			new DropDownParamData()  {Name = "ATE_GET_GdGood",Value = "16"},
			//			new DropDownParamData()   {Name = "ATE_GET_CurrentUmV",Value = "17"},
			//			new DropDownParamData()   {Name = "ATE_GET_CurrentWmV",Value = "18"},
			//			new DropDownParamData()  {Name = "ATE_GET_BusCurrentMv",Value = "19"},
			//			new DropDownParamData()  {Name = "ATE_GET_DigIn1",Value = "20"},
			//			new DropDownParamData()   {Name = "ATE_GET_DigIn2",Value = "21"},
			//			new DropDownParamData()   {Name = "ATE_GET_DigIn3",Value = "22"},
			//			new DropDownParamData()  {Name = "ATE_GET_DigIn4",Value = "23"},
			//			new DropDownParamData()   {Name = "ATE_GET_SafeMeasure_IDAC1",Value = "24"},
			//			new DropDownParamData()  {Name = "ATE_GET_WatchdogCounter",Value = "25"},
			//			new DropDownParamData()   {Name = "ATE_GET_EolFlagPass",Value = "26"}
			//		}
			//	},

			//	new ATE_ParamData() 
			//	{ 
			//		Name = "ATE Get", Cmd = "ate_get", DeviceType = DeviceTypesEnum.ATE, Save = true,
			//		ATECommand = new List<Entities.Models.DropDownParamData>()
			//		{
			//			new DropDownParamData() { Name = "ATE_GET_Vbus", Value = "0" },
			//			new DropDownParamData() { Name = "ATE_GET_Meas_5V",Value = "1"},
			//			new DropDownParamData() {Name = "ATE_GET_Vsw",Value = "2"},
			//			new DropDownParamData() {Name = "ATE_GET_BoardTemp1",Value = "3"},
			//			new DropDownParamData()  {Name = "ATE_GET_BoardTemp2",Value = "4"},
			//			new DropDownParamData() {Name = "ATE_GET_PhaseU",Value = "5"},
			//			new DropDownParamData()  {Name = "ATE_GET_PhaseV",Value = "6"},
			//			new DropDownParamData()  {Name = "ATE_GET_PhaseW",Value = "7"},
			//			new DropDownParamData() {Name = "ATE_GET_AqbA",Value = "8"},
			//			new DropDownParamData() {Name = "ATE_GET_AqbB",Value = "9"},
			//			new DropDownParamData()  {Name = "ATE_GET_AqbI",Value = "10"},
			//			new DropDownParamData()  {Name = "ATE_GET_Bus_Current",Value = "11"},
			//			new DropDownParamData()  {Name = "ATE_GET_CoolingTemp",Value = "12"},
			//			new DropDownParamData()   {Name = "ATE_GET_MotorTemp1",Value = "13"},
			//			new DropDownParamData()  {Name = "ATE_GET_MotorTemp2",Value = "14"},
			//			new DropDownParamData()  {Name = "ATE_GET_SerialNumber",Value = "15"},
			//			new DropDownParamData()  {Name = "ATE_GET_GdGood",Value = "16"},
			//			new DropDownParamData()   {Name = "ATE_GET_CurrentUmV",Value = "17"},
			//			new DropDownParamData()   {Name = "ATE_GET_CurrentWmV",Value = "18"},
			//			new DropDownParamData()  {Name = "ATE_GET_BusCurrentMv",Value = "19"},
			//			new DropDownParamData()  {Name = "ATE_GET_DigIn1",Value = "20"},
			//			new DropDownParamData()   {Name = "ATE_GET_DigIn2",Value = "21"},
			//			new DropDownParamData()   {Name = "ATE_GET_DigIn3",Value = "22"},
			//			new DropDownParamData()  {Name = "ATE_GET_DigIn4",Value = "23"},
			//			new DropDownParamData()   {Name = "ATE_GET_SafeMeasure_IDAC1",Value = "24"},
			//			new DropDownParamData()  {Name = "ATE_GET_WatchdogCounter",Value = "25"},
			//			new DropDownParamData()   {Name = "ATE_GET_EolFlagPass",Value = "26"}
			//		}
			//	},
			//};

			//ParamGroup paramGroup = new ParamGroup()
			//{
			//	GroupName = "ATE",
			//	GroupDescription = "Group of the ATE parameters",
			//	GroupType = GroupType.ATE,
			//	ParamList = new ObservableCollection<MCU_ParamData>()
			//};

			//foreach (var param in device.MCU_FullList)
			//{
			//	paramGroup.ParamList.Add((MCU_ParamData)param);
			//}

			//device.MCU_GroupList = new ObservableCollection<ParamGroup> { paramGroup };

			//Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
			//settings.Formatting = Newtonsoft.Json.Formatting.Indented;
			//settings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
			//var sz = Newtonsoft.Json.JsonConvert.SerializeObject(device, settings);
			//File.WriteAllText(@"C:\Projects\Evva\Evva\Data\Device Communications\ATE.json", sz);
		}

		private void Settings()
		{
			if(Docking != null) 
				Docking.OpenSettings();
		}

		private void ChangeDarkLight()
		{

			EvvaUserData.IsLightTheme = !EvvaUserData.IsLightTheme;

			if(Application.Current != null) 
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

			if(_logLineList != null)
				_logLineList.Dispose();

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

			if(_canMessageSender != null)
				_canMessageSender.StopAllCANMessages();
		}

		private void InitCommunicationSettings()
		{
			if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.NI_6002) ||
				DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.NI_6002_2))
			{

				_initNI.BindDevices();


				foreach (DeviceFullData device in DevicesContainer.DevicesFullDataList)
				{
					if (device.Device.DeviceType == DeviceTypesEnum.NI_6002 &&
						string.IsNullOrEmpty(_initNI.NI_a) == false)
					{
						(device.ConnectionViewModel as NI6002ConncetViewModel).DeviceName = _initNI.NI_a;
					}
					else if (device.Device.DeviceType == DeviceTypesEnum.NI_6002_2 &&
						string.IsNullOrEmpty(_initNI.NI_a) == false)
					{
						(device.ConnectionViewModel as NI6002ConncetViewModel).DeviceName = _initNI.NI_b;
					}
				}
			}

            Docking.OpenCommSettings();
		}

		private void Loaded()
		{
			CopyUserFilesToEvvaDir();
			AddJson();

			try
			{
				LoggerService.Init("Evva.log", Serilog.Events.LogEventLevel.Information,null);
				LoggerService.Inforamtion(this, "-------------------------------------- EVVA ---------------------");


				LoggerService.Inforamtion(this, "Starting Loaded");

				_logLineList = new LogLineListService();
				_initNI = new NI6002_Init(_logLineList);

				LoadEvvaUserData();

				if (string.IsNullOrEmpty(EvvaUserData.DeviceSetupUserData.MCUJsonPath))
					EvvaUserData.DeviceSetupUserData.MCUJsonPath = @"Data\Device Communications\param_defaults.json";
				if (string.IsNullOrEmpty(EvvaUserData.DeviceSetupUserData.MCUB2BJsonPath))
					EvvaUserData.DeviceSetupUserData.MCUB2BJsonPath = @"Data\Device Communications\param_defaults.json";
				if (string.IsNullOrEmpty(EvvaUserData.DeviceSetupUserData.DynoCommunicationPath))
					EvvaUserData.DeviceSetupUserData.DynoCommunicationPath = @"Data\Device Communications\Dyno Communication.json";
				if (string.IsNullOrEmpty(EvvaUserData.DeviceSetupUserData.NI6002CommunicationPath))
					EvvaUserData.DeviceSetupUserData.NI6002CommunicationPath = @"Data\Device Communications\NI_6002.json";





				_readDevicesFile = new ReadDevicesFileService();
				if (Application.Current != null)
				{
					_setupSelectionVM =
						new SetupSelectionViewModel(EvvaUserData.DeviceSetupUserData, _readDevicesFile);
					SetupSelectionWindowView setupSelectionView = new SetupSelectionWindowView();
					setupSelectionView.SetDataContext(_setupSelectionVM);
					bool? resutl = setupSelectionView.ShowDialog();
					if (resutl != true)
					{
						Closing(null);
						if (Application.Current != null)
							Application.Current.Shutdown();
						return;
					}
				}


				DevicesContainer = new DevicesContainer();
				DevicesContainer.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
				DevicesContainer.DevicesList = new ObservableCollection<DeviceData>();
				DevicesContainer.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();
				
				if(_setupSelectionVM != null)
					UpdateSetup(_setupSelectionVM.DevicesList);


				

				int actualAcquisitionRate = EvvaUserData.AcquisitionRate;
				AcquisitionRate = 5;
				AcquisitionRate = actualAcquisitionRate;

				CommunicationSettings = new CommunicationViewModel(DevicesContainer);

#if DEBUG
				DebugControlVisibility = Visibility.Visible;
#else
				DebugControlVisibility = Visibility.Collapsed;
#endif

				
				_canMessageSender = new CANMessageSenderViewModel(
					DevicesContainer,
					EvvaUserData.DeviceSetupUserData.MCUJsonPath, 
					EvvaUserData.ScriptUserData);



				RecordParam = new ParametersViewModel(
					DevicesContainer,
					EvvaUserData);


				_flashingHandler = new FlashingHandler(DevicesContainer);
				

				Run = new RunViewModel(
					RecordParam.RecordParamList.ParametersList,
					DevicesContainer,
					_flashingHandler,
					EvvaUserData.ScriptUserData,
					_canMessageSender,
					_logLineList);
				Run.CreateScriptLogDiagramViewEvent += Run_CreateScriptLogDiagramViewEvent;
				Run.ShowScriptLoggerViewEvent += Run_ShowScriptLoggerViewEvent;
				Run.ShowScriptLogDiagramViewEvent += Run_ShowScriptLogDiagramViewEvent;

				MonitorRecParam = new MonitorRecParamViewModel(
					DevicesContainer,
					RecordParam.RecordParamList.ParametersList,
					_canMessageSender);




				Tests = new TestsViewModel(DevicesContainer);


				Design = new DesignViewModel(
					DevicesContainer,
					_flashingHandler,
					EvvaUserData.ScriptUserData);

				Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

				try
				{
					foreach (DeviceFullData deviceFullData in DevicesContainer.DevicesFullDataList)
					{
						if(deviceFullData.Device.DeviceType == DeviceTypesEnum.MCU)
						{
							deviceFullData.CheckCommunication.FaultEvent += CheckCommunication_FaultEvent;
						}

						deviceFullData.InitCheckConnection();
					}
				}
				catch (Exception ex)
				{
					LoggerService.Error(this, "Failed to init the communication check", ex);

				}


				AppSettings = new SettingsViewModel(EvvaUserData);
				AppSettings.SettingsUpdatedEvent += SettingsUpdated;

				Faults = new DeviceHandler.Faults.FaultsMCUViewModel(
					DevicesContainer);
				//Faults.Loaded();
				SwitchRelayState = new SwitchRelayStateViewModel(DevicesContainer);

				DeviceSimulatorsViewModel deviceSimulatorsViewModel =
					new DeviceSimulatorsViewModel(DevicesContainer);


				if (Application.Current != null)
				{
					Docking = new DocingViewModel(
						AppSettings,
						Tests,
						Run,
						Design,
						RecordParam,
						MonitorRecParam,
						Faults,
						SwitchRelayState,
						CommunicationSettings,
						_setupSelectionVM,
						deviceSimulatorsViewModel,
						_canMessageSender);

					Run.CreateScriptLoggerWindow();
				}
				
				

				MonitorTypesList = new List<MonitorType>
				{
					new MonitorType() { Name = "Recording parameters" },
					new MonitorType() { Name = "Switch Relay State" },
				};

				IsMCUError = new MCUError() { SafetyOfficerErrorLevel = ActiveErrorLevelEnum.None };
				//Faults.ErrorEvent += MCUErrorEventHandler;


				AddMotorPowerOutputToTorqueKistler();


				if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.PowerSupplyEA))
				{
					EAPSRampupEnableVisibility = Visibility.Visible;
					EAPSRampupEnable(EvvaUserData.IsEAPSRampupEnable);
				}
				else
					EAPSRampupEnableVisibility = Visibility.Collapsed;

				LoggerService.Inforamtion(this, "Ending Loaded");
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to init the main window", "Startup Error", ex);
			}
		}

		private void Run_ShowScriptLoggerViewEvent()
		{
			Docking.OpenScriptLoggerParam();
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
						EvvaUserData.DeviceSetupUserData.MCUJsonPath);
				}
			}

			if (e.IsMCUB2BJsonPathChanged)
			{
				if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU_B2B))
				{
					UpdateMCUJson(
						"MCU - B2B",
						DeviceTypesEnum.MCU_B2B,
						EvvaUserData.DeviceSetupUserData.MCUB2BJsonPath);
				}
			}

			if (e.IsDynoJsonPathChanged && DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.Dyno))
			{
				string dynoPath = Path.Combine(EvvaUserData.DeviceSetupUserData.DynoCommunicationPath, "Dyno Communication.json");
				ObservableCollection<DeviceData> devicesList = new ObservableCollection<DeviceData>();
				_readDevicesFile.ReadFromJson(
					"Data\\Device Communications",
					dynoPath,
					devicesList);

				if (devicesList.Count == 0)
				{
					LoggerService.Error(this, "Dyno *.json was not found", "Error");
					return;
				}

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
					EvvaUserData.DeviceSetupUserData.NI6002CommunicationPath,
					devicesList);

				if(devicesList.Count == 0)
				{
					LoggerService.Error(this, "NI DAQ *.json was not found", "Error");
					return;
				}

				DeviceFullData deviceData = DevicesContainer.TypeToDevicesFullData[DeviceTypesEnum.NI_6002];

				int index = DevicesContainer.DevicesList.IndexOf(deviceData.Device);
				if (index >= 0 && devicesList.Count > 0)
				{
					DevicesContainer.DevicesList[index] = devicesList[0] as DeviceData;
				}

				devicesList[0].Name = deviceData.Device.Name;
				deviceData.Device = devicesList[0] as DeviceData;
			}

			// TODO: SafetyOfficer
			//if (e.IsMotorCommandsPathChanged)
			//{
			//	Run.RunScript.SelectMotor.UpdateMotorList(e.MotorCommandsPath);
			//}

			//if (e.IsControllerCommandsPathChanged)
			//{
			//	Run.RunScript.SelectMotor.UpdateControllerList(e.ControllerCommandsPath);
			//}

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

			if (devicesList[0].ParemetersList == null || devicesList[0].ParemetersList.Count == 0)
			{
				LoggerService.Error(this, "MCU *.json was not found", "Error");
				return;
			}

			MCU_DeviceData ateDevice = _deviceData_ATE.Clone() as MCU_DeviceData;
			MCU_DeviceData mcuDevice = devicesList[0] as MCU_DeviceData;
			mcuDevice.MCU_GroupList.Add(ateDevice.MCU_GroupList[0]);
			foreach (var param in ateDevice.MCU_FullList)
			{
				param.DeviceType = mcuDevice.DeviceType;
				param.Device = mcuDevice;
				mcuDevice.MCU_FullList.Add(param);
			}

			if (DevicesContainer.TypeToDevicesFullData.ContainsKey(type))
			{
				DeviceFullData deviceData = DevicesContainer.TypeToDevicesFullData[type];
				int index = DevicesContainer.DevicesList.IndexOf(deviceData.Device);
				if (index >= 0)
				{
					DevicesContainer.DevicesList[index] = devicesList[0];
				}

				devicesList[0].Name = deviceData.Device.Name;
				deviceData.Device = devicesList[0];
			}
			else
			{
				DevicesContainer.DevicesList.Add(devicesList[0]);

				DeviceFullData mcuFullData = new DeviceFullData_MCU(devicesList[0]);
				DevicesContainer.DevicesFullDataList.Add(mcuFullData);
				DevicesContainer.TypeToDevicesFullData.Add(type, mcuFullData);
			}
			

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

		private void OpenLoggerService()
		{
		}

		private void MonitorsDropDownMenuItem(string name)
		{
			switch (name)
			{
				case "Recording parameters":
					Docking.OpenMonitorRecParam();
					break;
				//case "Scurity parameters":
				//	Docking.OpenMonitorSecurityParam();
				//	break;
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

			UpdateSetup(_setupSelectionVM.DevicesList);

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

		

		private void UpdateSetup(ObservableCollection<DeviceData> deviceList)
		{
			try
			{
				
				#region Get the ATE device
				string path = Directory.GetCurrentDirectory();
				path = Path.Combine(path, @"Data\Device Communications\ATE.json");

				ObservableCollection<DeviceData> deviceList_ATE = new ObservableCollection<DeviceData>();
				_readDevicesFile.ReadFromATEJson(path, deviceList_ATE);
				if (deviceList_ATE != null && deviceList_ATE.Count > 0)
					_deviceData_ATE = deviceList_ATE[0] as MCU_DeviceData;

				foreach (DeviceData deviceData in deviceList)
				{
					if (deviceData is MCU_DeviceData mcuDevice && deviceData.DeviceType != DeviceTypesEnum.ATE)
					{
						ParamGroup ateGroup = mcuDevice.MCU_GroupList.ToList().Find((g) => g.GroupName == "ATE");
						if (ateGroup == null)
						{
							MCU_DeviceData ateDevice = _deviceData_ATE.Clone() as MCU_DeviceData;
							if (!(mcuDevice.MCU_GroupList.Contains(ateDevice.MCU_GroupList[0])))
							{
								mcuDevice.MCU_GroupList.Add(ateDevice.MCU_GroupList[0]);

								foreach (var param in ateDevice.MCU_FullList)
								{
									param.Device = mcuDevice;
									param.DeviceType = mcuDevice.DeviceType;
									mcuDevice.MCU_FullList.Add(param);
								}
							}
						}
					}
				}

				#endregion Get the ATE device

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


				if (DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.NI_6002) ||
					DevicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.NI_6002_2))
				{
					_initNI.BindDevices();
				}

				foreach (DeviceData device in newDevices)
				{
					DeviceFullData deviceFullData = DeviceFullData.Factory(device);
					if (deviceFullData == null)
						continue;

                    deviceFullData.Init("EVVA", _logLineList);


					if (device.DeviceType == Entities.Enums.DeviceTypesEnum.NI_6002 &&
						string.IsNullOrEmpty(_initNI.NI_a) == false)
                    {
                        (deviceFullData.ConnectionViewModel as NI6002ConncetViewModel).DeviceName = _initNI.NI_a;
                    }
                    else if (device.DeviceType == Entities.Enums.DeviceTypesEnum.NI_6002_2 &&
						string.IsNullOrEmpty(_initNI.NI_b) == false)
					{
                        (deviceFullData.ConnectionViewModel as NI6002ConncetViewModel).DeviceName = _initNI.NI_b;
                    }

                    DevicesContainer.DevicesFullDataList.Add(deviceFullData);
					DevicesContainer.DevicesList.Add(device as DeviceData);
					if (DevicesContainer.TypeToDevicesFullData.ContainsKey(device.DeviceType) == false)
						DevicesContainer.TypeToDevicesFullData.Add(device.DeviceType, deviceFullData);

					deviceFullData.Connect();
				}


				//if (Faults != null && Faults.IsLoaded)
				//{
				//	Faults.Loaded();
				//}

				//if (MonitorRecParam != null && MonitorRecParam.IsLoaded)
				//{
				//	MonitorRecParam.Loaded();
				//}

				//if (MonitorSecurityParam != null && MonitorSecurityParam.IsLoaded)
				//{
				//	MonitorSecurityParam.Loaded();
				//}

				

				WeakReferenceMessenger.Default.Send(new SETUP_UPDATEDMessage());
			}
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to init the devices", "Error", ex);
			}
		}

		private void MCUErrorEventHandler(ActiveErrorLevelEnum safetyOfficerErrorLevel)
		{
			IsMCUError.SafetyOfficerErrorLevel = safetyOfficerErrorLevel;
			OnPropertyChanged(nameof(IsMCUError));
		}

		private void ResetWindowsLayout()
		{
			try
			{
				Docking.RestorWindowsLayout();
			}
			catch (Exception ex) 
			{
				LoggerService.Error(this, "Failed to reload the windows layout", "Error", ex);
			}
		}




		

		private void RunCANMessageSender()
		{
			Docking.OpenCANMessageSender();
		}

		private void EAPSRampupEnable(bool isEAPSRampupEnable)
		{
			DeviceFullData deviceFullData = 
				DevicesContainer.TypeToDevicesFullData[DeviceTypesEnum.PowerSupplyEA];

			if(deviceFullData.DeviceCommunicator is PowerSupplayEA_Communicator communicator)
				communicator.SetIsUseRampForOnOff(isEAPSRampupEnable);
		}

		private void Fault()
		{
			Docking.OpenMonitorFaults();
		}

		private void CheckCommunication_FaultEvent(ActiveErrorLevelEnum activeErrorLevel)
		{
			ActiveErrorLevel = activeErrorLevel;
		}

		public void SetLoggerMessage_ForTests(string message)
		{
			LoggerService.Error(this, message);
		}

		public void AddMCUToTheDeviceContainer__ForTests(
			string mcuPath,
			string atePath)
		{
			ObservableCollection<DeviceData> deviceList_ATE = new ObservableCollection<DeviceData>();
			_readDevicesFile.ReadFromATEJson(atePath, deviceList_ATE);
			if (deviceList_ATE != null && deviceList_ATE.Count > 0)
				_deviceData_ATE = deviceList_ATE[0] as MCU_DeviceData;


			UpdateMCUJson(
				"MCU",
				DeviceTypesEnum.MCU,
				mcuPath);


		}

		#endregion Methods

		#region Commands


		public RelayCommand ChangeDarkLightCommand { get; private set; }
		public RelayCommand SettingsCommand { get; private set; }
		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }
		public RelayCommand CommunicationSettingsCommand { get; private set; }
		public RelayCommand LoadedCommand { get; private set; }

		public RelayCommand FaultCommand { get; private set; }


		public RelayCommand SetupSelectionCommand { get; private set; }

		public RelayCommand<string> MonitorsDropDownMenuItemCommand { get; private set; }

		public RelayCommand OpenDesignCommand { get; private set; }
		public RelayCommand OpenRunCommand { get; private set; }
		public RelayCommand OpenRecordingCommand { get; private set; }
		public RelayCommand OpenTestCommand { get; private set; }

		public RelayCommand OpenLoggerServiceCommand { get; private set; }


		public RelayCommand DeviceSimulatorCommand { get; private set; }


		public RelayCommand ResetWindowsLayoutCommand { get; private set; }



		public RelayCommand CANMessageSenderCommand { get; private set; }
		


		public RelayCommand<bool> EAPSRampupEnableCommand { get; private set; }

		#endregion Commands
	}
}
