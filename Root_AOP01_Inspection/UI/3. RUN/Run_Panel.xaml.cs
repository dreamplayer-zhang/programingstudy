using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using RootTools;
using RootTools.Module;
using Root_AOP01_Inspection.Module;
using Root_EFEM.Module;
using Root_AOP01_Inspection.UI._3._RUN;

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
        RTRCleanUnit m_wtrcleanunit;
        WTRArm m_wtr;
        RTR_RND.Arm m_arm;
        Loadport_Cymechs[] m_loadport = new Loadport_Cymechs[2];
        RFID_Brooks[] m_rfid = new RFID_Brooks[2];
        AOP01_Handler.eLoadport LoadportType;
        public Run_Panel()
        {
            InitializeComponent();
        }

        public void Init(MainVision mainvision, RTRCleanUnit wtrcleanunit, Loadport_Cymechs loadport1,
            Loadport_Cymechs loadport2, AOP01_Engineer engineer, RFID_Brooks rfid1, RFID_Brooks rfid2)
        {
            m_engineer = engineer;
            m_handler = engineer.m_handler;
            m_wtrcleanunit = wtrcleanunit;
            m_wtr = m_wtrcleanunit.p_aArm[0];
            m_arm = m_wtrcleanunit.m_dicArm[0];
            m_loadport[0] = loadport1;
            m_loadport[1] = loadport2;
            m_mainvision = mainvision;
            m_rfid[0] = rfid1;
            m_rfid[1] = rfid2;
            loadportA.Init(m_handler.m_aLoadport[0], m_engineer, m_rfid[0]);
            loadportB.Init(m_handler.m_aLoadport[1], m_engineer, m_rfid[1]);
            LoadportA_State.DataContext = loadport1;
            LoadportB_State.DataContext = loadport2;
            RTR_State.DataContext = wtrcleanunit;
            MainVision_State.DataContext = mainvision;
            InitFFU();
            InitTimer();
        }
        void InitFFU()
        {
            FanUI0.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[0];
            FanUI1.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[1];
            FanUI2.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[2];
            FanUI3.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[3];
            FanUI4.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[4];
            FanUI5.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[5];
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
            ExistRTR.Background = m_arm.m_diCheckVac.p_bIn == true && m_wtr.p_infoWafer != null ? Brushes.SteelBlue : Brushes.LightGray;
            ExistVision.Background = m_mainvision.m_diExistVision.p_bIn == true && m_mainvision.p_infoWafer != null ? Brushes.SteelBlue : Brushes.LightGray;
            //ExistLoadport.Background = (m_loadport[0].p_infoWafer != null) || (m_loadport[1].p_infoWafer != null) ? Brushes.SteelBlue : Brushes.LightGray;
            ButtonInitialize.IsEnabled = IsEnableInitialization();
            ButtonRecovery.IsEnabled = IsEnableRecovery();
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
            m_handler.CalcRecover();
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
            if (IsRunModule(m_loadport[0])) return true;
            if (IsRunModule(m_loadport[1])) return true;
            if (IsRunModule(m_wtrcleanunit)) return true;
            if (IsRunModule(m_handler.m_mainVision)) return true;
            return false;
        }
        bool IsRunModule(ModuleBase module)
        {
            if (module.p_eState == ModuleBase.eState.Run) return true;
            if (module.p_eState == ModuleBase.eState.Home) return true;
            if (module.p_eState == ModuleBase.eState.Error) return false;
            return (module.m_qModuleRun.Count > 0);
        }
        private void ButtonInitialize_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableInitialization() == false) return;
            EQ.p_bStop = false;
            m_handler.m_process.ClearInfoWafer();
            m_handler.m_nRnR = 0; //Init 할때 RNR 카운트초기화
            EQ.p_eState = EQ.eState.Home;
        }
        #endregion

        #region Button Pause & Resume
        bool bRunning = true;
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
            if (bRunning == true)
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
            m_engineer.m_handler.m_aop01.BuzzerOff();
        }

        private void DoorCheck_Click(object sender, RoutedEventArgs e)
        {
            if ((String)DoorCheck.Content == "DoorAlarm Off")
            {
                DoorCheck.Content = "DoorAlarm On";
                m_engineer.m_handler.m_aop01.m_bDoorAlarm = false;
            }
            else
            {
                DoorCheck.Content = "DoorAlarm Off";
                m_engineer.m_handler.m_aop01.m_bDoorAlarm = true;
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_bStop = true;
        }
    }
}
