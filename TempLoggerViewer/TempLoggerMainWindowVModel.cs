
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Enums;
using DeviceCommunicators.Interfaces;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceHandler.ViewModels;
using Entities.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TempLoggerViewer.ViewModels;

namespace TempLoggerViewer
{
	public class TempLoggerMainWindowVModel: ObservableObject
	{
		#region Properties

		public DevicesContainer DevicesContainter { get; set; }
		public DocingTempLoggerViewModel Docking { get; set; }

		public string Version { get; set; }

		#endregion Properties

		#region Fields

		protected CancellationTokenSource _cancellationTokenSource;
		protected CancellationToken _cancellationToken;

		#endregion Fields

		#region Constructor

		public TempLoggerMainWindowVModel()
		{
			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			InitDevicesContainter();

			CommunicationViewModel communicationSettings =
				new CommunicationViewModel(DevicesContainter);
			Docking = new DocingTempLoggerViewModel(communicationSettings);
			CommunicationSettingsCommand = new RelayCommand(InitCommunicationSettings);
			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);

			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;
		}

		#endregion Constructor

		#region Methods

		private void Closing(CancelEventArgs e)
		{
			if(_cancellationTokenSource != null)
				_cancellationTokenSource.Cancel();
		}

		private void InitDevicesContainter()
		{
			DevicesContainter = new DevicesContainer();
			DevicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
			DevicesContainter.DevicesList = new ObservableCollection<DeviceData>();
			DevicesContainter.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();

			string path = Directory.GetCurrentDirectory();
			ReadDevicesFileService readDevicesFile = new ReadDevicesFileService();
			ObservableCollection<DeviceData> deviceList = readDevicesFile.ReadAllFiles(
				path,
				null,
				null,
				null,
				null);

			foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
			{
				device.Disconnect();
			}

			DevicesContainter.DevicesFullDataList.Clear();
			DevicesContainter.DevicesList.Clear();
			DevicesContainter.TypeToDevicesFullData.Clear();


			foreach (DeviceData device in deviceList)
			{
				DeviceFullData deviceFullData = DeviceFullData.Factory(device);

				deviceFullData.Init();

				DevicesContainter.DevicesFullDataList.Add(deviceFullData);
				DevicesContainter.DevicesList.Add(device);
				if (DevicesContainter.TypeToDevicesFullData.ContainsKey(device.DeviceType) == false)
					DevicesContainter.TypeToDevicesFullData.Add(device.DeviceType, deviceFullData);
			}

			foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
			{
				device.Connect();
				device.InitCheckConnection();
				ReadData(device);
			}
		}

		private void InitCommunicationSettings()
		{
			Docking.OpenCommSettings();
		}

		protected void ReadData(DeviceFullData deviceFullData)
		{
			IDataLoggerCommunicator dataLoggerCommunicator =
				deviceFullData.DeviceCommunicator as IDataLoggerCommunicator;
			if (dataLoggerCommunicator == null)
				return;

			Task.Run(() =>
			{
				while (!_cancellationToken.IsCancellationRequested)
				{
					for(int i = 0; i < dataLoggerCommunicator.NumberOfChannels; i++)
					{
						deviceFullData.DeviceCommunicator.GetParamValue(
							deviceFullData.Device.ParemetersList[i],
							Callback);

						System.Threading.Thread.Sleep(1);
					}

					System.Threading.Thread.Sleep(1);
				}
			}, _cancellationToken);
		}

		private void Callback(DeviceParameterData param, CommunicatorResultEnum result, string errorDescription)
		{

		}

		#endregion Methods

				#region Commands

		public RelayCommand CommunicationSettingsCommand { get; private set; }
		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }

		#endregion Commands
	}
}
