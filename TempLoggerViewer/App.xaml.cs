using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TempLoggerViewer
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
	}
}
