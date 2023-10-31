
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using DeviceCommunicators.Models;

namespace Evva.Models
{
	public class DynoParamList : ObservableObject
	{
		public ObservableCollection<Dyno_ParamData> DynoParamsList { get; set; }

		public DynoParamList()
		{
			DynoParamsList = new ObservableCollection<Dyno_ParamData>()
			{
				new Dyno_ParamData() { Name = "Enable", Index = 470, SubIndex = 1, Units = "", Coefficient = 1 },
				new Dyno_ParamData() { Name = "Speed", Index = 472, SubIndex = 1, Units = "%", Coefficient = 0.01 },
				new Dyno_ParamData() { Name = "Refernce_speed", Index = 11, SubIndex = 0, Units = "Rpm", Coefficient = 1 },
				new Dyno_ParamData() { Name = "Torque", Index = 472, SubIndex = 3, Units = "%", Coefficient = 0.01 },
				new Dyno_ParamData() { Name = "Max_torque", Index = 57, SubIndex = 0, Units = "Nm", Coefficient = 0.01 },
				new Dyno_ParamData() { Name = "Acceleration_Time", Index = 12, SubIndex = 0, Units = "Sec", Coefficient = 0.001 },
				new Dyno_ParamData() { Name = "Deceleration_Time", Index = 13, SubIndex = 0, Units = "Sec", Coefficient = 0.001 },

				//Status
				new Dyno_ParamData() { Name = "Speed_Setpoint", Index = 50, SubIndex = 0, Units = "Rpm", Coefficient = 1 },
				new Dyno_ParamData() { Name = "Speed_Current", Index = 51, SubIndex = 0, Units = "Rpm", Coefficient = 1 },
				new Dyno_ParamData() { Name = "Torque_Setpoint", Index = 56, SubIndex = 1, Units = "Nm", Coefficient = 0.01 },
				new Dyno_ParamData() { Name = "Torque_X", Index = 56, SubIndex = 2, Units = "Nm", Coefficient = 0.01 },
				new Dyno_ParamData() { Name = "Vbus_DC", Index = 53, SubIndex = 0, Units = "V", Coefficient = 0.01 },
				new Dyno_ParamData() { Name = "Motor_Voltage", Index = 52, SubIndex = 0, Units = "V", Coefficient = 1 },
				new Dyno_ParamData() { Name = "Motor_Current", Index = 54, SubIndex = 0, Units = "A", Coefficient = 0.01 },
				new Dyno_ParamData() { Name = "Frequency", Index = 58, SubIndex = 0, Units = "Hz", Coefficient = 0.01 },

				//CAN Connection
				new Dyno_ParamData() { Name = "Power_In", Index = 980, SubIndex = 4, Units = "Kw", Coefficient = 0.001 },
				new Dyno_ParamData() { Name = "Power_Out", Index = 980, SubIndex = 1, Units = "Kw", Coefficient = 0.001 },
				new Dyno_ParamData() { Name = "Motor_temperature", Index = 61, SubIndex = 4, Units = "Deg C", Coefficient = 1 },
				new Dyno_ParamData() { Name = "Controller_temperature", Index = 63, SubIndex = 4, Units = "Deg C", Coefficient = 1 },
			};
		}
	}
}
