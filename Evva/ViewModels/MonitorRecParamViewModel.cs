
using CommunityToolkit.Mvvm.Messaging;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using Entities.Models;
using ScriptRunner.Models;
using System.Collections.ObjectModel;

namespace Evva.ViewModels
{
	public class MonitorRecParamViewModel: MonitorBaseViewModel
	{
		private bool _isRcordingListChanged;

		#region Constructor

		public MonitorRecParamViewModel(
			DevicesContainer devicesContainer,
			ObservableCollection<DeviceParameterData> logParametersList) :
			base(devicesContainer)
		{
			MonitorParamsList = new ObservableCollection<DeviceParameterData>();
			foreach (DeviceParameterData param in logParametersList)
			{
				MonitorParamsList.Add(param);
			}

			WeakReferenceMessenger.Default.Register<RECORD_LIST_CHANGEDMessage>(
				this, new MessageHandler<object, RECORD_LIST_CHANGEDMessage>(RECORD_LIST_CHANGEDHandler));

			_isRcordingListChanged = false;
		}

		#endregion Constructor


		#region Method

		//public override void Loaded()
		//{
		//	if (MonitorParamsList == null)
		//		return;

		//	ObservableCollection<DeviceParameterData> oldList = new ObservableCollection<DeviceParameterData>(MonitorParamsList);
		//	GetMonitorRecParamsList(oldList);
		//}

		//public void GetMonitorRecParamsList(
		//	ObservableCollection<DeviceParameterData> logParametersList)
		//{
			

		//	ObservableCollection<DeviceParameterData> oldList = MonitorParamsList;
		//	MonitorParamsList = new ObservableCollection<DeviceParameterData>();

		//	foreach (DeviceParameterData param in logParametersList)
		//	{
		//		if (oldList != null)
		//		{
		//			if (oldList.Contains(param) == false)
		//				AddSingleParamToRepository(param);
		//			else
		//			{
		//				oldList.Remove(param);
		//			}
		//		}
		//		else
		//			AddSingleParamToRepository(param);


		//		MonitorParamsList.Add(param);
		//	}

		//	if (oldList == null)
		//		return;

		//	foreach (DeviceParameterData param in oldList)
		//	{
		//		RemoveSingleParamToRepository(param);
		//	}
		//}

		
		private void RECORD_LIST_CHANGEDHandler(object sender, RECORD_LIST_CHANGEDMessage e)
		{
			_isRcordingListChanged = true;
			GetMonitorRecParamsList(e.LogParametersList);
			_isRcordingListChanged = false;
		}


		#endregion Method
	}
}
