using Root_VEGA_P.Engineer;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Root_VEGA_P
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        VEGA_P_Engineer m_engineer = new VEGA_P_Engineer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\VEGA_P")) Directory.CreateDirectory(@"C:\Recipe\VEGA_P");
            m_engineer.Init("VEGA_P");
            engineerUI.Init(m_engineer);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }

		#region Title Bar
		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}
		private void MaximizeButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Maximized;
			NormalizeButton.Visibility = Visibility.Visible;
			MaximizeButton.Visibility = Visibility.Collapsed;
		}
		private void NormalizeButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Normal;
			MaximizeButton.Visibility = Visibility.Visible;
			NormalizeButton.Visibility = Visibility.Collapsed;
		}
		private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				if (this.WindowState == WindowState.Maximized)
				{
					this.WindowState = WindowState.Normal;
					MaximizeButton.Visibility = Visibility.Visible;
					NormalizeButton.Visibility = Visibility.Collapsed;
				}
				else
				{
					this.WindowState = WindowState.Maximized;
					NormalizeButton.Visibility = Visibility.Visible;
					MaximizeButton.Visibility = Visibility.Collapsed;
				}
			}
			else
			{
				this.DragMove();
			}
		}
		#endregion

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
