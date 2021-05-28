using RootTools;
using RootTools.Comm;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;
using RootTools.Control;
using RootTools.OHT.Semi;
using RootTools.OHTNew;
using Root_EFEM.Module;
using System;
using System.Windows.Threading;
using static RootTools.Gem.XGem.XGem;

namespace Root_AOP01_Inspection.Module
{
    public class Loadport_AOP01 : ModuleBase, IWTRChild, ILoadport
    {
        ALID m_alid_WaferExist;
        public void SetAlarm()
        {
            m_alid_WaferExist.Run(true, "Aligner Wafer Exist Error");
        }
        #region ToolBox
        RS232 m_rs232;
        public DIO_I m_diPlaced;
        public DIO_I m_diPresent;
        public DIO_I m_diOpen;
        public DIO_I m_diClose;
        public DIO_I m_diReady;
        public DIO_I m_diRun;
        OHT _OHT;
        public OHT m_OHTNew
        {
            get { return _OHT; }
            set
            {
                _OHT = value;
            }
        }
        public OHT_Semi m_OHT;
        public bool m_bLoadCheck = false;
        public bool m_bUnLoadCheck = false;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_diPlaced, this, "Place");
            p_sInfo = m_toolBox.GetDIO(ref m_diPresent, this, "Present");
            p_sInfo = m_toolBox.GetDIO(ref m_diOpen, this, "Open");
            p_sInfo = m_toolBox.GetDIO(ref m_diClose, this, "Close");
            p_sInfo = m_toolBox.GetDIO(ref m_diReady, this, "Ready");
            p_sInfo = m_toolBox.GetDIO(ref m_diRun, this, "Run");
            p_sInfo = m_toolBox.GetComm(ref m_rs232, this, "RS232");
            p_sInfo = m_toolBox.GetOHT(ref m_OHT, this, p_infoCarrier, "OHT");
            if (bInit)
            {
                m_rs232.OnReceive += M_rs232_OnReceive;
                m_rs232.p_bConnect = true;
            }
        }
        #endregion

        //forget
        #region DIO Function
        public bool m_bPlaced = false;
        public bool CheckPlaced()
        {
            GemCarrierBase.ePresent present = m_OHT.m_bPODExist ? GemCarrierBase.ePresent.Exist : GemCarrierBase.ePresent.Empty;
            if (p_infoCarrier.CheckPlaced(present) != "OK") m_alidPlaced.Run(true, "Placed Sensor Remain Checked while Pod State = " + p_infoCarrier.p_eState);
            switch (p_infoCarrier.p_ePresentSensor)
            {
                case GemCarrierBase.ePresent.Empty: m_svidPlaced.p_value = false; break;
                case GemCarrierBase.ePresent.Exist: m_svidPlaced.p_value = true; break;
            }
            return m_svidPlaced.p_value;
        }
        #endregion

        #region IWTRChild
        public bool p_bLock { get; set; }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false) return false;
                Thread.Sleep(100);
            }
            return true;
        }

        public List<string> p_asChildSlot
        {
            get { return p_infoCarrier.p_asGemSlot; }
        }

        public InfoWafer p_infoWafer { get; set; }

        public InfoWafer GetInfoWafer(int nID)
        {
            return p_infoCarrier.GetInfoWafer(nID);
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            p_infoCarrier.SetInfoWafer(nID, infoWafer);
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            return p_infoCarrier.GetTeachWTR(infoWafer);
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready)
            {
                m_alidGetOK.Run(true, p_id + " eState not Ready");
                return p_id + " eState not Ready";
            }
            return p_infoCarrier.IsGetOK(nID);
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready)
            {
                m_alidPutOK.Run(true, p_id + " eState not Ready");
                return p_id + " eState not Ready";
            }
            return p_infoCarrier.IsPutOK(nID);
        }

        public string BeforeGet(int nID)
        {
            if (GetInfoWafer(nID) == null)
            {
                m_alidGetOK.Run(true, p_id + nID.ToString("00") + " BeforeGet : InfoWafer = null");
                return p_id + nID.ToString("00") + " BeforeGet : InfoWafer = null";
            }
            return IsRunOK();
        }

        public string BeforePut(int nID)
        {
            if (GetInfoWafer(nID) != null)
            {
                m_alidPutOK.Run(true, p_id + nID.ToString("00") + " BeforePut : InfoWafer != null");
                return p_id + nID.ToString("00") + " BeforePut : InfoWafer != null";
            }
            return IsRunOK();
        }

        public string AfterGet(int nID)
        {
            return IsRunOK();
        }

        public string AfterPut(int nID)
        {
            return IsRunOK();
        }

        public bool IsWaferExist(int nID = 0)
        {
            switch (p_infoCarrier.p_eState)
            {
                case InfoCarrier.eState.Empty: return false;
                case InfoCarrier.eState.Placed: return false;
            }
            return (p_infoCarrier.GetInfoWafer(nID) != null);
        }

        string IsRunOK()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return p_infoCarrier.IsRunOK();
        }

        public void RunTreeTeach(Tree tree)
        {
            p_infoCarrier.m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }

        public void ReadInfoWafer_Registry()
        {
            p_infoCarrier.ReadInfoWafer_Registry();
        }
        #endregion

        #region ErrorCode
        string[,] m_asErrorMsg = new string[,]
        {
            { "2", "Axis information is wrong." },
            { "3", "입력 Data가 시스템의 지정된 범위를 초과하였거나 단위, 숫자, 기호 등이 잘못 입력되었을 경우에 발생" },
            { "4", "Stop/EmStop Event가 발생.E-Stop Switch가 눌려져 있거나 Up/Down Limit Sensor가 Sensing되고 있음" },
            { "5", "Driver(Amp)가 OFF 상태임" },
            { "6", "Homing이 완료되지 않은 상태에서 명령을 수행했을 경우 발생" },
            { "7", "Servo Motor Driver에서 Error가 발생됨" },
            { "9", "Error가 Clear되지 않은 상태에서 다른 명령이 내려졌음" },
            { "10", "Scan(Re - mapping) 명령이 FOUP Close 상태에서 내려졌음" },
            { "11", "POD Docking 명령 후 정해진 시간(5초) 내에 POD Trans In Sensor가 Sensing 되지 않았음" },
            { "12", "POD Undocking 명령 후 정해진 시간(5초) 내에 POD Trans Out Sensor가 Sensing 되지 않았음" },
            { "13", "Latch Open 위치에서 Latch Open Sensor가 Sensing되지 않았음" },
            { "14", "Latch Close 위치에서 Latch Close Sensor가 Sensing되지 않았음" },
            { "15", "Mapping Arm Open 동작 후 일정 시간안에(overtime 5초) Mapping Arm Open Sensor가 Sensing되지 않았음" },
            { "16", "Mapping Arm Close 동작 후 일정 시간안에(overtime 5초) Mapping Arm Open Sensor가 Sensing되지 않았음" },
            { "17", "POD Trans In후 Vacuum solenoid가 On하였으나 이때 Vacuum Sensor가 On 되지 않았음.즉 Vacuum Cup과 FOUP door사이에 Vacuum압이 형성되지 않았음" },
            { "18", "Vacuum solenoid를 Off한 후 Vacuum sensor가 Off되지 않았음" },
            { "19", "FOUP가 PDO의 Stage위에 올바르게 놓이지 않았음" },
            { "20", "POD Door가 감지되지 않았음" },
            { "21", "FOUP가 없는 상태에서 동작을 시도 하였음" },
            { "22", "1. Close 동작 전에 Wafer Protrusion sensor가 Sensing 되었음 2. Up/Down 동작 중에 Wafer Protrusion sensor가 sensing 되었음(Safety)" },
            { "23", "Mapping을 시작하는 Start Position Data 가 불합리하게 Setting 되었음" },
            { "24", "Up 또는 Down 동작 시간이 10초를 초과함" },
            { "25", "Mapping이 끝나기 전에 Up/Down 동작이 멈춤" },
            { "26", "두개 이상의 Wafer가 겹침" },
            { "27", "Wafer가 두 Slot사이에 걸쳐 있음" },
            { "28", "Wafer Detection Sensor의 값이 비정상적임" },
            { "29", "너무 많은 Wafer가 Sensing 됨.즉 Wafer Sensing시 Chattering으로 인해 Wafer 감지 On/Off point가 너무 많음." },
            { "30", "FOUP를 PDO로부터 제거 한 10초 이내에 Unload button이 눌려지지 않았음." },
            { "31", "Time interval이 정의되고 Motor가 이 이상을 회전함." },
            { "32", "Homing중 정해지 시간 안에 Homing Bracket이 Homing Sensor를 벗어나지 못함." },
            { "33", "The motor working has stopped due to a reason except for Limit Sensor Detection." },
            { "34", "Homing중 움직이는 반대 방향의 Limit sensor가 sensing됨." },
            { "35", "Homing 동안 두개의 Limit sensor가 모두 sensing됨." },
            { "36", "동작중 위치 Data가 200Pulse이상의 오차가 남." },
            { "37", "Homing조건이 아님. 1. POD가 있을 경우 1.1 Vacuum이 On이고 Latch가 Unlock. 1.2 Vacuum이 Off고 Latch가 Lock 2. POD가 없을 경우 Vacuum이 On이고 Latch가 Lock." },
            { "38", "Latch가 Open/Close후 Limit Sensor가 감지 되지 않았다." },
            { "39", "1. 위치 지정값이 Sw Limit 값 이상임." },
            { "40", "Pinch point sensor가 감지됨." },
            { "47", "Response is generated to exceed defined time interval." },
            { "48", "Response is generated to exceed defined time interval." },
            { "49", "Response is generated to exceed defined time interval." },
            { "51", "Backup Data is not erased normally." },
            { "52", "Backup Data is not correctly written." },
            { "53", "Backup Data is not correctly written." },
            { "54", "Backup Data가 정상적으로 입력되지 않음." },
            { "55", "Motor가 운행 중 SW Limit 을 지나침." },
            { "56", "Motor가 운행 중 H/W Limit sensor가 감지됨." },
            { "57", "운행 중 목표 위치와 현재 위치간에 오차가 있음." },
            { "58", "운행 중 E-Stop button이 눌려짐." },
            { "59", "Servo Drive Error가 감지됨." },
            { "60", "Docking시 Stage가 뒤로 밀림" },
            { "61", "TP로부터 일정주기 동안 입력이 없음." },
            { "62", "TP가 연결되지 않았음." },
            { "63", "-" },
            { "64", "POD Lock 동작 후 POD Lock sensor가 감지되지 않았음." },
            { "65", "POD Unlock 동작 후 POD Unlock sensor가 감지되지 않았음." },
            { "66", "Port door open 동작 후 Port door open sensor가 감지되지 않았음." },
            { "67", "Port door close 동작 후 Port door close sensor가 감지되지 않았음." },
            { "68", "Maint_Mode 가 Enable임." },
            { "69", "Maint_Mode 가 Enable이 아님." },
            { "70", "정의되지 않은 명령어의 Parameter가 수신되었음." },
            { "71", "BCR로부터 Data가 수신되지 않음." },
            { "72", "The setting OK of BCR and data other than ERR are received." },
            { "73", "BCR로부터 받은 Error code가 0-18임." },
            { "74", "BCR이 정상적으로 동작하지 않음." },
            { "75", "BCR로부터 응답이 3초 동안 없음.." },
            { "76", "BCR이 disable되어 있음." },
            { "77", "명령 문자열이 200byte를 초과함." },
            { "78", "TP가 연결되어 있음." },
            { "79", "정의되지 않은 명령이 수신 되었음." },
            { "80", "Host로부터 문자열을 받는동안 10초이상동안 Data를 받지 못함." },
            { "81", "BCR setting으로부터 Error가 있음." },
            { "111", "Log Data가 존재하지 않음" },
            { "112", "Log Write 할 공간이 부족함" },
            { "150", "Z축을 움직일 때 Robot Retract Signal이 Off 되어있음" },
            { "152", "UNLOAD 완료 상태에서 TOPEN 명령 수신" },
            { "153", "EFEM, SFEM door가 열려있음" },
            { "155", "Unload동작의 Door Close 시간이 Hom 동작 Door Close 시간 보다 작음" },
            { "156", "Trans Out중 Obstacle_CHK_SEN 감지시 Foup_Door_Open 되어 있음" },
            { "157", "Mapping Arm Extend 후 Mapping Arm Sensor로 빛이 감지되지 않음." },
            { "158", "Door Close상태에서 MLIFT_DN 명령 함" }
        };

        string GetErrorString(string sRead)
        {
            string sCode = sRead.Substring(1, sRead.Length - 1);
            for (int n = 0; n < m_asErrorMsg.Length / 2; n++)
            {
                if (m_asErrorMsg[n, 0] == sCode) return m_asErrorMsg[n, 1];
            }
            return "Can't Find Error Massage !!";
        }
        #endregion

        #region Event State
        public enum eEventState
        {
            Connected,
            DisConnected,
            LoadButtonPushed,
            POD_IN,
            POD_OUT,
            StartReset,
            EndReset,
            FOUP_Started,
            FOUP_IncorrectPos,
            EStopPushed,
        }
        eEventState _eEvnetState = eEventState.DisConnected;
        public eEventState p_eEvnetState
        {
            get { return _eEvnetState; }
            set
            {
                if (_eEvnetState == value) return;
                _eEvnetState = value;
                OnPropertyChanged();
                switch (value)
                {
                    case eEventState.POD_IN: m_bPlaced = true; break;
                    case eEventState.POD_OUT: m_bPlaced = false; break;
                }
            }
        }

        class EventState
        {
            public string m_sCode;
            public eEventState m_eEvent;
            public EventState(string sCode, eEventState eEvent)
            {
                m_sCode = sCode;
                m_eEvent = eEvent;
            }
        }
        List<EventState> m_aEventState = new List<EventState>();
        void InitEvent()
        {
            m_aEventState.Add(new EventState("00000001", eEventState.Connected));
            m_aEventState.Add(new EventState("00000002", eEventState.DisConnected));
            m_aEventState.Add(new EventState("00000004", eEventState.LoadButtonPushed));
            m_aEventState.Add(new EventState("00000010", eEventState.POD_IN));
            m_aEventState.Add(new EventState("00000020", eEventState.POD_OUT));
            m_aEventState.Add(new EventState("00000100", eEventState.StartReset));
            m_aEventState.Add(new EventState("00000200", eEventState.EndReset));
            m_aEventState.Add(new EventState("00010000", eEventState.FOUP_Started));
            m_aEventState.Add(new EventState("00020000", eEventState.FOUP_IncorrectPos));
            m_aEventState.Add(new EventState("01000000", eEventState.EStopPushed));
        }

        string SetEvent(string sRead)
        {
            string sCode = sRead.Substring(1, sRead.Length - 1);
            foreach (EventState eventState in m_aEventState)
            {
                if (eventState.m_sCode == sCode)
                {
                    p_eEvnetState = eventState.m_eEvent;
                    return "OK";
                }
            }
            return "Invalid Event Code : " + sRead;
        }
        #endregion

        //forget
        #region Protocol 
        public enum eCmd
        {
            Home,
            ClearError,
            Load,
            Unload,
            GetMap,
        };

        Dictionary<eCmd, string> m_dicCmd = new Dictionary<eCmd, string>();
        void InitCmd()
        {
            m_dicCmd.Add(eCmd.Home, "HOM");
            m_dicCmd.Add(eCmd.ClearError, "RESET");
            m_dicCmd.Add(eCmd.Load, "LOAD");
            m_dicCmd.Add(eCmd.Unload, "UNLOAD");
            m_dicCmd.Add(eCmd.GetMap, "SCAN DN");
        }

        public class Protocol
        {
            public enum eState
            {
                Queue,
                Send,
                ACK,
                NAK,
                Done
            }
            public eState m_eState = eState.Queue;
            string m_sCmd = "";

            public string SendCmd()
            {
                if (m_eState != eState.Queue) return "Protocol State != Queue";
                m_loadport.SendCmd(m_sCmd);
                m_eState = eState.Send;
                return "OK";
            }

            public bool m_bValid = true;
            public string WaitDone(int secWait)
            {
                if (EQ.IsStop()) return "EQ Stop";
                if (m_loadport.m_rs232.p_bConnect == false)
                {
                    m_loadport.m_alidConnect.Run(true, "RS232 Connection Lost !!");
                    return "RS232 Connection Lost !!";
                }
                int nWait = 100 * secWait;
                while (nWait > 0)
                {
                    if (EQ.IsStop()) return "EQ Stop";
                    Thread.Sleep(10);
                    if (m_eState == eState.Done) return "OK";
                    nWait--;
                }
                m_bValid = false;
                m_loadport.m_alidCMD.Run(true, m_sCmd + " : WaitDone Timeout !!");
                return m_sCmd + " : WaitDone Timeout !!";
            }

            public eCmd m_eCmd;
            Loadport_AOP01 m_loadport;
            public Protocol(eCmd eCmd, Loadport_AOP01 loadport, params string[] asParam)
            {
                m_eCmd = eCmd;
                m_loadport = loadport;
                m_sCmd = m_loadport.m_dicCmd[eCmd];
                foreach (string sParam in asParam) m_sCmd += " " + sParam;
            }
        }
        Protocol m_protocolSend = null;
        #endregion

        //forget
        #region RS232
        Queue<Protocol> m_qProtocol = new Queue<Protocol>();
        bool m_bRunSend = false;
        bool m_bRunGem = false;

        Thread m_threadSend;
        Thread m_threadGem;
        void InitThread()
        {
            m_threadSend = new Thread(new ThreadStart(RunThreadSend));
            m_threadSend.Start();
            m_threadGem = new Thread(new ThreadStart(RunThreadGem));
            m_threadGem.Start();
        }
        void RunThreadGem()
        {
            m_bRunGem = true;
            SetCEIDStatus();
            Thread.Sleep(1000);
            while (m_bRunGem)
            {
                Thread.Sleep(100);
                CheckCEID();
                UpdateSVID();
            }
        }

        
        public void UpdateSVID()
        {
            m_svidLPRun.p_value = m_diRun.p_bIn;
            m_svidLPReady.p_value = m_diReady.p_bIn;
            m_svidLPOpen.p_value = m_diOpen.p_bIn;
            m_svidLPClose.p_value = m_diClose.p_bIn;
            m_svidLPPlacement.p_value = m_diPlaced.p_bIn;
            m_svidLPPresence.p_value = m_diPresent.p_bIn;
        }

        public void CheckCEID()
        {
            if (m_gem == null || m_gem.p_eControl != eControl.ONLINEREMOTE) return;
            //CEID 보고
            
            if (m_eAssociated != p_infoCarrier.p_eAssociated)
            {
                switch (p_infoCarrier.p_eAssociated)
                {
                    //Carrier ID Delete CEID : AssociatedToNotAssociated에서만 발생, 시나리오대로 사용 시 아래 CEID 보고하여 사용해야 함.
                    case GemCarrierBase.eAssociated.Associated:
                        {
                            //m_ceidLPNotAssociatedToAssociated.Send();
                            m_ceidCarrierIDDeleted.Send();
                            m_ceidLPAssociatedToAssociated.Send();
                            break;
                        }
                }
                m_eAssociated = p_infoCarrier.p_eAssociated;
            }
        }

        public void SetCEIDStatus()
        {
            m_eAssociated = p_infoCarrier.p_eAssociated;
        }

        void RunThreadSend()
        {
            m_bRunSend = true;
            Thread.Sleep(1000);
            while (m_bRunSend)
            {
                Thread.Sleep(10);
                if ((m_protocolSend != null) && (m_protocolSend.m_bValid == false)) m_protocolSend = null;
                if ((m_protocolSend == null) && (m_qProtocol.Count > 0))
                {
                    m_protocolSend = m_qProtocol.Dequeue();
                    p_sInfo = m_protocolSend.SendCmd();
                    if (p_sInfo != "OK")
                    {
                        m_protocolSend = null;
                        m_qProtocol.Clear();
                    }
                }
            }
        }

        private void M_rs232_OnReceive(string sRead)
        {
            m_log.Info(" <-- Recv] " + sRead);
            if (sRead.Length < 1) return;
            char cCode = sRead[0];
            switch (cCode)
            {
                case 'A':
                    if (m_protocolSend == null) p_sInfo = "Invalid ACK";
                    else m_protocolSend.m_eState = Protocol.eState.ACK;
                    break;
                case 'N':
                    if (m_protocolSend == null) p_sInfo = "Invalid NAK";
                    else
                    {
                        m_protocolSend.m_eState = Protocol.eState.NAK;
                        p_sInfo = "Nak : " + m_protocolSend.m_eCmd.ToString();
                        p_eState = eState.Error;
                        m_protocolSend = null;
                    }
                    break;
                case 'E':
                    p_eState = eState.Error;
                    p_sInfo = "Cymech Error " + sRead + " : " + GetErrorString(sRead);
                    break;
                case 'C':
                    p_sInfo = SetEvent(sRead);
                    break;
                case 'O':
                    if (m_protocolSend == null) p_sInfo = "Invalid Done";
                    else
                    {
                        if ((m_protocolSend.m_eState == Protocol.eState.ACK) || (m_protocolSend.m_eState == Protocol.eState.NAK))
                        {
                            m_protocolSend.m_eState = Protocol.eState.Done;
                            m_protocolSend = null;
                        }
                    }
                    break;
                case 'M':
                    if (SetLoadportMapData(sRead)) p_sInfo = "Invalid Map Data : " + sRead;
                    if (m_protocolSend != null)
                    {
                        if ((m_protocolSend.m_eState == Protocol.eState.ACK) || (m_protocolSend.m_eState == Protocol.eState.NAK))
                        {
                            m_protocolSend.m_eState = Protocol.eState.Done;
                            m_protocolSend = null;
                        }
                    }
                    break;
            }
        }

        bool SetLoadportMapData(string sMap)
        {
            sMap = sMap.Substring(1, sMap.Length - 1);
            string[] asMap = sMap.Split(',');
            if (asMap.Length < 3) return true;
            for (int n = 0; n < 3; n++) if (asMap[n].Length < 8) return true;
            List<GemSlotBase.eState> aSlot = new List<GemSlotBase.eState>();
            for (int n = 0; n < 32; n++) aSlot.Add(GemSlotBase.eState.Empty);
            SetLoadportMapData(aSlot, asMap[0], GemSlotBase.eState.Exist);
            SetLoadportMapData(aSlot, asMap[1], GemSlotBase.eState.Cross);
            SetLoadportMapData(aSlot, asMap[2], GemSlotBase.eState.Double);
            p_infoCarrier.SetMapData(aSlot);
            return false;
        }

        void SetLoadportMapData(List<GemSlotBase.eState> aSlot, string sMap, GemSlotBase.eState eState)
        {
            int iSlot = 31;
            for (int n = 0; n < 8; n++)
            {
                char cMap = sMap[n];
                if ((cMap & 0x08) > 0) aSlot[iSlot] = eState; iSlot--;
                if ((cMap & 0x04) > 0) aSlot[iSlot] = eState; iSlot--;
                if ((cMap & 0x02) > 0) aSlot[iSlot] = eState; iSlot--;
                if ((cMap & 0x01) > 0) aSlot[iSlot] = eState; iSlot--;
            }
        }

        string SendCmd(string sCmd)
        {
            if (EQ.IsStop()) return "EQ Stop";
            m_log.Info(" [ Send --> " + sCmd);
            return m_rs232.Send(sCmd);
        }
        #endregion

        #region Timeout
        int m_secRS232 = 2;
        int _secHome = 40;
        int m_secLoad = 20;
        int m_secUnload = 20;
        public int p_secHome
        {
            get { return _secHome; }
            set
            {
                if (_secHome == value) return;
                _secHome = value;
                OnPropertyChanged();
            }
        }
        void RunTimeoutTree(Tree tree)
        {
            m_secRS232 = tree.Set(m_secRS232, m_secRS232, "RS232", "Timeout (sec)");
            p_secHome = tree.Set(p_secHome, p_secHome, "Home", "Timeout (sec)");
            m_secLoad = tree.Set(m_secLoad, m_secLoad, "Load", "Timeout (sec)");
            m_secUnload = tree.Set(m_secUnload, m_secUnload, "Unload", "Timeout (sec)");
        }
        #endregion

        //forget
        #region RS232 Commend
        string CmdHome()
        {
            Protocol protocol = new Protocol(eCmd.Home, this);
            m_qProtocol.Enqueue(protocol);
            return protocol.WaitDone(p_secHome);
        }

        string CmdResetCPU()
        {
            Protocol protocol = new Protocol(eCmd.ClearError, this);
            m_qProtocol.Enqueue(protocol);
            return protocol.WaitDone(m_secRS232);
        }

        string CmdLoad()
        {
            if (IsLock())
            {
                m_alidLoad.Run(true, p_id + " Lock by WTR");
                return p_id + " Lock by WTR";
            }
            Protocol protocol = new Protocol(eCmd.Load, this);
            m_qProtocol.Enqueue(protocol);
            return protocol.WaitDone(m_secLoad);
        }

        string CmdUnload()
        {
            if (IsLock())
            {
                m_alidUnLoad.Run(true, p_id + " Lock by WTR");
                return p_id + " Lock by WTR";
            }
            Protocol protocol = new Protocol(eCmd.Unload, this);
            m_qProtocol.Enqueue(protocol);
            return protocol.WaitDone(m_secLoad);
        }

        string CmdGetMap()
        {
            Protocol protocol = new Protocol(eCmd.GetMap, this);
            m_qProtocol.Enqueue(protocol);
            return protocol.WaitDone(m_secLoad);
        }

        #endregion

        #region override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTimeoutTree(tree.GetTree("Timeout", false));
            p_infoCarrier.m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
        }

        public override void Reset()
        {
            Run(CmdResetCPU());
            p_eState = eState.Init;
            m_bNeedHome = true;
            base.Reset();
        }

        public override void ButtonHome()
        {
            m_bNeedHome = true;
            base.ButtonHome();
        }
        #endregion

        //forget
        #region StateHome
        bool m_bNeedHome = true;
        public override string StateHome()
        {

            if (EQ.p_bSimulate == false)
            {
                if (Run(CmdResetCPU()))
                {
                    m_alidHome.Run(true, p_sInfo);
                    return p_sInfo;
                }
                if (Run(CmdResetCPU()))
                {
                    m_alidHome.Run(true, p_sInfo);
                    return p_sInfo;
                }
                if (m_bNeedHome)
                {
                    if (Run(CmdHome()))
                    {
                        m_alidHome.Run(true, p_sInfo);
                        return p_sInfo;
                    }
                    m_bNeedHome = false;
                }
                else
                {
                    //                    if (m_diDoorOpen.p_bIn) return p_id + " Door Opened";
                    if (Run(CmdUnload()))
                    {
                        m_alidHome.Run(true, p_sInfo);
                        return p_sInfo;
                    }
                }
                if (!m_diPlaced.p_bIn && !m_diPresent.p_bIn)
                {
                    p_infoCarrier.p_eState = InfoCarrier.eState.Placed;
                    m_bPlaced = true;

                    if (Run(CmdLoad()))
                    {
                        m_alidHome.Run(true, p_sInfo);
                        return p_sInfo;
                    }
                    if (Run(CmdUnload()))
                    {
                        m_alidHome.Run(true, p_sInfo);
                        return p_sInfo;
                    }
                }
                else
                {
                    p_infoCarrier.p_eState = InfoCarrier.eState.Empty;
                    m_bPlaced = false;
                }
            }
            p_eState = eState.Ready;
            p_infoCarrier.AfterHome();
            return "OK";
        }
        #endregion

        //forget
        #region StateReady
        public override string StateReady()
        {
            //CheckPlaced();
            //if (p_infoCarrier.m_bReqReadCarrierID)
            //{
            //    p_infoCarrier.m_bReqReadCarrierID = false;
            //    StartRun(m_runReadPodID);
            //}
            if (p_infoCarrier.m_bReqLoad)
            {
                p_infoCarrier.m_bReqLoad = false;
                StartRun(m_runDocking);
            }

            if (p_infoCarrier.m_bReqGem)
            {
                p_infoCarrier.m_bReqGem = false;
                StartRun(m_runGem);
            }
            
            if (p_infoCarrier.m_bReqUnload)
            {
                p_infoCarrier.m_bReqUnload = false;
                StartRun(m_runUndocking);
            }
            return "OK";
        }
        #endregion

        #region GAF
        SVID m_svidPlaced;
        
        SVID m_svidLPRun;
        SVID m_svidLPReady;
        SVID m_svidLPOpen;
        SVID m_svidLPClose;
        SVID m_svidLPPlacement;
        SVID m_svidLPPresence;

        public CEID m_ceidDockingStart;
        public CEID m_ceidDockingEnd;
        public CEID m_ceidUnDockingStart;
        public CEID m_ceidUnDockingEnd;

        public CEID m_ceidLPAssociatedToAssociated;
        public CEID m_ceidLPNotAssociatedToAssociated;
        CEID m_ceidCarrierIDDeleted;
        CEID m_ceidJobReserved;

        ALID m_alidPlaced;
        public ALID m_alidEmpty;
        public ALID m_alidLoad;
        public ALID m_alidUnLoad;
        public ALID m_alidHome;
        public ALID m_alidCMD;
        public ALID m_alidConnect;
        public ALID m_alidInforeticle;
        public ALID m_alidGetOK;
        public ALID m_alidPutOK;
        //public CEID m_ceidUnloadReq;
        void InitGAF()
        {
            m_svidPlaced = m_gaf.GetSVID(this, "Placed");
            m_svidLPRun = m_gaf.GetSVID(this, p_id + "Run");
            m_svidLPReady = m_gaf.GetSVID(this, p_id + "Ready");
            m_svidLPOpen = m_gaf.GetSVID(this, p_id + "Open");
            m_svidLPClose = m_gaf.GetSVID(this, p_id + "Close");
            m_svidLPPlacement = m_gaf.GetSVID(this, p_id + "Placement");
            m_svidLPPresence = m_gaf.GetSVID(this, p_id + "Presence");

            
            m_ceidDockingStart = m_gaf.GetCEID(this, "Docking Start");
            m_ceidDockingEnd = m_gaf.GetCEID(this, "Docking End");
            m_ceidUnDockingStart = m_gaf.GetCEID(this, "Undocking Start");
            m_ceidUnDockingEnd = m_gaf.GetCEID(this, "UnDocking End");
            m_ceidJobReserved = m_gaf.GetCEID(this, "Job Reserved");

            m_ceidCarrierIDDeleted = m_gaf.GetCEID(this, "CarrierIDDeleted");
            //m_ceidLPNotAssociatedToAssociated = m_gaf.GetCEID(this, "LPNotAssociatedToAssociated");
            m_ceidLPAssociatedToAssociated = m_gaf.GetCEID(this, "LP_AssociatedToAssociated");
            
            m_alidPlaced = m_gaf.GetALID(this, "Placed Sensor Error", "Placed & Plesent Sensor Should be Checked");
            //m_ceidUnloadReq = m_gaf.GetCEID(this, "Unload Request");
            m_alidInforeticle = m_gaf.GetALID(this, "Info Reticle Error", "Info Reticle Error");
            m_alidLoad = m_gaf.GetALID(this, "Load", "Loading Motion Error");
            m_alidUnLoad = m_gaf.GetALID(this, "UnLoad", "UnLoading Motion Error");
            m_alidHome = m_gaf.GetALID(this, "Home_Loadport", "Home Motion Error");
            m_alidCMD = m_gaf.GetALID(this, "CMD_Loadport", "CMD Error");
            m_alidConnect = m_gaf.GetALID(this, "Connect_Loadport", "Connect Error");
            m_alidGetOK = m_gaf.GetALID(this, "Get Err to Loadport", "Get Imposible Error");
            m_alidPutOK = m_gaf.GetALID(this, "Put Err to Loadport", "Put Imposible Error");
            m_alidEmpty = m_gaf.GetALID(this, "Empty", "Pod Empty");
        }
        #endregion

        #region ILoadport
        public string RunDocking()
        {
            if (p_infoCarrier.p_eState == InfoCarrier.eState.Dock) return "OK";
            ModuleRunBase run = m_runDocking.Clone();
            StartRun(run);
            while (IsBusy() && (EQ.IsStop() == false)) Thread.Sleep(10);
            return EQ.IsStop() ? "EQ Stop" : "OK";
        }
                
        public string RunUndocking()
        {
            if (p_infoCarrier.p_eState != InfoCarrier.eState.Dock) return "OK";
            ModuleRunBase run = m_runUndocking.Clone();
            StartRun(run);
            while (IsBusy() && (EQ.IsStop() == false)) Thread.Sleep(10);
            return EQ.IsStop() ? "EQ Stop" : "OK";
        }

        public bool p_bPlaced { get { return m_diPlaced.p_bIn; } }
        public bool p_bPresent { get { return m_diPresent.p_bIn; } }
        #endregion

        IRFID _rfid;
        public IRFID m_rfid
        {
            get { return _rfid; }
            set
            {
                _rfid = value;
            }
        }
        public InfoCarrier p_infoCarrier { get; set; }
        public StopWatch m_swLotTime;

        GemCarrierBase.eAssociated m_eAssociated = GemCarrierBase.eAssociated.NotAssociated;
        
        public Loadport_AOP01(string id, IEngineer engineer, bool bEnableWaferSize, bool bEnableWaferCount)
        {
            p_bLock = false;
            p_id = id;
            InitCmd();

            p_infoCarrier = new InfoCarrier(this, id, engineer, bEnableWaferSize, bEnableWaferCount);
            m_aTool.Add(p_infoCarrier);

            m_swLotTime = new StopWatch();
            m_swLotTime.Stop();

            InitBase(id, engineer);
            InitEvent();
            InitGAF();

            if (m_gem != null) m_gem.OnGemRemoteCommand += M_gem_OnRemoteCommand;
            InitThread();
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
        public string p_swLotTime = "";
        private void M_timer_Tick(object sender, EventArgs e)
        {
            p_swLotTime = String.Format("{0:00}:{1:00}:{2:00}", m_swLotTime.Elapsed.Hours, m_swLotTime.Elapsed.Minutes, m_swLotTime.Elapsed.Seconds);
        }
        #endregion
        public override void ThreadStop()
        {
            if (m_bRunSend)
            {
                m_bRunSend = false;
                m_threadSend.Join();
            }
            if (m_bRunGem)
            {
                m_bRunGem = false;
                m_threadGem.Join();
            }
            
            base.ThreadStop();
        }

        private void M_gem_OnRemoteCommand(string sCmd, Dictionary<string, string> dicParam, long[] pnResult)
        {

        }

        #region ModuleRun
        public ModuleRunBase m_runDocking;
        public ModuleRunBase m_runUndocking;
        public ModuleRunBase m_runGem;

        public ModuleRunBase GetModuleRunUndocking()
        {
            return m_runUndocking;
        }
        public ModuleRunBase GetModuleRunDocking()
        {
            return m_runDocking;
        }
        public ModuleRunBase GetModuleRunGem()
        {
            return m_runGem;
        }

        protected override void InitModuleRuns()
        {
            m_runDocking = AddModuleRunList(new Run_Docking(this), false, "Docking Carrier to Work Position");
            m_runGem = AddModuleRunList(new Run_GemProcess(this), false, "Gem Slot Process Start");
            m_runUndocking = AddModuleRunList(new Run_Undocking(this), false, "Undocking Carrier from Work Position");
        }

        public class Run_Docking : ModuleRunBase
        {
            Loadport_AOP01 m_module;
            InfoCarrier m_infoCarrier;
            public Run_Docking(Loadport_AOP01 module)
            {
                m_module = module;
                m_infoCarrier = module.p_infoCarrier;
                InitModuleRun(module);
            }

            string m_sSimulCarrierID = "Carrier ID";
            bool m_bMapping = true;
            public bool m_bReadRFID = true;
            public override ModuleRunBase Clone()
            {
                Run_Docking run = new Run_Docking(m_module);
                run.m_sSimulCarrierID = m_sSimulCarrierID;
                run.m_bMapping = m_bMapping;
                run.m_bReadRFID = m_bReadRFID;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sSimulCarrierID = tree.Set(m_sSimulCarrierID, m_sSimulCarrierID, "Simulation CarrierID", "CarrierID When p_bSimulation", bVisible);
                m_bMapping = tree.Set(m_bMapping, m_bMapping, "Mapping", "Wafer Mapping When Loading", bVisible);
                m_bReadRFID = tree.Set(m_bReadRFID, m_bReadRFID, "Read RFID", "Read RFID", bVisible);
            }

            public override string Run()
            {
                string sResult = "OK";
                if (EQ.p_bSimulate)
                {
                    m_infoCarrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                    m_infoCarrier.p_sCarrierID = m_sSimulCarrierID;
                }
                else
                {
                    m_module.m_bUnLoadCheck = false;
                    if (m_infoCarrier.p_eState == InfoCarrier.eState.Dock) return "OK";
                    if (m_infoCarrier.p_eState != InfoCarrier.eState.Placed)
                    {
                        m_module.m_alidLoad.Run(true, p_id + " RunLoad, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString());
                        return p_id + " RunLoad, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString();
                    }
                    if (m_bReadRFID)
                    {
                       sResult = m_module.m_rfid.ReadRFID();
                       m_infoCarrier.p_sCarrierID = (sResult == "OK") ? m_module.m_rfid.m_sReadID : "";
                    }
                }

                //Docking 시작
                m_module.m_ceidDockingStart.Send();

                m_infoCarrier.p_eStateCarrierID = GemCarrierBase.eGemState.NotRead;

                m_infoCarrier.SetReqAssociated(GemCarrierBase.eAssociated.Associated);

                if (sResult == "OK")
                {
                    m_infoCarrier.SendCarrierID(m_infoCarrier.p_sCarrierID);
                }
                else
                {
                    return p_sInfo + " SendCarrierID : " + m_infoCarrier.p_sCarrierID;
                }

                m_infoCarrier.p_eStateSlotMap = GemCarrierBase.eGemState.NotRead;
                
                while (m_infoCarrier.p_eStateCarrierID != GemCarrierBase.eGemState.VerificationOK)
                {
                    Thread.Sleep(10);
                    if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                    if (m_infoCarrier.p_eStateCarrierID == GemCarrierBase.eGemState.VerificationFailed)
                        return p_sInfo + " infoCarrier.p_eStateCarrierID = " + m_infoCarrier.p_eStateCarrierID.ToString();
                }
                
                if (m_infoCarrier.p_eTransfer != GemCarrierBase.eTransfer.TransferBlocked)
                    return p_sInfo + " infoCarrier.p_eTransfer = " + m_infoCarrier.p_eTransfer.ToString();

                if (m_module.Run(m_module.CmdLoad()))
                {
                    m_module.m_alidLoad.Run(true, p_sInfo);
                    return p_sInfo;
                }

                m_infoCarrier.p_eState = InfoCarrier.eState.Dock;

                //Docking 종료 보고
                m_module.m_ceidDockingEnd.Send();

                m_infoCarrier.SendSlotMap();
                
                while (m_infoCarrier.p_eStateSlotMap != GemCarrierBase.eGemState.VerificationOK)
                {
                    Thread.Sleep(10);
                    if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                    if (m_infoCarrier.p_eStateSlotMap == GemCarrierBase.eGemState.VerificationFailed)
                        return p_sInfo + " infoCarrier.p_eStateSlotMap = " + m_infoCarrier.p_eStateSlotMap.ToString();
                }
                
                m_module.m_swLotTime.Start();
                m_module.m_bLoadCheck = true;
                return "OK";
            }

        }

        public class Run_Undocking : ModuleRunBase
        {
            Loadport_AOP01 m_module;
            InfoCarrier m_infoCarrier;
            public Run_Undocking(Loadport_AOP01 module)
            {
                m_module = module;
                m_infoCarrier = module.p_infoCarrier;
                InitModuleRun(module);
            }

            string m_sUndocking = "Undocking";
            public override ModuleRunBase Clone()
            {
                Run_Undocking run = new Run_Undocking(m_module);
                run.m_sUndocking = m_sUndocking;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sUndocking = tree.Set(m_sUndocking, m_sUndocking, "Undocking", "Carrier Undocking", bVisible, true);
            }

            public override string Run()
            {
                m_module.m_bLoadCheck = false;
                bool bUseXGem = m_module.m_engineer.p_bUseXGem;
                IGem m_gem = m_module.m_gem;
                if (!EQ.p_bSimulate)
                {
                    if (m_infoCarrier.p_eState != InfoCarrier.eState.Dock)
                    {
                        m_module.m_alidUnLoad.Run(true, p_id + " RunUnload, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString());
                        return p_id + " RunUnload, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString();
                    }
                }

                //if (m_module.Run(m_module.CmdGetMap())) return p_sInfo;
                if (bUseXGem)
                {
                    while (m_infoCarrier.p_eAccess != GemCarrierBase.eAccess.InAccessed)
                    {
                        Thread.Sleep(10);
                        if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                    }
                    if (m_gem.p_cjRun == null) return p_sInfo;
                    foreach (GemPJ pj in m_gem.p_cjRun.m_aPJ)
                    {
                        m_gem.SendPJComplete(pj.m_sPJobID);
                        Thread.Sleep(100);
                    }
                    while (m_gem.p_cjRun.p_eState != GemCJ.eState.Completed)
                    {
                        Thread.Sleep(10);
                        if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                    }
                }

                m_module.m_ceidUnDockingStart.Send();
                if (!EQ.p_bSimulate)
                {
                    if (m_module.Run(m_module.CmdUnload()))
                    {
                        m_module.m_alidUnLoad.Run(true, p_sInfo);
                        return p_sInfo;
                    }
                }

                m_infoCarrier.SetReqAssociated(GemCarrierBase.eAssociated.NotAssociated);
                m_infoCarrier.p_eState = InfoCarrier.eState.Placed;
                m_module.m_ceidUnDockingEnd.Send();

                m_infoCarrier.p_eReqTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
                m_module.m_swLotTime.Stop();
                m_module.m_swLotTime.Reset();
                m_module.m_bUnLoadCheck = true;
                return "OK";
            }
        }
        #endregion

        
        public class Run_GemProcess : ModuleRunBase
        {
            Loadport_AOP01 m_module;
            InfoCarrier m_infoCarrier;
            public Run_GemProcess(Loadport_AOP01 module)
            {
                m_module = module;
                m_infoCarrier = module.p_infoCarrier;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_GemProcess run = new Run_GemProcess(m_module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                
            }

            public override string Run()
            {
                string sResult = "OK";
                IGem m_gem = m_module.m_gem;
                if (m_gem == null || m_gem.p_eControl != eControl.ONLINEREMOTE) return p_id + " is not Gem Ready.";
                if (!EQ.p_bSimulate)
                {
                    if (m_infoCarrier.p_eState != InfoCarrier.eState.Dock)
                        return p_id + " RunUnload, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString();
                }

                while (m_gem.p_cjRun == null)
                {
                    Thread.Sleep(10);
                    if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                }

                while (m_infoCarrier.p_eAccess != GemCarrierBase.eAccess.InAccessed)
                {
                    Thread.Sleep(10);
                    if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                }

                for (int i = 0; i < m_infoCarrier.m_aGemSlot.Count; i++)
                {
                    if (m_infoCarrier.m_aGemSlot[i].p_eState == GemSlotBase.eState.Select)
                        m_infoCarrier.StartProcess(m_infoCarrier.m_aGemSlot[i].p_id);
                }

                return sResult;
            }
        }
    }
}

