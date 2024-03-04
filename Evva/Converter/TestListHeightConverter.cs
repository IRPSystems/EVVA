
using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows.Controls;

namespace Evva.Converter
{
	public class TestListHeightConverter : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is ListView listView))
				return value;

			double height = listView.ActualHeight - 60;
			return height;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
