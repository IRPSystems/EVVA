
using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Enums;
using Newtonsoft.Json;
using ScriptHandler.Models;
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

		public ObservableCollection<DeviceTypesEnum> SetupDevicesList { get; set; }

		public string LastSetupPath { get; set; }

		public int AcquisitionRate { get; set; }

		public ScriptUserData ScriptUserData { get; set; }

		public ObservableCollection<FaultData> FaultsMCUList { get; set; }

		public EvvaUserData()
		{
			AcquisitionRate = 5;
		}


		public static EvvaUserData LoadEvvaUserData(string dirName)
		{
			EvvaUserData EvvaUserData = null;

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
			EvvaUserData = JsonConvert.DeserializeObject(jsonString, settings) as EvvaUserData;
			
			if(EvvaUserData.ScriptUserData == null)
				EvvaUserData.ScriptUserData = new ScriptHandler.Models.ScriptUserData();

			if (File.Exists(EvvaUserData.DynoCommunicationPath) == false)
				EvvaUserData.DynoCommunicationPath = "Data\\Device Communications\\Dyno Communication.json";
			if (File.Exists(EvvaUserData.MCUJsonPath) == false)
				EvvaUserData.MCUJsonPath = "Data\\Device Communications\\param_defaults.json";
			if (File.Exists(EvvaUserData.MCUB2BJsonPath) == false)
				EvvaUserData.MCUB2BJsonPath = "Data\\Device Communications\\param_defaults.json";
			if (File.Exists(EvvaUserData.NI6002CommunicationPath) == false)
				EvvaUserData.NI6002CommunicationPath = "Data\\Device Communications\\NI_6002.json";


			return EvvaUserData;
		}



		public static void SaveEvvaUserData(
			string dirName,
			EvvaUserData EvvaUserData)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
				return;
			path = Path.Combine(path, "EvvaUserData.json");

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(EvvaUserData, settings);
			File.WriteAllText(path, sz);
		}
	}
}
