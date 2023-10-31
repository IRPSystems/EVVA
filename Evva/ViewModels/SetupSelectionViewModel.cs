
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

namespace Evva.ViewModels
{
	public class SetupSelectionViewModel: ObservableObject
	{
		#region Properties

		public ObservableCollection<DeviceBase> DevicesList { get; set; }
		public ObservableCollection<DeviceBase> DevicesSourceList { get; set; }


		public DeviceBase SetupSelectedItem { get; set; }

		#endregion Properties

		#region Fields

		private bool _isMouseDown;
		private Point _startPoint;

		private EvvaUserData _EvvaUserData;

		private ObservableCollection<DeviceBase> _devicesSourceList_Full;

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


			_devicesSourceList_Full = readDevicesFile.ReadAllFiles(
				@"Data\Device Communications\",
				EvvaUserData.MCUJsonPath,
				EvvaUserData.MCUB2BJsonPath,
				EvvaUserData.DynoCommunicationPath,
				EvvaUserData.NI6002CommunicationPath);
			DevicesSourceList = new ObservableCollection<DeviceBase>();
			foreach (DeviceBase device in _devicesSourceList_Full)
			{
				DevicesSourceList.Add(device);
			}

			DevicesList = new ObservableCollection<DeviceBase>();
			if (EvvaUserData.SetupDevicesList == null)
				return;

			
		

			foreach(DeviceTypesEnum deviceType in EvvaUserData.SetupDevicesList)
			{
				DeviceBase deviceBase =
					DevicesSourceList.ToList().Find((d) => d.DeviceType == deviceType);
				if (deviceBase != null)
				{
					DevicesList.Add(deviceBase);
					RemoveSetupDeviceFromSource(deviceBase.DeviceType);
				}
			}


			string str = "Startup Setup list: \r\n";
			foreach(DeviceBase deviceBase1 in DevicesList)
			{
				str += deviceBase1.DeviceType + "-" + deviceBase1.Name + "\r\n";
			}
			LoggerService.Inforamtion(this, str);

			str = "Source Setup list: \r\n";
			foreach (DeviceBase deviceBase1 in DevicesSourceList)
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

			DeviceBase device = DevicesSourceList.ToList().Find((d) => d.DeviceType == deviceType);
			if (device == null)
				return;

			LoggerService.Inforamtion(this, "Remove device " + device.DeviceType + "-" + device.Name + " from the sources list");

			DevicesSourceList.Remove(device);
		}

		private void DeleteDevice()
		{
			LoggerService.Inforamtion(this, "Remove device " + SetupSelectedItem.DeviceType + "-" + SetupSelectedItem.Name + " from the setup list");

			DevicesSourceList.Add(SetupSelectedItem);
			DevicesList.Remove(SetupSelectedItem);
		}


		#region Drag

		private void ListTools_MouseEnter(MouseEventArgs e)
		{
			
			if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
				_isMouseDown = true;
			else
				_isMouseDown = false;
		}

		private void ListTools_PreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{

			_isMouseDown = true;
			_startPoint = e.GetPosition(null);
		}

		private void ListTools_PreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_isMouseDown = false;
		}

		private void ListTools_MouseMove(MouseEventArgs e)
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

		private void ListScript_Drop(DragEventArgs e)
		{
			LoggerService.Inforamtion(this, "Object is dropped");


			if (e.Data.GetDataPresent("DeviceDrag"))
			{
				DeviceBase droppedDevice = e.Data.GetData("DeviceDrag") as DeviceBase;
				DeviceBase deviceBase = droppedDevice.Clone() as DeviceBase;

				List<DeviceBase> sameTypeDevice = 
					DevicesList.ToList().Where((d) => d.DeviceType == deviceBase.DeviceType).ToList();
				if (sameTypeDevice != null && sameTypeDevice.Count > 0)
				{
					deviceBase.Name = deviceBase.Name + (sameTypeDevice.Count + 1);
				}

				LoggerService.Inforamtion(this, "Add device " + deviceBase.DeviceType + "-"+ deviceBase.Name +" to the setup list");

				DevicesList.Add(deviceBase);
				RemoveSetupDeviceFromSource(deviceBase.DeviceType);
			}

			string str = "Source list:\r\n";
			foreach(DeviceBase deviceBase1 in DevicesSourceList)
				str += deviceBase1.DeviceType + "\r\n";
			LoggerService.Inforamtion(this, str);
		}

		private void ListScript_DragEnter(DragEventArgs e)
		{
			if (!e.Data.GetDataPresent("DeviceDrag"))
			{
				e.Effects = DragDropEffects.None;
			}
		}

		#endregion Drop



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
			foreach(DeviceBase deviceBase in DevicesList) 
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
				DeviceBase deviceBase = _devicesSourceList_Full.ToList().Find((d) => d.DeviceType == device.Item2);
				if(deviceBase == null) 
					continue;

				DeviceBase destDevice = deviceBase.Clone() as DeviceBase;
				destDevice.Name = device.Item1;
				DevicesList.Add(destDevice);
				RemoveSetupDeviceFromSource(deviceBase.DeviceType);
			}
		}

		private void CloseOK()
		{
			_EvvaUserData.SetupDevicesList = new ObservableCollection<DeviceTypesEnum>();
			foreach(DeviceBase deviceBase in DevicesList)
				_EvvaUserData.SetupDevicesList.Add(deviceBase.DeviceType);

			

			CloseOKEvent?.Invoke();
		}

		private void CloseCancel()
		{
			CloseCancelEvent?.Invoke();
		}


		#endregion Methods

		#region Commands

		#region Drag

		private RelayCommand<MouseEventArgs> _ListTools_MouseEnterCommand;
		public RelayCommand<MouseEventArgs> ListTools_MouseEnterCommand
		{
			get
			{
				return _ListTools_MouseEnterCommand ?? (_ListTools_MouseEnterCommand =
					new RelayCommand<MouseEventArgs>(ListTools_MouseEnter));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _ListTools_PreviewMouseLeftButtonDownCommant;
		public RelayCommand<MouseButtonEventArgs> ListTools_PreviewMouseLeftButtonDownCommant
		{
			get
			{
				return _ListTools_PreviewMouseLeftButtonDownCommant ?? (_ListTools_PreviewMouseLeftButtonDownCommant =
					new RelayCommand<MouseButtonEventArgs>(ListTools_PreviewMouseLeftButtonDown));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _ListTools_PreviewMouseLeftButtonUpCommant;
		public RelayCommand<MouseButtonEventArgs> ListTools_PreviewMouseLeftButtonUpCommant
		{
			get
			{
				return _ListTools_PreviewMouseLeftButtonUpCommant ?? (_ListTools_PreviewMouseLeftButtonUpCommant =
					new RelayCommand<MouseButtonEventArgs>(ListTools_PreviewMouseLeftButtonUp));
			}
		}

		private RelayCommand<MouseEventArgs> _ListTools_MouseMoveCommand;
		public RelayCommand<MouseEventArgs> ListTools_MouseMoveCommand
		{
			get
			{
				return _ListTools_MouseMoveCommand ?? (_ListTools_MouseMoveCommand =
					new RelayCommand<MouseEventArgs>(ListTools_MouseMove));
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

		private RelayCommand<DragEventArgs> _ListScript_DropCommand;
		public RelayCommand<DragEventArgs> ListScript_DropCommand
		{
			get
			{
				return _ListScript_DropCommand ?? (_ListScript_DropCommand =
					new RelayCommand<DragEventArgs>(ListScript_Drop));
			}
		}

		private RelayCommand<DragEventArgs> _ListScript_DragEnterCommand;
		public RelayCommand<DragEventArgs> ListScript_DragEnterCommand
		{
			get
			{
				return _ListScript_DragEnterCommand ?? (_ListScript_DragEnterCommand =
					new RelayCommand<DragEventArgs>(ListScript_DragEnter));
			}
		}

		#endregion Drop

		public RelayCommand SaveDeviceSetupCommand { get; private set; }
		public RelayCommand LoadDeviceSetupCommand { get; private set; }

		public RelayCommand CloseOKCommand { get; private set; }
		public RelayCommand CloseCancelCommand { get; private set; }

		public RelayCommand DeleteDeviceCommand { get; private set; }

		#endregion Commands

		#region Events

		public event Action CloseOKEvent;
		public event Action CloseCancelEvent;

		#endregion Events
	}
}
