
using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Models;
using DeviceHandler.Models;

namespace Evva.ViewModels
{
	public class ParametersViewModel : ObservableObject
	{
		public class RecordData: ObservableObject
		{
			public int Index { get; set; }
			public DeviceParameterData Data { get; set; }
		}

		#region Properties

		public DeviceHandler.ViewModel.ParametersViewModel FullParametersList { get; set; }

		public SelectedParametersListViewModel RecordParamList { get; set; }

		#endregion Properties

		#region Constructor

		public ParametersViewModel(
			DevicesContainer devicesContainer)
		{

			RecordParamList = new SelectedParametersListViewModel(devicesContainer);

			
			DragDropData dragDropData = new DragDropData();
			FullParametersList = new DeviceHandler.ViewModel.ParametersViewModel(dragDropData, devicesContainer, true);

		}

		#endregion Constructor

	}
}
