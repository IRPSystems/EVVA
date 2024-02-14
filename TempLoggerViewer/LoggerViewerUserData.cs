
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.IO;

namespace TempLoggerViewer.Models
{
	public class LoggerViewerUserData : ObservableObject
	{

		public string RecordingDirectory { get; set; }
		public string ChannelsNameDirectory { get; set; }



		public static LoggerViewerUserData LoadLoggerViewerData(string dirName)
		{
			LoggerViewerUserData loggerViewerUserData = null;

			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
			{
				return new LoggerViewerUserData();
			}
			path = Path.Combine(path, "LoggerViewerData.json");
			if (File.Exists(path) == false)
			{
				return new LoggerViewerUserData();
			}


			string jsonString = File.ReadAllText(path);
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			loggerViewerUserData = JsonConvert.DeserializeObject(jsonString, settings) as LoggerViewerUserData;
			
			

			return loggerViewerUserData;
		}



		public static void SaveLoggerViewerData(
			string dirName,
			LoggerViewerUserData loggerViewerUserData)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);
			path = Path.Combine(path, "LoggerViewerData.json");

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(loggerViewerUserData, settings);
			File.WriteAllText(path, sz);
		}
	}
}
