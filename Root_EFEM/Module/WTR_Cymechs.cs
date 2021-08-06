using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RootTools.GAFs;


namespace Root_EFEM.Module
{
    public class WTR_Cymechs : ModuleBase, IWTR
    {
        #region Comm
        enum eComm
        {
            TCPIP,
            RS232,
        }
        eComm _eComm = eComm.TCPIP; 
        eComm p_eComm
        {
            get { return _eComm; }
            set
            {
                _eComm = value;
                m_reg.Write("Comm", (int)p_eComm); 
            }
        }

        Registry m_reg; 
        void InitCommType(string id)
        {
            m_reg = new Registry(id);
            p_eComm = (eComm)m_reg.Read("Comm", (int)p_eComm); 
        }
        #endregion

        #region ToolBox
        public DIO_I m_diReticleCheck;
        public DIO_I m_diArmClose;
        TCPIPClient m_tcpip; 
        RS232 m_rs232;
        public override void GetTools(bool bInit)
        {
            InitALID();
            p_sInfo = m_toolBox.GetDIO(ref m_diReticleCheck, this, "Reticle Check Sensor");
            p_sInfo = m_toolBox.GetDIO(ref m_diArmClose, this, "ArmClose");
            switch (p_eComm)
            {
                case eComm.TCPIP:
                    p_sInfo = m_toolBox.GetComm(ref m_tcpip, this, "TCPIP"); 
                    if (bInit)
                    {
                        m_tcpip.EventReciveData += M_tcpip_EventReciveData;
                    }
                    break; 
                case eComm.RS232:
                    p_sInfo = m_toolBox.GetComm(ref m_rs232, this, "RS232");
                    if (bInit)
                    {
                        m_rs232.OnReceive += M_rs232_OnReceive;
                        m_rs232.p_bConnect = true;
                    }
                    break; 
            }

        }

        #endregion

        #region Arm
        public enum eArm
        {
            A,
            B,
        }
        public Dictionary<eArm, Arm> m_dicArm { get; set; } = new Dictionary<eArm, Arm>();
        protected virtual void InitArms(string id, IEngineer engineer)
        {
            m_dicArm.Add(eArm.A, new Arm(id, eArm.A, this, engineer, true, false));
            m_dicArm.Add(eArm.B, new Arm(id, eArm.B, this, engineer, true, false));
        }

        public List<WTRArm> p_aArm
        {
            get
            {
                List<WTRArm> aArm = new List<WTRArm>();
                foreach (Arm arm in m_dicArm.Values) aArm.Add(arm);
                return aArm;
            }
        }

        List<string> p_asArm
        {
            get
            {
                List<string> asArm = new List<string>();
                foreach (Arm arm in m_dicArm.Values)
                {
                    if (arm.p_bEnable) asArm.Add(arm.m_eArm.ToString());
                }
                return asArm;
            }
        }

        void RunTreeArm(Tree tree, ref eArm eArm, bool bVisible)
        {
            string sArm = eArm.ToString();
            sArm = tree.Set(sArm, sArm, p_asArm, "Arm", "Select WTR Arm", bVisible);
            foreach (Arm arm in m_dicArm.Values)
            {
                if (arm.m_eArm.ToString() == sArm) eArm = arm.m_eArm;
            }
        }

        public class Arm : WTRArm
        {
            public eArm m_eArm;

            WTR_Cymechs m_module;
            public Arm(string id, eArm arm, WTR_Cymechs module, IEngineer engineer, bool bEnableWaferSize, bool bEnableWaferCount)
            {
                m_eArm = arm;
                m_module = module;
                Init(id + "." + arm.ToString(), engineer, bEnableWaferSize, bEnableWaferCount); 
            }
            enum eCheckWafer
            {
                InfoWafer,
                Sensor,
            };
            eCheckWafer m_eCheckWafer = eCheckWafer.Sensor;

            public override bool IsWaferExist()
            {
                switch (m_eCheckWafer)
                {
                    case eCheckWafer.Sensor:
                        bool bExist = false;
                        //m_module.p_sInfo = m_module.RequestWafer(m_eArm, ref bExist);
                        if (m_module.m_diReticleCheck.p_bIn == true)  //lyj add
                        {                                            //lyj add
                            bExist = true;                           //lyj add
                        }                                            //lyj add
                        return bExist;
                    default: return (p_infoWafer != null);
                }
            }

            public override void RunTree(Tree tree)
            {
                m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "Wafer Check", "Wafer Check Option");
                base.RunTree(tree);
            }
        }
        #endregion

        #region ErrorCode
        enum eErrorMode
        {
            Operation,
            Motion,
            Motor,
            Aligner,
            Grip,
            SCARA,
            PA
        };
        int m_lErrorMode = 7; 
        enum eErrorAxis
        {
            NoAxis,
            Z1,
            T1,
            T2,
            A,
            B,
            Z2
        };
        int m_lErrorAxis = 7; 
        Dictionary<eErrorMode, string[,]> m_asErrorCode = new Dictionary<eErrorMode, string[,]>();
        void InitErrorString()
        {
            m_lErrorMode = Enum.GetNames(typeof(eErrorMode)).Length;
            m_lErrorAxis = Enum.GetNames(typeof(eErrorAxis)).Length; 
            m_asErrorCode.Add(eErrorMode.Operation, _asErrorOperation);
            m_asErrorCode.Add(eErrorMode.Motion, _asErrorMotion);
            m_asErrorCode.Add(eErrorMode.Motor, _asErrorMotor);
            m_asErrorCode.Add(eErrorMode.Aligner, _asErrorAligner);
            m_asErrorCode.Add(eErrorMode.Grip, _asErrorGrip);
            m_asErrorCode.Add(eErrorMode.SCARA, _asErrorSCARA);
            m_asErrorCode.Add(eErrorMode.PA, _asErrorPA);
        }
        string[,] _asErrorOperation = new string[,]
        {
            { "001", "Liveman Error" },
            { "002", "There is no wafer" },
            { "003", "There is a wafer" },
            { "004", "Check operation mode" },
            { "005", "Home all is not done" },
            { "006", "Controller is not ready" },
            { "007", "Station or slot number is wrong" },
            { "008", "Command is not correct" },
            { "009", "E-Stop/User IO is disconnected" },
            { "010", "Station is not match with arm" },
            { "011", "Goto is not do after arm changed" },
            { "012", "Error is not Cleared" },
            { "100", "Initialization is failed, Reboot Robot Controller" },
            { "101", "Host COM was not Initialized" },
            { "102", "TP COM was not Initialized" },
            { "103", "Check CDA Pressure" }
        };
        string[,] _asErrorMotion = new string[,]
        {
            { "001", "RA is not retracted" },
            { "002", "RB is not retracted" },
            { "005", "Check extend interlock" },
            { "009", "Check sensor signal" },
            { "010", "Drive is not Enabled" },
            { "012", "Error Clear is failed" }
        };
        string[,] _asErrorMotor = new string[,]
        {
            { "000", "Check motion board connection with drive" },
            { "011", "Control power supply under voltage protection" },
            { "012", "Overvoltage protection" },
            { "013", "Main power supply under voltage protection" },
            { "014", "Over current protection" },
            { "015", "Over-heat protection" },
            { "016", "Over-load protection" },
            { "018", "Over regeneration load protection" },
            { "021", "Encoder communication error protection" },
            { "023", "Encoder communication data error protection" },
            { "024", "Position deviation excess protection" },
            { "025", "Hybrid deviation excess error protection" },
            { "026", "Over-speed protection" },
            { "027", "Electronic gear error protection" },
            { "028", "External scale communication data error protection" },
            { "029", "Deviation counter overflow protection" },
            { "034", "Software limit protection" },
            { "035", "External scale communication error protection" },
            { "036", "EEPROM parameter error protection" },
            { "037", "EEPROM check code error protection" },
            { "038", "Over-travel inhibit input protection" },
            { "039", "Analog input excess protection" },
            { "040", "Absolute system down error protection" },
            { "041", "Absolute counter over error protection" },
            { "042", "Absolute over-speed error protection" },
            { "044", "Absolute single turn counter error protection" },
            { "045", "Absolute multi-turn counter error protection" },
            { "047", "Absolute status error protection" },
            { "048", "Encoder Z-phase error protection" },
            { "049", "Encoder CS signal error protection" },
            { "050", "External scale status 0 error protection" },
            { "051", "External scale status 1 error protection" },
            { "052", "External scale status 2 error protection" },
            { "053", "External scale status 3 error protection" },
            { "054", "External scale status 4 error protection" },
            { "055", "External scale status 5 error protection" },
            { "065", "CCWTL input excess protection" },
            { "066", "CWTL input excess protection" },
            { "095", "Motor automatic recognition error protection" },
            { "100", "Motor Power on is failed" },
            { "101", "Over Time Error" },
            { "102", "Check Reference Position" },
            { "103", "Check Current Position" },
            { "104", "Motor Power is not On" },
            { "105", "Check Extend Interlock IO" },
            { "106", "Check Wafer Presence" },
            { "107", "Check Current Position & Encoder value" },
            { "108", "Home Define is failed, Check serial cable with drive" },
            { "109", "Check Grip Status" },
            { "120", "Negative end limit protection" },
            { "121", "Positive end limit protection" },
        };
        string[,] _asErrorAligner = new string[,]
        {
            { "XXX", "No Error Msg" }
        };
        string[,] _asErrorGrip = new string[,]
        {
            { "100", "Gripper is not Move to UnGrip position" },
            { "101", "Gripper is not Move to Grip position" },
            { "106", "Check Wafer Presence" },
            { "109", "Check Grip Status" },
            { "130", "Place Moving Check Wafer Present" },
            { "131", "Place Done Check Wafer" },
            { "140", "Pick Moving Check Wafer Present" },
            { "141", "Pick Done Check Wafer" },
            { "200", "Check Wafer Error : Pick Start" },
            { "201", "Check Wafer Error : Pick Extend" },
            { "210", "Check Wafer Error : Place Start" },
            { "211", "Check Wafer Error : Place Extend" },
            { "400", "UnGrip Fail : Check Sensor Please" },
            { "401", "Grip Fail : Check Sensor Please" }
        };
        string[,] _asErrorSCARA = new string[,]
        {
            { "001", "Illegal command" },
            { "002", "Wrong number of stage" },
            { "003", "Wrong number of arm" },
            { "004", "Wrong number of slot" },
            { "005", "Illegal speed range" },
            { "006", "Wrong number of robot axis" },
            { "007", "Invalid value of axis location" },
            { "008", "Illegal argument value" },
            { "010", "Invalid argument type" },
            { "011", "Invalid robot number" },
            { "012", "Invalid value of pitch" },
            { "013", "Invalid value of up stroke" },
            { "014", "Invalid value of down stroke" },
            { "015", "Invalid value of total number of slot" },
            { "016", "Invalid value of mapping speed" },
            { "017", "Invalid value of reference thickness" },
            { "018", "Invalid value of thickness margin" },
            { "019", "Invalid value of existence margin" },
            { "020", "Invalid robot type number" },
            { "021", "Invalid arm type number" },
            { "022", "Invalid value of total number of axis" },
            { "023", "Invalid grip type number" },
            { "024", "Invalid value of mapping sensor" },
            { "025", "Invalid value of traverse axis" },
            { "026", "Invalid value of arm location" },
            { "027", "Invalid value of On/Off" },
            { "028", "Invalid signal number" },
            { "029", "Invalid value of delay time" },
            { "031", "Invalid value of retry count on error" },
            { "033", "Invalid value of arm distance" },
            { "034", "Invalid value of protruded material detect start position" },
            { "035", "Invalid value of protruded material detect count" },
            { "036", "Invalid value of clearance" },
            { "037", "Invalid value of material state" },
            { "038", "Invalid value of mode" },
            { "039", "Invalid value of offset" },
            { "041", "Invalid value of aligner argument" },
            { "042", "Aligner communication time-out error" },
            { "043", "Invalid value of IO" },
            { "051", "Data read busy" },
            { "052", "Data write busy" },
            { "053", "Robot control busy" },
            { "054", "Aligner busy" },
            { "055", "Ardiono busy*" },
            { "061", "Flash Busy" },
            { "081", "Stage info file load error" },
            { "180", "File Read Error" },
            { "181", "Pattern file load error" },
            { "182", "Profile file load error" },
            { "184", "Stage info file load error" },
            { "185", "Robot type file load error" },
            { "186", "Option file load error" },
            { "201", "Robot is busy" },
            { "202", "Servo power is off" },
            { "203", "On E-Stop" },
            { "204", "Robot is paused" },
            { "205", "Robot is not paused" },
            { "206", "Robot is not executing command" },
            { "207", "Robot is stopped" },
            { "208", "Robot has an error" },
            { "209", "Servo power is on" },
            { "211", "Robot paused by I/O signal" },
            { "212", "Robot is manual mode" },
            { "213", "Robot is auto mode" },
            { "215", "Robot Stopped by Extend Signal Disable" },
            { "222", "Error on material status during mapping" },
            { "224", "Material detected before mapping" },
            { "225", "Station does not match previous one" },
            { "232", "Map scan data does not exist" },
            { "233", "Map scan data does not Match" },
            { "234", "Map scan data detected over slot" },
            { "241", "Robot hand is already flipped" },
            { "242", "Robot hand is not flipped" },
            { "243", "Robot can't flip this position" },
            { "261", "Aligner module is not connected" },
            { "262", "Aligner connect fail" },
            { "271", "Need re-set variable pitch" },
            { "291", "Need stage teaching" },
            { "292", "Need stage parameter config" },
            { "293", "This stage is flipped location" },
            { "294", "This stage is not flipped location" },
            { "295", "Clearance value is set to be wrong" },
            { "301", "Handling material before GETFROM" },
            { "302", "Not Handling material before PUTINTO" },
            { "303", "Not Handling material after GETFROM" },
            { "304", "Handling material before PUTINTO" },
            { "307", "Illegal Check sensor status" },
            { "312", "Current robot position is dangerous" },
            { "350", "Slave Servo ON Timeout" },
            { "401", "Data read error" },
            { "402", "Writing host serial port time-out error" },
        };
        string[,] _asErrorPA = new string[,]
        {
            { "1xx", "Robot Related Errors" },
            { "2xx", "Standard System Errors" },
            { "3xx", "Hardware Device Related Errors" },
            { "4xx", "Configuration Parameter Database, Datalogger, and CPU Monitor Errors" },
            { "50x", "Input and Output Errors" },
            { "55x", "Controller Errors" },
            { "6xx", "Network, Socket, and Communication Errors" },
            { "7xx", "Language Related Errors" },
            { "8xx", "Language Related Errors" },
            { "9xx", "Servo Related Errors" }
        };

        string GetErrorString(string sCode)
        {
            if (sCode.Length < 10) return "Invalide Error Code Length : " + sCode;
            sCode = sCode.Substring(5, 5); 
            char cMode = sCode[0];
            int nMode = cMode - '0';
            if ((nMode < 0) || (nMode >= m_lErrorMode)) return "Invalide Error Mode : " + sCode;
            eErrorMode eMode = (eErrorMode)nMode;
            char cAxis = sCode[1];
            int nAxis = cAxis - '0';
            if ((nAxis < 0) || (nAxis >= m_lErrorAxis)) return "Invalid Error Axis : " + sCode;
            if (eMode != eErrorMode.PA) return GetErrorString(eMode, sCode); 
            else return GetErrorString(m_asErrorCode[eErrorMode.PA], sCode);
        }

        string GetErrorString(eErrorMode eMode, string sCode)
        {
            string sError = sCode.Substring(2, 3);
            string[,] asError = m_asErrorCode[eMode];
            for (int n = 0; n < asError.Length / 2; n++)
            {
                if (asError[n, 0] == sError) return asError[n, 1];
            }
            return "Can't Find Error Massage : " + sCode;
        }

        string GetErrorString(string[,] asError, string sCode)
        {
            string sError = sCode.Substring(2, 3);
            for (int n = 0; n < asError.Length / 2; n++)
            {
                if (IsSamePA(asError[n, 0], sCode)) return asError[n, 1];
            }
            return "Can't Find Error Massage : " + sCode;
        }

        bool IsSamePA(string s0, string s1)
        {
            for (int n = 0; n < 3; n++)
            {
                if ((s0[n] != 'x') || (s0[n] != s1[n])) return false; 
            }
            return true;
        }
        #endregion

        #region Timeout
        public int m_secRS232 = 2;
        public int m_secHome = 60;
        public int m_secMotion = 20;
        void RunTimeoutTree(Tree tree)
        {
            m_secRS232 = tree.Set(m_secRS232, m_secRS232, "RS232", "Timeout (sec)");
            m_secHome = tree.Set(m_secHome, m_secHome, "Home", "Timeout (sec)");
            m_secMotion = tree.Set(m_secMotion, m_secMotion, "Motion", "Timeout (sec)");
        }
        #endregion

        #region Comm
        Queue<Protocol> m_qProtocol = new Queue<Protocol>();
        bool m_bRunSend = false;
        Thread m_threadSend;
        void InitThread()
        {
            m_threadSend = new Thread(new ThreadStart(RunThreadSend));
            m_threadSend.Start();
        }

        void RunThreadSend()
        {
            m_bRunSend = true;
            Thread.Sleep(1000);
            while (m_bRunSend)
            {
                Thread.Sleep(10);
                if ((m_protocolSend == null) && (m_qProtocol.Count > 0))
                {
                    Thread.Sleep(50);
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
            if (m_protocolSend != null)
            {
                bool bDone = m_protocolSend.OnReceive(sRead);
                string[] sreads = sRead.Split(' ');
                m_alidRTRCmdError.Run(sreads[0] == "_ERR", "Cymechs Robot Error, Error Code : " + sreads[1]);
                if (bDone) m_protocolSend = null;
            }
        }

        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, System.Net.Sockets.Socket socket)
        {
            string sRead = Encoding.Default.GetString(aBuf, 0, nSize);
            m_log.Info(" <-- Recv] " + sRead);
            if (m_protocolSend != null)
            {
                bool bDone = m_protocolSend.OnReceive(sRead);
                string[] sreads = sRead.Split(' ');
                if(sreads.Length>=2)
                m_alidRTRCmdError.Run(sreads[0] == "_ERR", "Cymechs Robot Error, Error Code : " + sreads[1]);
                if (bDone) m_protocolSend = null;
            }
        }

        string SendCmd(string sCmd)
        {
            if (EQ.IsStop()) return "EQ Stop";
            m_log.Info(" [ Send --> " + sCmd);
            switch (p_eComm)
            {
                case eComm.TCPIP: return m_tcpip.Send(sCmd + "\r"); 
                case eComm.RS232: return m_rs232.Send(sCmd);
            }
            return "SendCmd Comm Type Error : " + p_eComm.ToString(); 
        }
        #endregion

        #region GAF
        public ALID m_alidRTRCmdError;
        public ALID m_alidRTRArmError;
        void InitALID()
		{
            m_alidRTRCmdError = m_gaf.GetALID(this, "Cymechs", "RTR CMD ERROR");
            m_alidRTRArmError = m_gaf.GetALID(this, "Cymechs", "RTR Arm ERROR");

        }
        #endregion

        #region Protocol
        public enum eCmd
        {
            CLEAR,
            HOME,
            GOTO,
            PICK,
            PLACE,
            ZAXIS,
            SERVO,
            ESTOP,
            RQ_WAFER,
        }
        public class Protocol
        {
            public string m_sRead = ""; 
            public bool OnReceive(string sRead)
            {
                int nIndex = sRead.IndexOf("\r");
                m_sRead = sRead.Substring(0, nIndex);
                return false;
            }

            bool m_bSend = false; 
            public string m_sCmd = "";
            public string SendCmd()
            {
                m_wtr.SendCmd(m_sCmd);
                m_bSend = true; 
                return "OK";
            }

            public string WaitReply(int secWait)
            {
                while (m_bSend == false)
                {
                    if (EQ.IsStop()) return "EQ Stop";
                    //if (m_wtr.m_rs232.p_bConnect == false) return "RS232 Connection Lost !!";
                    Thread.Sleep(10);
                }
                m_sRead = ""; 
                StopWatch sw = new StopWatch(); 
                while (true)
                {
                    if (EQ.IsStop()) return "EQ Stop";
                    //if (m_wtr.m_rs232.p_bConnect == false) return "RS232 Connection Lost !!";
                    if (m_sRead != "") return "OK";
                    if (sw.Elapsed.TotalSeconds > secWait) return "Timeover"; 
                    Thread.Sleep(10); 
                }
            }

            public eCmd m_eCmd;
            WTR_Cymechs m_wtr;
            public Protocol(eCmd eCmd, WTR_Cymechs loadport, string sCmd)
            {
                m_eCmd = eCmd;
                m_wtr = loadport;
                m_sCmd = sCmd; 
            }
        }
        Protocol m_protocolSend = null;
        #endregion

        #region RS232 Command
        string IsACK(Protocol protocol)
        {
            if (Run(protocol.WaitReply(m_secRS232)))
            {
                m_protocolSend = null;
                return p_sInfo;
            }
            switch (protocol.m_sRead)
            {
                case "_ACK": return "OK";
                default: 
                    m_protocolSend = null; 
                    return "Reply is not _ACK : " + protocol.m_sRead;
            }
        }

        string IsReady(Protocol protocol, int secDone)
        {
            try
            {
                if (Run(protocol.WaitReply(secDone))) return p_sInfo;
                switch (protocol.m_sRead)
                {
                    case "_RDY": return "OK";
                    default: return "Reply is not _RDY : " + protocol.m_sRead;
                }
            }
            finally { m_protocolSend = null; }
        }

        string Cmd(eCmd eCmd, string sCmd, int secDone)
        {
            Protocol protocol = new Protocol(eCmd, this, sCmd);
            m_qProtocol.Enqueue(protocol);
            if (Run(IsACK(protocol))) return sCmd + " ACK Error : "  + p_sInfo;
            if (Run(IsReady(protocol, secDone))) return sCmd + " READY Error : " + p_sInfo;
			return "OK";
        }

        string CmdClear()
        {
            return Cmd(eCmd.CLEAR, "CLEAR", m_secRS232); 
        }

        string CmdHome()
        {
            return Cmd(eCmd.HOME, "HOME ALL", m_secHome); 
        }

        string CmdGoto(int nStation, int nSlot, eArm eArm, bool bExtend, bool bUp) 
        {
            string sEX = bExtend ? " R EX" : " R RE";
            string sZ = bUp ? " Z UP" : " Z DN"; 
            string sCmd = "GOTO N " + nStation.ToString() + sEX + sZ + " SLOT " + nSlot.ToString() + " ARM " + eArm.ToString();
            return Cmd(eCmd.GOTO, sCmd, m_secMotion);
        }

        string CmdPick(int nStation, int nSlot, eArm eArm)
        {
            string sCmd = "PICK " + nStation.ToString() + " SLOT " + nSlot.ToString() + " ARM " + eArm.ToString();
            return Cmd(eCmd.PICK, sCmd, m_secMotion);
        }

        string CmdPlace(int nStation, int nSlot, eArm eArm)
        {
            string sCmd = "PLACE " + nStation.ToString() + " SLOT " + nSlot.ToString() + " ARM " + eArm.ToString();
            return Cmd(eCmd.PLACE, sCmd, m_secMotion);
        }

        string CmdZAxis(int nStation, int nSlot, eArm eArm, bool bUp)
        {
            string sZ = bUp ? " UP" : " DN";
            string sCmd = "ZAXIS " + nStation.ToString() + " SLOT " + nSlot.ToString() + sZ + " ARM " + eArm.ToString();
            return Cmd(eCmd.ZAXIS, sCmd, m_secMotion);
        }

        string CmdServo(bool bOn)
        {
            string sOn = bOn ? " ON" : " OFF";
            return Cmd(eCmd.SERVO, "SERVO " + sOn, m_secMotion);
        }

        string CmdEStop()
        {
            m_qModuleRun.Clear(); 
            return Cmd(eCmd.ESTOP, "ESTOP", m_secRS232); 
        }

        string Request(eCmd eCmd, string sCmd, ref string sRead)
        {
            try
            {
                Protocol protocol = new Protocol(eCmd, this, sCmd);
                m_qProtocol.Enqueue(protocol);
                if (Run(protocol.WaitReply(m_secRS232))) return p_sInfo;
                sRead = protocol.m_sRead;
                return "OK"; 
            }
            finally { m_protocolSend = null; } 
        }

        string RequestWafer(eArm eArm, ref bool bExist)
        {
            string sRead = ""; 
            if (Run(Request(eCmd.RQ_WAFER, "RQ WAFER ARM " + eArm.ToString(), ref sRead))) return p_sInfo;
            if (sRead.Contains("WAFER " + eArm.ToString()) == false) return "RQ WAFER Error : " + sRead;
            string[] asRead = sRead.Split(' ');
            if (asRead.Length < 3) return "RQ WAFER Error : " + sRead;
            bExist = (asRead[2] == "Y"); 
            return "OK";
        }
        #endregion

        #region StateHome
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            try
            {
                //foreach (IWTRChild child in p_aChild) child.p_bLock = true;
                if (Run(CmdHome())) return p_sInfo;
                p_eState = eState.Ready;
                foreach (IWTRChild child in p_aChild) child.p_bLock = false;
                return "OK";
            }
            finally
            {
                // if (p_sInfo != "OK") p_eState = eState.Error;
            }
        }
        #endregion

        #region InfoWafer UI
        InfoWaferWTR_UI m_ui;
        void InitInfoWaferUI()
        {
            m_ui = new InfoWaferWTR_UI();
            List<WTRArm> aArm = new List<WTRArm>();
            aArm.Add(m_dicArm[eArm.A]);
            aArm.Add(m_dicArm[eArm.B]);
            m_ui.Init(p_id + ".InfoWafer", aArm, m_engineer);
            m_aTool.Add(m_ui);
        }
        #endregion

        #region IWTRChild
        public List<string> m_asChild = new List<string>();
        List<IWTRChild> _aChild = new List<IWTRChild>();
        public List<IWTRChild> p_aChild { get { return _aChild; } }
        public void AddChild(params IWTRChild[] childs)
        {
            foreach (IWTRChild child in childs)
            {
                if (child != null)
                {
                    child.p_bLock = true;
                    p_aChild.Add(child);
                    m_asChild.Add(child.p_id);
                }
            }
            RunTree(Tree.eMode.Init);
            RunTreeRun(Tree.eMode.Init);

        }

        IWTRChild GetChild(string sChild)
        {
            foreach (IWTRChild child in p_aChild)
            {
                if (child.p_id == sChild) return child;
            }
            return null;
        }

        List<string> GetChildSlotNames(string sChild)
        {
            IWTRChild child = GetChild(sChild);
            if (child == null) return null;
            return child.p_asChildSlot;
        }

        int GetChildSlotID(string sChild, string sChildSlot)
        {
            List<string> asChildSlot = GetChildSlotNames(sChild);
            if (asChildSlot == null) return 0;
            for (int n = 0; n < asChildSlot.Count; n++)
            {
                if (sChildSlot == asChildSlot[n]) return n;
            }
            return 0;
        }

        public bool IsEnableRecovery()
        {
            if (m_dicArm[eArm.A].p_infoWafer != null) return true;
            if (m_dicArm[eArm.B].p_infoWafer != null) return true;
            return false; 
        }

        public void ReadInfoReticle_Registry()
        {
            m_dicArm[eArm.A].ReadInfoWafer_Registry();
            m_dicArm[eArm.B].ReadInfoWafer_Registry();
            foreach (IWTRChild child in p_aChild) child.ReadInfoWafer_Registry();
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
            p_eComm = (eComm)tree.Set(p_eComm, p_eComm, "Type", "Communication Type"); 
            m_dicArm[eArm.A].RunTree(tree.GetTree("Arm A", false));
            m_dicArm[eArm.B].RunTree(tree.GetTree("Arm B", false));
            foreach (IWTRChild child in p_aChild) child.RunTreeTeach(tree.GetTree("Teach", false));
            RunTimeoutTree(tree.GetTree("Timeout", false));
        }

        public override void Reset()
        {
            Run(CmdClear());
            p_eState = eState.Init;
            base.Reset();
        }

        public override void ButtonHome()
        {
            base.ButtonHome();
        }
        #endregion

        public WTR_Cymechs(string id, IEngineer engineer)
        {
            InitCommType(id); 
            InitErrorString(); 
            InitArms(id, engineer);
            InitBase(id, engineer);
            InitThread();
            InitInfoWaferUI();
        }

        public override void ThreadStop()
        {
            m_ui.ThreadStop(); 
            if (m_bRunSend)
            {
                m_bRunSend = false;
                m_threadSend.Join(); 
            }
            base.ThreadStop();
        }

        #region IWTR
        public ModuleRunBase CloneRunGet(string sChild, int nSlot)
        {
            Run_Get run = (Run_Get)m_runGet.Clone();
            run.p_sChild = sChild;
            run.m_nChildID = nSlot;
            return run;
        }

        public ModuleRunBase CloneRunPut(string sChild, int nSlot)
        {
            Run_Put run = (Run_Put)m_runPut.Clone();
            run.p_sChild = sChild;
            run.m_nChildID = nSlot;
            return run;
        }

        public string GetEnableAnotherArmID(ModuleRunBase runGet, WTRArm armPut, InfoWafer infoWafer)
        {
            eArm eArmPut = ((Arm)armPut).m_eArm;
            for (int n = 0; n < p_aArm.Count; n++)
            {
                Arm armGet = (Arm)p_aArm[n];
                if (armGet.m_eArm != eArmPut)
                {
                    if (armGet.IsEnableWaferSize(infoWafer))
                    {
                        ((Run_Get)runGet).m_eArm = armGet.m_eArm;
                        return armGet.m_id;
                    }
                }
            }
            return "Not Enable";
        }
        #endregion

        #region ModuleRun
        ModuleRunBase m_runGet;
        ModuleRunBase m_runPut;

        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Reset(this), false, "Reset WTR CPU");
            m_runGet = AddModuleRunList(new Run_Get(this), false, "WTR Run Get Motion");
            m_runPut = AddModuleRunList(new Run_Put(this), false, "WTR Run Put Motion");
            AddModuleRunList(new Run_CheckWafer(this), false, "Check Wafer");
        }
        public class Run_CheckWafer : ModuleRunBase
        {
            WTR_Cymechs m_module;
            public Run_CheckWafer(WTR_Cymechs module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bRunCheckWafer = true;
            public override ModuleRunBase Clone()
            {
                Run_CheckWafer run = new Run_CheckWafer(m_module);
                run.m_bRunCheckWafer = m_bRunCheckWafer;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bRunCheckWafer = tree.Set(m_bRunCheckWafer, m_bRunCheckWafer, "CheckWafer", "CheckWafer", bVisible, true);
            }

            public override string Run()
            {
                m_module.SendCmd("RQ WAFER ARM ALL");
                return "OK";
            }

        }
        public class Run_Reset : ModuleRunBase
        {
            WTR_Cymechs m_module;
            public Run_Reset(WTR_Cymechs module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bReset = true;
            public override ModuleRunBase Clone()
            {
                Run_Reset run = new Run_Reset(m_module);
                run.m_bReset = m_bReset;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bReset = tree.Set(m_bReset, m_bReset, "Reset", "Reset CPU", bVisible, true);
            }

            public override string Run()
            {
                return m_module.CmdClear();
            }

        }

        public class Run_Get : ModuleRunBase, IWTRRun
        {
            WTR_Cymechs m_module;
            public Run_Get(WTR_Cymechs module)
            {
                p_sChild = "";
                m_module = module;
                InitModuleRun(module);
            }

            public string p_sChild { get; set; }
            public bool p_isExchange { get; set; } = false;
            public int p_nExchangeSlot { get; set; } = -1;
            public eArm m_eArm = eArm.A;
            public int m_nChildID = 0;
            public override ModuleRunBase Clone()
            {
                Run_Get run = new Run_Get(m_module);
                run.m_eArm = m_eArm;
                run.p_sChild = p_sChild;
                run.m_nChildID = m_nChildID;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_module.RunTreeArm(tree, ref m_eArm, bVisible && !bRecipe);
                p_sChild = tree.Set(p_sChild, p_sChild, m_module.m_asChild, "Child", "WTR Child Name", bVisible);
                List<string> asChildSlot = m_module.GetChildSlotNames(p_sChild);
                if ((asChildSlot != null) && (asChildSlot.Count > 0))
                {
                    if ((m_nChildID < 0) || (m_nChildID >= asChildSlot.Count)) m_nChildID = 0;
                    string sChildSlot = asChildSlot[m_nChildID];
                    sChildSlot = tree.Set(sChildSlot, sChildSlot, asChildSlot, "Child ID", "WTR Child Slot", bVisible);
                    m_nChildID = m_module.GetChildSlotID(p_sChild, sChildSlot);
                }
                else m_nChildID = 0;
            }

            public void SetArm(WTRArm arm)
            {
                m_eArm = ((Arm)arm).m_eArm; 
            }

            public override string Run()
            {
                IWTRChild child = m_module.GetChild(p_sChild);
                if (child == null) return "WTR Child not Found : " + p_sChild;
                if (EQ.p_bSimulate)
                {
                    m_module.m_dicArm[m_eArm].p_infoWafer = child.GetInfoWafer(m_nChildID);
                    child.SetInfoWafer(m_nChildID, null);
                    return "OK";
                }
                int posWTR = child.GetTeachWTR(child.GetInfoWafer(m_nChildID));
                if (posWTR < 0) return "WTR Teach Position Not Defined";
                while (child.p_eState != eState.Ready)
                {
                    if (EQ.IsStop()) return "Stop";
                    Thread.Sleep(100);
                }
                if (m_module.m_dicArm[m_eArm].IsWaferExist()) return "Reticle is already exist.";
                if (m_module.Run(child.IsGetOK(m_nChildID))) return p_sInfo;
                try
                {
                    child.p_bLock = true;
					if (m_module.Run(m_module.CmdGoto(posWTR, m_nChildID + 1, m_eArm, false, false))) return p_sInfo;
                    if (m_module.Run(child.BeforeGet(m_nChildID))) return p_sInfo;
                    if (m_module.Run(m_module.CmdPick(posWTR, m_nChildID + 1, m_eArm))) return p_sInfo;
                    child.p_bLock = false;
                    m_log.Info("Material Location change : " + child.p_id + " -> " + "Robot");
                    child.AfterGet(m_nChildID);
                }
                finally 
                {
                    if (m_module.m_dicArm[m_eArm].IsWaferExist())
                    {
                        m_module.m_dicArm[m_eArm].p_infoWafer = child.GetInfoWafer(m_nChildID);
                        child.SetInfoWafer(m_nChildID, null);
                    }
                    else m_module.m_dicArm[m_eArm].p_infoWafer = null;
                }
                if (m_module.m_dicArm[m_eArm].IsWaferExist()) return "OK";
                return "WTR Get Error : Wafer Check Sensor not Detected at Arm = " + m_eArm.ToString();
            }
        }

        public class Run_Put : ModuleRunBase, IWTRRun
        {
            WTR_Cymechs m_module;
            public Run_Put(WTR_Cymechs module)
            {
                p_sChild = "";
                m_module = module;
                InitModuleRun(module);
            }

            public string p_sChild { get; set; }
            public bool p_isExchange { get; set; } = false;
            public int p_nExchangeSlot { get; set; } = -1;
            public eArm m_eArm = eArm.A;
            public int m_nChildID = 0;
            public override ModuleRunBase Clone()
            {
                Run_Put run = new Run_Put(m_module);
                run.m_eArm = m_eArm;
                run.p_sChild = p_sChild;
                run.m_nChildID = m_nChildID;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_module.RunTreeArm(tree, ref m_eArm, bVisible && !bRecipe);
                p_sChild = tree.Set(p_sChild, p_sChild, m_module.m_asChild, "Child", "WTR Child Name", bVisible);
                List<string> asChildSlot = m_module.GetChildSlotNames(p_sChild);
                if ((asChildSlot != null) && (asChildSlot.Count > 0))
                {
                    if ((m_nChildID < 0) || (m_nChildID >= asChildSlot.Count)) m_nChildID = 0;
                    string sChildSlot = asChildSlot[m_nChildID];
                    sChildSlot = tree.Set(sChildSlot, sChildSlot, asChildSlot, "Child ID", "WTR Child Slot", bVisible);
                    m_nChildID = m_module.GetChildSlotID(p_sChild, sChildSlot);
                }
                else m_nChildID = 0;
            }

            public void SetArm(WTRArm arm)
            {
                m_eArm = ((Arm)arm).m_eArm;
            }

            public override string Run()
            {
                IWTRChild child = m_module.GetChild(p_sChild);
                if (child == null) return "WTR Child not Found : " + p_sChild;
                if (EQ.p_bSimulate)
                {
                    child.SetInfoWafer(m_nChildID, m_module.m_dicArm[m_eArm].p_infoWafer);
                    m_module.m_dicArm[m_eArm].p_infoWafer = null;
                    return "OK";
                }
                while (child.p_eState != eState.Ready)
                {
                    if (EQ.IsStop()) return "Stop";
                    Thread.Sleep(100);
                }
                int posWTR = child.GetTeachWTR(m_module.m_dicArm[m_eArm].p_infoWafer);
                if (posWTR < 0) return "WTR Teach Position Not Defined";
                if (!m_module.m_dicArm[m_eArm].IsWaferExist()) return "Reticle is not exist in Robot-Arm.";
                if (m_module.Run(child.IsPutOK(m_module.m_dicArm[m_eArm].p_infoWafer, m_nChildID))) return p_sInfo;
                try
                {
                    child.p_bLock = true;
                    if (m_module.Run(m_module.CmdGoto(posWTR, m_nChildID + 1, m_eArm, false, true))) return p_sInfo;
                    if (m_module.Run(child.BeforePut(m_nChildID))) return p_sInfo;//check 필요
                    if (m_module.Run(m_module.CmdPlace(posWTR, m_nChildID + 1, m_eArm))) return p_sInfo;
                    child.p_bLock = false;
                    m_log.Info("Material Location change : " + "Robot" + " -> " + child.p_id);
                    child.AfterPut(m_nChildID);
                }
                finally
                {
                    if (!m_module.m_dicArm[m_eArm].IsWaferExist())
                    {
                        child.SetInfoWafer(m_nChildID, m_module.m_dicArm[m_eArm].p_infoWafer);
                        m_module.m_dicArm[m_eArm].p_infoWafer = null;
                    }
                    else child.SetInfoWafer(m_nChildID, null);
                }
                if (m_module.m_dicArm[m_eArm].IsWaferExist() == false) return "OK";
                return "WTR Put Error : Wafer Check Sensor not Detected at Child = " + child.p_id;
            }
        }
        #endregion

    }
}
