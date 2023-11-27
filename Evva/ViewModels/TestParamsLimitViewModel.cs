
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using DeviceHandler.Models;
using Entities.Enums;
using Entities.Models;
using ScriptHandler.Models;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Evva.ViewModels
{
	public class TestParamsLimitViewModel: ObservableObject
	{
		public enum TestResult
		{
			Success, Failure, None
		}

		public class TestData : ObservableObject
		{
			public DeviceParameterData Param { get; set; }
			public TestResult Result { get; set; }
		}

		#region Properties and Fields

		public ObservableCollection<TestData> ParametersList { get; set; }

		public double TestProgress { get; set; }

		public string ErrorText { get; set; }
		public Brush ErrorBackground { get; set; }
		public Brush ErrorForeground { get; set; }

		private DevicesContainer _devicesContainer;

		protected CancellationTokenSource _cancellationTokenSource;
		protected CancellationToken _cancellationToken;

		#endregion Properties and Fields

		#region Constructor

		public TestParamsLimitViewModel(DevicesContainer devicesContainer)
		{
			_devicesContainer = devicesContainer;

			TestCommand = new RelayCommand(Test);
			CancelCommand = new RelayCommand(Cancel);
			UnLoadedCommand = new RelayCommand(UnLoaded);
		}

		#endregion Constructor

		#region Methods

		public void Test()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;


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

			ErrorText = "Test in progress, please wait...";
			ErrorForeground = Application.Current.MainWindow.Foreground;

			Task.Run(() =>
			{

				for (int i = 0; i < ParametersList.Count && !_cancellationToken.IsCancellationRequested; i++)
				{
					TestData test = ParametersList[i];
					if (!(test.Param is MCU_ParamData mcuParam))
						continue;

					Application.Current.Dispatcher.Invoke(() =>
					{
						TestProgress = (((double)i + 1.0) / (double)ParametersList.Count) * 100.0;
					});

					TestResult result = TestResult.None;
					if (mcuParam.Range != null && mcuParam.Range.Count > 0)
					{
						result = TestRangeParam(
							mcuParam,
							scriptStepSetParameter);
					}
					else if(mcuParam.DropDown != null && mcuParam.DropDown.Count > 0)
					{
						result = TestDropDownParam(
							mcuParam,
							scriptStepSetParameter);
					}
					else
					{
						result = TestRegularParam(
							mcuParam,
							scriptStepSetParameter);
					}

					if (ErrorText.EndsWith("Communication timeout."))
					{
						Cancel();
						ErrorBackground = Brushes.Red;
						ErrorForeground = Brushes.White;
						continue;
					}

					else ErrorBackground = Brushes.Transparent;


					Application.Current.Dispatcher.Invoke(() =>
					{
						test.Result = result;
					});

					System.Threading.Thread.Sleep(1);

				}

				
			}, _cancellationToken);

			
		}

		private TestResult TestRangeParam(
			MCU_ParamData mcuParam,
			ScriptStepSetParameter scriptStepSetParameter)
		{
			#region Test out lower limit of range
			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = mcuParam.Range[0] - 1;
			scriptStepSetParameter.Execute();
			if(scriptStepSetParameter.IsPass == true) 
			{ 
				return TestResult.Failure;
			}
			if (scriptStepSetParameter.IsPass == false)
			{
				if (scriptStepSetParameter.ErrorMessage.EndsWith("Communication timeout."))
				{
					ErrorText = scriptStepSetParameter.ErrorMessage;
					return TestResult.Failure;
				}				
			}
			#endregion Test out of lower limit of range

			#region Test higher limit of range
			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = mcuParam.Range[1] + 1;
			scriptStepSetParameter.Execute();
			if (scriptStepSetParameter.IsPass == true)
			{
				return TestResult.Failure;
			}
			if (scriptStepSetParameter.IsPass == false)
			{
				if (scriptStepSetParameter.ErrorMessage.EndsWith("Communication timeout."))
				{
					ErrorText = scriptStepSetParameter.ErrorMessage;
					return TestResult.Failure;
				}
			}
			#endregion Test higher limit of range

			#region Test center of range

			double range = mcuParam.Range[1] - mcuParam.Range[0];
			double center = mcuParam.Range[0] + (range / 2);

			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = center;
			scriptStepSetParameter.Execute();
			if (scriptStepSetParameter.IsPass == false)
			{
				if (scriptStepSetParameter.ErrorMessage.EndsWith("Communication timeout."))
				{
					ErrorText = scriptStepSetParameter.ErrorMessage;
					return TestResult.Failure;
				}

				return TestResult.Failure;
			}
			#endregion center limit of range

			return TestResult.Success;
		}

		private TestResult TestDropDownParam(
			MCU_ParamData mcuParam,
			ScriptStepSetParameter scriptStepSetParameter)
		{
			if (mcuParam.DropDown == null && mcuParam.DropDown.Count == 0)
			{
				return TestResult.Failure;
			}

			double value;
			bool res = double.TryParse(mcuParam.DropDown[0].Value, out value);
			if (res == false) 
			{ 
				return TestResult.Failure;
			}

			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = value;
			scriptStepSetParameter.Execute();
			if (scriptStepSetParameter.IsPass == false)
			{
				if (scriptStepSetParameter.ErrorMessage.EndsWith("Communication timeout."))
				{
					ErrorText = scriptStepSetParameter.ErrorMessage;
					return TestResult.Failure;
				}

				return TestResult.Failure;
			}

			return TestResult.Success;
		}

		private TestResult TestRegularParam(
			MCU_ParamData mcuParam,
			ScriptStepSetParameter scriptStepSetParameter)
		{

			double value = 100;
			

			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = value;
			scriptStepSetParameter.Execute();
			if (scriptStepSetParameter.IsPass == false)
			{
				if (scriptStepSetParameter.ErrorMessage.EndsWith("Communication timeout."))
				{
					ErrorText = scriptStepSetParameter.ErrorMessage;
					return TestResult.Failure;
				}

				return TestResult.Failure;
			}

			return TestResult.Success;
		}


		private void Cancel()
		{
			_cancellationTokenSource.Cancel();

			Application.Current.Dispatcher.Invoke(() =>
			{
				TestProgress = 0;
			});
		}

		protected void UnLoaded()
		{
			Cancel();
		}

		#endregion Methods

		#region Commands

		public RelayCommand TestCommand { get; private set; }
		public RelayCommand CancelCommand { get; private set; }
		public RelayCommand UnLoadedCommand { get; private set; }

		#endregion Commands
	}
}
