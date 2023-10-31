using Evva.ViewModels;
using MahApps.Metro.Controls;
using System.Windows.Media.Imaging;
using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Evva
{
	/// <summary>
	/// Interaction logic for TestStudioMainWindow.xaml
	/// </summary>
	public partial class TestStudioMainWindow : MetroWindow
    {
		private bool mRestoreIfMove = false;

		public TestStudioMainWindow()
        {
            InitializeComponent();

            DataContext = new TestStudioMainWindowViewModel();
		}

		private void Border_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			DragMove();
		}

		private void SwitchWindowState()
		{
			switch (WindowState)
			{
				case WindowState.Normal:
					{
						WindowState = WindowState.Maximized;
						break;
					}
				case WindowState.Maximized:
					{
						WindowState = WindowState.Normal;
						break;
					}
			}
		}


		private void rctHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				if ((ResizeMode == ResizeMode.CanResize) || (ResizeMode == ResizeMode.CanResizeWithGrip))
				{
					SwitchWindowState();
				}

				return;
			}

			else if (WindowState == WindowState.Maximized)
			{
				mRestoreIfMove = true;
				return;
			}

			DragMove();
		}


		private void rctHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			mRestoreIfMove = false;
		}


		private void rctHeader_MouseMove(object sender, MouseEventArgs e)
		{
			if (mRestoreIfMove)
			{
				mRestoreIfMove = false;

				double percentHorizontal = e.GetPosition(this).X / ActualWidth;
				double targetHorizontal = RestoreBounds.Width * percentHorizontal;

				double percentVertical = e.GetPosition(this).Y / ActualHeight;
				double targetVertical = RestoreBounds.Height * percentVertical;				

				POINT lMousePosition;
				GetCursorPos(out lMousePosition);

				Left = lMousePosition.X - targetHorizontal;
				Top = lMousePosition.Y - targetVertical;

				WindowState = WindowState.Normal;
				DragMove();
			}
		}



		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetCursorPos(out POINT lpPoint);


		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public POINT(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}
		}

	}
}
