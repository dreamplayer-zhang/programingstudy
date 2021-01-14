using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using RootTools;
using RootTools.Module;
using Root_AOP01_Inspection.Module;
using Root_EFEM.Module;
using static Root_EFEM.Module.WTR_RND;
using RootTools.Gem;
using System.ComponentModel;
using Root_AOP01_Inspection.UI._3._RUN;
using System.Threading;

namespace Root_AOP01_Inspection
{
    /// <summary>
    /// Run_Panel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Run_Panel : UserControl
    {
        ManualJobSchedule m_manualjob;
        AOP01_Engineer m_engineer;
        AOP01_Handler m_handler;
        MainVision m_mainvision;
        WTRCleanUnit m_wtrcleanunit;
        WTRArm m_wtr;
        Arm m_arm;
        Loadport_Cymechs[] m_loadport = new Loadport_Cymechs[2];
        //Loadport_RND[] m_rndloadport = new Loadport_RND[2];
        AOP01_Handler.eLoadport LoadportType;
        public Run_Panel()
        {
            InitializeComponent();
        }

        public void Init(MainVision mainvision, WTRCleanUnit wtrcleanunit, Loadport_Cymechs loadport1,
            Loadport_Cymechs loadport2, AOP01_Engineer engineer)
        {
            m_engineer = engineer;
            m_handler = engineer.m_handler;
            m_wtrcleanunit = wtrcleanunit;
            m_wtr = m_wtrcleanunit.p_aArm[0];
            m_arm = m_wtrcleanunit.m_dicArm[0];
            m_loadport[0] = loadport1;
            m_loadport[1] = loadport2;
            m_mainvision = mainvision;
            loadportA.Init(m_handler.m_aLoadport[0], m_handler);
            loadportB.Init(m_handler.m_aLoadport[1], m_handler);
            LoadportA_State.DataContext = loadport1;
            LoadportB_State.DataContext = loadport2;
            RTR_State.DataContext = wtrcleanunit;
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
            ExistRTR.Background = m_arm.m_diCheckVac.p_bIn == true == true && m_wtr.p_infoWafer != null? Brushes.SteelBlue : Brushes.LightGray;
            ExistVision.Background = m_mainvision.m_diExistVision.p_bIn == true && m_mainvision.p_infoWafer != null? Brushes.SteelBlue : Brushes.LightGray;
            ExistLoadport.Background = (m_loadport[0].p_infoWafer != null) || (m_loadport[1].p_infoWafer != null) ? Brushes.SteelBlue : Brushes.LightGray;
        }
        #endregion
        #region Button Recovery
        bool IsEnableRecovery()
            {
                if (IsRunModule()) return false;
                if (m_handler.m_bIsPossible_Recovery == false) return false;
                // Daniel check
                if (EQ.p_eState != EQ.eState.Ready) return false;
                if (EQ.p_bStop == true) return false;
                return m_handler.IsEnableRecovery();
            }
            public bool m_bRecovery = false;
            private void ButtonRecovery_Click(object sender, RoutedEventArgs e)
            {
                if (IsEnableRecovery() == false) return;
                m_handler.m_bIsPossible_Recovery = false;
                m_handler.m_process.CalcRecover();
                EQ.p_bStop = false;
                EQ.p_eState = EQ.eState.Run;
                m_bRecovery = true;
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
			    switch (LoadportType)
			    {
				    case AOP01_Handler.eLoadport.Cymechs:
                    if (IsRunModule(m_loadport[0])) return true;
                    if (IsRunModule(m_loadport[1])) return true;
                    break;
			    }
                if (IsRunModule(m_wtrcleanunit)) return true;
                if (IsRunModule(m_handler.m_mainVision)) return true;
                return false;
            }
            bool IsRunModule(ModuleBase module)
            {
                if (module.p_eState == ModuleBase.eState.Run) return true;
                if (module.p_eState == ModuleBase.eState.Home) return true;
                return (module.m_qModuleRun.Count > 0);
            }
            private void ButtonInitialize_Click(object sender, RoutedEventArgs e)
            {
                if (IsEnableInitialization() == false) return;
                EQ.p_bStop = false;
                m_handler.m_process.ClearInfoWafer();
                EQ.p_eState = EQ.eState.Home;
                //EQ.p_eState = EQ.eState.Init;
                //m_mainvision.p_eState = ModuleBase.eState.Home;
                //m_wtrcleanunit.p_eState = ModuleBase.eState.Home;
                //m_loadport[0].p_eState = ModuleBase.eState.Home;
                //m_loadport[1].p_eState = ModuleBase.eState.Home;
                ////m_module.ButtonRun();
            }
            #endregion

            #region Button Pause & Resume
            bool bRunning=true;
            bool IsEnablePause()
            {
                if (EQ.p_eState != EQ.eState.Run) return false;
                return true;
            }
            bool IsEnableResume()
            {
                if (EQ.p_eState != EQ.eState.Ready) return false;
                if (m_handler.m_process.m_qSequence.Count <= 0) return false;
                return true;
            }
            private void ButtonPause_Click(object sender, RoutedEventArgs e)
            {
                if(bRunning==true)
                {
                    if (IsEnablePause() == false) return;
                    bRunning = false;
                    PauseResume.Content = "Resume";
                    EQ.p_eState = EQ.eState.Ready;
                }
                else
                {
                    if (IsEnableResume() == false) return;
                    PauseResume.Content = "Pause";
                    EQ.p_bStop = false;
                    EQ.p_eState = EQ.eState.Run;
                }
            }
            #endregion

            #region Button Alarm

            private void ButtonAlarmList_Click(object sender, RoutedEventArgs e)
            {
                m_handler.m_gaf.m_listALID.ShowPopup();

            }
            #endregion

            #region Button OHT
            private void ButtonOHT_Click(object sender, RoutedEventArgs e)
            {
                m_handler.m_aop01.ShowOHT();
            }
            #endregion

            private void ButtonBuzzerOff_Click(object sender, RoutedEventArgs e)
            {
                m_engineer.BuzzerOff();
            }
        }
}
