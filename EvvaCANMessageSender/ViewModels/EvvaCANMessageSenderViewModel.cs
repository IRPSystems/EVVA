
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Services;
using DeviceHandler.Interfaces;
using DeviceHandler.Models;
using Entities.Enums;
using Entities.Models;
using ScriptHandler.Models;
using System.Collections.ObjectModel;
using System;
using System.Windows;
using Communication;
using Newtonsoft.Json;
using System.Threading.Tasks;
using EvvaCANMessageSender.Models;
using DeviceHandler.Enums;
using DeviceCommunicators.Enums;
using DeviceCommunicators.MCU;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Threading;
using DeviceSimulators.ViewModels;
using DeviceHandler.ViewModels;
using DeviceCommunicators.Models;
using DeviceHandler.Models.DeviceFullDataModels;

namespace EvvaCANMessageSender.ViewModels
{
    public class EvvaCANMessageSenderViewModel: ObservableObject
    {
		public enum CANMessageForSenderStateEnum { None, Sending, Updated, Stopped }

		public class CANMessageForSenderData: ObservableObject
		{
			public ScriptStepCANMessage Message { get; set; }
			public CANMessageForSenderStateEnum State { get; set; }
		}

		#region Properties

		public IConnectionViewModel CanConnect { get; set; }

		public ObservableCollection<CANMessageForSenderData> CANMessagesList { get; set; }

		public bool IsNamedPipeConnected { get; set; }

		public DeviceFullData MCUDevice { get; set; }
		public EvvaCommunicationData EVVACommState { get; set; }

		public Visibility MCUSimulatorVisibility { get; set; }

		public MCUSimulatorMainWindowViewModel MCUSimulatorVM { get; set; }

		public ObservableCollection<string> StatusList { get; set; }

		#endregion Properties

		#region Fields


		private NamedPipeListnerService _namedPipeListner;

		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;


		#endregion Fields

		#region Constructor

		public EvvaCANMessageSenderViewModel()
        {
			try
			{
				ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
				MCUSimulatorCommand = new RelayCommand(MCUSimulator);

				StatusList = new ObservableCollection<string>();

				MCUSimulatorVisibility = Visibility.Collapsed;
				MCUSimulatorVM = null;

				_cancellationTokenSource = new CancellationTokenSource();
				_cancellationToken = _cancellationTokenSource.Token;

				InitMCUCommunication();

				EVVACommState = new EvvaCommunicationData()
				{
					Device = new CommDeviceData() { Name = "EVVA"},
				};

				Task.Run(() =>
				{
					_namedPipeListner = new NamedPipeListnerService();
					_namedPipeListner.MessageReceivedEvent += MessageReceivedEventHandler;
					_namedPipeListner.Init("CANMessage");

					EVVACommState.CommState = CommunicationStateEnum.Connected;
				}, _cancellationToken);

				CANMessagesList = new ObservableCollection<CANMessageForSenderData>();
			}
			catch (Exception ex) 
			{
				MessageBox.Show("Failed to init\r\n\r\n" + ex);
			}
		}

		#endregion Constructor

		#region Methods

		private void InitMCUCommunication()
		{
			ReadDevicesFileService reader = new ReadDevicesFileService();
			ObservableCollection<DeviceData> devicesList = new ObservableCollection<DeviceData>();
			reader.ReadFromMCUJson(
				"param_defaults.json",
				devicesList,
				"MCU",
				DeviceTypesEnum.MCU);

			MCUDevice = DeviceFullData.Factory(devicesList[0]);
			MCUDevice.Init("Evva");
			MCUDevice.Connect();
			CanConnect = MCUDevice.ConnectionViewModel;

			MCUDevice.CommState = CommunicationStateEnum.Initiated;
			DeviceParameterData param = new MCU_ParamData()
			{
				Cmd = "",
				Name = "Check MCU Comm",
			};

			//Task.Run(() =>
			//{
			//	while (MCUDevice.CommState != CommunicationStateEnum.Connected &&
			//			!_cancellationToken.IsCancellationRequested)
			//	{
			//		MCUDevice.DeviceCommunicator.GetParamValue(param, GetValueCallback);
			//		System.Threading.Thread.Sleep(1);
			//	}
			//}, _cancellationToken);
		}

		private void GetValueCallback(DeviceParameterData param, CommunicatorResultEnum result, string resultDescription)
		{
			if(result == CommunicatorResultEnum.OK)
				MCUDevice.CommState = CommunicationStateEnum.Connected;
		}


		private void Closing(CancelEventArgs e)
		{
			_cancellationTokenSource.Cancel();

			_namedPipeListner.Dispose();
			MCUDevice.Disconnect();
		}


		private void MessageReceivedEventHandler(string messageStr)
		{
			if (string.IsNullOrEmpty(messageStr))
				return;

			if (Application.Current == null)
				return;

			Application.Current.Dispatcher.Invoke(() =>
			{

				if (messageStr == "Clear")
				{
					ClearCANMessageList();
					return;
				}

				try
				{

					JsonSerializerSettings settings = new JsonSerializerSettings();
					settings.Formatting = Formatting.Indented;
					settings.TypeNameHandling = TypeNameHandling.All;
					ScriptStepBase baseStep =
						JsonConvert.DeserializeObject(messageStr, settings) as ScriptStepBase;

					if (baseStep is ScriptStepCANMessage canMessage)
					{
						StatusList.Add("Received CAN Message - ID: " + canMessage.NodeId);
						HandleCANMessage(canMessage);
					}
					else if (baseStep is ScriptStepCANMessageUpdate update)
					{
						StatusList.Add("Received Update - ID: " + update.CANID);
						HandleCANMessageUpdate(update);
					}
					else if (baseStep is ScriptStepCANMessageStop stop)
					{
						StatusList.Add("Received Stop - ID: " + stop.CANID);
						HandleCANMessageStop(stop);
					}

				}
				catch (Exception ex)
				{
					MessageBox.Show("Failed handling received message\r\n\r\n" + ex, "Error");
				}
			});
		}


		private void HandleCANMessage(ScriptStepCANMessage canMessage)
		{
			bool isCANMessageExist = IsCANMessageExist(canMessage.NodeId);
			if (isCANMessageExist)
			{
				return;
			}

			CANMessageForSenderData data = new CANMessageForSenderData()
			{
				Message = canMessage,
				State = CANMessageForSenderStateEnum.Sending,
			};
			CANMessagesList.Add(data);

			canMessage.Communicator = MCUDevice.DeviceCommunicator;
			canMessage.Execute();
		}

		private void HandleCANMessageUpdate(ScriptStepCANMessageUpdate update)
		{
			foreach (CANMessageForSenderData data in CANMessagesList)
			{
				if (data.Message == null)
					continue;

				if (data.Message.NodeId == update.CANID)
				{
					data.State = CANMessageForSenderStateEnum.Updated;

					update.StepToUpdate = data.Message;
					update.Execute();
				}
			}
		}

		private void HandleCANMessageStop(ScriptStepCANMessageStop stop)
		{
			foreach (CANMessageForSenderData data in CANMessagesList)
			{
				if (data.Message == null)
					continue;

				if (data.Message.NodeId == stop.CANID)
				{
					data.State = CANMessageForSenderStateEnum.Stopped;

					stop.StepToStop = data.Message;
					stop.Execute();
				}
			}
		}


		private void ClearCANMessageList()
		{
			foreach (CANMessageForSenderData data in CANMessagesList)
			{
				if (data.Message == null)
					continue;

				data.Message.StopContinuous();
			}

			CANMessagesList.Clear();
		}

		private void MCUSimulator()
		{
			if(MCUSimulatorVisibility == Visibility.Visible)
			{
				MCUSimulatorVisibility = Visibility.Collapsed;
				if(MCUSimulatorVM != null)
				{
					MCUSimulatorVM.Dispose();
					MCUSimulatorVM = null;
				}
			}
			else
			{
				MCUSimulatorVM = new MCUSimulatorMainWindowViewModel(MCUDevice.Device);

				(MCUSimulatorVM.ConnectVM as CanConnectViewModel).SelectedAdapter = "UDP Simulator";
				(MCUSimulatorVM.ConnectVM as CanConnectViewModel).RxPort = (CanConnect as CanConnectViewModel).TxPort;
				(MCUSimulatorVM.ConnectVM as CanConnectViewModel).TxPort = (CanConnect as CanConnectViewModel).RxPort;				

				MCUSimulatorVisibility = Visibility.Visible;
			}
		}


		private bool IsCANMessageExist(uint id)
		{
			foreach(CANMessageForSenderData data in CANMessagesList)
			{
				if (data.Message == null) 
					continue;

				if(data.Message.NodeId == id)
					return true;
			}

			return false;
		}

		#endregion Methods

		#region Commands

		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }
		public RelayCommand MCUSimulatorCommand { get; private set; }

		#endregion Commands
	}
}
