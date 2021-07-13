using Root_VEGA_P.Engineer;
using RootTools;
using RootTools.Memory;
using RootTools_Vision;
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

			if (!Directory.Exists(@"C:\Recipe\VEGA_P")) Directory.CreateDirectory(@"C:\Recipe\VEGA_P");

			m_engineer = new VEGA_P_Engineer();
			m_engineer = GlobalObjects.Instance.Register<VEGA_P_Engineer>();
			m_engineer.Init("VEGA_P");

			logView.Init(LogView._logView);
			DataContext = new MainVM(this);
		}

		VEGA_P_Engineer m_engineer;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
			InitMemory();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }

		void InitMemory()
		{
			MemoryTool memoryTool = m_engineer.ClassMemoryTool();
			MemoryGroup memoryGroup = memoryTool.GetPool(App.mPool).GetGroup(App.mGroup);
			memoryGroup.CreateMemory(App.mDome, 1, 1, 1000, 1000);
			memoryGroup.CreateMemory(App.mDoor, 1, 1, 1000, 1000);

			ImageData img = GlobalObjects.Instance.RegisterNamed<ImageData>(App.mDome, memoryTool.GetMemory(App.mPool, App.mGroup, App.mDome));
			MemoryData memoryData =memoryTool.GetMemory(App.mPool, App.mGroup, App.mDoor);
			if(img.m_MemData!=null)
            {
				img.p_nByte = memoryData.p_nByte;
				img.p_nPlane = memoryData.p_nCount;
            }

			img = GlobalObjects.Instance.RegisterNamed<ImageData>(App.mDoor, memoryTool.GetMemory(App.mPool, App.mGroup, App.mDoor));
			memoryData = memoryTool.GetMemory(App.mPool, App.mGroup, App.mDoor);

			if (img.m_MemData != null)
			{
				img.p_nByte = memoryData.p_nByte;
				img.p_nPlane = memoryData.p_nCount;
			}
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
