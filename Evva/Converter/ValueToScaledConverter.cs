
using System.Windows.Data;
using System;
using DeviceCommunicators.MCU;
using Entities.Models;
using System.Windows;
using DeviceCommunicators.Models;

namespace Evva.Converter
{
	public class ValueToScaledConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			
			if (!(values[1] is DeviceParameterData parameterData))
				return null;

			double d = 0;
			if (!(values[0] is string) &&
				values[0] != null)
			{
				if (values[0] == DependencyProperty.UnsetValue)
					return 0;

				d = System.Convert.ToDouble(values[0]);
			}

			if (parameterData is MCU_ParamData mcuParam)
				d = d / mcuParam.Scale;
			else if (parameterData is Dyno_ParamData dynoParam)
				d = d * dynoParam.Coefficient;

			return d.ToString();
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(value is DropDownParamData dropDownParam))
				return new object[] {0};


			int val;
			bool ret = int.TryParse(dropDownParam.Value, out val);
			if(!ret)
				return new object[] { 0 };


			return new object[] { val };
		}
	}
}
