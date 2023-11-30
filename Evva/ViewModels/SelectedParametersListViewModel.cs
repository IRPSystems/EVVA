
using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Models;
using static Evva.ViewModels.ParametersViewModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using ScriptRunner.Models;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;
using Services.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace Evva.ViewModels
{
	public class SelectedParametersListViewModel: ObservableObject
	{
		#region Properties

		public DeviceHandler.ViewModel.ParametersViewModel FullParametersList { get; set; }


		public ObservableCollection<DeviceParameterData> LogParametersList { get; set; }
		public ObservableCollection<RecordData> LogParametersList_WithIndex { get; set; }

		#endregion Properties

		#region Fields


		private DevicesContainer _devicesContainer;

		private const int _maxLoggingParams = 40;


		private System.Collections.IList _selectedItemsList;

		public const string DragDropFormat = "RecordParameter";

		private DragDropData _designDragDropData;

		#endregion Fields

		#region Constructor

		public SelectedParametersListViewModel(
			DevicesContainer devicesContainer)
		{
			_devicesContainer = devicesContainer;

			_designDragDropData = new DragDropData();


			DeletParameterLogListCommand = new RelayCommand<System.Collections.IList>(DeletParameterLogList);
			MoveUpCommand = new RelayCommand<RecordData>(MoveUp);
			MoveDownCommand = new RelayCommand<RecordData>(MoveDown);
			SaveParametersListCommand = new RelayCommand(SaveParametersList);
			LoadParametersListCommand = new RelayCommand(LoadParametersList);



			LogParametersList = new ObservableCollection<DeviceParameterData>();
			LogParametersList_WithIndex = new ObservableCollection<RecordData>();


			GetLogParamListFromFile();
		}

		#endregion Constructor

		#region Methods

		#region Save / Load

		public void SaveParametersList()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "JSON Files | *.json";
			bool? result = saveFileDialog.ShowDialog();
			if (result != true)
				return;

			string path = saveFileDialog.FileName;

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(LogParametersList, settings);
			System.IO.File.WriteAllText(path, sz);
		}

		public void LoadParametersList()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "JSON Files | *.json";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			string path = openFileDialog.FileName;


			string jsonString = System.IO.File.ReadAllText(path);

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			ObservableCollection<DeviceParameterData> parametersList = JsonConvert.DeserializeObject(jsonString, settings) as
				ObservableCollection<DeviceParameterData>;

			GetActualParameters(parametersList);

			WeakReferenceMessenger.Default.Send(new RECORD_LIST_CHANGEDMessage() { LogParametersList = LogParametersList });
		}

		#endregion Save / Load

		private void GetLogParamListFromFile()
		{
			string jsonString = System.IO.File.ReadAllText("Data\\Logger Default Params.json");

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			ObservableCollection<DeviceParameterData> parametersList = JsonConvert.DeserializeObject(jsonString, settings) as
				ObservableCollection<DeviceParameterData>;

			GetActualParameters(parametersList);
		}

		public void GetActualParameters(ObservableCollection<DeviceParameterData> parametersList)
		{
			if (LogParametersList == null)
				LogParametersList = new ObservableCollection<DeviceParameterData>();

			if (LogParametersList_WithIndex == null)
				LogParametersList_WithIndex = new ObservableCollection<RecordData>();

			LogParametersList.Clear();
			LogParametersList_WithIndex.Clear();

			foreach (DeviceParameterData parameterData in parametersList)
			{
				if (_devicesContainer.TypeToDevicesFullData.ContainsKey(parameterData.DeviceType) == false)
					continue;

				DeviceFullData deviceFullData =
					_devicesContainer.TypeToDevicesFullData[parameterData.DeviceType];
				if (deviceFullData == null || deviceFullData.Device == null)
					continue;

				DeviceParameterData actualParameterData =
					deviceFullData.Device.ParemetersList.ToList().Find((p) => p.Name == parameterData.Name);
				if (actualParameterData == null)
					continue;

				LogParametersList.Add(actualParameterData);
				LogParametersList_WithIndex.Add(new RecordData() { Data = actualParameterData });
			}



			while (LogParametersList.Count > _maxLoggingParams)
			{
				LogParametersList.RemoveAt(LogParametersList.Count - 1);
				LogParametersList_WithIndex.RemoveAt(LogParametersList.Count - 1);
			}

			SetIndeces();
		}

		private void DeletParameterLogList(System.Collections.IList paramsList)
		{
			List<RecordData> list = new List<RecordData>();
			foreach (RecordData data in paramsList)
				list.Add(data);

			foreach (RecordData data in list)
			{
				LogParametersList.Remove(data.Data);
				LogParametersList_WithIndex.Remove(data);
			}

			SetIndeces();

			WeakReferenceMessenger.Default.Send(new RECORD_LIST_CHANGEDMessage() { LogParametersList = LogParametersList });
		}

		#region Drag

		private void RecordingList_MouseEnter(MouseEventArgs e)
		{
			if (_designDragDropData.IsIgnor)
				return;

			if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
				_designDragDropData.IsMouseDown = true;
			else
				_designDragDropData.IsMouseDown = false;
		}

		private void RecordingList_PreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (_designDragDropData.IsIgnor)
				return;

			_designDragDropData.IsMouseDown = true;
			_designDragDropData.StartPoint = e.GetPosition(null);
		}

		private void RecordingList_PreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_designDragDropData.IsMouseDown = false;
		}

		private void RecordingList_MouseMove(MouseEventArgs e)
		{
			if (_designDragDropData.IsMouseDown == false)
				return;

			DragObject(e);
		}

		private void DragObject(MouseEventArgs e)
		{
			LoggerService.Inforamtion(this, "Object is draged");

			Point mousePos = e.GetPosition(null);
			Vector diff = _designDragDropData.StartPoint - mousePos;

			if (e.LeftButton == MouseButtonState.Pressed &&
				Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
				Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
			{

				ListView listView =
					FindAncestorService.FindAncestor<ListView>((DependencyObject)e.OriginalSource);
				if (listView == null)
					return;

				ListViewItem listViewItem =
						FindAncestorService.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
				if (listViewItem == null)
					return;

				if (!listView.SelectedItems.Contains(listViewItem.DataContext))
				{
					DataObject dragData = new DataObject(DragDropFormat, listViewItem.DataContext);
					DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
				}
				else
				{
					DataObject dragData = new DataObject(DragDropFormat, listView.SelectedItems);
					DragDrop.DoDragDrop(listView, dragData, DragDropEffects.Move);
				}

			}
		}



		#endregion Drag


		#region Drop

		private void RecordingList_Drop(DragEventArgs e)
		{
			LoggerService.Inforamtion(this, "Object is dropped");

			if (e.Data.GetDataPresent(DeviceHandler.ViewModel.ParametersViewModel.DragDropFormat))
			{
				int droppedOnIndex = -1;
				ListViewItem listViewItem =
						FindAncestorService.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
				if (listViewItem != null)
				{
					if (listViewItem.DataContext is RecordData recordData)
					{
						droppedOnIndex = LogParametersList_WithIndex.IndexOf(recordData);
					}
				}




				var dragData = e.Data.GetData(DeviceHandler.ViewModel.ParametersViewModel.DragDropFormat);

				if (dragData is ObservableCollection<object> list)
				{
					foreach (object obj in list)
					{
						if (!(obj is DeviceParameterData param))
							continue;

						if (LogParametersList.Count == _maxLoggingParams)
						{
							MessageBox.Show("Only up to 40 parameters are allowed");
							return;
						}

						AddParamToLogList(param, droppedOnIndex);

					}
				}
				else if (dragData is DeviceParameterData param)
				{
					AddParamToLogList(param, droppedOnIndex);
				}



			}
			else if (e.Data.GetDataPresent(DragDropFormat))
			{
				var dragData = e.Data.GetData(DragDropFormat);

				if (dragData is System.Collections.IList)
				{

					RecordData droppedOn = null;
					ListViewItem listViewItem =
						FindAncestorService.FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
					if (listViewItem != null)
					{
						droppedOn = listViewItem.DataContext as RecordData;
						if (droppedOn == null)
							return;
					}

					int destIndex = LogParametersList_WithIndex.IndexOf(droppedOn);


					MoveGroupOfParam(destIndex);


				}
			}
		}

		private void AddParamToLogList(
			DeviceParameterData param,
			int droppedOnIndex)
		{
			int index = LogParametersList.IndexOf(param);
			if (index >= 0)
			{
				MessageBox.Show("The parameter already exist");
				return;
			}

			if (droppedOnIndex == -1)
			{
				LogParametersList.Add(param);
				LogParametersList_WithIndex.Add(new RecordData() { Data = param });
			}
			else
			{
				LogParametersList.Insert(droppedOnIndex, param);
				LogParametersList_WithIndex.Insert(droppedOnIndex, new RecordData() { Data = param });
			}

			SetIndeces();

			WeakReferenceMessenger.Default.Send(new RECORD_LIST_CHANGEDMessage() { LogParametersList = LogParametersList });
		}


		private void RecordingList_DragEnter(DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DeviceHandler.ViewModel.ParametersViewModel.DragDropFormat))
			{
				e.Effects = DragDropEffects.None;
			}
		}

		#endregion Drop		


		#region Move UP/DOWN

		private void MoveGroupOfParam(
			int destIndex)
		{
			List<RecordData> list = new List<RecordData>();
			foreach (var item in _selectedItemsList)
				list.Add(item as RecordData);

			foreach (var item in list)
			{
				MoveParam(
					item,
					destIndex);

				destIndex = LogParametersList_WithIndex.IndexOf(item);
			}

			SetIndeces();

			WeakReferenceMessenger.Default.Send(new RECORD_LIST_CHANGEDMessage() { LogParametersList = LogParametersList });
		}

		private void MoveParam(
			RecordData paramToMove,
			int destIndex)
		{
			RecordData tempParam = paramToMove;

			int sourceIndex = LogParametersList_WithIndex.IndexOf(tempParam);

			LogParametersList.RemoveAt(sourceIndex);
			LogParametersList_WithIndex.RemoveAt(sourceIndex);

			if (destIndex < 0 || destIndex == (LogParametersList_WithIndex.Count - 1))
			{
				LogParametersList.Add(tempParam.Data);
				LogParametersList_WithIndex.Add(tempParam);
			}
			else
			{
				LogParametersList.Insert(destIndex, tempParam.Data);
				LogParametersList_WithIndex.Insert(destIndex, tempParam);
			}
		}

		private void MoveUp(RecordData param)
		{
			LoggerService.Inforamtion(this, "Moving parameter UP");

			int index = LogParametersList_WithIndex.IndexOf(param);
			if (index <= 0 || index - 1 < 0)
				return;

			MoveGroupOfParam(index - 1);
		}

		private void MoveDown(RecordData param)
		{
			LoggerService.Inforamtion(this, "Moving node DOWN");

			int index = LogParametersList_WithIndex.IndexOf(param);
			if (index >= LogParametersList.Count || index + 1 >= LogParametersList.Count)
				return;

			MoveGroupOfParam(index + _selectedItemsList.Count);
		}

		#endregion Move UP/DOWN

		private void RecordingList_SelectionChanged(SelectionChangedEventArgs e)
		{
			if (!(e.Source is ListView lv))
				return;

			_selectedItemsList = lv.SelectedItems;
		}

		private void RecordingList_KeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
			{
				DeletParameterLogList(_selectedItemsList);
			}
		}

		private void SetIndeces()
		{
			for (int i = 0; i < LogParametersList_WithIndex.Count; i++)
			{
				LogParametersList_WithIndex[i].Index = i + 1;
			}
		}

		#endregion Methods


		#region Commands

		public RelayCommand<System.Collections.IList> DeletParameterLogListCommand { get; private set; }
		public RelayCommand<RecordData> MoveUpCommand { get; private set; }
		public RelayCommand<RecordData> MoveDownCommand { get; private set; }

		public RelayCommand SaveParametersListCommand { get; private set; }
		public RelayCommand LoadParametersListCommand { get; private set; }


		#region Drag

		private RelayCommand<MouseEventArgs> _RecordingList_MouseEnterCommand;
		public RelayCommand<MouseEventArgs> RecordingList_MouseEnterCommand
		{
			get
			{
				return _RecordingList_MouseEnterCommand ?? (_RecordingList_MouseEnterCommand =
					new RelayCommand<MouseEventArgs>(RecordingList_MouseEnter));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _RecordingList_PreviewMouseLeftButtonDownCommant;
		public RelayCommand<MouseButtonEventArgs> RecordingList_PreviewMouseLeftButtonDownCommant
		{
			get
			{
				return _RecordingList_PreviewMouseLeftButtonDownCommant ?? (_RecordingList_PreviewMouseLeftButtonDownCommant =
					new RelayCommand<MouseButtonEventArgs>(RecordingList_PreviewMouseLeftButtonDown));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _RecordingList_PreviewMouseLeftButtonUpCommant;
		public RelayCommand<MouseButtonEventArgs> RecordingList_PreviewMouseLeftButtonUpCommant
		{
			get
			{
				return _RecordingList_PreviewMouseLeftButtonUpCommant ?? (_RecordingList_PreviewMouseLeftButtonUpCommant =
					new RelayCommand<MouseButtonEventArgs>(RecordingList_PreviewMouseLeftButtonUp));
			}
		}

		private RelayCommand<MouseEventArgs> _RecordingList_MouseMoveCommand;
		public RelayCommand<MouseEventArgs> RecordingList_MouseMoveCommand
		{
			get
			{
				return _RecordingList_MouseMoveCommand ?? (_RecordingList_MouseMoveCommand =
					new RelayCommand<MouseEventArgs>(RecordingList_MouseMove));
			}
		}





		#endregion Drag


		#region Drop

		private RelayCommand<DragEventArgs> _RecordingList_DropCommand;
		public RelayCommand<DragEventArgs> RecordingList_DropCommand
		{
			get
			{
				return _RecordingList_DropCommand ?? (_RecordingList_DropCommand =
					new RelayCommand<DragEventArgs>(RecordingList_Drop));
			}
		}

		private RelayCommand<DragEventArgs> _RecordingList_DragEnterCommand;
		public RelayCommand<DragEventArgs> RecordingList_DragEnterCommand
		{
			get
			{
				return _RecordingList_DragEnterCommand ?? (_RecordingList_DragEnterCommand =
					new RelayCommand<DragEventArgs>(RecordingList_DragEnter));
			}
		}

		#endregion Drop



		private RelayCommand<SelectionChangedEventArgs> _RecordingList_SelectionChangedCommand;
		public RelayCommand<SelectionChangedEventArgs> RecordingList_SelectionChangedCommand
		{
			get
			{
				return _RecordingList_SelectionChangedCommand ?? (_RecordingList_SelectionChangedCommand =
					new RelayCommand<SelectionChangedEventArgs>(RecordingList_SelectionChanged));
			}
		}

		private RelayCommand<KeyEventArgs> _RecordingList_KeyDownCommand;
		public RelayCommand<KeyEventArgs> RecordingList_KeyDownCommand
		{
			get
			{
				return _RecordingList_KeyDownCommand ?? (_RecordingList_KeyDownCommand =
					new RelayCommand<KeyEventArgs>(RecordingList_KeyDown));
			}
		}


		#endregion Commands
	}
}
