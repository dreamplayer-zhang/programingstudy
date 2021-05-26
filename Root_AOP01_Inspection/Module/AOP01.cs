using Root_EFEM.Module;
using RootTools;
using RootTools.Gem;
using RootTools.Comm;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.OHT.Semi;
using RootTools.OHTNew;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using static RootTools.Gem.XGem.XGem;

namespace Root_AOP01_Inspection.Module
{
    public class AOP01 : ModuleBase
    {
        #region ToolBox
        public enum eLamp
        {
            Red,
            Yellow,
            Green
        }
        enum eBuzzer
        {
            Buzzer1,
            Buzzer2,
            Buzzer3,
            Buzzer4,
            BuzzerOff
        }
        //eBuzzer m_eBuzzer = eBuzzer.BuzzerOff;
        string[] m_asLamp = Enum.GetNames(typeof(eLamp));
        string[] m_asBuzzer = Enum.GetNames(typeof(eBuzzer));
        public DIO_Os m_doLamp;


        DIO_Os m_doBuzzer;
        DIO_I m_diEMS;
        DIO_I m_diProtectionBar;
        DIO_I m_diMCReset;
        DIO_I m_diCDA1Low;
        DIO_I m_diCDA2Low;
        DIO_IO m_dioBuzzerOff;
        DIO_I m_diDoorLock;
        DIO_I m_diInterlock_Key;
        DIO_I m_diLightCurtain;
        DIO_I m_di4CH_LED_Cont_FAN;
        DIO_I m_di12CH_LED_Cont_FAN;
        DIO_I m_diPC_FAN;
        DIO_I m_diELECPNLDoor;
        DIO_I m_diETCDoor;
        DIO_I m_diPCDoor;
        DIO_I m_disideDoor;
        DIO_I m_diELECPNLDoorFan;
        DIO_I m_diETCDoorFan;
        public DIO_O m_doDoorLock_Use;


        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_doLamp, this, "Lamp", m_asLamp);
            p_sInfo = m_toolBox.GetDIO(ref m_doBuzzer, this, "Buzzer", m_asBuzzer, true, true);
            p_sInfo = m_toolBox.GetDIO(ref m_diEMS, this, "EMS");
            p_sInfo = m_toolBox.GetDIO(ref m_diProtectionBar, this, "ProtectionBar");
            p_sInfo = m_toolBox.GetDIO(ref m_diMCReset, this, "MC Reset");
            p_sInfo = m_toolBox.GetDIO(ref m_diCDA1Low, this, "CDA1 Low");
            p_sInfo = m_toolBox.GetDIO(ref m_diCDA2Low, this, "CDA2 Low");
            p_sInfo = m_toolBox.GetDIO(ref m_dioBuzzerOff, this, "Buzzer Off");
            p_sInfo = m_toolBox.GetDIO(ref m_diDoorLock, this, "Door Lock");
            p_sInfo = m_toolBox.GetDIO(ref m_diInterlock_Key, this, "InterLock Key");
            p_sInfo = m_toolBox.GetDIO(ref m_diLightCurtain, this, "Light Curtain");
            p_sInfo = m_toolBox.GetDIO(ref m_doDoorLock_Use, this, "Door Lock Use");

            p_sInfo = m_toolBox.GetDIO(ref m_di4CH_LED_Cont_FAN, this, "4CH LED Cont FAN");
            p_sInfo = m_toolBox.GetDIO(ref m_di12CH_LED_Cont_FAN, this, "12CH LED Cont FAN");
            p_sInfo = m_toolBox.GetDIO(ref m_diPC_FAN, this, "PC FAN");
            p_sInfo = m_toolBox.GetDIO(ref m_diELECPNLDoor, this, "ELEC PNL Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diETCDoor, this, "ETC Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diPCDoor, this, "PC Door");
            p_sInfo = m_toolBox.GetDIO(ref m_disideDoor, this, "Side Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diELECPNLDoorFan, this, "ELEC PNL Door Fan");
            p_sInfo = m_toolBox.GetDIO(ref m_diETCDoorFan, this, "ETC Door Fan");



            if (bInit) InitGAF();
        }
        #endregion
        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeLamp(tree.GetTree("Lamp", false));
        }
        eLamp m_eLampError = eLamp.Red;
        eLamp m_eLampRun = eLamp.Green;
        eLamp m_eLampReady = eLamp.Yellow;
        void RunTreeLamp(Tree tree)
        {
            m_eLampRun = (eLamp)tree.Set(m_eLampRun, m_eLampRun, "Run Lamp Set", "Run Lamp Set");
            m_eLampReady = (eLamp)tree.Set(m_eLampReady, m_eLampReady, "Ready Lamp Set", "Ready Lamp Set");
            m_eLampError = (eLamp)tree.Set(m_eLampError, m_eLampError, "Error Lamp Set", "Error Lamp Set");
        }
        
        #endregion
        #region GAF
        //ALID 생성
        ALID m_alidEMS;
        ALID m_alidEMO;
        ALID m_alidProtectionBar;
        ALID m_alidMCReset;
        ALID m_alidCDA1Low;
        ALID m_alidCDA2Low;
        ALID m_alidDoorLock;
        ALID m_alidLightCurtain;
        ALID m_alid4CH_LED_Cont_FAN;
        ALID m_alid12CH_LED_Cont_FAN;
        ALID m_alidPC_FAN;
        ALID m_alidELECPNLDoor;
        ALID m_alidETCDoor;
        ALID m_alidPCDoor;
        ALID m_alidsideDoor;
        ALID m_alidELECPNLDoorFan;
        ALID m_alidETCDoorFan;
        ALID m_alidETCError;

        //SVID 생성
        SVID m_svidMCReset;
        SVID m_svidEmergency;
        SVID m_svidDoorlock;
        SVID m_svidInterlockKeySW;
        SVID m_svidCDA1;
        SVID m_svidCDA2;
        SVID m_svidLightCurtain;
        SVID m_svidProtectionBar;
        SVID m_svid4CHFanAlarm;
        SVID m_svid12CHFanAlarm;
        SVID m_svidPCFanAlarm;
        SVID m_svidGuidePinFanAlarm;
        SVID m_svidElecPanelDoorOpen;
        SVID m_svidETCDoorOpen;
        SVID m_svidPCDoorOpen;
        SVID m_svidSideDoorOpen;
        SVID m_svidElecPanelFanAlarm;
        SVID m_svidETCDoorFanAlarm;
        SVID m_svidCDA1Power;
        SVID m_svidCDA2Power;
        SVID m_svidStageTemperature;
        SVID m_svidStageElectrostatic;
        SVID m_svidDoorLockStatus;
        SVID m_svidCPURate1;
        SVID m_svidMemoryRate1;
        SVID m_svidCPURate2;
        SVID m_svidMemoryRate2;

        void InitGAF()
        {
            m_alidEMS = m_gaf.GetALID(this, "EMS", "EMS Error");
            m_alidEMO = m_gaf.GetALID(this, "EMO", "EMO Error");
            m_alidProtectionBar = m_gaf.GetALID(this, "ProtectionBar", "ProtectionBar Error");
            m_alidMCReset = m_gaf.GetALID(this, "MC Reset", "MC Reset Error");
            m_alidCDA1Low = m_gaf.GetALID(this, "CDA1 Low", "CDA1 Low Error");
            m_alidCDA2Low = m_gaf.GetALID(this, "CDA2 Low", "CDA2 Low Error");
            m_alidDoorLock = m_gaf.GetALID(this, "Door Lock", "Door Lock Error");
            m_alidLightCurtain = m_gaf.GetALID(this, "Light Curtain", "Light Curtain Error");
            m_alid4CH_LED_Cont_FAN = m_gaf.GetALID(this, "4CH_LED_Cont_FAN", "4CH_LED_Cont_FAN Error");
            m_alid12CH_LED_Cont_FAN = m_gaf.GetALID(this, "12CH_LED_Cont_FAN", "12CH_LED_Cont_FAN Error");
            m_alidPC_FAN = m_gaf.GetALID(this, "PC_FAN", "PC_FAN Error");
            m_alidELECPNLDoor = m_gaf.GetALID(this, "ELEC PNL Door", "ELECPNLDoor Error");
            m_alidETCDoor = m_gaf.GetALID(this, "ETC Door", "ETC Door Error");
            m_alidPCDoor = m_gaf.GetALID(this, "PC Door", "PCDoor Error");
            m_alidsideDoor = m_gaf.GetALID(this, "Side Door", "Side Door Error");
            m_alidELECPNLDoorFan = m_gaf.GetALID(this, "ELEC PNL Door Fan", "ELEC PNL Door Fan Error");
            m_alidETCDoorFan = m_gaf.GetALID(this, "ETC Door Fan", "ETC Door Fan Error");
            m_alidETCError = m_gaf.GetALID(this, "Fan", "Fan Error");

            m_svidMCReset = m_gaf.GetSVID(this, "M/C Reset");
            m_svidEmergency = m_gaf.GetSVID(this, "Emergency");
            m_svidDoorlock = m_gaf.GetSVID(this, "Door Lock");
            m_svidInterlockKeySW = m_gaf.GetSVID(this, "Interlock Key SW");
            m_svidCDA1 = m_gaf.GetSVID(this, "CDA1");
            m_svidCDA2 = m_gaf.GetSVID(this, "CDA2");
            m_svidLightCurtain = m_gaf.GetSVID(this, "Light Curtain");
            m_svidProtectionBar = m_gaf.GetSVID(this, "Protection Bar");
            m_svid4CHFanAlarm = m_gaf.GetSVID(this, "4CH Light Fan Alarm");
            m_svid12CHFanAlarm = m_gaf.GetSVID(this, "12CH Light Fan Alarm");
            m_svidPCFanAlarm = m_gaf.GetSVID(this, "PC Fan Alarm");
            m_svidGuidePinFanAlarm = m_gaf.GetSVID(this, "Guide Pin FFU Alarm");
            m_svidElecPanelDoorOpen = m_gaf.GetSVID(this, "Elec Panel Door Open");
            m_svidETCDoorOpen = m_gaf.GetSVID(this, "ETC Door Open");
            m_svidPCDoorOpen = m_gaf.GetSVID(this, "PC Door Open");
            m_svidSideDoorOpen = m_gaf.GetSVID(this, "Side Door Open");
            m_svidElecPanelFanAlarm = m_gaf.GetSVID(this, "Elec Panel Fan Alarm");
            m_svidETCDoorFanAlarm = m_gaf.GetSVID(this, "ETC Door Fan Alarm");
            m_svidCDA1Power = m_gaf.GetSVID(this, "CDA1 Input Power");
            m_svidCDA2Power = m_gaf.GetSVID(this, "CDA2 Input Power");
            m_svidStageTemperature = m_gaf.GetSVID(this, "Stage Temperature");
            m_svidStageElectrostatic = m_gaf.GetSVID(this, "Stage Elec Static");
            m_svidDoorLockStatus = m_gaf.GetSVID(this, "Door Lock Status");
            m_svidCPURate1 = m_gaf.GetSVID(this, "PC1 CPU Utilization");
            m_svidMemoryRate1 = m_gaf.GetSVID(this, "PC1 Memory Utilization");
            m_svidCPURate2 = m_gaf.GetSVID(this, "PC2 CPU Utilization");
            m_svidMemoryRate2 = m_gaf.GetSVID(this, "PC2 Memory Utilization");
        }

        void UpdateSVID()
        {
            m_svidMCReset.p_value = m_diMCReset.p_bIn;
            m_svidEmergency.p_value = m_diEMS.p_bIn;
            m_svidDoorlock.p_value = m_diDoorLock.p_bIn;
            m_svidInterlockKeySW.p_value = m_diInterlock_Key.p_bIn;
            m_svidCDA1.p_value = m_diCDA1Low.p_bIn;
            m_svidCDA2.p_value = m_diCDA2Low.p_bIn;
            m_svidLightCurtain.p_value = m_diLightCurtain.p_bIn;
            m_svidProtectionBar.p_value = m_diProtectionBar.p_bIn;
            m_svid4CHFanAlarm.p_value = m_di4CH_LED_Cont_FAN.p_bIn;
            m_svid12CHFanAlarm.p_value = m_di12CH_LED_Cont_FAN.p_bIn;
            m_svidPCFanAlarm.p_value = m_diPC_FAN.p_bIn;
            m_svidGuidePinFanAlarm.p_value = false;
            m_svidElecPanelDoorOpen.p_value = m_diELECPNLDoor.p_bIn;
            m_svidETCDoorOpen.p_value = m_diETCDoor.p_bIn;
            m_svidPCDoorOpen.p_value = m_diPCDoor.p_bIn;
            m_svidSideDoorOpen.p_value = m_disideDoor.p_bIn;
            m_svidElecPanelFanAlarm.p_value = m_diELECPNLDoorFan.p_bIn;
            m_svidETCDoorFanAlarm.p_value = m_diETCDoorFan.p_bIn;
            m_svidCDA1Power.p_value = false;
            m_svidCDA2Power.p_value = false;
            m_svidStageTemperature.p_value = false;
            m_svidStageElectrostatic.p_value = false;
            m_svidDoorLockStatus.p_value = false;
            m_svidCPURate1.p_value = false;
            m_svidMemoryRate1.p_value = false;
            m_svidCPURate2.p_value = false;
            m_svidMemoryRate2.p_value = false;

        }
        #endregion

        #region Thread
        public EQ.eState m_eStatus = EQ.eState.Init;
        //int m_nLamp_count = 0;
        public bool m_bDoorAlarm = true;
        
        protected override void RunThread()
        {
            base.RunThread();
            if (!EQ.p_bSimulate)
            {
                if (m_eStatus != EQ.p_eState)
                {
                    switch (EQ.p_eState)
                    {
                        case EQ.eState.Error:
                            m_doDoorLock_Use.Write(false);
                            m_doBuzzer.Write(eBuzzer.Buzzer2);
                            m_doLamp.Write(m_eLampError);
                            break;
                        case EQ.eState.Run:
                        case EQ.eState.Recovery:
                            m_doDoorLock_Use.Write(true);
                            m_doLamp.Write(m_eLampRun);
                            break;
                        case EQ.eState.Home:
                            m_doBuzzer.Write(eBuzzer.Buzzer4);
                            m_doLamp.Write(m_eLampRun);
                            break;
                        case EQ.eState.Ready:
                        case EQ.eState.Idle:
                            m_doDoorLock_Use.Write(false);
                            BuzzerOff();
                            m_doLamp.Write(m_eLampReady);
                            break;
                        case EQ.eState.Init:
                            m_doDoorLock_Use.Write(false);
                            BuzzerOff();
                            m_doLamp.Write(m_eLampReady);
                            break;
                    }
                    m_eStatus = EQ.p_eState;
                }

                UpdateSVID();

                if (m_dioBuzzerOff.p_bIn || (m_gaf.m_listALID.p_aALID.Count < 1))
                    BuzzerOff();

                if (!m_diEMS.p_bIn)
                {
                    if (!m_diCDA1Low.p_bIn && !m_diCDA2Low.p_bIn)
                        m_alidEMS.Run(!m_diEMS.p_bIn, "EMS Error");
                    else
                        m_alidEMO.Run(!m_diEMS.p_bIn, "EMO Error");
                    m_alidMCReset.Run(!m_diMCReset.p_bIn, "Please Check State of the M/C Reset Button.");
                }
                else
                {
                    m_alidMCReset.Run(!m_diMCReset.p_bIn, "Please Check State of the M/C Reset Button.");
                    m_alidProtectionBar.Run(!m_diProtectionBar.p_bIn, "Please Check State of Protection Bar.");
                    m_alidCDA1Low.Run(m_diCDA1Low.p_bIn, "Please Check Value of CDA1");
                    m_alidCDA2Low.Run(m_diCDA2Low.p_bIn, "Please Check Value of CDA2");
                    m_alid4CH_LED_Cont_FAN.Run(!m_di4CH_LED_Cont_FAN.p_bIn, "Please Check 4CH LED FAN");
                    m_alid12CH_LED_Cont_FAN.Run(!m_di12CH_LED_Cont_FAN.p_bIn, "Please Check 12CH LED FAN");
                    m_alidPC_FAN.Run(!m_diPC_FAN.p_bIn, "Please Check PC FAN");
                    m_alidELECPNLDoorFan.Run(!m_diELECPNLDoorFan.p_bIn, "Please Check ELEC PNL DoorFan");
                    m_alidETCDoorFan.Run(!m_diETCDoorFan.p_bIn, "Please Check ETC DoorFan");
                    if (m_bDoorAlarm)
                    {
                        m_alidELECPNLDoor.Run(m_diELECPNLDoor.p_bIn, "Please Check ELEC PNL Door Open");
                        m_alidETCDoor.Run(!m_diETCDoor.p_bIn, "Please Check ETC Door Open");
                        m_alidPCDoor.Run(m_diPCDoor.p_bIn, "Please Check PC Door Open");
                        m_alidsideDoor.Run(!m_disideDoor.p_bIn, "Please Check Side Door Open");
                    }
                    if (m_diInterlock_Key.p_bIn)
                    {
                        m_alidDoorLock.Run(! m_diDoorLock.p_bIn, "Please Check the Doors");
                    }
                    m_alidLightCurtain.Run(!m_diLightCurtain.p_bIn, "Please Check LightCurtain");
                    foreach (OHT_Semi OHT in p_aOHT)
                    {
                        OHT.p_bLightCurtain = !m_diLightCurtain.p_bIn;
                        OHT.P_bProtectionBar = !m_diProtectionBar.p_bIn;
                    }
                }
            }
        }
        #endregion

        #region BuzzerOff
        public string BuzzerOff()
        {
            m_doBuzzer.Write(eBuzzer.Buzzer1, false);
            m_doBuzzer.Write(eBuzzer.Buzzer2, false);
            m_doBuzzer.Write(eBuzzer.Buzzer3, false);
            m_doBuzzer.Write(eBuzzer.Buzzer4, false);
            return "OK";
        }
        #endregion


        #region OHT
        List<OHT_Semi> p_aOHT
        {
            get
            {
                List<OHT_Semi> aOHT = new List<OHT_Semi>();
                AOP01_Handler handler = (AOP01_Handler)m_engineer.ClassHandler();
                foreach (ILoadport loadport in handler.m_aLoadport)
                {
                    aOHT.Add(((Loadport_AOP01)loadport).m_OHT);
                }
                return aOHT;
            }
        }
        #endregion

        public InfoCarrier p_infoCarrier { get; set; }


        public AOP01(string id, IEngineer engineer)
        {
            p_id = id;
            base.InitBase(id, engineer);
            p_infoCarrier = new InfoCarrier(this, id, engineer, true, true);
            InitTimer();
        }

        

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region Show OHT
        public void ShowOHT()
        {
            m_timer.Start();
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(10);
            m_timer.Tick += M_timer_Tick;
        }
        OHTs_UI m_uiOHT;
        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_timer.Stop();
            m_uiOHT = new OHTs_UI();
            m_uiOHT.Init((AOP01_Handler)m_engineer.ClassHandler());
            m_uiOHT.Show();
        }
        #endregion
    }
}
