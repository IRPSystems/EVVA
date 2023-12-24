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
				"MTAwODQzOUAzMjMwMmUzNDJlMzBsQlRMeUl0THVueXVMcWhEMnlCeVJLTnZZdFhLRUh2aEZGKytIdUVIRTRBPQ==");

        }

		protected override void OnStartup(StartupEventArgs e)
        {
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

			base.OnStartup(e);
			//System.Windows.Forms.Application.UseWaitCursor = true;
			//System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			//System.Windows.Forms.Application.DoEvents();
			//Initialize();
			SplashView splash = new SplashView();
			splash.Show();
			//	System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
			// Right now I'm showing main window right after splash screen but I will eventually wait until splash screen closes.
			TestStudioMainWindow main = new TestStudioMainWindow();
			
			main.Show();
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
