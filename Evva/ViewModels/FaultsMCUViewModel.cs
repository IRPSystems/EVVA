
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Enums;
using DeviceCommunicators.MCU;
using DeviceHandler.Enums;
using DeviceHandler.Models;
using DeviceHandler.Services;
using Entities.Models;
using ExcelDataReader;
using Evva.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using DeviceCommunicators.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using ScriptHandler.Enums;
using Entities.Enums;

namespace Evva.ViewModels
{
	public class FaultsMCUViewModel: ObservableObject
	{
		public enum ReadStatEnum
		{
			Number,
			Name,
			Opcode,
			Description,
		}

		#region Properties

		public ObservableCollection<FaultData> FaultsList 
		{
			get => _EvvaUserData.FaultsMCUList;
		}

		#endregion Properties

		#region Fields

		private const int _numOfBitsInFaultParam = 32;



		private object _lsbValue; 
		private object _msbValue;

		private MCU_ParamData _lsbParam;
		private MCU_ParamData _msbParam;

		private Timer _setFaultsTimer;

		private bool _isAllSelected;

		private EvvaUserData _EvvaUserData;



		public bool IsLoaded;
		private DevicesContainer _devicesContainer;

		#endregion Fields

		#region Constructor

		public FaultsMCUViewModel(
			DevicesContainer devicesContainer,
			EvvaUserData EvvaUserData)
		{
			_devicesContainer = devicesContainer;

			

			IsLoaded = false;

			_EvvaUserData = EvvaUserData;

			
			SelectAllCommand = new RelayCommand(SelectAll);

			//LoadedCommand = new RelayCommand(Loaded);
			//UnLoadedCommand = new RelayCommand(UnLoaded);

			_lsbValue = null;
			_msbValue = null;
			_isAllSelected = false;

			

			ReadFaults();

			int acquisitionRate = 1;
			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(Entities.Enums.DeviceTypesEnum.MCU))
			{
				DeviceFullData device = _devicesContainer.TypeToDevicesFullData[Entities.Enums.DeviceTypesEnum.MCU];
				acquisitionRate = device.ParametersRepository.AcquisitionRate;
			}

			if (acquisitionRate == 0)
				acquisitionRate = 1;

			_setFaultsTimer = new Timer(1000 / acquisitionRate);
			_setFaultsTimer.Elapsed += SetFaultsTimerElapsedEventHandler;
			_setFaultsTimer.Start();

			Loaded();
		}

		#endregion Constructor

		#region Methods

		public void Dispose()
		{
			_setFaultsTimer.Stop();

			IsLoaded = false;
		}


		public void Loaded()
		{
			//if (!_isFirstLoaded)
			//{
			//	_isFirstLoaded = true;
			//	return;
			//}


			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(Entities.Enums.DeviceTypesEnum.MCU) == false)
			{
				return;
			}

			DeviceFullData mcu_deviceFullData = _devicesContainer.TypeToDevicesFullData[Entities.Enums.DeviceTypesEnum.MCU];


			if (mcu_deviceFullData == null || mcu_deviceFullData.ParametersRepository == null)
				return;

			if (!(mcu_deviceFullData.Device is MCU_DeviceData mcu_Device))
				return;

			_lsbParam =
				mcu_Device.MCU_FullList.ToList().Find((p) => p.Name == "Faults Vector LSB") as MCU_ParamData;
			if (_lsbParam != null)
			{
				mcu_deviceFullData.ParametersRepository.Add(_lsbParam, RepositoryPriorityEnum.High, FaultReceived);
			}

			_msbParam =
				mcu_Device.MCU_FullList.ToList().Find((p) => p.Name == "Faults Vector MSB") as MCU_ParamData;
			if (_msbParam != null)
			{
				mcu_deviceFullData.ParametersRepository.Add(_msbParam, RepositoryPriorityEnum.High, FaultReceived);
			}

			DeviceParameterData paramFlthi =
					mcu_Device.MCU_FullList.ToList().Find((p) =>
												((MCU_ParamData)p).Cmd != null &&
												((MCU_ParamData)p).Cmd.ToLower() == "flthi");
			if (paramFlthi != null)
				mcu_deviceFullData.ParametersRepository.Add(paramFlthi, RepositoryPriorityEnum.High, HighestActiveFaultReceived);

			IsLoaded = true;
		}

		protected void UnLoaded()
		{
			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(Entities.Enums.DeviceTypesEnum.MCU) == false)
			{
				return;
			}

			DeviceFullData mcu_deviceFullData = _devicesContainer.TypeToDevicesFullData[Entities.Enums.DeviceTypesEnum.MCU];


			if (mcu_deviceFullData == null || mcu_deviceFullData.ParametersRepository == null)
				return;

			if (!(mcu_deviceFullData.Device is MCU_DeviceData mcu_Device))
				return;

			DeviceParameterData data =
				mcu_Device.MCU_FullList.ToList().Find((p) => p.Name == "Faults Vector LSB");
			if (data != null)
			{
				mcu_deviceFullData.ParametersRepository.Remove(data, FaultReceived);
			}

			data =
				mcu_Device.MCU_FullList.ToList().Find((p) => p.Name == "Faults Vector MSB");
			if (data != null)
			{
				mcu_deviceFullData.ParametersRepository.Remove(data, FaultReceived);
			}

			DeviceParameterData paramFlthi =
					mcu_Device.MCU_FullList.ToList().Find((p) =>
												((MCU_ParamData)p).Cmd != null &&
												((MCU_ParamData)p).Cmd.ToLower() == "flthi");
			if (paramFlthi != null)
				mcu_deviceFullData.ParametersRepository.Remove(paramFlthi, HighestActiveFaultReceived);

			IsLoaded = false;
		}




		private void ReadFaults()
		{
			if (_EvvaUserData.FaultsMCUList != null)
				return;

			IExcelDataReader reader;
			var stream = File.Open("Data\\fault list.xlsx", FileMode.Open, FileAccess.Read);
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			reader = ExcelReaderFactory.CreateReader(stream);

			var conf = new ExcelDataSetConfiguration
			{
				ConfigureDataTable = _ => new ExcelDataTableConfiguration
				{
					UseHeaderRow = true
				}
			};

			var dataSet = reader.AsDataSet(conf);

			var dataTable = dataSet.Tables[0];

			_EvvaUserData.FaultsMCUList = new ObservableCollection<FaultData>();

			int row = 0;
			for (; row < dataTable.Rows.Count; row++)
			{
				var item = dataTable.Rows[row][0];
				if (item is DBNull)
					continue;

				FaultData fault = new FaultData();
				for (int col = 0; col < dataTable.Columns.Count; col++)
				{
					item = dataTable.Rows[row][col];
					if (item is DBNull)
						continue;

					string strVal = item.ToString();
					strVal = strVal.Trim('{');
					strVal = strVal.Trim('}');

					switch ((ReadStatEnum)col)
					{
						case ReadStatEnum.Number:
							int bit;
							bool res = int.TryParse(strVal, out bit);
							if (res == false)
								continue;
							fault.Bit = bit;
							break;
						case ReadStatEnum.Name:
							break;
						case ReadStatEnum.Opcode:
							strVal = strVal.TrimStart('{');
							strVal = strVal.TrimEnd('u');
							fault.Opcode = strVal; 
							break;
						case ReadStatEnum.Description:
							strVal = strVal.TrimEnd('}');
							fault.Description = strVal; 
							break;
					}
				}

				if(!string.IsNullOrEmpty(fault.Opcode) && !string.IsNullOrEmpty(fault.Description))
					_EvvaUserData.FaultsMCUList.Add(fault);
			}

			reader.Close();
		}

		private void FaultReceived(DeviceParameterData param, CommunicatorResultEnum result, string errDescription)
		{
			if(result != CommunicatorResultEnum.OK)
			{
				if (param.Name.EndsWith("LSB"))
					_lsbValue = null;
				else if (param.Name.EndsWith("MSB"))
					_msbValue = null;
			}
			else
			{
				if (param.Name.EndsWith("LSB"))
					_lsbValue = param.Value;
				else if (param.Name.EndsWith("MSB"))
					_msbValue = param.Value;
			}
		}

		private void HighestActiveFaultReceived(DeviceParameterData param, CommunicatorResultEnum result, string errDescription)
		{
			if (Convert.ToInt32(_msbValue) == 0 && Convert.ToInt32(_lsbValue) == 0)
			{
				ErrorEvent?.Invoke(ActiveErrorLevelEnum.NoError);
				return;
			}

			if (!(param is MCU_ParamData mcuParam))
				return;

			uint uval = (uint)Convert.ToDouble(mcuParam.Value);
			uint errorState = (uval >> 8) & 0xF;


			ErrorEvent?.Invoke((ActiveErrorLevelEnum)errorState);
		}

		private void SetFaultsTimerElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			if (_lsbValue == null)
			{
				for (int i = 0; i < _numOfBitsInFaultParam && i < FaultsList.Count; i++)
					FaultsList[i].State = null;
			}
			else
			{
				
				if (_lsbValue is string str)
				{
					bool isFound = false;
					foreach (DropDownParamData ddp in _lsbParam.DropDown)
					{
						if (ddp.Name == str)
						{
							_lsbValue = ddp.Value;
							isFound = true;
							break;
						}
					}

					if (!isFound)
					{
						double d;
						double.TryParse(str, out d);
						_msbValue = d;
					}
				}

				int lsbValue = Convert.ToInt32(_lsbValue);

				for (int i = 0; i < _numOfBitsInFaultParam && i < FaultsList.Count; i++)
				{
					int bit = (lsbValue >> i) & 1;
					FaultsList[i].State = (bit == 1);
				}
			}

			if (_msbValue == null)
			{
				for (int i = _numOfBitsInFaultParam; i < FaultsList.Count; i++)
					FaultsList[i].State = null;
			}
			else
			{
				if (_msbValue is string str)
				{
					bool isFound = false;
					foreach (DropDownParamData ddp in _msbParam.DropDown)
					{
						if (ddp.Name == str)
						{
							_msbValue = ddp.Value;
							isFound = true;
							break;
						}
					}

					if(!isFound)
					{
						double d;
						double.TryParse(str, out d);
						_msbValue = d;
					}

				}
				

				int msbValue = Convert.ToInt32(_msbValue);

				for (int i = _numOfBitsInFaultParam; i < FaultsList.Count; i++)
				{
					int bit = (msbValue >> (i - _numOfBitsInFaultParam)) & 1;
					FaultsList[i].State = (bit == 1);
				}
			}

			
			
		}

		private void SelectAll()
		{
			_isAllSelected = !_isAllSelected;
			foreach (FaultData faultData in FaultsList)
			{
				faultData.IsVisible = _isAllSelected;
			}
		}

		#endregion Methods

		#region Commands

		//public RelayCommand LoadedCommand { get; private set; }
		//public RelayCommand UnLoadedCommand { get; private set; }

		public RelayCommand SelectAllCommand { get; private set; }

		#endregion Commands

		#region Events

		public event Action<ActiveErrorLevelEnum> ErrorEvent;

		#endregion Events
	}


}
