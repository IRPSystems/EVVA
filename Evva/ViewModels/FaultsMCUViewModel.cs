
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

			_setFaultsTimer = new Timer(1000 / ParametersRepositoryService.AcquisitionRate);
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

			DeviceParameterData data =
				mcu_Device.MCU_FullList.ToList().Find((p) => p.Name == "Faults Vector LSB");
			if (data != null)
			{
				mcu_deviceFullData.ParametersRepository.Add(data, RepositoryPriorityEnum.High, FaultReceived);
			}

			data =
				mcu_Device.MCU_FullList.ToList().Find((p) => p.Name == "Faults Vector MSB");
			if (data != null)
			{
				mcu_deviceFullData.ParametersRepository.Add(data, RepositoryPriorityEnum.High, FaultReceived);
			}

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

		private void SetFaultsTimerElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			bool isError = false;
			if (_lsbValue == null)
			{
				for (int i = 0; i < _numOfBitsInFaultParam && i < FaultsList.Count; i++)
					FaultsList[i].State = null;
			}
			else
			{
				int lsbValue = Convert.ToInt32(_lsbValue);

				for (int i = 0; i < _numOfBitsInFaultParam && i < FaultsList.Count; i++)
				{
					int bit = (lsbValue >> i) & 1;
					FaultsList[i].State = (bit == 1);

					isError |= (FaultsList[i].State == true);
				}
			}

			if (_msbValue == null)
			{
				for (int i = _numOfBitsInFaultParam; i < FaultsList.Count; i++)
					FaultsList[i].State = null;
			}
			else
			{
				int msbValue = Convert.ToInt32(_msbValue);

				for (int i = _numOfBitsInFaultParam; i < FaultsList.Count; i++)
				{
					int bit = (msbValue >> (i - _numOfBitsInFaultParam)) & 1;
					FaultsList[i].State = (bit == 1);

					isError |= (FaultsList[i].State == true);
				}
			}

			if (_lsbValue == null && _msbValue == null)
				ErrorEvent?.Invoke(null);
			else
				ErrorEvent?.Invoke(isError);
			
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

		public event Action<bool?> ErrorEvent;

		#endregion Events
	}


}
