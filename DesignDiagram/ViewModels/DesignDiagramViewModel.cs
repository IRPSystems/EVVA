
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.Interfaces;
using DesignDiagram.Views;
using Newtonsoft.Json;
using ScriptHandler.Models;
using ScriptHandler.Models.ScriptNodes;
using Services.Services;
using Syncfusion.UI.Xaml.Diagram;
using Syncfusion.UI.Xaml.Diagram.Stencil;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesignDiagram.ViewModels
{
	public class DesignDiagramViewModel: ObservableObject, IDocumentVM
	{
		#region Properties

		public ScriptData DesignDiagram { get; set; }

		public NodeCollection Nodes { get; set; }
		public SnapSettings SnapSettings { get; set; }
		public ConnectorCollection Connectors { get; set; }

		public Syncfusion.UI.Xaml.Diagram.CommandManager CommandManager { get; set; }

		public PageSettings PageSettings { get; set; }

		public double OffsetX { get; set; }
		public double OffsetY { get; set; }

		public object SelectedItems { get; set; }

		public string Name
		{
			get
			{
				if(DesignDiagram == null)
					return null;
				return DesignDiagram.Name;
			}
			set { }
		}

		#endregion Properties

		#region Fields

		private NodePropertiesView _nodeProperties;

		private bool _isInPropertyChanged;

		private const double _toolHeight = 35;
		private const double _toolWidth = 300;
		private const double _betweenTools = 45;
		private const double _toolOffsetX = 100;

		private bool _isMouseDown;
		private Point _startPoint;

		#endregion Fields

		#region Constructor

		public DesignDiagramViewModel(
			string name,
			string filePath,
			NodePropertiesView nodeProperties,
			double offsetX)
		{
			DesignDiagram = new ScriptData();
			DesignDiagram.Name = name;
			DesignDiagram.ScriptPath = filePath;

			_nodeProperties = nodeProperties;
			OffsetX = offsetX;

			_isInPropertyChanged = false;

			Nodes = new NodeCollection();
			PageSettings = new PageSettings();
			Connectors = new ConnectorCollection();

			ItemAddedCommand = new RelayCommand<object>(ItemAdded);
			ItemDeletedCommand = new RelayCommand<object>(ItemDeleted);
			ItemSelectedCommand = new RelayCommand<object>(ItemSelected);
			ItemSelectingCommand = new RelayCommand<object>(ItemSelecting);

			SaveDiagramCommand = new RelayCommand(Save);
			OpenDiagramCommand = new RelayCommand(Open);

			SetSnapAndGrid();

			ChangeDarkLight();

			OffsetY = 50;
			AddHeaderNode();

			SelectedItems = new SelectorViewModel();

			//for (int i = 0; i < 500; i++)
			//{
			//	InitNodeBySymbol(null, "ScriptNodeDelay");
			//}

			//Save();

		}

		#endregion Constructor

		#region Methods

		private void AddHeaderNode()
		{
			NodeViewModel node = new NodeViewModel();
			node.Content = DesignDiagram;
			node.ContentTemplate = 
				Application.Current.FindResource("ScriptLogDiagramTemplate_Script") as DataTemplate;
			node.UnitHeight = 50;
			node.UnitWidth = _toolWidth;

			node.OffsetX = 50;
			node.OffsetY = OffsetY;

			OffsetY += 35;

			node.Pivot = new Point(0, 0);

			Nodes.Add(node);
		}

		private void SetSnapAndGrid()
		{
			SnapSettings = new SnapSettings()
			{
				SnapConstraints = SnapConstraints.ShowLines,
				SnapToObject = SnapToObject.All,
			};
		}

		public void Save()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(DesignDiagram, settings);
			File.WriteAllText(DesignDiagram.ScriptPath, sz);
		}

		public async void Open()
		{
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{

				string jsonString = File.ReadAllText(DesignDiagram.ScriptPath);

				JsonSerializerSettings settings = new JsonSerializerSettings();
				settings.Formatting = Formatting.Indented;
				settings.TypeNameHandling = TypeNameHandling.All;
				DesignDiagram = JsonConvert.DeserializeObject(jsonString, settings) as ScriptData;

				foreach (ScriptNodeBase tool in DesignDiagram.ScriptItemsList)
				{
					string toolName = tool.GetType().Name;
					InitNodeBySymbol(null, toolName, tool);

					await Task.Delay(1);
				}
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, $"Failed to load the script \"{Name}\"", "Error", ex);
			}

			Mouse.OverrideCursor = null;
		}

		#region Add item

		private void ItemAdded(object item)
		{
			if (!(item is ItemAddedEventArgs itemAdded))
				return;

			if (itemAdded.Item is NodeViewModel node)
			{
				NodeAdded(itemAdded, node);
			}
			else if (itemAdded.Item is ConnectorViewModel connector)
			{
				if (connector.ID is string str && str.Contains("PassNext_"))
					return;
			}

		}

		private void NodeAdded(
			ItemAddedEventArgs itemAdded,
			NodeViewModel node)
		{
			ItemSelecting(null);

			if (itemAdded.OriginalSource is SymbolViewModel symbol)
			{
				InitNodeBySymbol(node, symbol.Symbol as string);
			}
			else
			{
				if (itemAdded != null && itemAdded.Info != null)
					InitNodeBySymbol(node, (itemAdded.Info as PasteCommandInfo).SourceId as string);
			}
		}

		private void InitNodeBySymbol(
			NodeViewModel node,
			string toolName,
			ScriptNodeBase tool = null)
		{
			if(node == null)
			{
				node = new NodeViewModel();
				node.Content = tool;
				Nodes.Add(node);
			}

			node.ID = toolName;
			node.Pivot = new Point(0, 0);

			if (tool == null)
			{
				SetContent(node, toolName);
			}

			SetNodeTemplateAndSize(node, toolName);
			SetPorts(node);
			

			node.PropertyChanged += Node_PropertyChanged;

			if (tool == null)
			{
				SetNextPassConnector(node);
				DesignDiagram.ScriptItemsList.Add(node.Content as ScriptNodeBase);
			}
		}

		private void SetNextPassConnector(
			NodeViewModel node)
		{
			if (Nodes.Count == 2)
				return;

			NodeViewModel prevLastNode =
				Nodes[Nodes.Count - 2];
			ScriptNodeBase prevLastTool = 
				prevLastNode.Content as ScriptNodeBase;
			if (prevLastTool == null)
				return;

			ScriptNodeBase tool = node.Content as ScriptNodeBase;
			prevLastTool.PassNext = tool;

			ConnectorViewModel simpleConnector = new ConnectorViewModel()
			{
				ID = $"PassNext_{prevLastNode.ID}",
				SourceNode = prevLastNode,
				SourcePort = (prevLastNode.Ports as PortCollection)[1],

				TargetNode = node,
				TargetPort = (node.Ports as PortCollection)[0],

				ConnectorGeometryStyle =
					Application.Current.FindResource("PassConnectorLineStyle") as Style,
				TargetDecorator =
					Application.Current.FindResource("ClosedSharp"),
				TargetDecoratorStyle = Application.Current.FindResource("DecoratorFillStyle") as Style
			};

			Connectors.Add(simpleConnector);

		}

		private void SetPorts(NodeViewModel node)
		{
			NodePortViewModel port = new NodePortViewModel()
			{
				NodeOffsetX = 0,
				NodeOffsetY = 0.25,
			};
			(node.Ports as PortCollection).Add(port);

			port = new NodePortViewModel()
			{
				NodeOffsetX = 0,
				NodeOffsetY = 0.75,
			};
			(node.Ports as PortCollection).Add(port);

			port = new NodePortViewModel()
			{
				NodeOffsetX = 1,
				NodeOffsetY = 0.25,
			};
			(node.Ports as PortCollection).Add(port);

			port = new NodePortViewModel()
			{
				NodeOffsetX = 1,
				NodeOffsetY = 0.75,
			};
			(node.Ports as PortCollection).Add(port);
		}

		private void SetContent(
			NodeViewModel node,
			string toolName)
		{
			switch (toolName)
			{
				case "ScriptNodeCompare":
					node.Content = new ScriptNodeCompare();
					break;
				case "ScriptNodeDelay":
					node.Content = new ScriptNodeDelay();
					break;
				case "ScriptNodeDynamicControl":
					node.Content = new ScriptNodeDynamicControl();
					break;
				case "ScriptNodeSweep":
					node.Content = new ScriptNodeSweep();
					break;
				case "ScriptNodeSetParameter":
					node.Content = new ScriptNodeSetParameter();
					break;
				case "ScriptNodeSetSaveParameter":
					node.Content = new ScriptNodeSetSaveParameter();
					break;
				case "ScriptNodeSaveParameter":
					node.Content = new ScriptNodeSaveParameter();
					break;
				case "ScriptNodeNotification":
					node.Content = new ScriptNodeNotification();
					break;
				case "ScriptNodeSubScript":
					node.Content = new ScriptNodeSubScript();
					break;
				case "ScriptNodeIncrementValue":
					node.Content = new ScriptNodeIncrementValue();
					break;
				case "ScriptNodeLoopIncrement":
					node.Content = new ScriptNodeLoopIncrement();
					break;
				case "ScriptNodeConverge":
					node.Content = new ScriptNodeConverge();
					break;
				case "ScriptNodeCompareRange":
					node.Content = new ScriptNodeCompareRange();
					break;
				case "ScriptNodeCompareWithTolerance":
					node.Content = new ScriptNodeCompareWithTolerance();
					break;
				case "ScriptNodeCANMessage":
					node.Content = new ScriptNodeCANMessage();
					break;
				case "ScriptNodeCANMessageUpdate":
					node.Content = new ScriptNodeCANMessageUpdate();
					break;
				case "ScriptNodeCANMessageStop":
					node.Content = new ScriptNodeCANMessageStop();
					break;
				case "ScriptNodeStopContinuous":
					node.Content = new ScriptNodeStopContinuous();
					break;
				case "ScriptNodeEOLFlash":
					node.Content = new ScriptNodeEOLFlash();
					break;
				case "ScriptNodeEOLCalibrate":
					node.Content = new ScriptNodeEOLCalibrate();
					break;
				case "ScriptNodeEOLSendSN":
					node.Content = new ScriptNodeEOLSendSN();
					break;
				case "ScriptNodeResetParentSweep":
					node.Content = new ScriptNodeResetParentSweep();
					break;
				case "ScriptNodeScopeSave":
					node.Content = new ScriptNodeScopeSave();
					break;
				case "ScriptNodeEOLPrint":
					node.Content = new ScriptNodeEOLPrint();
					break;
				case "ScriptNodeCompareBit":
					node.Content = new ScriptNodeCompareBit();
					break;
			}
		}

		private void SetNodeTemplateAndSize(
			NodeViewModel node,
			string toolName)
		{
			

			node.ContentTemplate = 
				Application.Current.FindResource("ScriptLogDiagramTemplate_Step") as DataTemplate;


			node.UnitHeight = _toolHeight;
			node.UnitWidth = _toolWidth;
			(node.Content as ScriptNodeBase).Height = _toolHeight;
			(node.Content as ScriptNodeBase).Width = _toolWidth;

			node.OffsetX = _toolOffsetX;
			node.OffsetY = OffsetY;
			(node.Content as ScriptNodeBase).OffsetX = _toolOffsetX;
			(node.Content as ScriptNodeBase).OffsetY = OffsetY;
			OffsetY += _betweenTools;
		}

		#endregion Add item

		private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(!(sender is NodeViewModel node))
				return;

			if (!(node.Content is ScriptNodeBase tool))
				return;
			
			if (e.PropertyName == "OffsetX")
			{
				if (_isInPropertyChanged)
					return;

				_isInPropertyChanged = true;
				node.OffsetX = (node.Content as ScriptNodeBase).OffsetX;
				_isInPropertyChanged = false;
			}

			if (e.PropertyName == "OffsetY")
			{
				if (_isInPropertyChanged)
					return;

				_isInPropertyChanged = true;
				node.OffsetY = (node.Content as ScriptNodeBase).OffsetY;
				_isInPropertyChanged = false;
			}

			if (e.PropertyName == "UnitWidth")
			{
				if (_isInPropertyChanged)
					return;

				_isInPropertyChanged = true;
				node.UnitWidth = (node.Content as ScriptNodeBase).Width;
				_isInPropertyChanged = false;
			}

			if (e.PropertyName == "UnitHeight")
			{
				if (_isInPropertyChanged)
					return;

				_isInPropertyChanged = true;
				node.UnitHeight = (node.Content as ScriptNodeBase).Height;
				_isInPropertyChanged = false;
			}
		}

		private void ItemSelected(object e)
		{
			if (!(e is ItemSelectedEventArgs itemSelectedData))
				return;

			if (!(itemSelectedData.Item is NodeViewModel node))
				return;

			if (!(node.Content is ScriptNodeBase toolBase))
				return;

			SetPropertyGridSelectedNode(toolBase);
		}

		private void ItemSelecting(object e)
		{
			SelectorViewModel svm = (SelectedItems as SelectorViewModel);
			svm.SelectorConstraints = svm.SelectorConstraints & ~SelectorConstraints.QuickCommands;
		}

		private void SetPropertyGridSelectedNode(ScriptNodeBase toolBase)
		{
			_nodeProperties.DataContext = toolBase;
		}

		private void ItemDeleted(object item)
		{
			if (!(item is ItemDeletedEventArgs itemDeleted))
				return;

			if (!(itemDeleted.Item is NodeViewModel node))
				return;

			DesignDiagram.ScriptItemsList.Remove(node.Content as ScriptNodeBase);

			ReAragneNodex();
		}

		private void ReAragneNodex()
		{

			OffsetY = 50 + 35;

			foreach (NodeViewModel nodeItem in Nodes)
			{
				if (!(nodeItem.Content is ScriptNodeBase scriptNodeBase))
					continue;

				scriptNodeBase.OffsetX = _toolOffsetX;
				scriptNodeBase.OffsetY = OffsetY;
				nodeItem.OffsetX = _toolOffsetX;
				nodeItem.OffsetY = OffsetY;

				OffsetY += _betweenTools;
			}
		}

		public void ChangeDarkLight()
		{
			PageSettings.PageBackground =
				App.Current.Resources["MahApps.Brushes.ThemeBackground"] as SolidColorBrush;
		}


		#region Drag

		private void Diagram_MouseEnter(MouseEventArgs e)
		{
			if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
				_isMouseDown = true;
			else
				_isMouseDown = false;
		}

		private void Diagram_PreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			_isMouseDown = true;
			_startPoint = e.GetPosition(null);
		}

		private void Diagram_PreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_isMouseDown = false;
		}

		private void Diagram_MouseMove(MouseEventArgs e)
		{
			if (_isMouseDown == false)
				return;

			DragObject(e);
		}

		private void DragObject(MouseEventArgs e)
		{
			try
			{
				Point mousePos = e.GetPosition(null);
				Vector diff = _startPoint - mousePos;

				if (e.LeftButton == MouseButtonState.Pressed &&
					//Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
				{

					Node node = FindAncestorService.FindAncestor<Node>((DependencyObject)e.OriginalSource);
					if (node == null)
						return;

					DataObject dragData = new DataObject("SfDiagram", node);
					DragDrop.DoDragDrop(
						node,
						dragData,
						DragDropEffects.Move);

				}
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to handle dropped item", "Error", ex);
			}
		}

		

		#endregion Drag


		#region Drop

		private void Diagram_Drop(DragEventArgs e)
		{
			Node nodeDropedOn =
					FindAncestorService.FindAncestor<Node>((DependencyObject)e.OriginalSource);

			NodeViewModel nodeVMDropedOn = null;
			if (nodeDropedOn != null)
				nodeVMDropedOn = nodeDropedOn.DataContext as NodeViewModel;


			var dragData = e.Data.GetData("SfDiagram");
			if (!(dragData is Node dragedNode))
				return;

			NodeViewModel dragedNodeVM = null;
			if (dragedNode.DataContext is SelectorViewModel selectorVM)
			{
				dragedNodeVM = selectorVM.SelectedItem as NodeViewModel;
			}
			else if (dragedNode.DataContext is NodeViewModel)
			{
				dragedNodeVM = (NodeViewModel)dragedNode.DataContext;
			}
			else if(dragedNodeVM == null)
			{
				return;
			}

			NodeViewModel temp = dragedNodeVM;
			int dragedIndex = Nodes.IndexOf(dragedNodeVM);
			Nodes.RemoveAt(dragedIndex);

			int dropedOnIndex = Nodes.IndexOf(nodeVMDropedOn);
			if (dropedOnIndex < 0)
			{
				Nodes.Add(temp);
			}
			else
			{
				Nodes.Insert(dropedOnIndex, dragedNodeVM);
			}

			ReAragneNodex();
		}


		private void Diagram_DragEnter(DragEventArgs e)
		{
			//if (!e.Data.GetDataPresent(DeviceHandler.ViewModel.ParametersViewModel.DragDropFormat))
			//{
			//	e.Effects = DragDropEffects.None;
			//}
		}

		#endregion Drop		

		#endregion Methods

		#region Commands

		public RelayCommand<object> ItemAddedCommand { get; private set; }
		public RelayCommand<object> ItemDeletedCommand { get; private set; }
		public RelayCommand<object> ItemSelectedCommand { get; private set; }
		public RelayCommand<object> ItemSelectingCommand { get; private set; }


		public RelayCommand SaveDiagramCommand { get; private set; }
		public RelayCommand OpenDiagramCommand { get; private set; }

		public RelayCommand CopyCommand { get; private set; }
		public RelayCommand PastCommand { get; private set; }
		public RelayCommand SaveCommand { get; private set; }


		#region Drag

		private RelayCommand<MouseEventArgs> _Diagram_MouseEnterCommand;
		public RelayCommand<MouseEventArgs> Diagram_MouseEnterCommand
		{
			get
			{
				return _Diagram_MouseEnterCommand ?? (_Diagram_MouseEnterCommand =
					new RelayCommand<MouseEventArgs>(Diagram_MouseEnter));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _Diagram_PreviewMouseLeftButtonDownCommant;
		public RelayCommand<MouseButtonEventArgs> Diagram_PreviewMouseLeftButtonDownCommant
		{
			get
			{
				return _Diagram_PreviewMouseLeftButtonDownCommant ?? (_Diagram_PreviewMouseLeftButtonDownCommant =
					new RelayCommand<MouseButtonEventArgs>(Diagram_PreviewMouseLeftButtonDown));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _Diagram_PreviewMouseLeftButtonUpCommant;
		public RelayCommand<MouseButtonEventArgs> Diagram_PreviewMouseLeftButtonUpCommant
		{
			get
			{
				return _Diagram_PreviewMouseLeftButtonUpCommant ?? (_Diagram_PreviewMouseLeftButtonUpCommant =
					new RelayCommand<MouseButtonEventArgs>(Diagram_PreviewMouseLeftButtonUp));
			}
		}

		private RelayCommand<MouseEventArgs> _Diagram_MouseMoveCommand;
		public RelayCommand<MouseEventArgs> Diagram_MouseMoveCommand
		{
			get
			{
				return _Diagram_MouseMoveCommand ?? (_Diagram_MouseMoveCommand =
					new RelayCommand<MouseEventArgs>(Diagram_MouseMove));
			}
		}





		#endregion Drag


		#region Drop

		private RelayCommand<DragEventArgs> _Diagram_DropCommand;
		public RelayCommand<DragEventArgs> Diagram_DropCommand
		{
			get
			{
				return _Diagram_DropCommand ?? (_Diagram_DropCommand =
					new RelayCommand<DragEventArgs>(Diagram_Drop));
			}
		}

		private RelayCommand<DragEventArgs> _Diagram_DragEnterCommand;
		public RelayCommand<DragEventArgs> Diagram_DragEnterCommand
		{
			get
			{
				return _Diagram_DragEnterCommand ?? (_Diagram_DragEnterCommand =
					new RelayCommand<DragEventArgs>(Diagram_DragEnter));
			}
		}

		#endregion Drop

		#endregion Commands
	}
}
