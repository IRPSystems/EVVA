using Controls.Views;
using ControlzEx.Theming;
using ParamLimitsTest;
using System.Windows;

namespace Evva
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
				"MzEyMTgxNUAzMjM0MmUzMDJlMzBQNU0vejdSQmVGc1psckxrbSt5UEU0NFNmRzlQajBYTVNnS2c4MkVzdjNzPQ==");

        }

		protected override void OnStartup(StartupEventArgs e)
        {
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

			base.OnStartup(e);
			
			SplashView splash = new SplashView();
			splash.AppName = "EVVA";
			splash.Show();

			// Right now I'm showing main window right after splash screen but I will eventually wait until splash screen closes.
			MainWindow = new TestStudioMainWindow();
			MainWindow.Show();
			splash.Close();

			
		}

        public static void ChangeDarkLight(bool isLightTheme)
        {
            if (isLightTheme)
			    ThemeManager.Current.ChangeTheme(Current, "Light.Blue");
            else
				ThemeManager.Current.ChangeTheme(Current, "Dark.Blue");
		}
    }
}
