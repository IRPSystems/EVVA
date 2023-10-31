using ControlzEx.Theming;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
