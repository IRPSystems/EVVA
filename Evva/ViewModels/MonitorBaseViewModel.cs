
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceCommunicators.Models;
using DeviceHandler.Enums;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Models;
using ScriptRunner.Models;
using Services.Services;
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
		private bool _isStartupEnded;

		public bool IsOpened;

		public bool IsLoaded;

		private bool _isExecutLoaded
		{
			get
			{
				if(IsOpened == false)
					return false;

				if (_isStartupEnded)
					return true;

				if(_isFirstLoaded)
					return true;

				return false;
			}
		}

		#endregion Fields

		#region Constructor

		public MonitorBaseViewModel(
			DevicesContainer devicesContainer)
		{
			_isFirstLoaded = true;
			IsOpened = false;
			_devicesContainer = devicesContainer;

			LoadedCommand = new RelayCommand(Loaded);
			UnLoadedCommand = new RelayCommand(UnLoaded);

			IsLoaded = false;

			WeakReferenceMessenger.Default.Register<STARTUP_ENDEDMessage>(
				this, new MessageHandler<object, STARTUP_ENDEDMessage>(STARTUP_ENDEDMessageHandler));
		}

		#endregion Constructor


		#region Method

		public void Dispose() 
		{
		}

		public virtual void Loaded()
		{
			if (!_isExecutLoaded)
				return;

			if (MonitorParamsList == null)
				return;

			_isFirstLoaded = false;


			ObservableCollection<DeviceParameterData> oldList = new ObservableCollection<DeviceParameterData>(MonitorParamsList);
			GetMonitorRecParamsList(oldList, true);

			IsLoaded = true;
		}

		public void GetMonitorRecParamsList(
			ObservableCollection<DeviceParameterData> logParametersList,
			bool isAlwaysAdd = false)
		{
			try
			{

				ObservableCollection<DeviceParameterData> oldList = 
					new ObservableCollection<DeviceParameterData>(MonitorParamsList);
				MonitorParamsList = new ObservableCollection<DeviceParameterData>();

				foreach (DeviceParameterData param in logParametersList)
				{
					if (oldList != null && isAlwaysAdd == false)
					{
						if (oldList.Contains(param) == false)
							AddSingleParamToRepository(param);
						else
						{
							oldList.Remove(param);
						}
					}
					else
						AddSingleParamToRepository(param);


					MonitorParamsList.Add(param);
				}

				if (oldList == null || isAlwaysAdd)
					return;

				foreach (DeviceParameterData param in oldList)
				{
					RemoveSingleParamToRepository(param);
				}
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to load parameters", "Error", ex);
			}
		}

		protected void AddSingleParamToRepository(DeviceParameterData data)
		{
			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(data.DeviceType) == false)
				return;

			DeviceFullData deviceFullData =
				_devicesContainer.TypeToDevicesFullData[data.DeviceType];
			if (deviceFullData == null)
				return;

			if (deviceFullData.ParametersRepository != null)
				deviceFullData.ParametersRepository.Add(data, RepositoryPriorityEnum.Medium, null);
		}

		protected void UnLoaded()
		{
			if (MonitorParamsList == null)
				return;

			foreach (DeviceParameterData data in MonitorParamsList)
			{
				RemoveSingleParamToRepository(data);
			}

			IsLoaded = false;
		}

		protected void RemoveSingleParamToRepository(DeviceParameterData data)
		{
			DeviceFullData deviceFullData =
					_devicesContainer.TypeToDevicesFullData[data.DeviceType];
			if (deviceFullData == null)
				return;

			deviceFullData.ParametersRepository.Remove(data, null);
		}

		private void STARTUP_ENDEDMessageHandler(object sender, STARTUP_ENDEDMessage e)
		{
			_isStartupEnded = true;
		}

		#endregion Method

		#region Commands

		public RelayCommand LoadedCommand { get; private set; }
		public RelayCommand UnLoadedCommand { get; private set; }

		#endregion Commands
	}
}
