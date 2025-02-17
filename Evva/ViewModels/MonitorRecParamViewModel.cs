
using CommunityToolkit.Mvvm.Messaging;
using DeviceCommunicators.DBC;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using Entities.Models;
using ScriptRunner.Models;
using ScriptRunner.ViewModels;
using System.Collections.ObjectModel;
using static ScriptRunner.ViewModels.CANMessageSenderViewModel;
using System.Linq;

namespace Evva.ViewModels
{
	public class MonitorRecParamViewModel: MonitorBaseViewModel
	{
		private bool _isRcordingListChanged;

		private CANMessageSenderViewModel _canMessageSender;

		#region Constructor

		public MonitorRecParamViewModel(
			DevicesContainer devicesContainer,
			ObservableCollection<DeviceParameterData> logParametersList,
			CANMessageSenderViewModel canMessageSender) :
			base(devicesContainer)
		{
			_canMessageSender = canMessageSender;

			GetMonitorRecParamsList(logParametersList);

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
			

			foreach (DeviceParameterData param in logParametersList)
			{


				//if (param.Value == null)
				//	return;


		//		MonitorParamsList.Add(param);
		//	}

		//	if (oldList == null)
		//		return;

				if (param is DBC_ParamData dbcParam)
				{
					CANMessageForSenderData canMsgData = _canMessageSender.CANMessagesList.ToList().Find((d) =>
								d.Message.NodeId == dbcParam.ParentMessage.ID);
					if (canMsgData != null)
						continue;
				}

		
		private void RECORD_LIST_CHANGEDHandler(object sender, RECORD_LIST_CHANGEDMessage e)
		{
			_isRcordingListChanged = true;
			GetMonitorRecParamsList(e.LogParametersList);
			_isRcordingListChanged = false;
		}

		protected override bool IsAddSingleParamToRepository(DeviceParameterData param)
		{
			if(param is DBC_ParamData dbcParam)
			{
				CANMessageForSenderData canMsgData = _canMessageSender.CANMessagesList.ToList().Find((d) =>
							d.Message.NodeId == dbcParam.ParentMessage.ID);
				if (canMsgData != null)
					return false;
			}

			return true;
		}

		#endregion Method
	}
}
