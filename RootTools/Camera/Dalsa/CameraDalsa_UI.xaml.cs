using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.Camera.Dalsa
{
    /// <summary>
    /// CameraDalsa_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CameraDalsa_UI : UserControl
    {
        public CameraDalsa_UI()
        {
            InitializeComponent();
        }

        CameraDalsa m_cameraDalsa;
        public void Init(CameraDalsa cameraDalsa)
        {
            m_cameraDalsa = cameraDalsa;
            DataContext = cameraDalsa;
            treeUI.Init(cameraDalsa.p_treeRoot);

            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, System.EventArgs e)
        {
            bool bCanGrab = m_cameraDalsa.p_bConnect && (m_cameraDalsa.m_sapXfer.Grabbing == false);
            buttonGrab.IsEnabled = bCanGrab;
            buttonLive.IsEnabled = bCanGrab;
            buttonStop.IsEnabled = m_cameraDalsa.p_bConnect && m_cameraDalsa.m_sapXfer.Grabbing; 
        }
        #endregion

        #region Grab
        private void buttonGrab_Click(object sender, RoutedEventArgs e)
        {
            m_cameraDalsa.p_sInfo = m_cameraDalsa.StartGrab(); 
        }

        private void buttonLive_Click(object sender, RoutedEventArgs e)
        {
            m_cameraDalsa.p_sInfo = m_cameraDalsa.StartLive(); 
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            m_cameraDalsa.p_sInfo = m_cameraDalsa.StopGrab(); 
        }
        #endregion
    }
}
