using Root_Vega.ManualJob;
using Root_Vega.Module;
using RootTools;
using RootTools.Inspects;
using RootTools.Module;
using RootTools.Trees;
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
            loadportA.Init(m_handler.m_aLoadport[0], m_handler);
            loadportB.Init(m_handler.m_aLoadport[1], m_handler);

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
            if (IsRunModule()) return false; 
            switch (EQ.p_eState)
            {
                case EQ.eState.Run: return false;
                case EQ.eState.Home: return false; 
            }
            return true; 
        }

        bool IsRunModule()
        {
            if (IsRunModule(m_handler.m_aLoadport[0])) return true;
            if (IsRunModule(m_handler.m_aLoadport[1])) return true;
            if (IsRunModule(m_handler.m_robot)) return true;
            if (IsRunModule(m_handler.m_sideVision)) return true;
            if (IsRunModule(m_handler.m_patternVision)) return true;
            return false; 
        }

        bool IsRunModule(ModuleBase module)
        {
            if (module.p_eState == ModuleBase.eState.Run) return true; 
            if (module.p_eState == ModuleBase.eState.Home) return true; 
            return (module.m_qModuleRun.Count > 0);
        }
        private void buttonInitialization_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableInitialization() == false) return;
            EQ.p_bStop = false;
            m_handler.m_process.ClearInfoReticle(); 
            EQ.p_eState = EQ.eState.Home;

            // Camera Connect
            PatternVision patternVision = m_handler.m_patternVision;
            SideVision sideVision = m_handler.m_sideVision;

            if (patternVision.m_CamMain != null && patternVision.m_CamMain.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init) patternVision.m_CamMain.Connect();
            if (patternVision.m_CamRADS != null && patternVision.m_CamRADS.p_CamInfo._OpenStatus == false) patternVision.m_CamRADS.Connect();
            if (patternVision.m_CamAlign1 != null && patternVision.m_CamAlign1.p_CamInfo._OpenStatus == false) patternVision.m_CamAlign1.Connect();
            if (patternVision.m_CamAlign2 != null && patternVision.m_CamAlign2.p_CamInfo._OpenStatus == false) patternVision.m_CamAlign2.Connect();

            if (sideVision.p_CamSide != null && sideVision.p_CamSide.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init) sideVision.p_CamSide.Connect();
            if (sideVision.p_CamBevel != null && sideVision.p_CamBevel.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init) sideVision.p_CamBevel.Connect();
            if (sideVision.p_CamSideVRS != null && sideVision.p_CamSideVRS.p_CamInfo._OpenStatus == false) sideVision.p_CamSideVRS.Connect();
            if (sideVision.p_CamAlign1 != null && sideVision.p_CamAlign1.p_CamInfo._OpenStatus == false) sideVision.p_CamAlign1.Connect();
            if (sideVision.p_CamAlign2 != null && sideVision.p_CamAlign2.p_CamInfo._OpenStatus == false) sideVision.p_CamAlign2.Connect();
            if (sideVision.p_CamLADS != null && sideVision.p_CamLADS.p_CamInfo._OpenStatus == false) sideVision.p_CamLADS.Connect();

        }
        #endregion

        #region Button Recovery
        bool IsEnableRecovery()
        {
            if (IsRunModule()) return false;
            if (EQ.p_eState != EQ.eState.Ready) return false;
            if (EQ.p_bStop == true) return false;
            return m_handler.IsEnableRecovery(); 
        }

        private void buttonRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (m_handler.m_bIsPossible_Recovery == false) return; // Daniel check
            if (IsEnableRecovery() == false) return;
            m_handler.m_process.CalcRecover();
            EQ.p_eState = EQ.eState.Run; 
        }
        #endregion

        #region Button OHT
        private void buttonOHT_Click(object sender, RoutedEventArgs e)
        {
            ManualOHT_UI ui = new ManualOHT_UI();
            ui.Init(m_handler); 
            ui.Show(); 
        }
        #endregion
    }
}
