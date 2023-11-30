
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

		public Record_SelectedParametersListViewModel RecordParamList { get; set; }
		public SelectedParametersListViewModel AbsoluteParamList { get; set; }

		#endregion Properties

		#region Constructor

		public ParametersViewModel(
			DevicesContainer devicesContainer)
		{

			RecordParamList = new Record_SelectedParametersListViewModel(devicesContainer, "Record Parameters List");
			AbsoluteParamList = new SelectedParametersListViewModel(devicesContainer, "Absolute Parameters List");


			DragDropData dragDropData = new DragDropData();
			FullParametersList = new DeviceHandler.ViewModel.ParametersViewModel(dragDropData, devicesContainer, true);

		}

		#endregion Constructor

	}
}
