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

namespace Evva.Views
{
	/// <summary>
	/// Interaction logic for TestsView.xaml
	/// </summary>
	public partial class TestsView : UserControl
	{
		public TestsView()
		{
			InitializeComponent();
		}

		private void DataGridKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
			}
		}
    }
}
