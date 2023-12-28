using Controls.ViewModels;
using DeviceHandler.ViewModels;
using DeviceHandler.Views;
using Evva.ViewModels;
using Evva.Views;
using ParamLimitsTest;
using ScriptHandler.Services;
using ScriptHandler.ViewModels;
using ScriptHandler.Views;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.IO;
using System.Windows.Controls;

namespace TempLoggerViewer.ViewModels
{
	public class DocingTempLoggerViewModel : DocingBaseViewModel
	{
		#region Fields

		private ContentControl _communicationSettings;

		#endregion Fields

		#region Constructor

		public DocingTempLoggerViewModel(
			CommunicationViewModel communicationSettings) :
			base("DockingMain")
		{

			CreateWindows(
				communicationSettings);
		}

		#endregion Constructor

		#region Methods

		private void CreateWindows(
			CommunicationViewModel communicationSettings)
		{
			DockFill = true;




			_communicationSettings = new ContentControl();
			CommunicationView communication = new CommunicationView() { DataContext = communicationSettings };
			_communicationSettings.Content = communication;
			SetHeader(_communicationSettings, "Communication Settings");
			SetFloatParams(_communicationSettings);
			Children.Add(_communicationSettings);



		}


		private void SetFloatParams(ContentControl control)
		{
			SetSizetoContentInDock(control, true);
			SetSizetoContentInFloat(control, true);
			SetState(control, DockState.Hidden);
		}


		public void OpenCommSettings()
		{
			SetState(_communicationSettings, DockState.Float);
		}


		public void RestorWindowsLayout()
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, "TempLoggerViewer");
			path = Path.Combine(path, "Default.txt");
			if (System.IO.File.Exists(path))
				LoadDockState(path);
		}

		#endregion Methods
	}
}
