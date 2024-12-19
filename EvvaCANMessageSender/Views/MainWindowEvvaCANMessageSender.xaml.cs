using EvvaCANMessageSender.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EvvaCANMessageSender.Views
{
	/// <summary>
	/// Interaction logic for MainWindowEvvaCANMessageSender.xaml
	/// </summary>
	public partial class MainWindowEvvaCANMessageSender : MetroWindow
	{
		public MainWindowEvvaCANMessageSender()
		{
			InitializeComponent();

			DataContext = new EvvaCANMessageSenderViewModel();
		}
	}
}
