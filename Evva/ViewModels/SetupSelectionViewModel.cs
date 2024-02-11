
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Services;
using Entities.Models;
using Evva.Models;
using Services.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Win32;
using Newtonsoft.Json;
using Entities.Enums;
using System.IO;
using DeviceCommunicators.Models;

namespace Evva.ViewModels
{
	public class SetupSelectionViewModel: ObservableObject
	{
		#region Properties

		public ObservableCollection<DeviceData> DevicesList { get; set; }
		public ObservableCollection<DeviceData> DevicesSourceList { get; set; }


		public DeviceData DestListSelectedItem { get; set; }
		public DeviceData SourceListSelectedItem { get; set; }

		#endregion Properties

		#region Fields

		private bool _isMouseDown;
		private Point _startPoint;

		private EvvaUserData _EvvaUserData;

		private ObservableCollection<DeviceData> _devicesSourceList_Full;

		private DeviceData _selectedDevice;

		#endregion Fields

		#region Constructor

		public SetupSelectionViewModel(
			EvvaUserData EvvaUserData,
			ReadDevicesFileService readDevicesFile)
		{
			_EvvaUserData = EvvaUserData;

			SaveDeviceSetupCommand = new RelayCommand(SaveDeviceSetup);
			LoadDeviceSetupCommand = new RelayCommand(LoadDeviceSetup);

			CloseOKCommand = new RelayCommand(CloseOK);
			CloseCancelCommand = new RelayCommand(CloseCancel);

			DeleteDeviceCommand = new RelayCommand(DeleteDevice);

			MoveDeviceToDestCommand = new RelayCommand(MoveDeviceToDest);
			MoveDeviceToSourceCommand = new RelayCommand(MoveDeviceToSource);

			_selectedDevice = null;

			_devicesSourceList_Full = readDevicesFile.ReadAllFiles(
				@"Data\Device Communications\",
				EvvaUserData.MCUJsonPath,
				EvvaUserData.MCUB2BJsonPath,
				EvvaUserData.DynoCommunicationPath,
				EvvaUserData.NI6002CommunicationPath);
			DevicesSourceList = new ObservableCollection<DeviceData>();
			foreach (DeviceData device in _devicesSourceList_Full)
			{
				DevicesSourceList.Add(device);
			}

			DevicesList = new ObservableCollection<DeviceData>();
			if (EvvaUserData.SetupDevicesList == null)
				return;

			
		

			foreach(DeviceTypesEnum deviceType in EvvaUserData.SetupDevicesList)
			{
				DeviceData deviceBase =
					DevicesSourceList.ToList().Find((d) => d.DeviceType == deviceType);
				if (deviceBase != null)
				{
					DevicesList.Add(deviceBase);
					RemoveSetupDeviceFromSource(deviceBase.DeviceType);
				}
			}


			string str = "Startup Setup list: \r\n";
			foreach(DeviceData deviceBase1 in DevicesList)
			{
				str += deviceBase1.DeviceType + "-" + deviceBase1.Name + "\r\n";
			}
			LoggerService.Inforamtion(this, str);

			str = "Source Setup list: \r\n";
			foreach (DeviceData deviceBase1 in DevicesSourceList)
			{
				str += deviceBase1.DeviceType + "-" + deviceBase1.Name + "\r\n";
			}
			LoggerService.Inforamtion(this, str);

		}

		#endregion Constructor

		#region Methods

		private void RemoveSetupDeviceFromSource(DeviceTypesEnum deviceType)
		{
			LoggerService.Inforamtion(this, "Remove device " + deviceType + " from the sources list");

			DeviceData device = DevicesSourceList.ToList().Find((d) => d.DeviceType == deviceType);
			if (device == null)
				return;

			LoggerService.Inforamtion(this, "Remove device " + device.DeviceType + "-" + device.Name + " from the sources list");

			DevicesSourceList.Remove(device);
		}

		private void DeleteDevice()
		{
			LoggerService.Inforamtion(this, "Remove device " + DestListSelectedItem.DeviceType + "-" + DestListSelectedItem.Name + " from the setup list");

			DevicesSourceList.Add(DestListSelectedItem);
			DevicesList.Remove(DestListSelectedItem);
		}


		#region Drag

		private void SourceList_MouseEnter(MouseEventArgs e)
		{
			
			if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
				_isMouseDown = true;
			else
				_isMouseDown = false;
		}

		private void SourceList_PreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{

			_isMouseDown = true;
			_startPoint = e.GetPosition(null);
		}

		private void SourceList_PreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_isMouseDown = false;
		}

		private void SourceList_MouseMove(MouseEventArgs e)
		{
			if (_isMouseDown == false)
				return;

			DragObject(e);
		}

		private void ListParams_MouseMove(MouseEventArgs e)
		{
			if (_isMouseDown == false)
				return;

			DragObject(e);
		}

		private void DragObject(MouseEventArgs e)
		{
		
			Point mousePos = e.GetPosition(null);
			Vector diff = _startPoint - mousePos;

			if (e.LeftButton == MouseButtonState.Pressed &&
				Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
				Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
			{

				string formate = "DeviceDrag";

				// Get the dragged ListViewItem
				ListView listView =
					FindAnchestor<ListView>((DependencyObject)e.OriginalSource);
				ListViewItem listViewItem =
					FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

				DependencyObject sourceObject = listViewItem;

				object item = null;
				if (listView != null && listViewItem != null)
				{
					// Find the data behind the ListViewItem
					item = listView.ItemContainerGenerator.
						ItemFromContainer(listViewItem);
				}

				if (item == null)
				{
					TreeView treeView =
						FindAnchestor<TreeView>((DependencyObject)e.OriginalSource);
					TreeViewItem treeViewItem =
						FindAnchestor<TreeViewItem>((DependencyObject)e.OriginalSource);

					sourceObject = treeViewItem;

					if (treeView != null && treeViewItem != null)
					{
						// Find the data behind the TreeViewItem
						//item = treeView.ItemContainerGenerator.
						//	ItemFromContainer(treeViewItem);
						//if (item == null)
						item = treeViewItem.DataContext;

						if (item == null)
							return;
					}
					else
						return;
				}


				DataObject dragData = new DataObject(formate, item);
				DragDrop.DoDragDrop(sourceObject, dragData, DragDropEffects.Move);
			}
		}

		public static T FindAnchestor<T>(DependencyObject current)
					where T : DependencyObject
		{
			do
			{
				if (current is T)
				{
					return (T)current;
				}
				current = VisualTreeHelper.GetParent(current);
			}
			while (current != null);
			return null;
		}

		#endregion Drag

		#region Drop

		private void DestList_Drop(DragEventArgs e)
		{
			LoggerService.Inforamtion(this, "Object is dropped");


			if (e.Data.GetDataPresent("DeviceDrag"))
			{
				DeviceData droppedDevice = e.Data.GetData("DeviceDrag") as DeviceData;
				AddDeviceToDestList(droppedDevice);
			}

			string str = "Source list:\r\n";
			foreach(DeviceData deviceBase1 in DevicesSourceList)
				str += deviceBase1.DeviceType + "\r\n";
			LoggerService.Inforamtion(this, str);
		}

		

		private void DestList_DragEnter(DragEventArgs e)
		{
			if (!e.Data.GetDataPresent("DeviceDrag"))
			{
				e.Effects = DragDropEffects.None;
			}
		}

		#endregion Drop

		#region Load/Save

		private void SaveDeviceSetup()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Device Setup Files | *.setup";
			saveFileDialog.InitialDirectory = _EvvaUserData.LastSetupPath;
			bool? result = saveFileDialog.ShowDialog();
			if (result != true)
				return;


			_EvvaUserData.LastSetupPath = Path.GetDirectoryName(saveFileDialog.FileName);
			string path = saveFileDialog.FileName;

			List<(string, DeviceTypesEnum)> devicesList = new List<(string, DeviceTypesEnum)>();
			foreach(DeviceData deviceBase in DevicesList) 
			{ 
				devicesList.Add((deviceBase.Name, deviceBase.DeviceType));
			}

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(devicesList, settings);
			System.IO.File.WriteAllText(path, sz);
		}

		private void LoadDeviceSetup()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Device Setup Files | *.setup";
			openFileDialog.InitialDirectory = _EvvaUserData.LastSetupPath;
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			_EvvaUserData.LastSetupPath = Path.GetDirectoryName(openFileDialog.FileName);
			string path = openFileDialog.FileName;


			string jsonString = System.IO.File.ReadAllText(path);

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			List<(string, DeviceTypesEnum)> devicesList = JsonConvert.DeserializeObject(jsonString, settings) as
				List<(string, DeviceTypesEnum)>;

			DevicesList.Clear();

			foreach ((string, DeviceTypesEnum) device in devicesList) 
			{
				DeviceData deviceBase = _devicesSourceList_Full.ToList().Find((d) => d.DeviceType == device.Item2);
				if(deviceBase == null) 
					continue;

				DeviceData destDevice = deviceBase.Clone() as DeviceData;
				destDevice.Name = device.Item1;
				DevicesList.Add(destDevice);
				RemoveSetupDeviceFromSource(deviceBase.DeviceType);
			}
		}

		#endregion Load/Save

		private void AddDeviceToDestList(DeviceData deviceData_Source)
		{
			DeviceData deviceData = deviceData_Source.Clone() as DeviceData;

			//List<DeviceData> sameTypeDevice =
			//	DevicesList.ToList().Where((d) => d.DeviceType == deviceData.DeviceType).ToList();
			//if (sameTypeDevice != null && sameTypeDevice.Count > 0)
			//{
			//	deviceData.Name = deviceData.Name + (sameTypeDevice.Count + 1);
			//}

			LoggerService.Inforamtion(this, "Add device " + deviceData.DeviceType + "-" + deviceData.Name + " to the setup list");

			DevicesList.Add(deviceData);
			RemoveSetupDeviceFromSource(deviceData.DeviceType);
		}

		private void CloseOK()
		{
			_EvvaUserData.SetupDevicesList = new ObservableCollection<DeviceTypesEnum>();
			foreach(DeviceData deviceBase in DevicesList)
				_EvvaUserData.SetupDevicesList.Add(deviceBase.DeviceType);

			

			CloseOKEvent?.Invoke();
		}

		private void CloseCancel()
		{
			CloseCancelEvent?.Invoke();
		}

		private void MoveDeviceToDest()
		{
			AddDeviceToDestList(SourceListSelectedItem);
		}

		private void MoveDeviceToSource()
		{
			DeleteDevice();
		}

		#endregion Methods

		#region Commands

		#region Drag

		private RelayCommand<MouseEventArgs> _SourceList_MouseEnterCommand;
		public RelayCommand<MouseEventArgs> SourceList_MouseEnterCommand
		{
			get
			{
				return _SourceList_MouseEnterCommand ?? (_SourceList_MouseEnterCommand =
					new RelayCommand<MouseEventArgs>(SourceList_MouseEnter));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _SourceList_PreviewMouseLeftButtonDownCommant;
		public RelayCommand<MouseButtonEventArgs> SourceList_PreviewMouseLeftButtonDownCommant
		{
			get
			{
				return _SourceList_PreviewMouseLeftButtonDownCommant ?? (_SourceList_PreviewMouseLeftButtonDownCommant =
					new RelayCommand<MouseButtonEventArgs>(SourceList_PreviewMouseLeftButtonDown));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _SourceList_PreviewMouseLeftButtonUpCommant;
		public RelayCommand<MouseButtonEventArgs> SourceList_PreviewMouseLeftButtonUpCommant
		{
			get
			{
				return _SourceList_PreviewMouseLeftButtonUpCommant ?? (_SourceList_PreviewMouseLeftButtonUpCommant =
					new RelayCommand<MouseButtonEventArgs>(SourceList_PreviewMouseLeftButtonUp));
			}
		}

		private RelayCommand<MouseEventArgs> _SourceList_MouseMoveCommand;
		public RelayCommand<MouseEventArgs> SourceList_MouseMoveCommand
		{
			get
			{
				return _SourceList_MouseMoveCommand ?? (_SourceList_MouseMoveCommand =
					new RelayCommand<MouseEventArgs>(SourceList_MouseMove));
			}
		}

		private RelayCommand<MouseEventArgs> _ListParams_MouseMoveCommand;
		public RelayCommand<MouseEventArgs> ListParams_MouseMoveCommand
		{
			get
			{
				return _ListParams_MouseMoveCommand ?? (_ListParams_MouseMoveCommand =
					new RelayCommand<MouseEventArgs>(ListParams_MouseMove));
			}
		}


		#endregion Drag

		#region Drop

		private RelayCommand<DragEventArgs> _DestList_DropCommand;
		public RelayCommand<DragEventArgs> DestList_DropCommand
		{
			get
			{
				return _DestList_DropCommand ?? (_DestList_DropCommand =
					new RelayCommand<DragEventArgs>(DestList_Drop));
			}
		}

		private RelayCommand<DragEventArgs> _DestList_DragEnterCommand;
		public RelayCommand<DragEventArgs> DestList_DragEnterCommand
		{
			get
			{
				return _DestList_DragEnterCommand ?? (_DestList_DragEnterCommand =
					new RelayCommand<DragEventArgs>(DestList_DragEnter));
			}
		}

		#endregion Drop





		public RelayCommand SaveDeviceSetupCommand { get; private set; }
		public RelayCommand LoadDeviceSetupCommand { get; private set; }

		public RelayCommand CloseOKCommand { get; private set; }
		public RelayCommand CloseCancelCommand { get; private set; }

		public RelayCommand DeleteDeviceCommand { get; private set; }

		public RelayCommand MoveDeviceToDestCommand { get; private set; }
		public RelayCommand MoveDeviceToSourceCommand { get; private set; }

		#endregion Commands

		#region Events

		public event Action CloseOKEvent;
		public event Action CloseCancelEvent;

		#endregion Events
	}
}
