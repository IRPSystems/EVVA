
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceHandler.Enums;

namespace EvvaCANMessageSender.Models
{
    public class CommDeviceData: ObservableObject
    {
        public string Name { get; set; }
    }

    public class EvvaCommunicationData: ObservableObject
    {
        public CommDeviceData Device { get; set; }
        public CommunicationStateEnum CommState { get; set; }
	}
}
