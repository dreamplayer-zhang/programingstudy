using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools
{
    /// <summary>
    /// CyUSBTool_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CyUSBTool_UI : UserControl
    {
        public CyUSBTool_UI()
        {
            InitializeComponent();
        }

        #region Comm Log
        ObservableCollection<CyUSBTool.CommLog> m_aCommLog = new ObservableCollection<CyUSBTool.CommLog>();
        void CheckCommLog()
        {
            if (m_usb.m_aCommLog.Count == 0) return;
            while (m_usb.m_aCommLog.Count > 0)
            {
                m_aCommLog.Insert(0, m_usb.m_aCommLog.Dequeue());
                if (m_aCommLog.Count > 128) m_aCommLog.RemoveAt(m_aCommLog.Count - 1);
            }
        }
        #endregion

        CyUSBTool m_usb; 
        public void Init(CyUSBTool usb)
        {
            m_usb = usb;
            this.DataContext = m_usb;
            listViewLog.ItemsSource = m_aCommLog;

            m_timer.Interval = TimeSpan.FromMilliseconds(50);
            m_timer.Tick += M_timer_Tick; 
            m_timer.Start();
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            CheckCommLog();
        }

        private void ButtonRead_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            double fPower = 0;
            m_usb.Read(m_usb.p_nCh, ref fPower); 
        }

        private void ButtonWrite_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_usb.p_sInfo = m_usb.Write(m_usb.p_nCh, m_usb.p_fPower); 
        }
    }
}
