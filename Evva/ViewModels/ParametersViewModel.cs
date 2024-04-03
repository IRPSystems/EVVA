
using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Models;
using DeviceHandler.Models;
using CommunityToolkit.Mvvm.Input;
using ScriptHandler.Models;
using Evva.Models;
using System.IO;
using Microsoft.Win32;
using DBCFileParser.Services;
using DBCFileParser.Model;
using DeviceCommunicators.DBC;
using System.Collections.ObjectModel;
using DeviceCommunicators.Models;
using System;
using Services.Services;
using DeviceHandler.Models.DeviceFullDataModels;
using CommunityToolkit.Mvvm.Messaging;

namespace Evva.ViewModels
{
	public class ParametersViewModel : ObservableObject
	{
		

		#region Properties

		public DeviceHandler.ViewModel.ParametersViewModel FullParametersList { get; set; }

		public Record_SelectedParametersListViewModel RecordParamList { get; set; }
		public Absolute_SelectedParametersListViewModel AbsoluteParamList { get; set; }

		public string DBCFilePath { get; set; }

		#endregion Properties

		#region Fields

		private EvvaUserData _evvaUserData;
		private DevicesContainer _devicesContainer;

		#endregion Fields

		#region Constructor

		public ParametersViewModel(
			DevicesContainer devicesContainer,
			EvvaUserData evvaUserData)
		{
			_devicesContainer = devicesContainer;
			_evvaUserData = evvaUserData;

			LoadDBCFileCommand = new RelayCommand(LoadDBCFile);

			RecordParamList = new Record_SelectedParametersListViewModel(devicesContainer, "Record Parameters List");
			AbsoluteParamList = new Absolute_SelectedParametersListViewModel(devicesContainer, "Absolute Parameters List");


			DragDropData dragDropData = new DragDropData();
			FullParametersList = new DeviceHandler.ViewModel.ParametersViewModel(dragDropData, devicesContainer, true);

		}

		#endregion Constructor

		#region Methods

		private void LoadDBCFile()
		{
			string initDir = Path.GetDirectoryName(_evvaUserData.LastParamsDBCPath);
			if (string.IsNullOrEmpty(initDir))
				initDir = "";
			if (Directory.Exists(initDir) == false)
				initDir = "";
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "DBC Files | *.dbc";
			openFileDialog.InitialDirectory = initDir;
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			DBCFilePath = openFileDialog.FileName;
			_evvaUserData.LastParamsDBCPath = openFileDialog.FileName;

			LoadDBCFile(DBCFilePath);
		}

		private void LoadDBCFile(string dbcFilePath)
		{ 
			try
			{ 

				var dbc = Parser.ParseFromPath(dbcFilePath);
				if (dbc == null)
					return;


				DBC_DeviceData dbcDevice = new DBC_DeviceData()
				{
					DeviceType = Entities.Enums.DeviceTypesEnum.DBC,
					Name = "DBC", // - " + Path.GetFileName(dbcFilePath),
					ParemetersList = new ObservableCollection<DeviceParameterData>(),
					DBC_GroupList = new ObservableCollection<DBC_ParamGroup>(),
					DBCFilePath = dbcFilePath,
				};

				foreach (Message message in dbc.Messages)
				{
					DBC_ParamGroup dbcGroup = new DBC_ParamGroup()
					{
						Name = message.Name,
						ID = message.ID,
						DeviceType = Entities.Enums.DeviceTypesEnum.DBC,
						ParamsList = new ObservableCollection<DBC_ParamData>()
					};
					dbcDevice.DBC_GroupList.Add(dbcGroup);

					foreach (Signal signal in message.Signals)
					{

						if (signal.Unit == "�C")
							signal.Unit = "˚C";
						else if (signal.Unit == "�")
							signal.Unit = "˚";

						DBC_ParamData dbcParam = new DBC_ParamData()
						{
							Name = signal.Name,
							Units = signal.Unit,
							Signal = signal,
							ParentMessage = message,
							DeviceType = Entities.Enums.DeviceTypesEnum.DBC,
						};

						dbcGroup.ParamsList.Add(dbcParam);
						dbcDevice.ParemetersList.Add(dbcParam);
					}
				}

				DeviceFullData mcuFullData = null;
				if (_devicesContainer.TypeToDevicesFullData.ContainsKey(Entities.Enums.DeviceTypesEnum.MCU))
					mcuFullData = _devicesContainer.TypeToDevicesFullData[Entities.Enums.DeviceTypesEnum.MCU];


				DeviceFullData deviceFullData = new DeviceFullData_DBC(dbcDevice, mcuFullData);
				_devicesContainer.DevicesFullDataList.Add(deviceFullData);
				_devicesContainer.DevicesList.Add(dbcDevice);
				_devicesContainer.TypeToDevicesFullData.Add(Entities.Enums.DeviceTypesEnum.DBC, deviceFullData);

				WeakReferenceMessenger.Default.Send(new SETUP_UPDATEDMessage());

				//FullParametersList.DevicesList.Add(dbcDevice);
			}
			catch(Exception ex)
			{
				LoggerService.Error(this, "Failed to load DBC file", "Error", ex);
			}
		}

		#endregion Methods

		#region Commands

		public RelayCommand LoadDBCFileCommand { get; private set; }

		#endregion Commands
	}
}
