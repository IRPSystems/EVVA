
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using Evva.Models;
using Microsoft.Win32;
using System;
using System.Windows;
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

		#endregion Properties

		#region Fields

		private EvvaUserData _EvvaUserData;

		private bool _isUpdated;

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

			_isUpdated = false;

			BrowseMCUJsonCommand = new RelayCommand(BrowseMCUJson);
			BrowseMCUB2BJsonCommand = new RelayCommand(BrowseMCUB2BJson);
			BrowseDynoJsonCommand = new RelayCommand(BrowseDynoJson);
			BrowseNI600JsonCommand = new RelayCommand(BrowseNI600Json);
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

		private void Update()
		{
			_isUpdated = true;
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

			SettingsUpdatedEvent?.Invoke(settings);
		}

		public void Unloaded()
		{


			if (_isUpdated)
			{
				_isUpdated = false;
				return;
			}

			MessageBoxResult result = MessageBox.Show(
				"You did not click the \"Update\" button. \r\nDo you wish to update the changes you've made?",
				"Settings Update Warning",
				MessageBoxButton.YesNo);
			if(result == MessageBoxResult.Yes) 
			{
				Update();
				_isUpdated = false;
			}
		}


		#endregion Methods

		#region Commands

		public RelayCommand BrowseMCUJsonCommand { get; set; }
		public RelayCommand BrowseMCUB2BJsonCommand { get; set; }
		public RelayCommand BrowseDynoJsonCommand { get; set; }
		public RelayCommand BrowseNI600JsonCommand { get; set; }


		public RelayCommand UpdateCommand { get; set; }

		public RelayCommand<TextChangedEventArgs> ControllerTextChangedCommand { get; set; }

		#endregion Commands

		#region Events

		public event Action<SETTINGS_UPDATEDMessage> SettingsUpdatedEvent;

		#endregion Events
	}
}
