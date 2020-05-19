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
            comboPool.IsEnabled = m_cameraBasler.p_bConnect; 
            comboGroup.IsEnabled = m_cameraBasler.p_bConnect;
            comboMemory.IsEnabled = m_cameraBasler.p_bConnect;
            bool bCanGrab = m_cameraBasler.p_bConnect && (m_cameraBasler.m_cam.StreamGrabber.IsGrabbing == false);
            buttonSingle.IsEnabled = bCanGrab;
            buttonContinuous.IsEnabled = bCanGrab;
            buttonLive.IsEnabled = bCanGrab;
            buttonStop.IsEnabled = m_cameraBasler.p_bConnect && m_cameraBasler.m_cam.StreamGrabber.IsGrabbing; 
        }
        #endregion

        #region Memory
        private void comboPool_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            comboPool.ItemsSource = m_cameraBasler.m_memoryTool.m_asPool;
        }

        private void comboPool_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboGroup.ItemsSource = null;
            comboMemory.ItemsSource = null;
            comboGroup.SelectedIndex = -1;
            comboMemory.SelectedIndex = -1; 
            if (comboPool.SelectedValue == null) return;
            m_cameraBasler.p_sMemoryPool = comboPool.SelectedValue.ToString();
            if (m_cameraBasler.m_memoryPool == null) return;
            comboGroup.ItemsSource = m_cameraBasler.m_memoryPool.m_asGroup; 
        }

        private void comboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboMemory.ItemsSource = null;
            comboMemory.SelectedIndex = -1;
            if (comboGroup.SelectedValue == null) return;
            m_cameraBasler.p_sMemoryGroup = comboGroup.SelectedValue.ToString();
            if (m_cameraBasler.m_memoryGroup == null) return;
            comboMemory.ItemsSource = m_cameraBasler.m_memoryGroup.m_asMemory; 
        }

        private void comboMemory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboMemory.SelectedValue == null) return;
            m_cameraBasler.p_sMemoryData = comboMemory.SelectedValue.ToString(); 
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
