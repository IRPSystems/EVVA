using System.Windows.Controls;

namespace Evva.Views
{
	/// <summary>
	/// Interaction logic for ScriptLogView.xaml
	/// </summary>
	public partial class ScriptLogView : UserControl
	{
		public ScriptLogView()
		{
			InitializeComponent();
		}

		private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			dg.ScrollIntoView(e.Row.Item);
		}
	}
}
