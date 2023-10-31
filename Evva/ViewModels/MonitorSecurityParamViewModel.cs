
using CommunityToolkit.Mvvm.Messaging;
using DeviceCommunicators.MCU;
using DeviceHandler.Models;
using Entities.Models;
using Evva.Models;
using ScriptHandler.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Evva.ViewModels
{
	public class MonitorSecurityParamViewModel : MonitorBaseViewModel
	{
		#region Fields

		private List<MotorSettingsData> _motorSettingsList;
		private List<ControllerSettingsData> _controllerSettingsList;

		#endregion Fields

		#region Constructor

		public MonitorSecurityParamViewModel(
			DevicesContainer devicesContainer,
			List<MotorSettingsData> motorSettingsList,
			List<ControllerSettingsData> controllerSettingsList) :
			base(devicesContainer)
		{
			_motorSettingsList = motorSettingsList;
			_controllerSettingsList = controllerSettingsList;

			GetMonitorParamsList(
				motorSettingsList,
				controllerSettingsList);

			WeakReferenceMessenger.Default.Register<SETTINGS_UPDATEDMessage>(
				this, new MessageHandler<object, SETTINGS_UPDATEDMessage>(SETTINGS_UPDATEDMessageHandler));
		}

		#endregion Constructor


		#region Method

		private void GetMonitorParamsList(
			List<MotorSettingsData> motorSettingsList,
			List<ControllerSettingsData> controllerSettingsList)
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

			if (motorSettingsList != null)
			{
				MotorSettingsData firstMotor = motorSettingsList[0];
				foreach (ParameterValueData paramValue in firstMotor.StatusParameterValueList)
				{
					string parameterName = paramValue.ParameterName.Trim();
					DeviceParameterData data =
						mcu_Device.MCU_FullList.ToList().Find((p) => p.Name == parameterName);

					if (data != null)
						MonitorParamsList.Add(data);
				}
			}

			ControllerSettingsData firstController = controllerSettingsList[0];
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
				GetMonitorParamsList(
					_motorSettingsList,
					_controllerSettingsList);				
			}

			Loaded();
		}

		#endregion Method
	}
}
