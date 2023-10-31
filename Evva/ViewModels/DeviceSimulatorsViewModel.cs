
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using DeviceSimulators.ViewModels;
using Entities.Enums;
using Entities.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Evva.ViewModels
{
    public class DeviceSimulatorsViewModel: ObservableObject
    {
        public ObservableCollection<DeviceSimulatorViewModel> ViewModelsList { get; set; }


        public DeviceSimulatorsViewModel(DevicesContainer devicesContainer)
        {
			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);


			ViewModelsList = new ObservableCollection<DeviceSimulatorViewModel>();

			foreach (DeviceFullData deviceFullData in devicesContainer.DevicesFullDataList) 
            { 
                switch(deviceFullData.Device.DeviceType) 
                {

                    case DeviceTypesEnum.Dyno:
                        ViewModelsList.Add(new DynoSimulatorMainWindowViewModel(deviceFullData.Device));
                        break;

					case DeviceTypesEnum.MCU:
					case DeviceTypesEnum.MCU_B2B:
						ViewModelsList.Add(new MCUSimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.PowerSupplyBK:
						ViewModelsList.Add(new PSBKSimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.PowerSupplyEA:
						ViewModelsList.Add(new PSEASimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.BTMTempLogger:
						ViewModelsList.Add(new BTMTempLoggerSimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.SwitchRelay32:
						ViewModelsList.Add(new SwitchRelaySimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.TorqueKistler:
						ViewModelsList.Add(new TKSimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.KeySight:
                        break;

				}
            }

		}

		private void Closing(CancelEventArgs e)
        {
            foreach(DeviceSimulatorViewModel vm in ViewModelsList)
            {
                vm.Disconnect();
            }
        }

		private void SearchText_TextChanged(TextChangedEventArgs e)
		{
			if (!(e.Source is TextBox textBox))
				return;

			if (!(textBox.DataContext is DeviceSimulatorViewModel simulator))
				return;

			foreach (DeviceParameterData param in simulator.ParametersList)
			{
				if (param.Name.ToLower().Contains(textBox.Text.ToLower()))
					param.Visibility = Visibility.Visible;
				else
					param.Visibility = Visibility.Collapsed;
			}
		}


		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }

		private RelayCommand<TextChangedEventArgs> _SearchText_TextChangedCommand;
		public RelayCommand<TextChangedEventArgs> SearchText_TextChangedCommand
		{
			get
			{
				return _SearchText_TextChangedCommand ?? (_SearchText_TextChangedCommand =
					new RelayCommand<TextChangedEventArgs>(SearchText_TextChanged));
			}
		}
	}
}
