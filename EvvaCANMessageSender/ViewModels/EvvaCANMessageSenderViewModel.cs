
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using ScriptRunner.ViewModels;
using DeviceHandler.Models;
using ScriptHandler.Models;

namespace EvvaCANMessageSender.ViewModels
{
    public class EvvaCANMessageSenderViewModel: ObservableObject
    {
		#region Properties

		public CANMessageSenderViewModel CANMessageSenderVM { get; set; }

		#endregion Properties

		#region Fields


		private ScriptUserData _scriptUserData;


		#endregion Fields

		#region Constructor

		public EvvaCANMessageSenderViewModel()
        {
			try
			{
				ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);

				_scriptUserData = new ScriptUserData();
				CANMessageSenderVM = new CANMessageSenderViewModel(
					null,
					@"C:\Projects\Evva_1.4.2.0\Evva\Data\Device Communications\param_defaults.json",
					_scriptUserData);
			}
			catch (Exception ex) 
			{
				MessageBox.Show("Failed to init\r\n\r\n" + ex);
			}
		}

		#endregion Constructor

		#region Methods

		private void Closing(CancelEventArgs e)
		{
			CANMessageSenderVM.Closing(null);
		}

		#endregion Methods

		#region Commands

		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }

		#endregion Commands
	}
}
