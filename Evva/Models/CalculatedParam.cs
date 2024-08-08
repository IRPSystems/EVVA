
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using DeviceHandler.Interfaces;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Models;
using Newtonsoft.Json;
using ScriptHandler.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace Evva.Models
{
	internal class CalculatedParam: DeviceParameterData, ICalculatedParamete
	{
		public ObservableCollection<DeviceParameterData> ParametersList { get; set; }
		public string Formula { get; set; }

		private ScriptStepGetParamValue _scriptStepGetParamValue;

		[JsonIgnore]
		public ObservableCollection<DeviceFullData> DevicesList { get; set; }

		public CalculatedParam() 
		{
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

				List<DeviceFullData> devicedList = new List<DeviceFullData>(DevicesList);
				List<object> valuesList = new List<object>();
				foreach (DeviceParameterData parameterData in ParametersList)
				{
					DeviceFullData device = 
						devicedList.Find((d) => d.Device.DeviceType == Entities.Enums.DeviceTypesEnum.TorqueKistler);
					if (device == null || device.DeviceCommunicator == null)
						continue;

					_scriptStepGetParamValue.Parameter = parameterData;
					_scriptStepGetParamValue.Communicator = device.DeviceCommunicator;
					EOLStepSummeryData eolStepSummeryData;
					_scriptStepGetParamValue.SendAndReceive(out eolStepSummeryData);

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
				d = Math.Abs(d);
				Value = d;

			}
			catch (Exception ex) 
			{
				LoggerService.Error(this, "Failed to calculate", ex);
			}
		}
	}
}
