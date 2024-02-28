
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.BrainChild;
using DeviceCommunicators.Enums;
using DeviceCommunicators.FieldLogger;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using DeviceHandler.Enums;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceHandler.ViewModels;
using Entities.Enums;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using ScriptRunner.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using TempLoggerViewer.Models;
using TempLoggerViewer.ViewModels;

namespace TempLoggerViewer
{
	public class TempLoggerMainWindowVModel : ObservableObject
	{

		public class LoggerDevice : ObservableObject
		{ 
			public DeviceData Device { get; set; }

			public bool IsThermocoupleK { get; set; }
			public bool IsThermocoupleT { get; set; }

			public bool IsRecord { get; set; }
		}


		#region Properties

		public DevicesContainer DevicesContainter { get; set; }
		public DocingTempLoggerViewModel Docking { get; set; }

		public ObservableCollection<LoggerDevice> LoggerDevicesList { get; set; }

		public string Version { get; set; }

		public string RecordDirectory { get; set; }

		public bool IsRecording { get; set; }

		#endregion Properties

		#region Fields

		private ParamRecordingService _paramRecording;
		private LoggerViewerUserData _loggerViewerUserData;

		#endregion Fields

		#region Constructor

		public TempLoggerMainWindowVModel()
		{
			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			InitDevicesContainter();

			CommunicationViewModel communicationSettings =
				new CommunicationViewModel(DevicesContainter);
			Docking = new DocingTempLoggerViewModel(communicationSettings);
			CommunicationSettingsCommand = new RelayCommand(InitCommunicationSettings);
			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);

			SaveNamesCommand = new RelayCommand(SaveNames);
			LoadNamesCommand = new RelayCommand(LoadNames);

			BrowseRecordFileCommand = new RelayCommand(BrowseRecordFile);
			StartRecordingCommand = new RelayCommand(StartRecording);
			StopRecordingCommand = new RelayCommand(StopRecording);
			ThermocoupleTypeCommand = new RelayCommand<LoggerDevice>(ThermocoupleType);

			_paramRecording = new ParamRecordingService(DevicesContainter);

			_loggerViewerUserData = LoggerViewerUserData.LoadLoggerViewerData("TempLoggerViewer");
			RecordDirectory = _loggerViewerUserData.RecordingDirectory;
		}

		#endregion Constructor

		#region Methods

		private void Closing(CancelEventArgs e)
		{

			foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
			{
				device.Disconnect();
				foreach (DeviceParameterData param in device.Device.ParemetersList)
				{
					device.ParametersRepository.Remove(param, Callback);
				}
			}

			LoggerViewerUserData.SaveLoggerViewerData(
				"TempLoggerViewer",
				_loggerViewerUserData);
		}

		private void InitDevicesContainter()
		{
			DevicesContainter = new DevicesContainer();
			DevicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
			DevicesContainter.DevicesList = new ObservableCollection<DeviceData>();
			DevicesContainter.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();

			string path = Directory.GetCurrentDirectory();
			ReadDevicesFileService readDevicesFile = new ReadDevicesFileService();
			ObservableCollection<DeviceData> deviceList = readDevicesFile.ReadAllFiles(
				path,
				null,
				null,
				null,
				null);

			foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
			{
				device.Disconnect();
			}

			DevicesContainter.DevicesFullDataList.Clear();
			DevicesContainter.DevicesList.Clear();
			DevicesContainter.TypeToDevicesFullData.Clear();


			foreach (DeviceData device in deviceList)
			{
				DeviceFullData deviceFullData = DeviceFullData.Factory(device);

				deviceFullData.Init("EVVA");

				DevicesContainter.DevicesFullDataList.Add(deviceFullData);
				DevicesContainter.DevicesList.Add(device);
				if (DevicesContainter.TypeToDevicesFullData.ContainsKey(device.DeviceType) == false)
					DevicesContainter.TypeToDevicesFullData.Add(device.DeviceType, deviceFullData);
			}

			LoggerDevicesList = new ObservableCollection<LoggerDevice>();
			foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
			{
				LoggerDevicesList.Add(new LoggerDevice() 
				{ 
					Device = device.Device, 
					IsThermocoupleK = true,
					IsRecord = true,
				});

				device.Connect();
				device.InitCheckConnection();

				foreach(DeviceParameterData param in device.Device.ParemetersList) 
				{
					device.ParametersRepository.Add(param, RepositoryPriorityEnum.High, Callback);
				}
			}
		}

		private void InitCommunicationSettings()
		{
			Docking.OpenCommSettings();
		}

		private void Callback(DeviceParameterData param, CommunicatorResultEnum result, string errorDescription)
		{
		}

		private void BrowseRecordFile()
		{
			CommonOpenFileDialog commonOpenFile = new CommonOpenFileDialog();
			commonOpenFile.IsFolderPicker = true;
			commonOpenFile.InitialDirectory = _loggerViewerUserData.RecordingDirectory;
			CommonFileDialogResult results = commonOpenFile.ShowDialog();
			if (results != CommonFileDialogResult.Ok)
				return;

			RecordDirectory = commonOpenFile.FileName;
			_loggerViewerUserData.RecordingDirectory = RecordDirectory;
		}

		private void StartRecording()
		{
			if (string.IsNullOrEmpty(RecordDirectory))
				return;

			ObservableCollection<DeviceParameterData> recordParamsList = 
				new ObservableCollection<DeviceParameterData>();
			foreach(LoggerDevice loggerDevice in LoggerDevicesList)
			{
				DeviceFullData deviceFullData =
					DevicesContainter.TypeToDevicesFullData[loggerDevice.Device.DeviceType];
				if (deviceFullData.DeviceCommunicator.IsInitialized == false)
					continue;

				if(loggerDevice.IsRecord == false) 
					continue;

				foreach(DeviceParameterData param in loggerDevice.Device.ParemetersList) 
				{
					recordParamsList.Add(param);
				}
			}

			_paramRecording.StartRecording("Temp Logger", RecordDirectory, recordParamsList, true);
			IsRecording = true;
		}

		private void StopRecording()
		{
			_paramRecording.StopRecording();
			IsRecording = false;
		}

		#region Set TC type

		private void ThermocoupleType(LoggerDevice loggerDevice)
		{
			DeviceFullData deviceFullData = DevicesContainter.TypeToDevicesFullData[loggerDevice.Device.DeviceType];

			if (loggerDevice.Device.DeviceType == DeviceTypesEnum.BrainChild)
			{
				ThermocoupleType_BrainChild(loggerDevice, deviceFullData);
			}
			else if (loggerDevice.Device.DeviceType == DeviceTypesEnum.FieldLogger)
			{
				ThermocoupleType_FieldLogger(loggerDevice, deviceFullData);
			}
		}

		private void ThermocoupleType_BrainChild(
			LoggerDevice loggerDevice,
			DeviceFullData deviceFullData)
		{
			char tcType = ' ';
			if (loggerDevice.IsThermocoupleK)
				tcType = 'K';
			else if (loggerDevice.IsThermocoupleT)
				tcType = 'T';
			((BrainChild_Communicator)deviceFullData.DeviceCommunicator).SetTCType(tcType);
		}

		private void ThermocoupleType_FieldLogger(
			LoggerDevice loggerDevice,
			DeviceFullData deviceFullData)
		{
			char tcType = ' ';
			if (loggerDevice.IsThermocoupleK)
				tcType = 'K';
			else if (loggerDevice.IsThermocoupleT)
				tcType = 'T';
			((FieldLogger_Communicator)deviceFullData.DeviceCommunicator).SetTCType(tcType);
		}

		#endregion Set TC type


		#region Channels name

		private void SaveNames()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Text Files | *.txt";
			saveFileDialog.InitialDirectory = _loggerViewerUserData.ChannelsNameDirectory;
			bool? result = saveFileDialog.ShowDialog();
			if (result != true)
				return;

			_loggerViewerUserData.ChannelsNameDirectory = 
				Path.GetDirectoryName(saveFileDialog.FileName);

			using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
			{
				foreach(DeviceData device in DevicesContainter.DevicesList)
				{
					sw.WriteLine("Device Name:" + device.Name);

					foreach (DeviceParameterData param in device.ParemetersList) 
					{ 
						sw.WriteLine(param.Name);
					}

					sw.WriteLine("");
					sw.WriteLine("");
				}
				
			}
		}

		private void LoadNames()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Text Files | *.txt";
			openFileDialog.InitialDirectory = _loggerViewerUserData.ChannelsNameDirectory;
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;



			_loggerViewerUserData.ChannelsNameDirectory =
				Path.GetDirectoryName(openFileDialog.FileName);

			string fileData = string.Empty;
			using(StreamReader sr = new StreamReader(openFileDialog.FileName)) 
			{ 
				fileData = sr.ReadToEnd();
			}

			string[] fileLines = fileData.Split("\r\n");

			DeviceData device = null;
			int paramCounter = 0;
			foreach(string line in fileLines) 
			{ 
				if(string.IsNullOrEmpty(line)) 
					continue;

				if(line.StartsWith("Device Name:"))
				{
					paramCounter = 0;
					string name = line.Substring("Device Name:".Length);
					device = DevicesContainter.DevicesList.ToList().Find((d) => d.Name == name);
					continue;
				}

				device.ParemetersList[paramCounter++].Name = line;
			}
		}

		#endregion Channels name

		#endregion Methods

		#region Commands

		public RelayCommand CommunicationSettingsCommand { get; private set; }
		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }

		public RelayCommand SaveNamesCommand { get; private set; }
		public RelayCommand LoadNamesCommand { get; private set; }

		public RelayCommand BrowseRecordFileCommand { get; private set; }
		public RelayCommand StartRecordingCommand { get; private set; }
		public RelayCommand StopRecordingCommand { get; private set; }
		public RelayCommand<LoggerDevice> ThermocoupleTypeCommand { get; private set; }

		#endregion Commands
	}
}
