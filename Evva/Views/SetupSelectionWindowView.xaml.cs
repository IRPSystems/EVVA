using Evva.ViewModels;
using MahApps.Metro.Controls;

namespace Evva.Views
{
	/// <summary>
	/// Interaction logic for SetupSelectionView.xaml
	/// </summary>
	public partial class SetupSelectionWindowView : MetroWindow
	{
		private SetupSelectionViewModel _vm;
		public SetupSelectionWindowView()
		{
			InitializeComponent();
		}

		public void SetDataContext(SetupSelectionViewModel vm)
		{
			_vm = vm;
			DataContext = vm;
			vm.CloseOKEvent += OK_Click;
			vm.CloseCancelEvent += Cancel_Click;
		}

		private void Cancel_Click()
		{
			DialogResult = false;
			Close();

			_vm.CloseOKEvent -= OK_Click;
			_vm.CloseCancelEvent -= Cancel_Click;
		}

		private void OK_Click()
		{
			DialogResult = true;
			Close();

			_vm.CloseOKEvent -= OK_Click;
			_vm.CloseCancelEvent -= Cancel_Click;
		}
	}
}
