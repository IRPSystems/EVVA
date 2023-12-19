
using CommunityToolkit.Mvvm.Messaging;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using Entities.Models;
using ScriptHandler.Models;
using ScriptRunner.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace Evva.ViewModels
{
	public class MonitorSecurityParamViewModel : MonitorBaseViewModel
	{
		#region Fields

		private RunScriptService _scriptService;

		#endregion Fields

		#region Constructor

		public MonitorSecurityParamViewModel(
			DevicesContainer devicesContainer,
			RunScriptService scriptService) :
			base(devicesContainer)
		{
			_scriptService = scriptService;

			//GetMonitorParamsList();

			WeakReferenceMessenger.Default.Register<SETTINGS_UPDATEDMessage>(
				this, new MessageHandler<object, SETTINGS_UPDATEDMessage>(SETTINGS_UPDATEDMessageHandler));
		}

		#endregion Constructor


		#region Method

		private void GetMonitorParamsList()
		{
			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(Entities.Enums.DeviceTypesEnum.MCU) == false)
			{
				return;
			}

			DeviceFullData mcu_deviceFullData = _devicesContainer.TypeToDevicesFullData[Entities.Enums.DeviceTypesEnum.MCU];
			if (mcu_deviceFullData == null)
				return;

			if (!(mcu_deviceFullData.Device is MCU_DeviceData mcu_Device))
				return;

			if (mcu_Device.MCU_FullList == null)
				return;

			MonitorParamsList = new ObservableCollection<DeviceParameterData>();

			if (_scriptService.SelectMotor.MotorTypesList != null)
			{
				MotorSettingsData firstMotor = _scriptService.SelectMotor.MotorTypesList[0];
				foreach (ParameterValueData paramValue in firstMotor.StatusParameterValueList)
				{
					string parameterName = paramValue.ParameterName.Trim();
					DeviceParameterData data =
						mcu_Device.MCU_FullList.ToList().Find((p) => p.Name == parameterName);

					if (data != null)
						MonitorParamsList.Add(data);
				}
			}

			ControllerSettingsData firstController = _scriptService.SelectMotor.ControllerTypesList[0];
			foreach (ParameterValueData paramValue in firstController.StatusParameterValueList)
			{
				string parameterName = paramValue.ParameterName.Trim();
				DeviceParameterData data =
					mcu_Device.MCU_FullList.ToList().Find((p) => p.Name == paramValue.ParameterName);
				if(data != null)
					MonitorParamsList.Add(data);
			}
		}

		private void SETTINGS_UPDATEDMessageHandler(object sender, SETTINGS_UPDATEDMessage e)
		{
			if (e.IsMCUJsonPathChanged)
			{				
				GetMonitorParamsList();				
			}

			Loaded();
		}

		#endregion Method
	}
}
