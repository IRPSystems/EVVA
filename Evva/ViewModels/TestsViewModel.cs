
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using System;
using System.Windows;
using DeviceCommunicators.Enums;
using DeviceHandler.Models;
using DeviceCommunicators.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceCommunicators.MCU;
using static System.Net.Mime.MediaTypeNames;

namespace Evva.ViewModels
{
	public class TestsViewModel : ObservableObject
	{
		#region Fields	

		private DocingViewModel _docking;

		#endregion Fields

		#region Properties

		public DevicesContainer DevicesContainer { get; set; }

		public bool IsDoSaftyOfficerAbort { get; set; }

		#endregion Properties

		#region Constructor

		public TestsViewModel(
			DevicesContainer devicesContainer)
		{
			DevicesContainer = devicesContainer;

			SetCommand = new RelayCommand<DeviceParameterData>(Set);
			GetCommand = new RelayCommand<DeviceParameterData>(Get);
			EditCommand = new RelayCommand<DeviceParameterData>(Edit);

		}

		#endregion Constructor


		#region Methods





		private void Set(DeviceParameterData deviceParam)
		{
			object value = null;
			if (deviceParam.IsEditing == false)
				value = deviceParam.Value;
			else if (deviceParam.IsEditing == true)
				value = deviceParam.EditValue;

			if (value == null)
				return;

			if (value is string str)
			{
				if (string.IsNullOrEmpty(str))
					return;

				double d;
				bool res = double.TryParse(str, out d);
				if (!res)
					return;

				value = d;
			}



			DeviceFullData deviceFullData = DevicesContainer.TypeToDevicesFullData[deviceParam.DeviceType];

			deviceFullData.DeviceCommunicator.SetParamValue(deviceParam, Convert.ToDouble(value), MessageCallback);

			deviceParam.IsEditing = false;
		}

		private void Get(DeviceParameterData deviceParam)
		{
			if (DevicesContainer.TypeToDevicesFullData.ContainsKey(deviceParam.DeviceType) == false)
				return;

			DeviceFullData deviceFullData = DevicesContainer.TypeToDevicesFullData[deviceParam.DeviceType];
			deviceFullData.DeviceCommunicator.GetParamValue(deviceParam, MessageCallback);
		}

		private void Edit(DeviceParameterData deviceParam)
		{
			if(deviceParam.IsEditing == true)
				deviceParam.EditValue = deviceParam.Value;
		}

		private void MessageCallback(DeviceParameterData param, CommunicatorResultEnum result, string resultDescription)
		{
			if (result != CommunicatorResultEnum.OK)
			{
				MessageBox.Show("Failed to get response\r\n" + resultDescription);
			}
		}

		private void SearchText_TextChanged(TextChangedEventArgs e)
		{
			if (!(e.Source is TextBox textBox))
				return;

			if (!(textBox.DataContext is DeviceData deviceData))
				return;

			if(deviceData is MCU_DeviceData mcuDevice)
			{
				SearchText_TextChanged_MCU(
					mcuDevice,
					textBox.Text);
			}

			foreach (DeviceParameterData param in deviceData.ParemetersList)
			{
				if (param.Name.ToLower().Contains(textBox.Text.ToLower()))
					param.Visibility = Visibility.Visible;
				else
					param.Visibility = Visibility.Collapsed;
			}
		}

		private void SearchText_TextChanged_MCU(
			MCU_DeviceData mcuDevice, 
			string text)
		{

			foreach (DeviceParameterData groupParam in mcuDevice.MCU_GroupList)
			{
				if(!(groupParam is ParamGroup group))
					continue;

				bool isFound = false;
				foreach (DeviceParameterData param in group.ParamList)
				{
					if (param.Name.ToLower().Contains(text.ToLower()))
					{
						isFound = true;
						param.Visibility = Visibility.Visible;
					}
					else
						param.Visibility = Visibility.Collapsed;
				}

				if (isFound)
				{
					group.IsExpanded = true;
					group.Visibility = Visibility.Visible;
				}
				else
				{
					group.IsExpanded = false;
					group.Visibility = Visibility.Collapsed; 
				}
			}
		}


		#endregion Methods

		#region Commands



		public RelayCommand<DeviceParameterData> SetCommand { get; private set; }
		public RelayCommand<DeviceParameterData> GetCommand { get; private set; }
		public RelayCommand<DeviceParameterData> EditCommand { get; private set; }



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
