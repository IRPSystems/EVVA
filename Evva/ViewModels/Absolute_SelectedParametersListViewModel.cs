﻿
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using DeviceHandler.ViewModels;
using Entities.Models;
using System.Collections.Specialized;

namespace Evva.ViewModels
{
	public class Absolute_SelectedParametersListViewModel : SelectedParametersListViewModel
	{
		
		#region Fields


		#endregion Fields

		#region Constructor

		public Absolute_SelectedParametersListViewModel(
			DevicesContainer devicesContainer,
			string title) :
			base(devicesContainer, title) 
		{
			ParametersList.CollectionChanged += ParametersList_CollectionChanged;
		}



		#endregion Constructor

		#region Methods

		private void ParametersList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (DeviceParameterData param in e.NewItems)
				{
					param.IsAbsolute = (e.Action == NotifyCollectionChangedAction.Add);
				}
			}
			
			if (e.OldItems != null)
			{
				foreach (DeviceParameterData param in e.OldItems)
				{
					param.IsAbsolute = (e.Action == NotifyCollectionChangedAction.Add);
				}
			}
		}

		#endregion Methods

	}
}
