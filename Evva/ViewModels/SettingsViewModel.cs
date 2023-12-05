
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using Evva.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Controls;

namespace Evva.ViewModels
{
	public class SettingsViewModel:ObservableObject
	{
		#region Properties
		
		public string Param_defaultsPath { get; set; }
		public string Param_defaultsB2BPath { get; set; }
		public string DynoCommunicationPath { get; set; }
		public string NI6002CommunicationPath { get; set; }


		public string MotorCommandsPath { get; set; }
		public string ControllerCommandsPath { get; set; }

		#endregion Properties

		#region Fields

		private EvvaUserData _EvvaUserData;

		#endregion Fields

		#region Constructor

		public SettingsViewModel(
			EvvaUserData EvvaUserData)
		{
			_EvvaUserData = EvvaUserData;
			Param_defaultsPath = _EvvaUserData.MCUJsonPath;
			Param_defaultsB2BPath = _EvvaUserData.MCUB2BJsonPath;
			DynoCommunicationPath = _EvvaUserData.DynoCommunicationPath;
			NI6002CommunicationPath = _EvvaUserData.NI6002CommunicationPath;

			BrowseMCUJsonCommand = new RelayCommand(BrowseMCUJson);
			BrowseMCUB2BJsonCommand = new RelayCommand(BrowseMCUB2BJson);
			BrowseDynoJsonCommand = new RelayCommand(BrowseDynoJson);
			BrowseNI600JsonCommand = new RelayCommand(BrowseNI600Json);

			BrowseMotorCommandsCommand = new RelayCommand(BrowseMotorCommands);
			BrowseControllerCommandsCommand = new RelayCommand(BrowseControllerCommands);

			RestoreMotorsDefaultCommand = new RelayCommand(RestoreMotorsDefault);
			RestoreControllerDefaultCommand = new RelayCommand(RestoreControllerDefault);

			UpdateCommand = new RelayCommand(Update);

		}

		#endregion Constructor

		#region Methods

		private void BrowseMCUJson()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "JSON Files (*.json) | *.json";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			Param_defaultsPath = openFileDialog.FileName;
		}

		private void BrowseMCUB2BJson()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "JSON Files (*.json) | *.json";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			Param_defaultsB2BPath = openFileDialog.FileName;
		}

		private void BrowseDynoJson()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "JSON Files (*.json) | *.json";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			DynoCommunicationPath = openFileDialog.FileName;
		}
		private void BrowseNI600Json()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "JSON Files (*.json) | *.json";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			NI6002CommunicationPath = openFileDialog.FileName;
		}



		private void BrowseMotorCommands()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Excel Files (*.xlsx) | *.xlsx";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			MotorCommandsPath = openFileDialog.FileName;
		}

		private void BrowseControllerCommands()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Excel Files (*.xlsx) | *.xlsx";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			ControllerCommandsPath = openFileDialog.FileName;
		}

		private void Update()
		{
			SETTINGS_UPDATEDMessage settings = new SETTINGS_UPDATEDMessage();

			if (_EvvaUserData.MCUJsonPath != Param_defaultsPath)
				settings.IsMCUJsonPathChanged = true;

			if (_EvvaUserData.MCUB2BJsonPath != Param_defaultsB2BPath)
				settings.IsMCUB2BJsonPathChanged = true;

			if (_EvvaUserData.DynoCommunicationPath != DynoCommunicationPath)
				settings.IsDynoJsonPathChanged = true;

			if (_EvvaUserData.NI6002CommunicationPath != NI6002CommunicationPath)
				settings.IsNI6002JsonPathChanged = true;

			_EvvaUserData.MCUB2BJsonPath = Param_defaultsB2BPath;
			_EvvaUserData.MCUJsonPath = Param_defaultsPath;
			_EvvaUserData.DynoCommunicationPath = DynoCommunicationPath;
			_EvvaUserData.NI6002CommunicationPath = NI6002CommunicationPath;


			string currentPath = Directory.GetCurrentDirectory();
			string path = Path.Combine(currentPath, @"Data\Motor Security Command Parameters.xlsx");
			if (string.IsNullOrEmpty(MotorCommandsPath) == false && MotorCommandsPath != path)
			{
				settings.IsMotorCommandsPathChanged = true;
				settings.MotorCommandsPath = MotorCommandsPath;
			}

			path = Path.Combine(currentPath, @"Data\Controller Security Command Parameters.xlsx");
			if (string.IsNullOrEmpty(ControllerCommandsPath) == false && ControllerCommandsPath != path)
			{
				settings.IsControllerCommandsPathChanged = true;
				settings.ControllerCommandsPath = ControllerCommandsPath;
			}


			SettingsUpdatedEvent?.Invoke(settings);
		}

		private void RestoreMotorsDefault()
		{
			SETTINGS_UPDATEDMessage settings = new SETTINGS_UPDATEDMessage();
			settings.IsMotorCommandsPathChanged = true;
			settings.MotorCommandsPath = @"Data\Motor Security Command Parameters.xlsx";
			SettingsUpdatedEvent?.Invoke(settings);
		}

		private void RestoreControllerDefault()
		{

			SETTINGS_UPDATEDMessage settings = new SETTINGS_UPDATEDMessage();
			settings.IsControllerCommandsPathChanged = true;
			settings.ControllerCommandsPath = @"Data\Controller Security Command Parameters.xlsx";
			SettingsUpdatedEvent?.Invoke(settings);
		}


		#endregion Methods

		#region Commands

		public RelayCommand BrowseMCUJsonCommand { get; set; }
		public RelayCommand BrowseMCUB2BJsonCommand { get; set; }
		public RelayCommand BrowseDynoJsonCommand { get; set; }
		public RelayCommand BrowseNI600JsonCommand { get; set; }

		public RelayCommand BrowseMotorCommandsCommand { get; set; }
		public RelayCommand BrowseControllerCommandsCommand { get; set; }


		public RelayCommand RestoreMotorsDefaultCommand { get; set; }
		public RelayCommand RestoreControllerDefaultCommand { get; set; }


		public RelayCommand UpdateCommand { get; set; }

		public RelayCommand<TextChangedEventArgs> ControllerTextChangedCommand { get; set; }

		#endregion Commands

		#region Events

		public event Action<SETTINGS_UPDATEDMessage> SettingsUpdatedEvent;

		#endregion Events
	}
}
