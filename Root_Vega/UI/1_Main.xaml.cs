using Root_Vega.ManualJob;
using RootTools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Root_Vega
{
    /// <summary>
    /// _1_Main.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class _1_Main : UserControl
    {
        public _1_Main()
        {
            InitializeComponent();
        }

        Vega_Engineer m_engineer;
        Vega_Handler m_handler; 
        public void Init(Vega_Engineer engineer)
        {
            m_engineer = engineer;
            m_handler = engineer.m_handler; 
            moduleVision.Init(m_handler.m_patternVision);
            moduleSideVision.Init(m_handler.m_sideVision);
            moduleRobot.Init(m_handler.m_robot);
            loadportA.Init(m_handler.m_aLoadport[0]);
            loadportB.Init(m_handler.m_aLoadport[1]);

            InitTimer();
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            buttonResume.IsEnabled = IsEnableResume();
            buttonPause.IsEnabled = IsEnablePause();
            buttonInitialization.IsEnabled = IsEnableInitialization();
            buttonRecovery.IsEnabled = IsEnableRecovery(); 
        }
        #endregion

        #region Button Resume
        bool IsEnableResume()
        {
            if (EQ.p_eState != EQ.eState.Ready) return false;
            if (m_handler.m_process.m_qSequence.Count <= 0) return false; 
            return true; 
        }

        private void buttonResume_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableResume() == false) return;
            EQ.p_eState = EQ.eState.Run; 
        }
        #endregion

        #region Button Pause
        bool IsEnablePause()
        {
            if (EQ.p_eState != EQ.eState.Run) return false;
            return true;
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnablePause() == false) return;
            EQ.p_eState = EQ.eState.Ready; 
        }
        #endregion

        #region Button Initialization
        bool IsEnableInitialization()
        {
            switch (EQ.p_eState)
            {
                case EQ.eState.Run: return false;
                case EQ.eState.Home: return false; 
            }
            return true; 
        }
        private void buttonInitialization_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableInitialization() == false) return; 
            m_handler.m_process.m_qSequence.Clear(); //forget 
            EQ.p_eState = EQ.eState.Home; 
        }
        #endregion

        #region Button Recovery
        bool IsEnableRecovery()
        {
            if (EQ.p_eState != EQ.eState.Ready) return false;
            return m_handler.IsEnableRecovery(); 
        }

        private void buttonRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableRecovery() == false) return;
            m_handler.m_process.CalcRecover();
            EQ.p_eState = EQ.eState.Run; 
        }
        #endregion

        #region Button OHT
        private void buttonOHT_Click(object sender, RoutedEventArgs e)
        {
            ManualOHT_UI ui = new ManualOHT_UI();
            ui.Show(); 
        }
        #endregion
    }
}
