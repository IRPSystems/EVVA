
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using DeviceHandler.Interfaces;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using Entities.Models;
using Microsoft.Win32;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ParamLimitsTest
{
    public class MainWindowViewModel: ObservableObject
    {
        public TestParamsLimitViewModel TestParamsLimit { get; set; }

		public IConnectionViewModel CanConnect { get; set; }

		public DevicesContainer DevicesContainter { get; set; }

		public string Version { get; set; }

		private DeviceFullData _mcuDevice;

		public MainWindowViewModel() 
        {
			try
			{
				LoggerService.Init("ParamLimitsTest.log", Serilog.Events.LogEventLevel.Information);
				LoggerService.Inforamtion(this, "-------------------------------------- ParamLimitsTest ---------------------");

				Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();


				ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
				LoadJsonCommand = new RelayCommand(LoadJson);

				ReadDevicesFileService reader = new ReadDevicesFileService();
				ObservableCollection<DeviceData> devicesList = new ObservableCollection<DeviceData>();
				reader.ReadFromMCUJson(
					"param_defaults.json",
					devicesList,
					"MCU",
					DeviceTypesEnum.MCU);

				_mcuDevice = DeviceFullData.Factory(devicesList[0]);

				_mcuDevice.Init();
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
			catch(Exception ex)
			{
				LoggerService.Error(this, "Constructor failed", "Constructor Error", ex);
			}
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
			ObservableCollection<DeviceData> devicesList = new ObservableCollection<DeviceData>();
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
		}

		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }
		public RelayCommand LoadJsonCommand { get; private set; }
	}
}
