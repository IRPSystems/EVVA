using MahApps.Metro.Controls;
using ScriptHandler.Models;
using ScriptHandler.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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

namespace TempLoggerViewer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class TempLoggerMainWindow : MetroWindow
	{
		public TempLoggerMainWindow()
		{
			InitializeComponent();			

			DataContext = new TempLoggerMainWindowVModel();
		}
	}
}
