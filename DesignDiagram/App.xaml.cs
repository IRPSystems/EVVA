using ControlzEx.Theming;
using Services.Services;
using System.Windows;
using System.Windows.Threading;

namespace DesignDiagram
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
				 "MzM3MDg2M0AzMjM0MmUzMDJlMzBCT2dsKzBPUW9HbXFrM1J3aWxQR2k5UDVOZXNDdE4zdGJCSjI5N2lpWGlJPQ==");

			this.DispatcherUnhandledException += App_DispatcherUnhandledException;


		}

		

		public static void ChangeDarkLight(bool isLightTheme)
		{
			if (Current == null)
				return;

			if (isLightTheme)
				ThemeManager.Current.ChangeTheme(Current, "Light.Cobalt");
			else
				ThemeManager.Current.ChangeTheme(Current, "Dark.Cobalt");
		}



		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			LoggerService.Error("Un-handled exception caught", "Error", e.Exception);
			e.Handled = true;
		}
	}

}
