
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Evva.Models;
using System.Collections.ObjectModel;
using Entities.Models;
using System.Windows.Controls;
using System;
using System.Windows;
using DeviceCommunicators.Enums;
using ScriptHandler.Models;
using DeviceHandler.Models;
using Evva.Views;
using ParamLimitsTest;
using DeviceCommunicators.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using System.Windows.Input;
using DeviceHandler.ParamGetSetList;
using System.Windows.Media;
using DeviceCommunicators.MCU;
using System.Security.Cryptography;
using System.Text;
using Services.Services;

namespace Evva.ViewModels
{
	public class TestsViewModel: ObservableObject
	{
		#region Fields	

		private DocingViewModel _docking;
		private TestParamsLimitViewModel _testParamsLimitViewModel;

		#endregion Fields

		#region Properties

		public DevicesContainer DevicesContainer { get; set; }

		public bool IsDoSaftyOfficerAbort { get; set; }





		public Visibility HelpToolVisibility { get; set; }
		public Visibility SaveVisibility { get; set; }
		public Visibility ButtonsVisibility { get; set; }

		public Action<KeyEventArgs> TextBox_KeyUpEvent { get; set; }
		public Action<ComboBox> ComboBox_DropDownClosedEvent { get; set; }
		public Action<DeviceParameterData> HexTextBox_EnterEvent { get; set; }
		public Action<KeyEventArgs> HexTextBox_HexKeyDownEvent { get; set; }
		public Action<KeyEventArgs> HexTextBox_HexKeyUpEvent { get; set; }

		public Action<DeviceParameterData> ButtonGet_ClickEvent { get; set; }
		public Action<DeviceParameterData> ButtonSet_ClickEvent { get; set; }
		public Action<DeviceParameterData> ButtonSave_ClickEvent { get; set; }


		#endregion Properties

		#region Constructor

		public TestsViewModel(
			DevicesContainer devicesContainer)
		{
			DevicesContainer = devicesContainer;

			SetCommand = new RelayCommand<DeviceParameterData>(Set);
			GetCommand = new RelayCommand<DeviceParameterData>(Get);
			TestParamsLimitCommand = new RelayCommand(TestParamsLimit);



			HelpToolVisibility = Visibility.Visible;
			SaveVisibility = Visibility.Visible;
			ButtonsVisibility = Visibility.Visible;


			TextBox_KeyUpEvent = TextBox_KeyUp;
			ComboBox_DropDownClosedEvent = ComboBox_DropDownClosed;
			HexTextBox_EnterEvent = HexTextBox_Enter;
			HexTextBox_HexKeyDownEvent = HexTextBox_HexKeyDown;
			HexTextBox_HexKeyUpEvent = TextBox_KeyUp;

			ButtonGet_ClickEvent = Get;
			ButtonSet_ClickEvent = Set;
			ButtonSave_ClickEvent = Save;

			foreach(DeviceData deviceData in DevicesContainer.DevicesList) 
			{
				foreach (DeviceParameterData param in deviceData.ParemetersList)
					param.IsEnabled = true;
			}

		}

		#endregion Constructor


		#region Methods

		public void CreateTestParamsLimitWindow(
			DocingViewModel docking)
		{
			_docking = docking;

			_testParamsLimitViewModel =
				new TestParamsLimitViewModel(DevicesContainer);
			_docking.CreateParamsLimitTester(_testParamsLimitViewModel);

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

		private void Save(DeviceParameterData param)
		{
			if (!(param is MCU_ParamData mcuParam))
				return;

			if (mcuParam.Cmd == null)
				return;


			Set(param);

			byte[] id = new byte[3];

			using (var md5 = MD5.Create())
			{
				Array.Copy(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(mcuParam.Cmd)), 0, id, 0, 3);

			}

			var hex_id = BitConverter.ToString(id).Replace("-", "").ToLower();

			var msg = Convert.ToInt32(hex_id, 16);

			DeviceFullData deviceFullData = DevicesContainer.TypeToDevicesFullData[param.DeviceType];
			deviceFullData.DeviceCommunicator.SetParamValue(new MCU_ParamData() { Cmd = "save_param" }, msg, MessageCallback);
		}

		public void ComboBox_DropDownClosed(ComboBox comboBox)
		{
			if (ButtonsVisibility == Visibility.Collapsed)
				return;

			if (!(comboBox.DataContext is DeviceParameterData param))
				return;

			if (param is IParamWithDropDown dropDown &&
				(dropDown.DropDown == null || dropDown.DropDown.Count == 0))
			{
				return;
			}

			ParamGetSetListViewModel.SetBackForeGround(
						Application.Current.FindResource("MahApps.Brushes.Accent2") as SolidColorBrush,
						Brushes.White,
						param);
		}

		private void TextBox_KeyUp(KeyEventArgs e)
		{
			if (ButtonsVisibility == Visibility.Collapsed)
				return;

			DeviceParameterData param = null;
			if (e.Source is TextBox textBox)
				param = textBox.DataContext as DeviceParameterData;
			else if (e.Source is ComboBox comboBox)
				param = comboBox.DataContext as DeviceParameterData;



			if (e.Key == Key.Enter)
			{
				Set(param);

				e.Handled = true;

				ParamGetSetListViewModel.SetBackForeGround(
						Brushes.Transparent,
						Application.Current.FindResource("MahApps.Brushes.ThemeForeground") as SolidColorBrush,
						param);
				return;
			}

			ParamGetSetListViewModel.SetBackForeGround(
						Application.Current.FindResource("MahApps.Brushes.Accent2") as SolidColorBrush,
						Brushes.White,
						param);
		}

		private void HexTextBox_Enter(DeviceParameterData param)
		{
			Set(param);
			ParamGetSetListViewModel.SetBackForeGround(
						Brushes.Transparent,
						Application.Current.FindResource("MahApps.Brushes.ThemeForeground") as SolidColorBrush,
						param);
		}

		private void HexTextBox_HexKeyDown(KeyEventArgs e)
		{
			if (!(e.Source is TextBox textBox))
				return;

			if (!(textBox.DataContext is DeviceParameterData param))
				return;

			ParamGetSetListViewModel.SetBackForeGround(
						Application.Current.FindResource("MahApps.Brushes.Accent2") as SolidColorBrush,
						Brushes.White,
						param);
		}














		private void MessageCallback(DeviceParameterData param, CommunicatorResultEnum result, string errDescription)
		{
			if (Application.Current == null)
				return;


			try
			{
				if (result == CommunicatorResultEnum.OK)
				{
					ParamGetSetListViewModel.SetBackForeGround(
						Brushes.Transparent,
						Application.Current.FindResource("MahApps.Brushes.ThemeForeground") as SolidColorBrush,
						param);
					param.ErrorDescription = null;



					if (param is MCU_ParamData mcuParam)
					{
						param.Value = GetFormatedValuesService.GetString(mcuParam.Format, mcuParam.Value);
					}
				}
				else
				{
					ParamGetSetListViewModel.SetBackForeGround(
						Brushes.Red,
						Brushes.White,
						param);


					if (result == CommunicatorResultEnum.NoResponse && string.IsNullOrEmpty(errDescription))
						errDescription = "No connection";

					param.ErrorDescription = errDescription;

					

				}
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Error at callback", ex);
			}
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

		private void TestParamsLimit()
		{
			_docking.OpenTestParamsLimit();
		}


		#endregion Methods

		#region Commands


		public RelayCommand TestParamsLimitCommand { get; private set; }

		public RelayCommand<DeviceParameterData> SetCommand { get; private set; }
		public RelayCommand<DeviceParameterData> GetCommand { get; private set; }



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
