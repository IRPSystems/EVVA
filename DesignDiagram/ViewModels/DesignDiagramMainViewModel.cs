
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DesignDiagram.Views;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using ScriptHandler.Models;
using ScriptHandler.Models.ScriptNodes;
using System.Collections.ObjectModel;
using System.Reflection;

namespace DesignDiagram.ViewModels
{
    public class DesignDiagramMainViewModel: ObservableObject
    {
		#region Properties

		public string Version { get; set; }

		public DesignDiagramDockingViewModes Docking { get; set; }
		public StencilViewModel Stencil { get; set; }
		public DevicesContainer DevicesContainer { get; set; }
		public NodePropertiesView NodeProperties { get; set; }

		#endregion Properties

		#region Constructor

		public DesignDiagramMainViewModel()
		{
			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			InitDeviceContainer();

			NodeProperties = new NodePropertiesView() { DataContext = null };
			Stencil = new StencilViewModel();
			Docking = new DesignDiagramDockingViewModes(DevicesContainer, Stencil, NodeProperties);

			DesignDiagramViewModel designDiagramViewModel = new DesignDiagramViewModel(
				"Name", @"C:\Users\smadar\Documents\Scripts\Test scripts\Project 4\Project 4.scr", NodeProperties, 100);
			DesignDiagramView designDiagramView = new DesignDiagramView();
			Docking.AddDocument(designDiagramViewModel, designDiagramView);
		}

		#endregion Constructor

		#region Methods

		private void InitDeviceContainer()
		{
			DevicesContainer = new DevicesContainer();
			DevicesContainer.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
			DevicesContainer.DevicesList = new ObservableCollection<DeviceData>();
			DevicesContainer.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();

			ReadDevicesFileService reader = new ReadDevicesFileService();
			ObservableCollection<DeviceData> devicesList = new ObservableCollection<DeviceData>();
			reader.ReadFromMCUJson(
				@"C:\Users\smadar\Documents\Stam\Json files\_param_defaults_All.json",
				devicesList,
				"MCU",
				DeviceTypesEnum.MCU);

			if (devicesList[0].ParemetersList == null || devicesList[0].ParemetersList.Count == 0)
			{
				return;
			}

			DevicesContainer.DevicesList.Add(devicesList[0]);

			if (DevicesContainer.DevicesFullDataList.Count == 0)
			{
				DeviceFullData fullData = new DeviceFullData_MCU(devicesList[0], false);
				DevicesContainer.DevicesFullDataList.Add(fullData);
				DevicesContainer.TypeToDevicesFullData.Add(DeviceTypesEnum.MCU, fullData);
				fullData.Init("TrueDriveManager", null);
				fullData.InitCheckConnection();
			}
			else
			{
				DeviceFullData fullData = DevicesContainer.DevicesFullDataList[0];
				fullData.Device = devicesList[0];
			}


			WeakReferenceMessenger.Default.Send(new SETUP_UPDATEDMessage());
		}

		#endregion Methods
	}
}
