using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace RootTools.Control.ACS
{
    /// <summary>
    /// ACS_Buffer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ACS_Buffer_UI : UserControl
    {
        public ACS_Buffer_UI()
        {
            InitializeComponent();
        }

        ACS.Buffer m_buffer;
        public void Init(ACS.Buffer buffer)
        {
            m_buffer = buffer;
            DataContext = buffer;
            InitTimer(); 
        }

        #region Button
        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            m_buffer.Run(); 
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            m_buffer.Stop(); 
        }

        void InvalidateButton(bool bRun)
        {
            buttonRun.IsEnabled = !bRun;
            buttonStop.IsEnabled = bRun;
            buttonRun.Foreground = bRun ? Brushes.Red : Brushes.Black;
            buttonStop.Foreground = bRun ? Brushes.Black : Brushes.Red;
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
            InvalidateButton(m_buffer.m_bRun); 
        }
        #endregion
    }
}
