
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Enums;
using DeviceHandler.Models;
using Entities.Models;
using System;
using System.Collections.ObjectModel;

namespace Evva.ViewModels
{
	public class MonitorBaseViewModel : ObservableObject, IDisposable
	{
		#region Properties


		public ObservableCollection<DeviceParameterData> MonitorParamsList { get;set; }

		#endregion Properties

		#region Fields

		protected DevicesContainer _devicesContainer;
		private bool _isFirstLoaded;

		public bool IsLoaded;

		#endregion Fields

		#region Constructor

		public MonitorBaseViewModel(
			DevicesContainer devicesContainer)
		{
			_isFirstLoaded = false;
			_devicesContainer = devicesContainer;

			LoadedCommand = new RelayCommand(Loaded);
			UnLoadedCommand = new RelayCommand(UnLoaded);

			IsLoaded = false;
		}

		#endregion Constructor


		#region Method

		public void Dispose() 
		{
		}

		public void Loaded()
		{
			if(!_isFirstLoaded)
			{
				_isFirstLoaded = true;
				return;
			}

			if (MonitorParamsList == null)
				return;

			foreach (DeviceParameterData data in MonitorParamsList)
			{
				if (_devicesContainer.TypeToDevicesFullData.ContainsKey(data.DeviceType) == false)
					continue;

				DeviceFullData deviceFullData =
					_devicesContainer.TypeToDevicesFullData[data.DeviceType];
				if (deviceFullData == null)
					continue;

				if(deviceFullData.ParametersRepository != null)
					deviceFullData.ParametersRepository.Add(data, RepositoryPriorityEnum.Medium, null);
			}

			IsLoaded = true;
		}

		protected void UnLoaded()
		{
			if (MonitorParamsList == null)
				return;

			foreach (DeviceParameterData data in MonitorParamsList)
			{
				DeviceFullData deviceFullData =
					_devicesContainer.TypeToDevicesFullData[data.DeviceType];
				if (deviceFullData == null)
					return;

				deviceFullData.ParametersRepository.Remove(data, null);
			}

			IsLoaded = false;
		}

		#endregion Method

		#region Commands

		public RelayCommand LoadedCommand { get; private set; }
		public RelayCommand UnLoadedCommand { get; private set; }

		#endregion Commands
	}
}
