
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using DeviceHandler.Models;
using Entities.Enums;
using Entities.Models;
using ScriptHandler.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace Evva.ViewModels
{
	public class TestParamsLimitViewModel: ObservableObject
	{
		public enum TestResultEnum
		{
			Success, Failure, None
		}

		public enum TestTypeEnum
		{
			ValueValid,
			SmallerThanRange,
			LargerThanRange,
			DropDownValue
		}

		public class TestData : ObservableObject
		{
			public DeviceParameterData Param { get; set; }
			public TestResultEnum Result { get; set; }
		}

		public class TestReprotData
		{
			public string ParamName { get; set; }
			public double Value { get; set; }
			public TestTypeEnum TestType { get; set; }
			public bool IsPass { get; set; }
			public string ErrorDescription { get; set; }
		}

		#region Properties and Fields

		public ObservableCollection<TestData> ParametersList { get; set; }

		public double TestProgress { get; set; }

		public string ErrorText { get; set; }
		public Brush ErrorBackground { get; set; }
		public Brush ErrorForeground { get; set; }

		private DevicesContainer _devicesContainer;

		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private List<TestReprotData> _testReprotDataList;

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

			_testReprotDataList = new List<TestReprotData>();

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
					ParametersList.Add(new TestData { Param = param, Result = TestResultEnum.None });
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

					TestResultEnum result = TestResultEnum.None;
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


					ErrorText = ErrorText.Replace("\r\n", " - ");

					if (ErrorText.EndsWith("Communication timeout."))
					{
						Cancel();
						ErrorBackground = Brushes.Red;
						ErrorForeground = Brushes.White;
						continue;
					}

					else ErrorBackground = Brushes.Transparent;


					test.Result = result;
					

					System.Threading.Thread.Sleep(1);

				}

				if (!ErrorText.EndsWith("Communication timeout."))
				{
					ErrorText = "Test Ended";
					ErrorForeground = Application.Current.MainWindow.Foreground;
					ErrorBackground = Brushes.Transparent;
				}


			}, _cancellationToken);

			
		}

		#region Test param


		private TestResultEnum TestRangeParam(
			MCU_ParamData mcuParam,
			ScriptStepSetParameter scriptStepSetParameter)
		{
			#region Test out lower limit of range
			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = mcuParam.Range[0] - 1;
			scriptStepSetParameter.Execute();
			if(scriptStepSetParameter.IsPass == true) 
			{
				SetTestReprotItem(
					mcuParam.Name,
					TestTypeEnum.SmallerThanRange,
					scriptStepSetParameter.Value,
					false,
					scriptStepSetParameter.ErrorMessage);
				return TestResultEnum.Failure;
			}
			if (scriptStepSetParameter.IsPass == false)
			{
				if (scriptStepSetParameter.ErrorMessage.EndsWith("Communication timeout."))
				{
					SetTestReprotItem(
						mcuParam.Name,
						TestTypeEnum.SmallerThanRange,
						scriptStepSetParameter.Value,
						false,
						scriptStepSetParameter.ErrorMessage);
					ErrorText = scriptStepSetParameter.ErrorMessage;
					return TestResultEnum.Failure;
				}				
			}

			SetTestReprotItem(
					mcuParam.Name,
					TestTypeEnum.SmallerThanRange,
					scriptStepSetParameter.Value,
					true,
					null);
			#endregion Test out of lower limit of range

			#region Test higher limit of range
			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = mcuParam.Range[1] + 1;
			scriptStepSetParameter.Execute();
			if (scriptStepSetParameter.IsPass == true)
			{
				SetTestReprotItem(
					mcuParam.Name,
					TestTypeEnum.LargerThanRange,
					scriptStepSetParameter.Value,
					false,
					scriptStepSetParameter.ErrorMessage);
				return TestResultEnum.Failure;
			}
			if (scriptStepSetParameter.IsPass == false)
			{
				if (scriptStepSetParameter.ErrorMessage.EndsWith("Communication timeout."))
				{
					SetTestReprotItem(
						mcuParam.Name,
						TestTypeEnum.LargerThanRange,
						scriptStepSetParameter.Value,
						false,
						scriptStepSetParameter.ErrorMessage);
					ErrorText = scriptStepSetParameter.ErrorMessage;
					return TestResultEnum.Failure;
				}
			}

			SetTestReprotItem(
					mcuParam.Name,
					TestTypeEnum.LargerThanRange,
					scriptStepSetParameter.Value,
					true,
					null);
			#endregion Test higher limit of range

			#region Test center of range

			double range = mcuParam.Range[1] - mcuParam.Range[0];
			double center = mcuParam.Range[0] + (range / 2);

			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = center;
			scriptStepSetParameter.Execute();
			if (scriptStepSetParameter.IsPass == false)
			{
				SetTestReprotItem(
					mcuParam.Name,
					TestTypeEnum.ValueValid,
					scriptStepSetParameter.Value,
					false,
					scriptStepSetParameter.ErrorMessage);

				if (scriptStepSetParameter.ErrorMessage.EndsWith("Communication timeout."))
				{
					ErrorText = scriptStepSetParameter.ErrorMessage;
					return TestResultEnum.Failure;
				}

				return TestResultEnum.Failure;
			}

			SetTestReprotItem(
					mcuParam.Name,
					TestTypeEnum.ValueValid,
					scriptStepSetParameter.Value,
					true,
					null);
			#endregion center limit of range

			return TestResultEnum.Success;
		}

		private TestResultEnum TestDropDownParam(
			MCU_ParamData mcuParam,
			ScriptStepSetParameter scriptStepSetParameter)
		{
			if (mcuParam.DropDown == null && mcuParam.DropDown.Count == 0)
			{
				SetTestReprotItem(
					mcuParam.Name,
					TestTypeEnum.DropDownValue,
					0,
					false,
					"No drop-down values found");
				return TestResultEnum.Failure;
			}

			double value;
			bool res = double.TryParse(mcuParam.DropDown[0].Value, out value);
			if (res == false) 
			{
				SetTestReprotItem(
					mcuParam.Name,
					TestTypeEnum.DropDownValue,
					value,
					false,
					"The value at the drop down is not value"); 
				return TestResultEnum.Failure;
			}

			scriptStepSetParameter.Parameter = mcuParam;
			scriptStepSetParameter.Value = value;
			scriptStepSetParameter.Execute();
			if (scriptStepSetParameter.IsPass == false)
			{
				if (scriptStepSetParameter.ErrorMessage.EndsWith("Communication timeout."))
				{
					SetTestReprotItem(
						mcuParam.Name,
						TestTypeEnum.DropDownValue,
						value,
						false,
						scriptStepSetParameter.ErrorMessage); 
					ErrorText = scriptStepSetParameter.ErrorMessage;
					return TestResultEnum.Failure;
				}

				SetTestReprotItem(
						mcuParam.Name,
						TestTypeEnum.DropDownValue,
						value,
						false,
						scriptStepSetParameter.ErrorMessage);
				return TestResultEnum.Failure;
			}

			SetTestReprotItem(
				mcuParam.Name,
				TestTypeEnum.DropDownValue,
				value,
				true,
				null);
			return TestResultEnum.Success;
		}

		private TestResultEnum TestRegularParam(
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
					SetTestReprotItem(
						mcuParam.Name,
						TestTypeEnum.ValueValid,
						value,
						false,
						scriptStepSetParameter.ErrorMessage);
					ErrorText = scriptStepSetParameter.ErrorMessage;
					return TestResultEnum.Failure;
				}

				SetTestReprotItem(
						mcuParam.Name,
						TestTypeEnum.ValueValid,
						value,
						false,
						scriptStepSetParameter.ErrorMessage);
				return TestResultEnum.Failure;
			}

			SetTestReprotItem(
						mcuParam.Name,
						TestTypeEnum.ValueValid,
						value,
						true,
						null);

			return TestResultEnum.Success;
		}


		private void SetTestReprotItem(
			string name,
			TestTypeEnum testType,
			double value,
			bool isPass,
			string errorDescription)
		{
			TestReprotData testReprotData = new TestReprotData()
			{
				ParamName = name,
				TestType = testType,
				Value = value,
				IsPass = isPass,
				ErrorDescription = errorDescription
			};

			_testReprotDataList.Add(testReprotData);
		}

		#endregion Test param

		private void Cancel()
		{
			if(_cancellationTokenSource != null)
				_cancellationTokenSource.Cancel();

			TestProgress = 0;
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
