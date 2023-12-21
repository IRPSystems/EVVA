
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceHandler.ViewModels;
using Entities.Enums;
using Entities.Models;
using Evva.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using TempLoggerViewer.ViewModels;

namespace TempLoggerViewer
{
	public class TempLoggerMainWindowVModel: ObservableObject
	{
		public DevicesContainer DevicesContainter { get; set; }
		public DocingTempLoggerViewModel Docking { get; set; }

		public string Version { get; set; }

		public TempLoggerMainWindowVModel()
		{
			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			InitDevicesContainter();

			CommunicationViewModel communicationSettings =
				new CommunicationViewModel(DevicesContainter);
			Docking = new DocingTempLoggerViewModel(communicationSettings);
			CommunicationSettingsCommand = new RelayCommand(InitCommunicationSettings);
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

				deviceFullData.Init();

				DevicesContainter.DevicesFullDataList.Add(deviceFullData);
				DevicesContainter.DevicesList.Add(device);
				if (DevicesContainter.TypeToDevicesFullData.ContainsKey(device.DeviceType) == false)
					DevicesContainter.TypeToDevicesFullData.Add(device.DeviceType, deviceFullData);
			}

			foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
				device.Connect();
		}

		private void InitCommunicationSettings()
		{
			Docking.OpenCommSettings();
		}


		public RelayCommand CommunicationSettingsCommand { get; private set; }
	}
}
