
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
using Entities.Enums;

namespace Evva.ViewModels
{
	public class ParametersViewModel : ObservableObject
	{
		

		#region Properties

		public DeviceHandler.ViewModel.ParametersViewModel FullParametersList { get; set; }

		public Record_SelectedParametersListViewModel RecordParamList { get; set; }
		public Absolute_SelectedParametersListViewModel AbsoluteParamList { get; set; }

		

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

			FullParametersList.DBCRemoveEvent += DBCList_DBCRemoveEvent;

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

			_evvaUserData.LastParamsDBCPath = openFileDialog.FileName;

			DBC_DeviceData dbcDevice = null;
			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.DBC) == false)
			{
				dbcDevice = new DBC_DeviceData()
				{
					DeviceType = DeviceTypesEnum.DBC,
					Name = "DBC",
					ParemetersList = new ObservableCollection<DeviceParameterData>(),
					DBCFilePath = openFileDialog.FileName,
					DBC_FilesList = new ObservableCollection<DBC_File>(),
				};

				DeviceFullData mcuDevice = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];

				DeviceFullData deviceFullData = new DeviceFullData_DBC(dbcDevice, mcuDevice);
				_devicesContainer.DevicesFullDataList.Add(deviceFullData);
				_devicesContainer.DevicesList.Add(dbcDevice);
				_devicesContainer.TypeToDevicesFullData.Add(DeviceTypesEnum.DBC, deviceFullData);


			}
			else 
			{
				DeviceFullData dfd = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.DBC];
				dbcDevice = dfd.Device as DBC_DeviceData;
			}

			dbcDevice.DBCLoad(openFileDialog.FileName);

			WeakReferenceMessenger.Default.Send(new SETUP_UPDATEDMessage());
		}

		private void DBCList_DBCRemoveEvent(DeviceParameterData param)
		{
			if (!(param is DBC_File dbc_File))
				return;

			DeviceFullData dfd = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.DBC];
			DBC_DeviceData dbcDevice = dfd.Device as DBC_DeviceData;

			dbcDevice.DBC_FilesList.Remove(dbc_File);
		}

		#endregion Methods

		#region Commands

		public RelayCommand LoadDBCFileCommand { get; private set; }

		#endregion Commands
	}
}
