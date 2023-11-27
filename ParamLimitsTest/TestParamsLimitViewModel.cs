
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using DeviceCommunicators.MCU;
using DeviceHandler.Models;
using Entities.Enums;
using Entities.Models;
using Microsoft.Win32;
using ScriptHandler.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace ParamLimitsTest
{
	public class TestParamsLimitViewModel: ObservableObject
	{

		public enum TestTypeEnum
		{
			ValueValid,
			SmallerThanRange,
			LargerThanRange,
			DropDownValue
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

		public double TestProgress { get; set; }

		public string ErrorText { get; set; }
		public Brush ErrorBackground { get; set; }
		public Brush ErrorForeground { get; set; }

		public ObservableCollection<TestReprotData> TestReprotDataList { get; set; }

		#endregion Properties

		#region Fields

		private DevicesContainer _devicesContainer;

		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private DeviceFullData _mcuDevice;


		#endregion Fields

		#region Constructor

		public TestParamsLimitViewModel(DevicesContainer devicesContainer)
		{
			_devicesContainer = devicesContainer;

			TestCommand = new RelayCommand(Test);
			CancelCommand = new RelayCommand(Cancel);
			UnLoadedCommand = new RelayCommand(UnLoaded);
			ExportCommand = new RelayCommand(Export);
		}

		#endregion Constructor

		#region Methods

		public void Test()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;

			TestReprotDataList = new ObservableCollection<TestReprotData>();

			TestProgress = 0;

			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU) == false)
				return;

			_mcuDevice = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];
			if (_mcuDevice == null || _mcuDevice.Device == null)
				return;



			RunTest();
		}

		private void RunTest()
		{
			ScriptStepSetParameter scriptStepSetParameter = new ScriptStepSetParameter()
			{
				Communicator = _mcuDevice.DeviceCommunicator,
			};

			ErrorText = "Test in progress, please wait...";
			ErrorForeground = Application.Current.MainWindow.Foreground;

			Task.Run(() =>
			{
				try
				{

					for (int i = 0; i < _mcuDevice.Device.ParemetersList.Count && !_cancellationToken.IsCancellationRequested; i++)
					{
						DeviceParameterData test = _mcuDevice.Device.ParemetersList[i];
						if (!(test is MCU_ParamData mcuParam))
							continue;

						Application.Current.Dispatcher.Invoke(() =>
						{
							TestProgress = (((double)i + 1.0) / (double)_mcuDevice.Device.ParemetersList.Count) * 100.0;
						});

						if (mcuParam.Range != null && mcuParam.Range.Count > 0)
						{
							TestRangeParam(
								mcuParam,
								scriptStepSetParameter);
						}
						else if (mcuParam.DropDown != null && mcuParam.DropDown.Count > 0)
						{
							TestDropDownParam(
								mcuParam,
								scriptStepSetParameter);
						}
						else
						{
							TestRegularParam(
								mcuParam,
								scriptStepSetParameter);
						}


						ErrorText = ErrorText.Replace("\r\n", " - ");

						if (ErrorText.EndsWith("Communication timeout."))
						{
							Application.Current.Dispatcher.Invoke(() =>
							{
								Cancel();
								ErrorBackground = Brushes.Red;
								ErrorForeground = Brushes.White;
							});
							continue;
						}

						else ErrorBackground = Brushes.Transparent;



						System.Threading.Thread.Sleep(1);

					}

					if (!ErrorText.EndsWith("Communication timeout."))
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							ErrorText = "Test Ended";
							ErrorForeground = Application.Current.MainWindow.Foreground;
							ErrorBackground = Brushes.Transparent;
						});
					}
				}
				catch(Exception ex)
				{
					LoggerService.Error(this, "Faild to test the parameters", ex);
				}

			}, _cancellationToken);

			
		}

		#region Test param


		private void TestRangeParam(
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
				return;
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
					return;
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
				return;
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
					return;
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
					return;
				}

				return;
			}

			SetTestReprotItem(
					mcuParam.Name,
					TestTypeEnum.ValueValid,
					scriptStepSetParameter.Value,
					true,
					null);
			#endregion center limit of range
		}

		private void TestDropDownParam(
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
				return;
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
				return;
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
					return;
				}

				SetTestReprotItem(
						mcuParam.Name,
						TestTypeEnum.DropDownValue,
						value,
						false,
						scriptStepSetParameter.ErrorMessage);
				return;
			}

			SetTestReprotItem(
				mcuParam.Name,
				TestTypeEnum.DropDownValue,
				value,
				true,
				null);
		}

		private void TestRegularParam(
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
					return;
				}

				SetTestReprotItem(
						mcuParam.Name,
						TestTypeEnum.ValueValid,
						value,
						false,
						scriptStepSetParameter.ErrorMessage);
				return;
			}

			SetTestReprotItem(
						mcuParam.Name,
						TestTypeEnum.ValueValid,
						value,
						true,
						null);
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

			Application.Current.Dispatcher.Invoke(() =>
			{
				TestReprotDataList.Add(testReprotData);
			});
		}

		#endregion Test param

		private void Cancel()
		{
			if(_cancellationTokenSource != null)
				_cancellationTokenSource.Cancel();

			TestProgress = 0;
		}

		private void UnLoaded()
		{
			Cancel();
		}

		private void Export()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "CSV Files | *.CSV";
			bool? result = saveFileDialog.ShowDialog();
			if (result != true)
				return;

			string path = saveFileDialog.FileName;




			using (TextWriter _textWriter = new StreamWriter(path, false, System.Text.Encoding.UTF8))
			{
				using (CsvWriter _csvWriter = new CsvWriter(_textWriter, CultureInfo.CurrentCulture))
				{
					_csvWriter.WriteField("Param. Name");
					_csvWriter.WriteField("Value");
					_csvWriter.WriteField("Test Type");
					_csvWriter.WriteField("Is Pass");
					_csvWriter.WriteField("Error Description");
					_csvWriter.NextRecord();

					foreach(TestReprotData testReprot in TestReprotDataList)
					{
						_csvWriter.WriteRecord(testReprot);
						_csvWriter.NextRecord();
					}
				}
			}

		}

		#endregion Methods

		#region Commands

		public RelayCommand TestCommand { get; private set; }
		public RelayCommand CancelCommand { get; private set; }
		public RelayCommand UnLoadedCommand { get; private set; }
		public RelayCommand ExportCommand { get; private set; }

		#endregion Commands
	}
}
