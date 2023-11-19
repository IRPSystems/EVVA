
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using DeviceHandler.Models;
using Entities.Enums;
using Entities.Models;
using ScriptHandler.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

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

		#region Properties and Fields

		public ObservableCollection<TestData> ParametersList { get; set; }

		public double TestProgress { get; set; }

		private DevicesContainer _devicesContainer;

		#endregion Properties and Fields

		#region Constructor

		public TestParamsLimitViewModel(DevicesContainer devicesContainer)
		{
			_devicesContainer = devicesContainer;

			TestCommand = new RelayCommand(Test);
		}

		#endregion Constructor

		#region Methods

		public void Test()
		{
			TestProgress = 0;
			ParametersList = new ObservableCollection<TestData>();

			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU) == false)
				return;

			DeviceFullData mcuDevice = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];
			if (mcuDevice == null || mcuDevice.Device == null)
				return;



			foreach (DeviceParameterData param in mcuDevice.Device.ParemetersList)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					ParametersList.Add(new TestData { Param = param, Result = TestResult.None });
				});

				System.Threading.Thread.Sleep(1);
			}

			RunTest(mcuDevice);
		}

		private void RunTest(DeviceFullData mcuDevice)
		{
			ScriptStepSetParameter scriptStepSetParameter = new ScriptStepSetParameter()
			{
				Communicator = mcuDevice.DeviceCommunicator,
			};

			ScriptStepGetParamValue scriptStepGetParamValue = new ScriptStepGetParamValue()
			{
				Communicator = mcuDevice.DeviceCommunicator,
			};

			Task.Run(() =>
			{

				for (int i = 0; i < ParametersList.Count; i++)
				{
					TestData test = ParametersList[i];
					if (!(test.Param is MCU_ParamData mcuParam))
						continue;

					Application.Current.Dispatcher.Invoke(() =>
					{
						TestProgress = (((double)i + 1.0) / (double)ParametersList.Count) * 100.0;
					});

					if (mcuParam.Range != null && mcuParam.Range.Count > 0)
					{
						TestResult result = TestRangeParam(
							mcuParam,
							scriptStepSetParameter,
							scriptStepGetParamValue);
						test.Result = result;
					}

					System.Threading.Thread.Sleep(1);

				}

				Application.Current.Dispatcher.Invoke(() =>
				{
					OnPropertyChanged(nameof(ParametersList));
				});

				
			});

			
		}

		private TestResult TestRangeParam(
			MCU_ParamData mcuParam,
			ScriptStepSetParameter scriptStepSetParameter,
			ScriptStepGetParamValue scriptStepGetParamValue)
		{
			#region Test lower limit of range
			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = mcuParam.Range[0] - 1;
			scriptStepSetParameter.Execute();

			scriptStepGetParamValue.Parameter = mcuParam;
			scriptStepGetParamValue.SendAndReceive();
			if(scriptStepGetParamValue.IsPass == false)
				return TestResult.Failure;
			#endregion Test lower limit of range

			#region Test higher limit of range
			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = mcuParam.Range[1] + 1;
			scriptStepSetParameter.Execute();

			scriptStepGetParamValue.Parameter = mcuParam;
			scriptStepGetParamValue.SendAndReceive();
			if (scriptStepGetParamValue.IsPass == false)
				return TestResult.Failure;
			#endregion Test higher limit of range

			#region Test center of range

			double range = mcuParam.Range[1] - mcuParam.Range[0];
			double center = mcuParam.Range[0] + (range / 2);

			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = center;
			scriptStepSetParameter.Execute();

			scriptStepGetParamValue.Parameter = mcuParam;
			scriptStepGetParamValue.SendAndReceive();
			if (scriptStepGetParamValue.IsPass == false)
				return TestResult.Failure;
			#endregion center limit of range

			return TestResult.Success;
		}

		#endregion Methods

		#region Commands

		public RelayCommand TestCommand { get; private set; }

		#endregion Commands
	}
}
