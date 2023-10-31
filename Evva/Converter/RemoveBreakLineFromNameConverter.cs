
using System.Globalization;
using System.Windows.Data;
using System;

namespace Evva.Converter
{
	public class RemoveBreakLineFromNameConverter : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is string valStr))
				return value;

			if (string.IsNullOrEmpty(valStr))
				return "";

			valStr = valStr.Replace("\r", " ");
			valStr = valStr.Replace("\n", " ");
			return valStr;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
