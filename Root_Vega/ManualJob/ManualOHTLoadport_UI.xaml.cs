using Root_Vega.Module;
using RootTools.Gem;
using RootTools.OHT;
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
            SetBrush(buttonAccessManual, m_loadport.m_infoPod.p_bAccessLP_Manual && p_bBlink);
            SetBrush(buttonAccessAuto, m_loadport.m_infoPod.p_bAccessLP_Auto && p_bBlink);
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
        void TimerLoadportState()
        {
            SetBrush(buttonStateLoading, m_OHT.m_doLoadReq.p_bOn && p_bBlink); 
            SetBrush(buttonStateUnloading, m_OHT.m_doUnloadReq.p_bOn && p_bBlink);
            textBlockPlaced.Foreground = m_loadport.m_dioPlaced.p_bIn ? Brushes.White : Brushes.Gray;
            textBlockPresent.Foreground = m_loadport.m_dioPresent.p_bIn ? Brushes.White : Brushes.Gray;
            bool bPodIn = p_bBlink ? m_loadport.m_dioPlaced.p_bIn : m_loadport.m_dioPresent.p_bIn;
            imageInPod.Visibility = bPodIn ? Visibility.Visible : Visibility.Hidden;
            imageOutPod.Visibility = bPodIn ? Visibility.Hidden : Visibility.Visible;
        }

        void SetBrush(Button button, bool bOn)
        {
            button.Foreground = bOn ? Brushes.Red : Brushes.Black;
            button.Background = bOn ? Brushes.Yellow : Brushes.DimGray;
        }

        #endregion

        #region OHT DI
        void TimerDI()
        {
            SetBrushDI(buttonDIValid, m_OHT.m_diValid);
            SetBrushDI(buttonDICS0, m_OHT.m_diCS[0]);
            SetBrushDI(buttonDICS1, m_OHT.m_diCS[1]);
            SetBrushDI(buttonDITrReq, m_OHT.m_diTrReq);
            SetBrushDI(buttonDIBusy, m_OHT.m_diBusy);
            SetBrushDI(buttonDICompt, m_OHT.m_diComplete);
            SetBrushDI(buttonDICont, m_OHT.m_diContinue);
        }

        void SetBrushDI(Button button, OHTBase.DI di)
        {
            bool bOn = di.p_bOn; 
            button.Foreground = bOn ? Brushes.Red : Brushes.Black;
            if (di.p_bWait) bOn = p_bBlink ? bOn : !bOn; 
            button.Background = bOn ? Brushes.Yellow : Brushes.DimGray;
        }
        #endregion

        #region OHT DO
        void TimerDO()
        {
            SetBrushDO(buttonDOLReq, m_OHT.m_doLoadReq);
            SetBrushDO(buttonDOUReq, m_OHT.m_doUnloadReq);
            SetBrushDO(buttonDOReady, m_OHT.m_doReady);
            SetBrushDO(buttonDOHoAvbl, m_OHT.m_doHoAvailable);
            SetBrushDO(buttonDOES, m_OHT.m_doES);
        }

        void SetBrushDO(Button button, OHTBase.DO dio)
        {
            bool bOn = dio.p_bOn;
            button.Foreground = bOn ? Brushes.Red : Brushes.Black;
            if (dio.p_bWait) bOn = p_bBlink ? bOn : !bOn;
            button.Background = bOn ? Brushes.Yellow : Brushes.DimGray;
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

        #region Timer
        int _nBlink = 0;
        bool p_bBlink { get { return _nBlink < 5; } }

        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            TimerAccessMode();
            TimerLoadportState();
            TimerDI();
            TimerDO();
            _nBlink = (_nBlink + 1) % 10;
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
