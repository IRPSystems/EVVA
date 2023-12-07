
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Services;
using DeviceHandler.Interfaces;
using DeviceHandler.Models;
using Entities.Enums;
using Entities.Models;
using Microsoft.Win32;
using Services.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ParamLimitsTest
{
    public class MainWindowViewModel: ObservableObject
    {
        public TestParamsLimitViewModel TestParamsLimit { get; set; }

		public IConnectionViewModel CanConnect { get; set; }

		public DevicesContainer DevicesContainter { get; set; }

		private DeviceFullData _mcuDevice;

		public MainWindowViewModel() 
        {
			LoggerService.Init("ParamLimitsTest.log", Serilog.Events.LogEventLevel.Information);
			LoggerService.Inforamtion(this, "-------------------------------------- ParamLimitsTest ---------------------");


			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
			LoadJsonCommand = new RelayCommand(LoadJson);

			ReadDevicesFileService reader = new ReadDevicesFileService();
			ObservableCollection<DeviceBase> devicesList = new ObservableCollection<DeviceBase>();
			reader.ReadFromMCUJson(
				"param_defaults.json",
				devicesList,
				"MCU",
				DeviceTypesEnum.MCU);

			_mcuDevice = new DeviceFullData(devicesList[0] as DeviceData);

			_mcuDevice.Init();
			(_mcuDevice.DeviceCommunicator as MCU_Communicator).InitMessageDict(_mcuDevice.Device);
			_mcuDevice.Connect();
			_mcuDevice.InitCheckConnection();
			CanConnect = _mcuDevice.ConnectionViewModel;

			DevicesContainter = new DevicesContainer();
			DevicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
			DevicesContainter.DevicesList = new ObservableCollection<DeviceData>();
			DevicesContainter.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();

			DevicesContainter.DevicesFullDataList.Add(_mcuDevice);
			DevicesContainter.DevicesList.Add(_mcuDevice.Device);
			DevicesContainter.TypeToDevicesFullData.Add(_mcuDevice.Device.DeviceType, _mcuDevice);


			TestParamsLimit = new TestParamsLimitViewModel(DevicesContainter);
        }

		private void Closing(CancelEventArgs e)
		{
			_mcuDevice.Disconnect();
		}

		private void LoadJson()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "JSON Files|*.json";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			ReadDevicesFileService reader = new ReadDevicesFileService();
			ObservableCollection<DeviceBase> devicesList = new ObservableCollection<DeviceBase>();
			reader.ReadFromMCUJson(
				openFileDialog.FileName,
				devicesList,
				"MCU",
				DeviceTypesEnum.MCU);

			int index = DevicesContainter.DevicesList.IndexOf(_mcuDevice.Device);
			if (index >= 0)
			{
				DevicesContainter.DevicesList[index] = devicesList[0] as DeviceData;
			}

			devicesList[0].Name = _mcuDevice.Device.Name;
			_mcuDevice.Device = devicesList[0] as DeviceData;

			((MCU_Communicator)(_mcuDevice.DeviceCommunicator)).InitMessageDict(
						_mcuDevice.Device);
		}

		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }
		public RelayCommand LoadJsonCommand { get; private set; }
	}
}
