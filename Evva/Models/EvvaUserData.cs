
using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Theming;
using Entities.Enums;
using Newtonsoft.Json;
using ScriptHandler.Models;
using Services.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Evva.Models
{
	public class EvvaUserData: ObservableObject
	{
		public bool IsLightTheme { get; set; }

		public string MCUJsonPath { get; set; }
		public string MCUB2BJsonPath { get; set; }
		public string DynoCommunicationPath { get; set; }
		public string NI6002CommunicationPath { get; set; }

		public string YokoConfigFilePath { get; set; }

		public ObservableCollection<DeviceTypesEnum> SetupDevicesList { get; set; }

		public string LastSetupPath { get; set; }
		public string LastParamsDBCPath { get; set; }

		public int AcquisitionRate { get; set; }

		public ScriptUserData ScriptUserData { get; set; }

		public ObservableCollection<FaultData> FaultsMCUList { get; set; }

		public bool IsEAPSRampupEnable { get; set; }



		public static EvvaUserData LoadEvvaUserData(string dirName)
		{

			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
			{
				return new EvvaUserData();
			}
			path = Path.Combine(path, "EvvaUserData.json");
			if (File.Exists(path) == false)
			{
				return new EvvaUserData();
			}


			string jsonString = File.ReadAllText(path);
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			EvvaUserData evvaUserData = JsonConvert.DeserializeObject(jsonString, settings) as EvvaUserData;
			if (evvaUserData == null)
				return evvaUserData;
			
			if(evvaUserData.ScriptUserData == null)
				evvaUserData.ScriptUserData = new ScriptHandler.Models.ScriptUserData();

			string errorDesc = string.Empty;
			if (File.Exists(evvaUserData.DynoCommunicationPath) == false)
			{
				errorDesc += "The path \"" + evvaUserData.DynoCommunicationPath + "\" was not found.\r\n\r\n";
				evvaUserData.DynoCommunicationPath = "Data\\Device Communications\\Dyno Communication.json";
			}
			if (File.Exists(evvaUserData.MCUJsonPath) == false)
			{
				errorDesc += "The path \"" + evvaUserData.MCUJsonPath + "\" was not found.\r\n\r\n";
				evvaUserData.MCUJsonPath = "Data\\Device Communications\\param_defaults.json";
			}
			if (File.Exists(evvaUserData.MCUB2BJsonPath) == false)
			{
				errorDesc += "The path \"" + evvaUserData.MCUB2BJsonPath + "\" was not found.\r\n\r\n";
				evvaUserData.MCUB2BJsonPath = "Data\\Device Communications\\param_defaults.json";
			}
			if (File.Exists(evvaUserData.NI6002CommunicationPath) == false)
			{
				errorDesc += "The path \"" + evvaUserData.NI6002CommunicationPath + "\" was not found.\r\n\r\n";
				evvaUserData.NI6002CommunicationPath = "Data\\Device Communications\\NI_6002.json";
			}

			if (string.IsNullOrEmpty(errorDesc) == false)
			{
				errorDesc += "The default paths will be used";
				LoggerService.Error(evvaUserData, errorDesc, "Error");
			}


			return evvaUserData;
		}



		public static void SaveEvvaUserData(
			string dirName,
			EvvaUserData EvvaUserData)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);
			path = Path.Combine(path, "EvvaUserData.json");

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(EvvaUserData, settings);
			File.WriteAllText(path, sz);
		}
	}
}
