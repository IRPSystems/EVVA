
using System.Windows.Data;
using System;
using System.Windows;
using System.Windows.Media;
using ScriptHandler.Enums;

namespace Evva.Converter
{
	public class ContentConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return values[1];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
