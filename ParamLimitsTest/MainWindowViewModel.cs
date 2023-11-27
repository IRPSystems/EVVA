
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Services;
using DeviceHandler.Interfaces;
using DeviceHandler.Models;
using Entities.Enums;
using Entities.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ParamLimitsTest
{
    public class MainWindowViewModel: ObservableObject
    {
        public TestParamsLimitViewModel TestParamsLimit { get; set; }

		public IConnectionViewModel CanConnect { get; set; }

		public DevicesContainer DevicesContainter { get; set; }

		public MainWindowViewModel() 
        {
			ReadDevicesFileService reader = new ReadDevicesFileService();
			ObservableCollection<DeviceBase> devicesList = new ObservableCollection<DeviceBase>();
			reader.ReadFromMCUJson(
				"param_defaults.json",
				devicesList,
				"MCU",
				DeviceTypesEnum.MCU);

			DeviceFullData MCUDevice = new DeviceFullData(devicesList[0] as DeviceData);

			MCUDevice.Init();
			(MCUDevice.DeviceCommunicator as MCU_Communicator).InitMessageDict(MCUDevice.Device);
			MCUDevice.Connect();
			MCUDevice.InitCheckConnection();
			CanConnect = MCUDevice.ConnectionViewModel;

			DevicesContainter = new DevicesContainer();
			DevicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
			DevicesContainter.DevicesList = new ObservableCollection<DeviceData>();
			DevicesContainter.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();

			DevicesContainter.DevicesFullDataList.Add(MCUDevice);
			DevicesContainter.DevicesList.Add(MCUDevice.Device);
			DevicesContainter.TypeToDevicesFullData.Add(MCUDevice.Device.DeviceType, MCUDevice);


			TestParamsLimit = new TestParamsLimitViewModel(DevicesContainter);
        }
    }
}
