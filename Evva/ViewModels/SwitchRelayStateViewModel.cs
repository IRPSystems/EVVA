
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using DeviceHandler.Enums;
using DeviceHandler.Models;
using Entities.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;

namespace Evva.ViewModels
{
	public class SwitchRelayStateViewModel: ObservableObject
	{

		#region Properties

		public BitwiseNumberDisplayData SwitchRelayState { get; set; }

		#endregion Properties

		#region Fields


		private DevicesContainer _devicesContainer;
		private System.Timers.Timer _updateStateTimer;

		private bool _isFirstLoaded;

		public bool IsLoaded;

		#endregion Fields

		#region Constructor

		public SwitchRelayStateViewModel(
			DevicesContainer devicesContainer)
		{
			_devicesContainer = devicesContainer;


			_isFirstLoaded = false;
			
			LoadedCommand = new RelayCommand(Loaded);
			UnLoadedCommand = new RelayCommand(UnLoaded);

			SwitchRelayState = new BitwiseNumberDisplayData(is64Bit: false, isZeroBased: false);

			_updateStateTimer = new System.Timers.Timer(1000);
			_updateStateTimer.Elapsed += UpdateStateTimerElapsed;
		}

		#endregion Constructor

		#region Methods

		public void Loaded()
		{
			if (!_isFirstLoaded)
			{
				_isFirstLoaded = true;
				return;
			}

			_updateStateTimer.Start();

			IsLoaded = true;
		}

		protected void UnLoaded()
		{
			_updateStateTimer.Stop();
		}



		private void UpdateStateTimerElapsed(object sender, ElapsedEventArgs e)
		{
			DeviceFullData sr_deviceFullData = 
				_devicesContainer.TypeToDevicesFullData[Entities.Enums.DeviceTypesEnum.SwitchRelay32];


			if (sr_deviceFullData == null || sr_deviceFullData.ParametersRepository == null)
				return;


			DeviceParameterData data =
				sr_deviceFullData.Device.ParemetersList.ToList().Find((p) => p.Name == "All relay status");
			if (data != null && data.Value != null )
			{
				if (!(data.Value is double))
				{
					SwitchRelayState.NumericValue = Convert.ToUInt64(data.Value);
				}
			}
		}

		#endregion Methods


		#region Commands

		public RelayCommand LoadedCommand { get; private set; }
		public RelayCommand UnLoadedCommand { get; private set; }

		#endregion Commands
	}
}
