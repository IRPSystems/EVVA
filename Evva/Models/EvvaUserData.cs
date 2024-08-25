
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceHandler.Models;
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

		public DeviceSetupUserData DeviceSetupUserData { get; set; }

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

			if(evvaUserData.DeviceSetupUserData == null)
				evvaUserData.DeviceSetupUserData = new DeviceSetupUserData();

			string errorDesc = string.Empty;
			if (File.Exists(evvaUserData.DeviceSetupUserData.DynoCommunicationPath) == false)
			{
				errorDesc += "The path \"" + evvaUserData.DeviceSetupUserData.DynoCommunicationPath + "\" was not found.\r\n\r\n";
				evvaUserData.DeviceSetupUserData.DynoCommunicationPath = "Data\\Device Communications\\Dyno Communication.json";
			}
			if (File.Exists(evvaUserData.DeviceSetupUserData.MCUJsonPath) == false)
			{
				errorDesc += "The path \"" + evvaUserData.DeviceSetupUserData.MCUJsonPath + "\" was not found.\r\n\r\n";
				evvaUserData.DeviceSetupUserData.MCUJsonPath = "Data\\Device Communications\\param_defaults.json";
			}
			if (File.Exists(evvaUserData.DeviceSetupUserData.MCUB2BJsonPath) == false)
			{
				errorDesc += "The path \"" + evvaUserData.DeviceSetupUserData.MCUB2BJsonPath + "\" was not found.\r\n\r\n";
				evvaUserData.DeviceSetupUserData.MCUB2BJsonPath = "Data\\Device Communications\\param_defaults.json";
			}
			if (File.Exists(evvaUserData.DeviceSetupUserData.NI6002CommunicationPath) == false)
			{
				errorDesc += "The path \"" + evvaUserData.DeviceSetupUserData.NI6002CommunicationPath + "\" was not found.\r\n\r\n";
				evvaUserData.DeviceSetupUserData.NI6002CommunicationPath = "Data\\Device Communications\\NI_6002.json";
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
