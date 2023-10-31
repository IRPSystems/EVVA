
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace Evva.Models
{
	public class FaultData: ObservableObject
	{
		public int Bit { get; set; }
		public string Description { get; set; }
		public string Opcode { get; set; }

		public bool? State { get; set; }

		public bool IsVisible { get; set; }
	}
}
