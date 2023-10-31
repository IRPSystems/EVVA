
using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows.Media;
using System.Windows;
using ScriptHandler.Interfaces;

namespace Evva.Converter
{
	public class ScriptStepDiagramForegroundConverter : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is ISubScript subScript &&
				subScript.Script == null)
			{
				return Brushes.White;
			}

			return Application.Current.MainWindow.Foreground;

		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
