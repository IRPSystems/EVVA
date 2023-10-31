
using CommunityToolkit.Mvvm.Messaging;
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
			GetMonitorRecParamsList(logParametersList);

			WeakReferenceMessenger.Default.Register<RECORD_LIST_CHANGEDMessage>(
				this, new MessageHandler<object, RECORD_LIST_CHANGEDMessage>(RECORD_LIST_CHANGEDHandler));

			_isRcordingListChanged = false;
		}

		#endregion Constructor


		#region Method


		public void GetMonitorRecParamsList(
			ObservableCollection<DeviceParameterData> logParametersList)
		{
			if (_isRcordingListChanged)
				UnLoaded();

			MonitorParamsList = new ObservableCollection<DeviceParameterData>();

			foreach (DeviceParameterData param in logParametersList)
			{
				

				//if (param.Value == null)
				//	return;

				//double d = 0;
				//if (param.Value is string str)
				//	double.TryParse(str, out d);
				//else
				//{
				//	d = Convert.ToDouble(param.Value);
				//}

				//if (param is MCU_ParamData mcuParam)
				//	d *= mcuParam.Scale;
				//else if (param is Dyno_ParamData dynoParam)
				//	d *= dynoParam.Coefficient;

				//param.Value = d;


				MonitorParamsList.Add(param);
			}

			if (_isRcordingListChanged)
				Loaded();
		}

		
		private void RECORD_LIST_CHANGEDHandler(object sender, RECORD_LIST_CHANGEDMessage e)
		{
			_isRcordingListChanged = true;
			GetMonitorRecParamsList(e.LogParametersList);
			_isRcordingListChanged = false;
		}


		#endregion Method
	}
}
