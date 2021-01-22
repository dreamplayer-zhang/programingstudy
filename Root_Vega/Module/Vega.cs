using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Text;
using System.Threading;

namespace Root_Vega.Module
{
    public class Vega : ModuleBase
    {
        #region ToolBox
        public enum eLamp
        { 
            Red,
            Yellow,
            Green
        }
        public enum eIonizer
        {
            LP1,
            LP2
        }
        public enum eBuzzer
        { 
            Buzzer1,
            Buzzer2,
            Buzzer3,
            Buzzer4,
        }
        bool _eIonizerState = false;

        string[] m_asLamp = Enum.GetNames(typeof(eLamp));
        string[] m_asIonizer = Enum.GetNames(typeof(eIonizer));
        string[] m_asBuzzer = Enum.GetNames(typeof(eBuzzer));
        public DIO_Os m_doLamp;
        public DIO_Os m_doBuzzer;
        DIO_I m_diEMS;
        DIO_I m_diProtectionBar;
        DIO_I m_diMCReset;
        DIO_I m_diCDALow;
        DIO_I m_diVaccumLow;
        public DIO_IO m_dioBuzzerOffButton;
        public DIO_I m_diDoorLock;
        DIO_I m_diInterlock_Key;
        public DIO_O m_doDoorLock_Use;
        DIO_Is m_diIonizerAlarmCheck;
        DIO_I m_diElecPnl_1_FanAlarm;
        DIO_I m_diElecPnl_2_FanAlarm;
        DIO_I m_diPCPnl_FanAlarm;
        DIO_I m_diPC_FanAlarm;
        DIO_I m_di12chLED1FanAlarm;
        DIO_I m_di12chLED2FanAlarm;
        public DIO_O m_doIonizerOnOff;


        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_doLamp, this, "Lamp", m_asLamp);
            p_sInfo = m_toolBox.Get(ref m_doBuzzer, this, "Buzzer", m_asBuzzer, true, true);
            p_sInfo = m_toolBox.Get(ref m_diEMS, this, "EMS");
            p_sInfo = m_toolBox.Get(ref m_diProtectionBar, this, "ProtectionBar");
            p_sInfo = m_toolBox.Get(ref m_diMCReset, this, "MC Reset");
            p_sInfo = m_toolBox.Get(ref m_diCDALow, this, "CDA Low");
            p_sInfo = m_toolBox.Get(ref m_diVaccumLow, this, "Vaccum Low");
            p_sInfo = m_toolBox.Get(ref m_dioBuzzerOffButton, this, "Buzzer Off Button");
            p_sInfo = m_toolBox.Get(ref m_diDoorLock, this, "Door Lock");
            p_sInfo = m_toolBox.Get(ref m_doDoorLock_Use, this, "Door Lock Use");
            p_sInfo = m_toolBox.Get(ref m_diIonizerAlarmCheck, this, "Ionizer Alarm Check",m_asIonizer,true,true);
            p_sInfo = m_toolBox.Get(ref m_diElecPnl_1_FanAlarm, this, "Elec Panel 1 Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diElecPnl_2_FanAlarm, this, "Elec Panel 2 Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diPCPnl_FanAlarm, this, "PC Panel Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diPC_FanAlarm, this, "PC Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_diInterlock_Key, this, "InterLock Key");
            p_sInfo = m_toolBox.Get(ref m_di12chLED1FanAlarm, this, "12Ch LED 1 Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_di12chLED2FanAlarm, this, "12Ch LED 2 Fan Alarm");
            p_sInfo = m_toolBox.Get(ref m_doIonizerOnOff, this, "Ionizer On/Off");
            p_sInfo = m_RFID.GetTools(this, bInit);

            if (bInit)
            {
                InitALID();
                m_doIonizerOnOff.Write(false);
            }
        }
        #endregion

        #region GAF
        //ALID 생성
        ALID m_alidEMS;
        ALID m_alidProtectionBar;
        ALID m_alidMCReset;
        ALID m_alidCDALow;
        ALID m_alidVaccumLow;
        ALID m_alidElecPnl_1_FanAlarm;
        ALID m_alidElecPnl_2_FanAlarm;
        ALID m_alidPCPnl_FanAlarm;
        ALID m_alidPC_FanAlarm;
        ALID m_alidLED1FanAlarm;
        ALID m_alidLED2FanAlarm;
        ALID m_alidDoorLock;
        ALID m_alidIonizerAlarm;
        ALID m_alidRecipeLoadAlarm;
        ALID m_alidFeatureSearchFailAlarm;
        ALID m_alidAlignFailAlarm;

        void InitALID()
        {
			m_alidEMS = m_gaf.GetALID(this, "EMS", "EMS Error");
			m_alidProtectionBar = m_gaf.GetALID(this, "ProtectionBar", "ProtectionBar Error");
            m_alidMCReset = m_gaf.GetALID(this, "MC Reset", "MC Reset Error");
            m_alidCDALow = m_gaf.GetALID(this, "CDA Low", "CDA Low Error");
            m_alidVaccumLow = m_gaf.GetALID(this, "Vaccum Low", "Vaccum Low");
            m_alidDoorLock = m_gaf.GetALID(this, "Door Lock", "Door Lock Error");
            m_alidIonizerAlarm = m_gaf.GetALID(this, "Ionizer State Alarm", "Ionizer State Error");
            m_alidElecPnl_1_FanAlarm = m_gaf.GetALID(this, "ElecPanel 1 Fan Alarm", "Elect Panel 1 Fan Alarm");
            m_alidElecPnl_2_FanAlarm = m_gaf.GetALID(this, "Elecanel 2 Fan Alarm", "Elect Panel 2 Fan Alarm");
            m_alidPCPnl_FanAlarm = m_gaf.GetALID(this, "PCPanel Fan Alarm", "PC Panel Fan Alarm");
            m_alidPC_FanAlarm = m_gaf.GetALID(this, "PC Fan Alarm", "PC Fan Alarm");
            m_alidRecipeLoadAlarm = m_gaf.GetALID(this, "Recipe Not Loaded Alarm", "Recipe Not Loaded Alarm");
            m_alidFeatureSearchFailAlarm = m_gaf.GetALID(this, "Feature Search Fail Alarm", "Feature Search Fail Alarm");
            m_alidAlignFailAlarm = m_gaf.GetALID(this, "Align Fail Alarm", "Align Fail Alarm");
        }
        #endregion

        #region Thread
        public EQ.eState m_eStatus = EQ.eState.Init;
        public StopWatch m_swIonizerOn;
        protected override void RunThread()
        {
            base.RunThread();
			#region Lamp & Alarm
			if (m_eStatus != EQ.p_eState)
			{
				switch (EQ.p_eState)
				{
					case EQ.eState.Error:
                        m_doDoorLock_Use.Write(false);
                        //m_doBuzzer.Write(eBuzzer.Buzzer2);
                        m_doLamp.Write(eLamp.Red);
						break;
					case EQ.eState.Run:
                        m_doDoorLock_Use.Write(true);
                        //m_doBuzzer.Write(eBuzzer.Buzzer4);
                        m_doLamp.Write(eLamp.Green);
                        break;
					case EQ.eState.Home: 
                        //m_doBuzzer.Write(eBuzzer.Buzzer3);
                        m_doLamp.Write(eLamp.Green);
                        break;
					case EQ.eState.Ready:
                        m_doDoorLock_Use.Write(false);
                        BuzzerOff();
                        m_doLamp.Write(eLamp.Yellow);
                        break;
					case EQ.eState.Init:
                        m_doDoorLock_Use.Write(false);
                        BuzzerOff();
                        m_doLamp.Write(eLamp.Yellow);
                        break;
				}
				m_eStatus = EQ.p_eState;
			}
			if (m_dioBuzzerOffButton.p_bIn || (m_gaf.m_listALID.p_aALID.Count < 1))
                BuzzerOff();
            #endregion

            #region Interlock
            if (m_swIonizerOn.ElapsedMilliseconds > 30000)
            {
                m_doIonizerOnOff.Write(false);
            }
            else if (m_swIonizerOn.IsRunning && m_swIonizerOn.ElapsedMilliseconds > 2000)
            {
                m_alidIonizerAlarm.Run(!m_diIonizerAlarmCheck.ReadDI(eIonizer.LP1) || !m_diIonizerAlarmCheck.ReadDI(eIonizer.LP2), "Please Check State of X-ray Ionizer");
            }
            if (m_bDoorlock_Use && m_diInterlock_Key.p_bIn)
            {
                m_alidDoorLock.Run(!m_diDoorLock.p_bIn, "Please Check the Doors");
            }
            m_alidEMS.Run(!m_diEMS.p_bIn, "Please Check the Emergency Buttons");
			m_alidProtectionBar.Run(m_diProtectionBar.p_bIn, "Please Check State of Protection Bar");
            if (m_robot != null)
            {
                if (!m_diMCReset.p_bIn) m_robot.p_bDisableHomeWhenArmOpen = true;
            }
            m_alidMCReset.Run(!m_diMCReset.p_bIn, "Please Check State of the M/C Reset Button");
			m_alidCDALow.Run(m_diCDALow.p_bIn, "Please Check Value of CDA");
            m_alidVaccumLow.Run(m_diVaccumLow.p_bIn, "Please Check Value of Vaccum");
            
            m_alidElecPnl_1_FanAlarm.Run(!m_diElecPnl_1_FanAlarm.p_bIn, "Please Check Electronic Panel 1 Fan");
            m_alidElecPnl_2_FanAlarm.Run(!m_diElecPnl_2_FanAlarm.p_bIn, "Please Check Electronic Panel 1 Fan");
            m_alidPCPnl_FanAlarm.Run(!m_diPCPnl_FanAlarm.p_bIn, "Please Check PC Panel Fan");
            m_alidPC_FanAlarm.Run(!m_diPC_FanAlarm.p_bIn, "Please Check PC Fan");
            m_alidRecipeLoadAlarm.Run(!App.m_engineer.m_recipe.Loaded, "Please Load Recipe");
            m_alidFeatureSearchFailAlarm.Run(App.m_engineer.m_InspManager.m_bFeatureSearchFail, "Feature Search Failed...");
            m_alidAlignFailAlarm.Run(App.m_engineer.m_InspManager.m_bAlignFail, "Align Failed...");
            #endregion
        }
        #endregion

        #region
        public string BuzzerOff()
        {
            m_doBuzzer.Write(eBuzzer.Buzzer1, false);
            m_doBuzzer.Write(eBuzzer.Buzzer2, false);
            m_doBuzzer.Write(eBuzzer.Buzzer3, false);
            m_doBuzzer.Write(eBuzzer.Buzzer4, false);
            return "OK";
        }
		#endregion

		#region RFID
		public class RFID
        {
            #region CRC
            ushort[] m_uCRC =
            {
                0x0000,    0x1021,    0x2042,    0x3063,    0x4084,    0x50a5,    0x60c6,    0x70e7,
                0x8108,    0x9129,    0xa14a,    0xb16b,    0xc18c,    0xd1ad,    0xe1ce,    0xf1ef,
                0x1231,    0x0210,    0x3273,    0x2252,    0x52b5,    0x4294,    0x72f7,    0x62d6,
                0x9339,    0x8318,    0xb37b,    0xa35a,    0xd3bd,    0xc39c,    0xf3ff,    0xe3de,
                0x2462,    0x3443,    0x0420,    0x1401,    0x64e6,    0x74c7,    0x44a4,    0x5485,
                0xa56a,    0xb54b,    0x8528,    0x9509,    0xe5ee,    0xf5cf,    0xc5ac,    0xd58d,
                0x3653,    0x2672,    0x1611,    0x0630,    0x76d7,    0x66f6,    0x5695,    0x46b4,
                0xb75b,    0xa77a,    0x9719,    0x8738,    0xf7df,    0xe7fe,    0xd79d,    0xc7bc,
                0x48c4,    0x58e5,    0x6886,    0x78a7,    0x0840,    0x1861,    0x2802,    0x3823,
                0xc9cc,    0xd9ed,    0xe98e,    0xf9af,    0x8948,    0x9969,    0xa90a,    0xb92b,
                0x5af5,    0x4ad4,    0x7ab7,    0x6a96,    0x1a71,    0x0a50,    0x3a33,    0x2a12,
                0xdbfd,    0xcbdc,    0xfbbf,    0xeb9e,    0x9b79,    0x8b58,    0xbb3b,    0xab1a,
                0x6ca6,    0x7c87,    0x4ce4,    0x5cc5,    0x2c22,    0x3c03,    0x0c60,    0x1c41,
                0xedae,    0xfd8f,    0xcdec,    0xddcd,    0xad2a,    0xbd0b,    0x8d68,    0x9d49,
                0x7e97,    0x6eb6,    0x5ed5,    0x4ef4,    0x3e13,    0x2e32,    0x1e51,    0x0e70,
                0xff9f,    0xefbe,    0xdfdd,    0xcffc,    0xbf1b,    0xaf3a,    0x9f59,    0x8f78,
                0x9188,    0x81a9,    0xb1ca,    0xa1eb,    0xd10c,    0xc12d,    0xf14e,    0xe16f,
                0x1080,    0x00a1,    0x30c2,    0x20e3,    0x5004,    0x4025,    0x7046,    0x6067,
                0x83b9,    0x9398,    0xa3fb,    0xb3da,    0xc33d,    0xd31c,    0xe37f,    0xf35e,
                0x02b1,    0x1290,    0x22f3,    0x32d2,    0x4235,    0x5214,    0x6277,    0x7256,
                0xb5ea,    0xa5cb,    0x95a8,    0x8589,    0xf56e,    0xe54f,    0xd52c,    0xc50d,
                0x34e2,    0x24c3,    0x14a0,    0x0481,    0x7466,    0x6447,    0x5424,    0x4405,
                0xa7db,    0xb7fa,    0x8799,    0x97b8,    0xe75f,    0xf77e,    0xc71d,    0xd73c,
                0x26d3,    0x36f2,    0x0691,    0x16b0,    0x6657,    0x7676,    0x4615,    0x5634,
                0xd94c,    0xc96d,    0xf90e,    0xe92f,    0x99c8,    0x89e9,    0xb98a,    0xa9ab,
                0x5844,    0x4865,    0x7806,    0x6827,    0x18c0,    0x08e1,    0x3882,    0x28a3,
                0xcb7d,    0xdb5c,    0xeb3f,    0xfb1e,    0x8bf9,    0x9bd8,    0xabbb,    0xbb9a,
                0x4a75,    0x5a54,    0x6a37,    0x7a16,    0x0af1,    0x1ad0,    0x2ab3,    0x3a92,
                0xfd2e,    0xed0f,    0xdd6c,    0xcd4d,    0xbdaa,    0xad8b,    0x9de8,    0x8dc9,
                0x7c26,    0x6c07,    0x5c64,    0x4c45,    0x3ca2,    0x2c83,    0x1ce0,    0x0cc1,
                0xef1f,    0xff3e,    0xcf5d,    0xdf7c,    0xaf9b,    0xbfba,    0x8fd9,    0x9ff8,
                0x6e17,    0x7e36,    0x4e55,    0x5e74,    0x2e93,    0x3eb2,    0x0ed1,    0x1ef0
            };

            ushort CalcCRC(byte[] aByte, int nCount)
            {
                ushort uCRC = 0x0000;
                for (int n = 0; n < nCount; n++)
                {
                    uCRC = (ushort)((uCRC << 8) ^ m_uCRC[((uCRC >> 8) ^ aByte[n]) & 0xff]);
                }
                return uCRC;
            }
            #endregion

            #region RS232
            RS232 m_rs232;
            public string GetTools(ModuleBase module, bool bInit)
            {
                string sInfo = module.m_toolBox.Get(ref m_rs232, module, "RFID RS232");
                if (bInit)
                {
                    m_rs232.OnReceive += M_rs232_OnReceive;
                    m_rs232.p_bConnect = true;
                }
                return sInfo;
            }

            string m_sRFID = ""; 
            private void M_rs232_OnReceive(string sRead)
            {
                if (sRead.Length < 14) return;
                byte[] aBuf = Encoding.ASCII.GetBytes(sRead);
                if (aBuf[10] != 0x03) return;
                if (aBuf[11] != 0x01) return;
                if (aBuf[12] != 0x01) return;
                m_sRFID = ""; 
                for (int i = sRead.Length - 5; i > 12; i--) m_sRFID += sRead[i];
                m_bOnRead = false; 
            }

            StopWatch m_swRead = new StopWatch(); 
            bool m_bOnRead = false; 
            public string ReadRFID(byte nCh, out string sRFID)
            {
                m_swRead.Restart(); 
                m_bOnRead = true; 
                m_aCmd[5] = nCh;
                CalcSend();
                m_rs232.m_sp.Write(m_aSend, 0, 15); 
                while (m_swRead.ElapsedMilliseconds < 10000)
                {
                    if (m_bOnRead == false)
                    {
                        sRFID = m_sRFID;
                        return "OK"; 
                    }
                    Thread.Sleep(20); 
                }
                sRFID = ""; 
                return "RFID Read Fail : Timeout"; 
            }
            #endregion

            #region Command Buffer
            byte[] m_aCmd = { 0x02, 0x01, 0x14, 0x00, 0x04, 0x00, 0x03, 0x01, 0x01 };
            byte[] m_aSend = { 0x10, 0x02, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xcc, 0xcc, 0x10, 0x03 };

            void CalcSend()
            {
                for (int n = 0; n < m_aCmd.Length; n++) m_aSend[2 + n] = m_aCmd[n];
                ushort uCRC = CalcCRC(m_aCmd, m_aCmd.Length);
                m_aSend[m_aCmd.Length + 2] = Convert.ToByte((uCRC >> 8) & 0x00ff);
                m_aSend[m_aCmd.Length + 3] = Convert.ToByte(uCRC & 0x00ff);
            }
            #endregion
        }
        public RFID m_RFID = new RFID();
        #endregion

        #region Tree
        bool m_bDoorlock_Use = false;
        public override void RunTree(Tree tree)
        {
            RunTreeInterLock(tree.GetTree("InterLock", false));
            base.RunTree(tree);
            //
        }
        void RunTreeInterLock(Tree tree)
        {
            m_bDoorlock_Use = tree.Set(m_bDoorlock_Use, m_bDoorlock_Use, "Doorlock Use", "Doorlock Use");
        }
        #endregion

        Robot_RND m_robot;
        public Vega(string id, IEngineer engineer)
        {
            p_id = id;
            base.InitBase(id, engineer);
            m_robot = ((Vega_Handler)engineer.ClassHandler()).m_robot;
            m_dioBuzzerOffButton.Write(true);
            m_swIonizerOn = new StopWatch();
            m_swIonizerOn.Stop();
        }

        public override void ThreadStop()
        {
            m_dioBuzzerOffButton.Write(false);
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_ReadRFID(this), false, "Read RFID");
        }

        public class Run_ReadRFID : ModuleRunBase
        {
            Vega m_module;
            public Run_ReadRFID(Vega module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_nCh = 1;
            string m_sSimulCarrierID = "CarrierID";
            public override ModuleRunBase Clone()
            {
                Run_ReadRFID run = new Run_ReadRFID(m_module);
                run.m_nCh = m_nCh;
                run.m_sSimulCarrierID = m_sSimulCarrierID;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nCh = tree.Set(m_nCh, m_nCh, "Channel", "RFID Channel");
                m_sSimulCarrierID = tree.Set(m_sSimulCarrierID, m_sSimulCarrierID, "Simulation CarrierID", "CarrierID When p_bSimulation", bVisible && EQ.p_bSimulate);
            }

            public override string Run()
            {
                string sResult = "OK";
                string sCarrierID = "";
                if (EQ.p_bSimulate) sCarrierID = m_sSimulCarrierID;
                else
                {
                    sResult = m_module.m_RFID.ReadRFID((byte)m_nCh, out sCarrierID);
                    m_module.m_log.Info("Read RFID : " + sCarrierID);
                    StringBuilder sHex = new StringBuilder(); 
                    foreach (char ch in sCarrierID) sHex.AppendFormat("{0:x2} ", ch);
                    m_module.m_log.Info("ReaHex RFID : " + sHex.ToString());
                }
                return sResult;
            }
        }
        #endregion
    }
}
