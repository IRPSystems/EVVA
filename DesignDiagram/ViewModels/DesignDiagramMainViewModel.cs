
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DesignDiagram.Views;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using Microsoft.Win32;
using ScriptHandler.Models;
using ScriptHandler.Models.ScriptNodes;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace DesignDiagram.ViewModels
{
    public class DesignDiagramMainViewModel: ObservableObject
    {
		#region Properties

		public string Version { get; set; }

		public DesignDiagramDockingViewModel Docking { get; set; }
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
			Docking = new DesignDiagramDockingViewModel(DevicesContainer, Stencil, NodeProperties);

			//DesignDiagramViewModel designDiagramViewModel = new DesignDiagramViewModel(
			//	"Name", @"C:\Users\smadar\Documents\Scripts\Test scripts\Project 4\Project 4.scr", NodeProperties, 100);
			//DesignDiagramView designDiagramView = new DesignDiagramView();
			//Docking.AddDocument(designDiagramViewModel, designDiagramView);


			NewScriptCommand = new RelayCommand(NewScript);
			LoadScriptCommand = new RelayCommand(LoadScript);
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

		private void NewScript()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Script file (*.scr)|*.scr|Test file (*.tst)|*.tst";
			bool? result = saveFileDialog.ShowDialog();
			if (result != true)
				return;

			string scriptName = Path.GetFileName(saveFileDialog.FileName);
			scriptName = scriptName.Replace(".db", string.Empty);

			DesignDiagramViewModel vm = new DesignDiagramViewModel(
					scriptName,
					saveFileDialog.FileName,
					NodeProperties,
					100);

			Docking.AddDocument(vm, new DesignDiagramView());
			//_designDashboardList.Add(vm);

			vm.Save();
		}

		private void LoadScript()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Script file (*.scr)|*.scr|Test file (*.tst)|*.tst";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			string scriptName = Path.GetFileName(openFileDialog.FileName);
			scriptName = scriptName.Replace(".db", string.Empty);

			DesignDiagramViewModel vm = new DesignDiagramViewModel(
					scriptName,
					openFileDialog.FileName,
					NodeProperties,
					100);

			vm.Open(openFileDialog.FileName);

			Docking.AddDocument(vm, new DesignDiagramView());
			//_designDashboardList.Add(vm);
		}

		#endregion Methods

		#region Commands


		public RelayCommand NewScriptCommand { get; private set; }
		public RelayCommand LoadScriptCommand { get; private set; }


		#endregion Commands
	}
}
