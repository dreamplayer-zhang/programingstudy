using Root_Vega.Module;
using RootTools.Gem;
using RootTools.OHT.Semi;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Vega.ManualJob
{
    /// <summary>
    /// ManualOHT_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualOHTLoadport_UI : UserControl
    {
        public ManualOHTLoadport_UI()
        {
            InitializeComponent();
        }

        #region AccessMode
        void TimerAccessMode()
        {
            SetBrush(buttonAccessManual, m_loadport.m_infoPod.p_bAccessLP_Manual);
            SetBrush(buttonAccessAuto, m_loadport.m_infoPod.p_bAccessLP_Auto);
        }

        private void buttonAccessManual_Click(object sender, RoutedEventArgs e)
        {
            m_loadport.m_infoPod.p_eReqAccessLP = GemCarrierBase.eAccessLP.Manual; 
        }

        private void buttonAccessAuto_Click(object sender, RoutedEventArgs e)
        {
            m_loadport.m_infoPod.p_eReqAccessLP = GemCarrierBase.eAccessLP.Auto; 
        }
        #endregion

        #region OHT State
        int m_nBlink = 0; 
        void TimerLoadportState()
        {
            SetBrush(buttonStateLoading, m_OHT.m_doLoadReq.p_bOn && (m_nBlink < 5)); 
            SetBrush(buttonStateUnloading, m_OHT.m_doUnloadReq.p_bOn && (m_nBlink < 5));
            textBlockPlaced.Foreground = m_loadport.m_dioPlaced.p_bIn ? Brushes.White : Brushes.Gray;
            textBlockPresent.Foreground = m_loadport.m_dioPresent.p_bIn ? Brushes.White : Brushes.Gray;
            bool bPodIn = (m_nBlink < 5) ? m_loadport.m_dioPlaced.p_bIn : m_loadport.m_dioPresent.p_bIn;
            imageInPod.Visibility = bPodIn ? Visibility.Visible : Visibility.Hidden;
            imageOutPod.Visibility = bPodIn ? Visibility.Hidden : Visibility.Visible;
            m_nBlink = (m_nBlink + 1) % 10; 
        }
        #endregion

        #region OHT DI
        void TimerDI()
        {
            SetBrush(buttonDIValid, m_OHT.m_diValid.p_bOn); 
            SetBrush(buttonDICS0, m_OHT.m_diCS[0].p_bOn);
            SetBrush(buttonDICS1, m_OHT.m_diCS[1].p_bOn);
            SetBrush(buttonDITrReq, m_OHT.m_diTrReq.p_bOn);
            SetBrush(buttonDIBusy, m_OHT.m_diBusy.p_bOn);
            SetBrush(buttonDICompt, m_OHT.m_diComplete.p_bOn);
            SetBrush(buttonDICont, m_OHT.m_diContinue.p_bOn);
        }
        #endregion

        #region OHT DO
        void TimerDO()
        {
            SetBrush(buttonDOLReq, m_OHT.m_doLoadReq.p_bOn);
            SetBrush(buttonDOUReq, m_OHT.m_doUnloadReq.p_bOn);
            SetBrush(buttonDOReady, m_OHT.m_doReady.p_bOn);
            SetBrush(buttonDOHoAvbl, m_OHT.m_doHoAvailable.p_bOn);
            SetBrush(buttonDOES, m_OHT.m_doES.p_bOn);
        }

        private void buttonDOLReq_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doLoadReq.Toggle(); 
        }

        private void buttonDOUReq_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doUnloadReq.Toggle(); 
        }

        private void buttonDOReady_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doReady.Toggle(); 
        }

        private void buttonDOHoAvbl_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doHoAvailable.Toggle(); 
        }

        private void buttonDOES_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doES.Toggle(); 
        }
        #endregion

        #region OHT p_sInfo
        private void buttonRetry_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doLoadReq.p_bOn = false;
            m_OHT.m_doUnloadReq.p_bOn = false;
            m_OHT.m_doReady.p_bOn = false;
            m_OHT.p_sInfo = ""; 
        }
        #endregion

        #region Brushes
        void SetBrush(Button button, bool bOn)
        {
            button.Foreground = bOn ? Brushes.Black : Brushes.White; 
            button.Background = bOn ? Brushes.Yellow : Brushes.DimGray; 
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            TimerAccessMode();
            TimerLoadportState();
            TimerDI();
            TimerDO(); 
        }
        #endregion

        Loadport m_loadport;
        OHT_Semi m_OHT; 
        public void Init(Loadport loadport)
        {
            m_loadport = loadport;
            m_OHT = loadport.m_OHT; 
            DataContext = loadport.m_OHT;

            m_timer.Interval = TimeSpan.FromSeconds(0.1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }
    }
}
