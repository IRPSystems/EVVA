
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

		#endregion Fields

		#region Constructor

		public ParametersViewModel(
			DevicesContainer devicesContainer,
			EvvaUserData evvaUserData)
		{
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
			try
			{
				string initDir = _evvaUserData.LastParamsDBCPath;
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
				_evvaUserData.LastParamsDBCPath =
					Path.GetDirectoryName(openFileDialog.FileName);

				var dbc = Parser.ParseFromPath(DBCFilePath);
				if (dbc == null)
					return;


				DeviceData dbcDevice = new DeviceData()
				{
					DeviceType = Entities.Enums.DeviceTypesEnum.DBC,
					Name = "DBC",
					ParemetersList = new ObservableCollection<DeviceParameterData>()
				};

				foreach (Message message in dbc.Messages)
				{
					DBC_ParamGroup dbcGroup = new DBC_ParamGroup()
					{
						Name = message.Name,
						ID = message.ID,
						ParamsList = new ObservableCollection<DBC_ParamData>()
					};
					dbcDevice.ParemetersList.Add(dbcGroup);

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
						};

						dbcGroup.ParamsList.Add(dbcParam);
					}
				}

				FullParametersList.DevicesList.Add(dbcDevice);
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
