using RootTools.Gem;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace RootTools.OHTNew
{
    /// <summary>
    /// OHT_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OHT_UI : UserControl
    {
        public OHT_UI()
        {
            InitializeComponent();
        }

        public OHT m_OHT;
        GemCarrierBase m_carrier = null;
        public void Init(OHT OHT)
        {
            m_OHT = OHT;
            m_carrier = OHT.m_carrier;
            DataContext = OHT;

            InitDIOButton(); 
            InitTimer();
        }

        #region AccessMode
        void TimerAccessMode()
        {
            bool bAuto = (m_carrier.p_eAccessLP == GemCarrierBase.eAccessLP.Auto);
            SetBrush(buttonAccessManual, !bAuto && p_bBlink);
            SetBrush(buttonAccessAuto, bAuto && p_bBlink);
        }

        private void buttonAccessManual_Click(object sender, RoutedEventArgs e)
        {
            m_carrier.p_eReqAccessLP = GemCarrierBase.eAccessLP.Manual;
        }

        private void buttonAccessAuto_Click(object sender, RoutedEventArgs e)
        {
            m_carrier.p_eReqAccessLP = GemCarrierBase.eAccessLP.Auto;
        }
        #endregion

        #region OHT State
        void TimerLoadportState()
        {
            SetBrush(buttonStateLoading, m_OHT.m_doLoadReq.p_bOn && p_bBlink);
            SetBrush(buttonStateUnloading, m_OHT.m_doUnloadReq.p_bOn && p_bBlink);
            textBlockPlaced.Foreground = !m_OHT.m_diPlaced.p_bIn ? Brushes.White : Brushes.Gray;
            textBlockPresent.Foreground = !m_OHT.m_diPresent.p_bIn ? Brushes.White : Brushes.Gray;
            bool bPodIn = p_bBlink ? m_OHT.m_diPlaced.p_bIn : m_OHT.m_diPresent.p_bIn;
            imageInPod.Visibility = bPodIn ? Visibility.Visible : Visibility.Hidden;
            imageOutPod.Visibility = bPodIn ? Visibility.Hidden : Visibility.Visible;
        }

        void SetBrush(Button button, bool bOn)
        {
            button.Foreground = bOn ? Brushes.Red : Brushes.Black;
            button.Background = bOn ? Brushes.Yellow : Brushes.DimGray;
        }
        #endregion

        #region OHT DIO
        List<OHT_ButtonUI> m_aButtonUI = new List<OHT_ButtonUI>(); 
        void InitDIOButton()
        {
            gridDI.Children.Clear();
            gridDI.RowDefinitions.Clear();
            for (int n = 0; n < m_OHT.p_aDI.Count; n++) gridDI.RowDefinitions.Add(new RowDefinition());
            for (int n = 0; n < m_OHT.p_aDI.Count; n++)
            {
                OHT.DI DI = m_OHT.p_aDI[n]; 
                if (DI != null)
                {
                    OHT_ButtonUI ui = new OHT_ButtonUI();
                    ui.Init(DI, this);
                    Grid.SetRow(ui, n);
                    gridDI.Children.Add(ui);
                    m_aButtonUI.Add(ui); 
                }
            }
            gridDO.Children.Clear();
            gridDO.RowDefinitions.Clear();
            for (int n = 0; n < m_OHT.p_aDO.Count; n++) gridDO.RowDefinitions.Add(new RowDefinition());
            for (int n = 0; n < m_OHT.p_aDO.Count; n++)
            {
                OHT.DO DO = m_OHT.p_aDO[n]; 
                if (DO != null)
                {
                    OHT_ButtonUI ui = new OHT_ButtonUI();
                    ui.Init(DO, this);
                    Grid.SetRow(ui, n);
                    gridDO.Children.Add(ui);
                    m_aButtonUI.Add(ui);
                }
            }
        }

        void TimerDIO()
        {
            foreach (OHT_ButtonUI ui in m_aButtonUI) ui.OnTimer(p_bBlink);
        }
        #endregion

        #region Reset
        void TimerReset()
        {
            buttonRetry.IsEnabled = (m_OHT.p_eState == OHT.eState.Error); 
        }
        #endregion

        #region OHT p_sInfo
        private void buttonRetry_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doLoadReq.p_bOn = false;
            m_OHT.m_doUnloadReq.p_bOn = false;
            m_OHT.m_doReady.p_bOn = false;
            m_OHT.p_sInfo = "";
            m_OHT.p_eState = OHT.eState.All_Off;
        }
        #endregion

        #region Timer
        int _nBlink = 0;
        public bool p_bBlink { get { return _nBlink < 5; } }

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            TimerAccessMode();
            TimerLoadportState();
            TimerDIO();
            TimerReset();
            _nBlink = (_nBlink + 1) % 10;
        }
        #endregion
    }
}
