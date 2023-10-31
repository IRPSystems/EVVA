
using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows.Media;
using ScriptRunner.Models;
using ScriptHandler.Interfaces;

namespace Evva.Converter
{
	public class ScriptStepDiagramBackgroundConverter : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is ISubScript subScript &&
				subScript.Script == null)
			{
				return Brushes.Red;
			}

			return Brushes.Transparent;

		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
