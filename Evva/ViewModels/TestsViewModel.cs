
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Evva.Models;
using System.Collections.ObjectModel;
using Entities.Models;
using System.Windows.Controls;
using System;
using ScriptRunner.Services;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using DeviceCommunicators.Enums;
using ScriptHandler.Models;
using DeviceHandler.Models;
using DeviceCommunicators.MCU;

namespace Evva.ViewModels
{
	public class TestsViewModel: ObservableObject
	{
		#region Fields


		private SaftyOfficerService _saftyOfficer;

		public ObservableCollection<MotorSettingsData> MotorSettingsList;
		public ObservableCollection<ControllerSettingsData> ControllerSettingsList;

		private ParamRecordingService _paramRecording;

		#endregion Fields

		#region Properties

		public DevicesContainer DevicesContainer { get; set; }

		public string SaftyOfficerStatus
		{
			get 
			{ 
				if(_saftyOfficer == null)
					return ""; 

				return _saftyOfficer.SaftyOfficerStatus;
			}
		}

		public bool IsDoSaftyOfficerAbort { get; set; }

		#endregion Properties

		#region Constructor

		public TestsViewModel(
			DevicesContainer devicesContainer,
			ObservableCollection<MotorSettingsData> motorSettingsList,
			ObservableCollection<ControllerSettingsData> controllerSettingsList,
			ObservableCollection<DeviceParameterData> logParametersList)
		{
			DevicesContainer = devicesContainer;
			MotorSettingsList = motorSettingsList;
			ControllerSettingsList = controllerSettingsList;

			SetCommand = new RelayCommand<DeviceParameterData>(Set);
			GetCommand = new RelayCommand<DeviceParameterData>(Get);
			StopScurityOfficerCommand = new RelayCommand(StopScurityOfficer);
			RunRecordingCommand = new RelayCommand(RunRecording);
			StopRecordingCommand = new RelayCommand(StopRecording);

			SendCANMessageCommand = new RelayCommand(SendCANMessage);

			_saftyOfficer = new SaftyOfficerService();
			_saftyOfficer.StatusReportEvent += SafteyOfficerStatusReportEvent;

			_paramRecording = new ParamRecordingService(
				logParametersList,
				DevicesContainer);

			WeakReferenceMessenger.Default.Register<SETTINGS_UPDATEDMessage>(
				this, new MessageHandler<object, SETTINGS_UPDATEDMessage>(SETTINGS_UPDATEDMessageHandler));

		}

		#endregion Constructor


		#region Methods

		private void SETTINGS_UPDATEDMessageHandler(object sender, SETTINGS_UPDATEDMessage e)
		{
			
		}


		private void Set(DeviceParameterData deviceParam)
		{
			DeviceFullData deviceFullData = DevicesContainer.TypeToDevicesFullData[deviceParam.DeviceType];
			deviceFullData.DeviceCommunicator.SetParamValue(deviceParam, Convert.ToDouble(deviceParam.Value), MessageCallback);
		}

		private void Get(DeviceParameterData deviceParam)
		{
			DeviceFullData deviceFullData = DevicesContainer.TypeToDevicesFullData[deviceParam.DeviceType];
			deviceFullData.DeviceCommunicator.GetParamValue(deviceParam, MessageCallback);
		}

		private void MessageCallback(DeviceParameterData param, CommunicatorResultEnum result, string resultDescription)
		{
			if (result != CommunicatorResultEnum.OK)
			{
				MessageBox.Show("Failed to get response\r\n" + resultDescription);
			}
		}

		
		private void StopScurityOfficer()
		{
			_saftyOfficer.Stop();
		}

		private void SafteyOfficerStatusReportEvent()
		{
			OnPropertyChanged(nameof(SaftyOfficerStatus));
		}


		private void RunRecording()
		{
			_paramRecording.StartRecording("Test Recording", null);
		}

		private void StopRecording()
		{
			_paramRecording.StopRecording();
		}


		private void SearchText_TextChanged(TextChangedEventArgs e)
		{
			if(!(e.Source is TextBox textBox))
				return;

			if(!(textBox.DataContext is DeviceData deviceData)) 
				return;

			foreach(DeviceParameterData param in deviceData.ParemetersList) 
			{ 
				if(param.Name.ToLower().Contains(textBox.Text.ToLower()))
					param.Visibility = Visibility.Visible;
				else
					param.Visibility = Visibility.Collapsed;
			}
		}



		private void SendCANMessage()
		{
			byte[] buffer = new byte[] { 1, 0, 0, 0, 64, 0, 0, 0, 0, };
			uint nodeId = 0;

			DeviceFullData deviceFullData = 
				DevicesContainer.TypeToDevicesFullData[Entities.Enums.DeviceTypesEnum.MCU];

			deviceFullData.DeviceCommunicator.SendMessage(true, nodeId, buffer, null);
		}

		#endregion Methods

		#region Commands

		public RelayCommand<DeviceParameterData> SetCommand { get; private set; }
		public RelayCommand<DeviceParameterData> GetCommand { get; private set; }
		public RelayCommand RunScurityOfficerCommand { get; private set; }
		public RelayCommand StopScurityOfficerCommand { get; private set; }
		public RelayCommand RunRecordingCommand { get; private set; }
		public RelayCommand StopRecordingCommand { get; private set; }



		public RelayCommand SendCANMessageCommand { get; private set; }

		private RelayCommand<TextChangedEventArgs> _SearchText_TextChangedCommand;
		public RelayCommand<TextChangedEventArgs> SearchText_TextChangedCommand
		{
			get
			{
				return _SearchText_TextChangedCommand ?? (_SearchText_TextChangedCommand =
					new RelayCommand<TextChangedEventArgs>(SearchText_TextChanged));
			}
		}

		#endregion Commands

	}
}
