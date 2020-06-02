using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Root_Siltron
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

        #region Window Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
        }
        #endregion

        Siltron_Engineer m_engineer = new Siltron_Engineer();
        void Init()
        {
            m_engineer.Init("Siltron");
            engineerUI.Init(m_engineer); 
            InitTimer();
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }
    }
}
