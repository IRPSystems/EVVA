
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using Entities.Enums;
using Entities.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TempLoggerViewer
{
	public class TempLoggerViewModel: ObservableObject
	{
		public DevicesContainer DevicesContainter { get; set; }
		//public DocingViewModel Docking { get; set; }

		public TempLoggerViewModel()
		{
			InitDevicesContainter();

			CommunicationSettingsCommand = new RelayCommand(InitCommunicationSettings);
		}

		private void InitDevicesContainter()
		{ 
			DevicesContainter = new DevicesContainer();

			ReadDevicesFileService readDevicesFile = new ReadDevicesFileService();
			ObservableCollection<DeviceBase> deviceList = readDevicesFile.ReadAllFiles(
				"",
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


			foreach (DeviceBase device in deviceList)
			{
				DeviceFullData deviceFullData = new DeviceFullData(device as DeviceData);

				deviceFullData.Init();

				DevicesContainter.DevicesFullDataList.Add(deviceFullData);
				DevicesContainter.DevicesList.Add(device as DeviceData);
				if (DevicesContainter.TypeToDevicesFullData.ContainsKey(device.DeviceType) == false)
					DevicesContainter.TypeToDevicesFullData.Add(device.DeviceType, deviceFullData);
			}

			foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
				device.Connect();

			
		}

		private void InitCommunicationSettings()
		{
			//Docking.OpenCommSettings();
		}


		public RelayCommand CommunicationSettingsCommand { get; private set; }
	}
}
