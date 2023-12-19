
using Entities.Models;
using System.Collections.ObjectModel;
using DeviceHandler.Models;
using Newtonsoft.Json;
using DeviceCommunicators.Models;

namespace Evva.ViewModels
{
	public class Record_SelectedParametersListViewModel : SelectedParametersListViewModel
	{
		
		#region Fields


		private const int _maxLoggingParams = 40;

		#endregion Fields

		#region Constructor

		public Record_SelectedParametersListViewModel(
			DevicesContainer devicesContainer,
			string title) :
			base(devicesContainer, title) 
		{
			_limitOfParametersList = _maxLoggingParams;
			IsLimitParametersList = true;
			GetLogParamListFromFile();
		}

		#endregion Constructor

		#region Methods


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

		

		#endregion Methods

	}
}
