
using DeviceCommunicators.General;
using DeviceHandler.Interfaces;
using DeviceHandler.Models;
using Entities.Models;
using ScriptHandler.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace Evva.Models
{
	internal class CalculatedParam: DeviceParameterData, ICalculatedParamete
	{
		public ObservableCollection<DeviceParameterData> ParametersList { get; set; }
		public string Formula { get; set; }

		private DevicesContainer _devicesContainer;
		private ScriptStepGetParamValue _scriptStepGetParamValue;

		public CalculatedParam(DevicesContainer devicesContainer) 
		{
			_devicesContainer = devicesContainer;

			_scriptStepGetParamValue = new ScriptStepGetParamValue();
		}

		public void Calculate()
		{
			try
			{
				DataTable dt = new DataTable();
				for (byte i = 0; i < ParametersList.Count; i++)
				{
					byte b = Convert.ToByte('A');
					b += i;
					string name = Convert.ToChar(b).ToString();
					dt.Columns.Add(name, typeof(double));
				}

				List<object> valuesList = new List<object>();
				foreach (DeviceParameterData parameterData in ParametersList)
				{
					if (_devicesContainer.TypeToDevicesFullData.ContainsKey(parameterData.DeviceType) == false)
						continue;

					DeviceFullData device =
						_devicesContainer.TypeToDevicesFullData[parameterData.DeviceType];
					if (device == null || device.DeviceCommunicator == null)
						continue;

					_scriptStepGetParamValue.Parameter = parameterData;
					_scriptStepGetParamValue.Communicator = device.DeviceCommunicator;
					_scriptStepGetParamValue.SendAndReceive();

					if (_scriptStepGetParamValue.Parameter.Value == null)
					{
						Value = double.NaN;
						return;
					}

					valuesList.Add((double)_scriptStepGetParamValue.Parameter.Value);

					System.Threading.Thread.Sleep(1);
				}


				dt.Rows.Add(valuesList.ToArray());


				dt.Columns.Add("result", typeof(double), Formula);

				var valResult = dt.Rows[0]["result"];
				if (valResult == null)
					Value = double.NaN;

				double d = Convert.ToDouble(valResult);
				Value = d;

			}
			catch (Exception ex) 
			{
				LoggerService.Error(this, "Failed to calculate", ex);
			}
		}
	}
}
