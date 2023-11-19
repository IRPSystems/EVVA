
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using Entities.Enums;
using Entities.Models;
using System.Collections.ObjectModel;

namespace Evva.ViewModels
{
	public class TestParamsLimitViewModel: ObservableObject
	{
		public enum TestResult
		{
			Success, Failure, None
		}

		public class TestData
		{
			public DeviceParameterData Param { get; set; }
			public TestResult Result { get; set; }
		}
		
		public ObservableCollection<TestData> ParametersList { get; set; }

		private DevicesContainer _devicesContainer;

		public TestParamsLimitViewModel(DevicesContainer devicesContainer)
		{
			_devicesContainer = devicesContainer;

			TestCommand = new RelayCommand(Test);
		}

		public void Test()
		{
			ParametersList = new ObservableCollection<TestData>();

			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU))
			{
				DeviceFullData mcuDevice = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];
				foreach(DeviceParameterData param in mcuDevice.Device.ParemetersList) 
				{
					ParametersList.Add(new TestData { Param = param, Result = TestResult.None });
				}
			}

			
		}

		public RelayCommand TestCommand { get; private set; }

	}
}
