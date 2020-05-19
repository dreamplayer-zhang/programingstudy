using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.Camera.BaslerPylon
{
    /// <summary>
    /// CameraBasler_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CameraBasler_UI : UserControl
    {
        public CameraBasler_UI()
        {
            InitializeComponent();
        }

        CameraBasler m_cameraBasler;
        public void Init(CameraBasler cameraBasler)
        {
            m_cameraBasler = cameraBasler;
            DataContext = cameraBasler;
            treeUI.Init(cameraBasler.p_treeRoot);

            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick; 
            m_timer.Start();
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, System.EventArgs e)
        {
            bool bCanGrab = m_cameraBasler.p_bConnect && (m_cameraBasler.m_cam.StreamGrabber.IsGrabbing == false);
            buttonSingle.IsEnabled = bCanGrab;
            buttonContinuous.IsEnabled = bCanGrab;
            buttonLive.IsEnabled = bCanGrab;
            buttonStop.IsEnabled = m_cameraBasler.p_bConnect && m_cameraBasler.m_cam.StreamGrabber.IsGrabbing; 
        }
        #endregion

        #region Grab
        private void buttonSingle_Click(object sender, RoutedEventArgs e)
        {
            m_cameraBasler.p_sInfo = m_cameraBasler.StartGrab(1, m_cameraBasler.m_nGrabOffset); 
        }

        private void buttonContinuous_Click(object sender, RoutedEventArgs e)
        {
            m_cameraBasler.p_sInfo = m_cameraBasler.StartGrab(m_cameraBasler.m_nGrabCount, m_cameraBasler.m_nGrabOffset);
        }

        private void buttonLive_Click(object sender, RoutedEventArgs e)
        {
            m_cameraBasler.p_sInfo = m_cameraBasler.StartLive(m_cameraBasler.m_nGrabOffset); 
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            m_cameraBasler.p_sInfo = m_cameraBasler.StopGrab(); 
        }
        #endregion
    }
}
