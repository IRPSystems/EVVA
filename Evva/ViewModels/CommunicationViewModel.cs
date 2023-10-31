
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceHandler.Models;
using Services.Services;
using System;

namespace Evva.ViewModels
{
	public class CommunicationViewModel: ObservableObject
	{

		#region Properties

		public DevicesContainer DevicesContainer { get; set; }

		#endregion Properties


		#region Constructor

		public CommunicationViewModel(
			DevicesContainer devicesContainer)
		{
			try
			{
				DevicesContainer = devicesContainer;
			}
			catch(Exception ex)
			{
				LoggerService.Error(this, "Failed to init CommunicationViewModel", ex);
			}
		}

		#endregion Constructor

		
	}
}
