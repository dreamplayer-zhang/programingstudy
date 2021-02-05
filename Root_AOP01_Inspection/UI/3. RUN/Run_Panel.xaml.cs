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
        //ManualJobSchedule m_manualjob;
        AOP01_Engineer m_engineer;
        AOP01_Handler m_handler;
        MainVision m_mainvision;
        BacksideVision m_backsidevision;
        //RTRCleanUnit m_rtrcleanunit;
        RTR_RND m_rndrtr;
        WTRArm m_rtrarm;
        RTR_RND.Arm m_arm;
        Loadport_AOP01[] m_loadport = new Loadport_AOP01[2];
        RFID_Brooks[] m_rfid = new RFID_Brooks[2];
        AOP01_Handler.eLoadport LoadportType;
        public Run_Panel()
        {
            InitializeComponent();
        }

        public void Init(MainVision mainvision, BacksideVision backsidevision, RTR_RND rtr, Loadport_AOP01 loadport1,
            Loadport_AOP01 loadport2, AOP01_Engineer engineer, RFID_Brooks rfid1, RFID_Brooks rfid2)
        {
            m_engineer = engineer;
            m_handler = engineer.m_handler;
            m_rndrtr = rtr;
            m_rtrarm = m_rndrtr.p_aArm[0];
            m_arm = m_rndrtr.m_dicArm[0];
            m_loadport[0] = loadport1;
            m_loadport[1] = loadport2;
            m_mainvision = mainvision;
            m_backsidevision = backsidevision;
            m_rfid[0] = rfid1;
            m_rfid[1] = rfid2;
            loadportA.Init(m_handler.m_aLoadport[0], m_engineer, m_rfid[0]);
            loadportB.Init(m_handler.m_aLoadport[1], m_engineer, m_rfid[1]);
            LoadportA_State.DataContext = loadport1;
            LoadportB_State.DataContext = loadport2;
            RTR_State.DataContext = m_rndrtr;
            //progressBarSequence.DataContext = m_handler.m_process;
            //textblockSequence.DataContext = m_handler.m_process;
            Machine_State.DataContext = EQ.m_EQ;
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
        public string p_sLotElapsedTime = "";
        private void M_timer_Tick(object sender, EventArgs e)
        {
            CheckMainVisionState();
            ExistRTR.Background = m_arm.m_diCheckVac.p_bIn == true && m_rtrarm.p_infoWafer != null ? Brushes.SteelBlue : Brushes.LightGray;
            ExistVision.Background = (m_mainvision.m_diExistVision.p_bIn == true && m_mainvision.p_infoWafer != null)||
                (m_backsidevision.m_diExistVision.p_bIn == true && m_backsidevision.p_infoWafer != null) ? Brushes.SteelBlue : Brushes.LightGray;
            if (m_loadport[0].m_swLotTime.IsRunning) textblockRunTime1.Text = m_loadport[0].p_swLotTime;
            if (m_loadport[1].m_swLotTime.IsRunning) textblockRunTime2.Text = m_loadport[1].p_swLotTime;
            ButtonInitialize.IsEnabled = IsEnableInitialization();
            ButtonRecovery.IsEnabled = IsEnableRecovery();
            TimerLamp();
            if (EQ.p_eState != EQ.eState.Recovery)
                m_rndrtr.m_bRecovery = false;
        }
        #endregion
        #region Button Recovery
        bool IsEnableRecovery()
        {
            if (IsRunModule()) return false;
            if (IsErrorModule()) return false;
            if (m_handler.m_bIsPossible_Recovery == false) return false;
            // Daniel check
            if (EQ.p_eState != EQ.eState.Ready && EQ.p_eState != EQ.eState.Idle) return false;
            if (EQ.p_bStop == true) return false;
            return m_handler.IsEnableRecovery();
        }

        private void ButtonRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableRecovery() == false) return;
            m_handler.m_bIsPossible_Recovery = false;
            m_handler.CalcRecover();
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Recovery;
            m_rndrtr.m_bRecovery = true;
        }
        #endregion

        #region Button Initialization

        bool IsEnableInitialization()
        {
            if (!IsRunModule()) return true;

            switch (EQ.p_eState)
            {
                case EQ.eState.Run: return false;
                case EQ.eState.Recovery: return false;
                case EQ.eState.Home: return false;
            }
            return true;
        }

        bool IsRunModule()
        {
            if (IsRunModule(m_loadport[0]) || IsRunModule(m_loadport[1]) || IsRunModule(m_rndrtr) || IsRunModule(m_handler.m_mainVision) || IsRunModule(m_handler.m_backsideVision))
                return true;
            //if (IsRunModule(m_loadport[0])) return true;
            //if (IsRunModule(m_loadport[1])) return true;
            //if (IsRunModule(m_rtrcleanunit)) return true;
            //if (IsRunModule(m_handler.m_mainVision)) return true;
            //if (IsRunModule(m_handler.m_backsideVision)) return true;
            return false;
        }
        bool IsErrorModule()
        {
            if (IsErrorModule(m_loadport[0]) || IsErrorModule(m_loadport[1]) || IsErrorModule(m_rndrtr) || IsErrorModule(m_handler.m_mainVision) || IsErrorModule(m_handler.m_backsideVision))
                return true;
            else 
                return false;
        }
        bool IsErrorModule(ModuleBase module)
        {
            if (module.p_eState == ModuleBase.eState.Error) 
                return true;
            else 
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
            m_handler.m_process.m_nSequencePercent = 0;
            m_handler.m_process.ClearInfoWafer();
            m_handler.m_nRnR = 0; //Init 할때 RNR 카운트초기화
            m_handler.m_aLoadport[EQ.p_nRunLP].p_infoCarrier.p_eState = InfoCarrier.eState.Placed;  //210201 모니터링필요 EQ.Stop 되고 이닛누르면 간헐적으로 Loadport Docking 상태로 무언정지 생김
            EQ.p_eState = EQ.eState.Home;
        }
        #endregion

        #region Button Pause & Resume
        bool bRunning = true;
        bool IsEnablePause()
        {
            if (EQ.p_eState != EQ.eState.Run) return false;
            if (EQ.p_eState != EQ.eState.Recovery) return false;
            return true;
        }
        bool IsEnableResume()
        {
            if (EQ.p_eState != EQ.eState.Ready && EQ.p_eState != EQ.eState.Idle) return false;
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

        #region Button Buzzer, Door, Stop
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
            EQ.p_bStop = true; //수정 필요
        }
        #endregion
        void TimerLamp()
        {
            AOP01 aop01 = m_engineer.m_handler.m_aop01;
            gridLampR.Background = aop01.m_doLamp.ReadDO(AOP01.eLamp.Red) ? Brushes.OrangeRed : Brushes.LightGray;
            gridLampY.Background = aop01.m_doLamp.ReadDO(AOP01.eLamp.Yellow) ? Brushes.Gold : Brushes.LightGray;
            gridLampG.Background = aop01.m_doLamp.ReadDO(AOP01.eLamp.Green) ? Brushes.LightGreen : Brushes.LightGray;
        }

        public void CheckMainVisionState()
        {
            if (m_mainvision.p_eState == ModuleBase.eState.Error || m_backsidevision.p_eState == ModuleBase.eState.Error)
                MainVision_State.Text = "Error";
            else if (m_mainvision.p_eState == ModuleBase.eState.Run || m_backsidevision.p_eState == ModuleBase.eState.Run)
                MainVision_State.Text = "Run";
            else if (m_mainvision.p_eState == ModuleBase.eState.Home || m_backsidevision.p_eState == ModuleBase.eState.Home)
                MainVision_State.Text = "Home";
            else if (m_mainvision.p_eState == ModuleBase.eState.Init && m_backsidevision.p_eState == ModuleBase.eState.Init)
                MainVision_State.Text = "Init";
            else if (m_mainvision.p_eState == ModuleBase.eState.Ready && m_backsidevision.p_eState == ModuleBase.eState.Ready)
                MainVision_State.Text = "Ready";
        }
    }
}
