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
        Loadport_RND[] m_rndloadport = new Loadport_RND[2];
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
            m_mainvision = mainvision;
            m_loadport[0] = loadport1;
            m_loadport[1] = loadport2;
            LoadportA_State.DataContext = loadport1;
            LoadportB_State.DataContext = loadport2;
            RTR_State.DataContext = wtrcleanunit;
            LoadportType = AOP01_Handler.eLoadport.Cymechs;
            m_manualjob = new ManualJobSchedule(m_loadport[0],m_loadport[1], m_engineer);
            InitButtonLoad();
            InitTimer();
        }
        public void Init(MainVision mainvision, WTRCleanUnit wtrcleanunit, Loadport_RND loadport1,
            Loadport_RND loadport2, AOP01_Engineer engineer)
        {
            m_engineer = engineer;
            m_handler = engineer.m_handler;
            m_wtrcleanunit = wtrcleanunit;
            m_wtr = m_wtrcleanunit.p_aArm[0];
            m_arm = m_wtrcleanunit.m_dicArm[0];
            m_mainvision = mainvision;
            m_rndloadport[0] = loadport1;
            m_rndloadport[1] = loadport2;
            LoadportA_State.DataContext = loadport1;
            LoadportB_State.DataContext = loadport2;
            RTR_State.DataContext = wtrcleanunit;
            LoadportType = AOP01_Handler.eLoadport.RND;
            m_manualjob = new ManualJobSchedule(m_rndloadport[0], m_rndloadport[1], m_engineer);
            InitButtonLoad();
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
			switch (LoadportType)
			{
				case AOP01_Handler.eLoadport.Cymechs:
                    ExistLoadport.Background = (m_loadport[0].p_infoWafer != null) || (m_loadport[1].p_infoWafer != null) ? Brushes.SteelBlue : Brushes.LightGray;
                    Placed1.Background = m_loadport[0].m_diPlaced.p_bIn == false ? Brushes.SteelBlue : Brushes.LightGray;
                    Present1.Background = m_loadport[0].m_diPresent.p_bIn == false ? Brushes.SteelBlue : Brushes.LightGray;
                    //Load1.Background = m_loadport[0].m_bLoadCheck == true ? Brushes.SteelBlue : Brushes.LightGray;
                    //UnLoad1.Background = m_loadport[0].m_bUnLoadCheck == true ? Brushes.SteelBlue : Brushes.LightGray;
                    Alarm1.Background = m_loadport[0].p_eState == ModuleBase.eState.Error ? Brushes.Red : Brushes.LightGray;
                    Placed2.Background = m_loadport[1].m_diPlaced.p_bIn == false ? Brushes.SteelBlue : Brushes.LightGray;
                    Present2.Background = m_loadport[1].m_diPresent.p_bIn == false ? Brushes.SteelBlue : Brushes.LightGray;
                    //Load2.Background = m_loadport[1].m_bLoadCheck ? Brushes.SteelBlue : Brushes.LightGray;
                    //UnLoad2.Background = m_loadport[1].m_bUnLoadCheck ? Brushes.SteelBlue : Brushes.LightGray;
                    Alarm2.Background = m_loadport[1].p_eState == ModuleBase.eState.Error ? Brushes.Red : Brushes.LightGray;
                    break;
				case AOP01_Handler.eLoadport.RND:
                default:
                    ExistLoadport.Background = (m_rndloadport[0].p_infoWafer != null) || (m_rndloadport[1].p_infoWafer != null) ? Brushes.SteelBlue : Brushes.LightGray;
                    Placed1.Background = m_rndloadport[0].m_diPlaced.p_bIn == false ? Brushes.SteelBlue : Brushes.LightGray;
                    Present1.Background = m_rndloadport[0].m_diPresent.p_bIn == false ? Brushes.SteelBlue : Brushes.LightGray;
                    //Load1.Background = m_rndloadport[0].m_bLoadCheck == true ? Brushes.SteelBlue : Brushes.LightGray;
                    //UnLoad1.Background = m_rndloadport[0].m_bUnLoadCheck == true ? Brushes.SteelBlue : Brushes.LightGray;
                    Alarm1.Background = m_rndloadport[0].p_eState == ModuleBase.eState.Error ? Brushes.Red : Brushes.LightGray;
                    Placed2.Background = m_rndloadport[1].m_diPlaced.p_bIn == false ? Brushes.SteelBlue : Brushes.LightGray;
                    Present2.Background = m_rndloadport[1].m_diPresent.p_bIn == false ? Brushes.SteelBlue : Brushes.LightGray;
                    //Load2.Background = m_rndloadport[1].m_bLoadCheck ? Brushes.SteelBlue : Brushes.LightGray;
                    //UnLoad2.Background = m_rndloadport[1].m_bUnLoadCheck ? Brushes.SteelBlue : Brushes.LightGray;
                    Alarm2.Background = m_rndloadport[1].p_eState == ModuleBase.eState.Error ? Brushes.Red : Brushes.LightGray;
                    break;                 
            }
            //ButtonLoad1.IsEnabled = IsEnableLoad(0);
            ButtonUnLoadReq1.IsEnabled = IsEnableUnload(0);
            ButtonLoad2.IsEnabled = IsEnableLoad(1);
            ButtonUnLoadReq2.IsEnabled = IsEnableUnload(1);
        }
        #endregion
        #region Button Load UnLoad
        BackgroundWorker m_bgwLoad = new BackgroundWorker();
        void InitButtonLoad()
        {
            m_bgwLoad.DoWork += M_bgwLoad_DoWork;
            m_bgwLoad.RunWorkerCompleted += M_bgwLoad_RunWorkerCompleted;
        }
        bool IsEnableLoad(int LPNum)
        {
            bool bReadyLoadport = (m_loadport[LPNum].p_eState == ModuleBase.eState.Ready);
            bool bReadyToLoad = (m_loadport[LPNum].p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad);
            bReadyToLoad = true;
            bool bReadyState = (m_loadport[LPNum].m_qModuleRun.Count == 0);
            bool bEQReadyState = (EQ.p_eState == EQ.eState.Ready);
            if (m_loadport[LPNum].p_infoCarrier.p_eState != InfoCarrier.eState.Placed) return false;

            if (m_handler.IsEnableRecovery() == true) return false;
            return bReadyLoadport && bReadyToLoad && bReadyState && bEQReadyState && !m_loadport[LPNum].m_diPresent.p_bIn; //forget 조건
        }
        bool IsEnableUnload(int LPNum)
        {
            bool bReadyLoadport = m_loadport[LPNum].p_eState == ModuleBase.eState.Ready;
            bool bPlace = m_loadport[LPNum].CheckPlaced();
            bool bReadyToUnload = m_loadport[LPNum].p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload;
            bool bAccess = m_loadport[LPNum].m_OHT.p_eAccessLP == GemCarrierBase.eAccessLP.Auto;
            return bReadyLoadport && bPlace && bReadyToUnload && bAccess;
        }
        public bool m_btnload1 = false;
        public bool m_btnload2 = false;
        private void ButtonLoad1_Click(object sender, RoutedEventArgs e)
        {
            //if (IsEnableLoad(0) == false) return;
            m_btnload1 = true;
            if (m_manualjob.ShowPopup(m_engineer) == false) return;
            m_bgwLoad.RunWorkerAsync();
        }

        private void ButtonUnLoadReq1_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableUnload(0) == false) return;
            //m_loadport[0].m_ceidUnload.Send();
        }

        private void ButtonLoad2_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableLoad(1) == false) return;
            m_btnload2 = true;
            if (m_manualjob.ShowPopup(m_engineer) == false) return;
            m_bgwLoad.RunWorkerAsync();
        }

        private void ButtonUnLoadReq2_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableUnload(1) == false) return;
            //m_loadport.m_ceidUnload.Send();
        }


        private void M_bgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            if (m_btnload1 == true)
            {
                m_btnload1 = false;
                //ModuleRunBase moduleRun = m_loadport.m_runReadPodID.Clone();
                //m_loadport[0].StartRun(moduleRun);
                //Thread.Sleep(100);
                //while ((EQ.IsStop() != true) && m_loadport[0].m_qModuleRun.Count > 0) Thread.Sleep(10);
                //while ((EQ.IsStop() != true) && m_loadport[0].p_eState == ModuleBase.eState.Run) Thread.Sleep(10);
                ModuleRunBase moduleRun = m_loadport[0].m_runDocking.Clone();
                m_loadport[0].StartRun(moduleRun);
                Thread.Sleep(100);
                while ((EQ.IsStop() != true) && m_loadport[0].m_qModuleRun.Count > 0) Thread.Sleep(10);
                while ((EQ.IsStop() != true) && m_loadport[0].p_eState == ModuleBase.eState.Run) Thread.Sleep(10);
                
            }
            else if(m_btnload2 == true)
            {
                m_btnload2 = false;
                //ModuleRunBase moduleRun = m_loadport.m_runReadPodID.Clone();
                //m_loadport[0].StartRun(moduleRun);
                //Thread.Sleep(100);
                //while ((EQ.IsStop() != true) && m_loadport[0].m_qModuleRun.Count > 0) Thread.Sleep(10);
                //while ((EQ.IsStop() != true) && m_loadport[0].p_eState == ModuleBase.eState.Run) Thread.Sleep(10);
                ModuleRunBase moduleRun = m_loadport[0].m_runDocking.Clone();
                m_loadport[0].StartRun(moduleRun);
                Thread.Sleep(100);
                while ((EQ.IsStop() != true) && m_loadport[1].m_qModuleRun.Count > 0) Thread.Sleep(10);
                while ((EQ.IsStop() != true) && m_loadport[1].p_eState == ModuleBase.eState.Run) Thread.Sleep(10);
            }
        }

        private void M_bgwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch (m_loadport[0].p_eState)
            {
                case ModuleBase.eState.Ready:
                    m_loadport[0].p_infoCarrier.p_eState = InfoCarrier.eState.Dock;
                    if (m_manualjob.SetInfoPod() != "OK") return;
                    m_loadport[0].p_infoCarrier.StartProcess(m_loadport[0].p_infoWafer.p_id);
                    Thread.Sleep(100);
                    EQ.p_eState = EQ.eState.Run;
                    break;
            }
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
                case AOP01_Handler.eLoadport.RND:
                    default:
                    if (IsRunModule(m_rndloadport[0])) return true;
                    if (IsRunModule(m_rndloadport[1])) return true;
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
