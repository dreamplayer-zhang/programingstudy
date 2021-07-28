using Root_WindII.Engineer;
using RootTools;
using RootTools_Vision;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Root_WindII
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

			GlobalObjects.Instance.Register<DialogService>(this);

		}

        //WindII_Engineer m_engineer = new WindII_Engineer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
			//if (!Directory.Exists(@"C:\Recipe\Wind2")) Directory.CreateDirectory(@"C:\Recipe\Wind2");
			//m_engineer.Init("Wind2");
			//engineerUI.Init(m_engineer);
			InitTimer();
		}

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            GlobalObjects.Instance.Get<WindII_Engineer>().ThreadStop();
        }
			
		DispatcherTimer m_timer = new DispatcherTimer();
		
		void InitTimer()
		{
			m_timer.Interval = TimeSpan.FromMilliseconds(20);
			m_timer.Tick += M_timer_Tick;
			m_timer.Start();
		}

		private void M_timer_Tick(object sender, EventArgs e)
		{
			string strControlState = "NULL";

			if (GlobalObjects.Instance.Get<WindII_Engineer>().ClassGem() != null)
				strControlState = GlobalObjects.Instance.Get<WindII_Engineer>().ClassGem().p_eControl.ToString();
			
			if (strControlState == "NULL")
			{
				strControlState = "OFFLINE";
			}
			GemControlState.Text = strControlState;
		}
	}
}
