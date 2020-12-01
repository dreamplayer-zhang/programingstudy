using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_EFEM.Module
{
    public class WTR_Cymechs : ModuleBase, IWTR
    {
        #region ToolBox
        RS232 m_rs232;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            if (bInit)
            {
                m_rs232.OnReceive += M_rs232_OnReceive;
                m_rs232.p_bConnect = true;
            }
        }
        #endregion

        //forget
        #region WTR_RND_Arm
        public enum eArm
        {
            A,
            B,
        }
        public Dictionary<eArm, Arm> m_dicArm = new Dictionary<eArm, Arm>();
        void InitArms(string id)
        {
            m_dicArm.Add(eArm.A, new Arm(id, eArm.A, this));
            m_dicArm.Add(eArm.B, new Arm(id, eArm.B, this));
        }

        public class Arm : WTRArm
        {
            enum eWaferExist
            {
                Sensor,
                InfoWafer
            }
            eWaferExist m_eWaferExist = eWaferExist.Sensor;
            public bool IsWaferExist()
            {
                switch (m_eWaferExist)
                {
                    //case eWaferExist.Sensor: return m_diCheckVac.p_bIn; //forget
                    default: return (p_infoWafer != null);
                }
            }

            public override void RunTree(Tree tree)
            {
                m_eWaferExist = (eWaferExist)tree.Set(m_eWaferExist, m_eWaferExist, "WaferExist", "Wafer Exist Check");
                base.RunTree(tree);
            }

            public eArm m_eArm;
            public Arm(string id, eArm eArm, WTR_Cymechs wtr)
            {
                m_eArm = eArm;
                Init(id + "." + eArm.ToString(), wtr);
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

        #region Protocol
        public enum eCmd
        {
            HLLO,
            CLEAR,
            HOME,
            GOTO,
            PICK,
            PLACE,
            ZAXIS,
            SERVO,
            ESTOP
        }
        public enum eRequest
        {
            WAFER,
            SERVO,
            POS
        }

        public class Protocol
        {
            public enum eType
            {
                Command,
                Request
            }
            public eType m_eType = eType.Command;

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

            public bool OnReceive(string sRead)
            {
                if (m_eType == eType.Command)
                {
                    string sCode = sRead.Substring(0, 4); 
                    switch (sCode)
                    {
                        case "_ACK":
                            m_eState = eState.ACK;
                            return false;
                        case "_NAK":
                            m_eState = eState.NAK;
                            m_wtr.p_sInfo = "NAK : " + m_sCmd;
                            return true;
                        case "_RDY":
                            m_eState = eState.Done; 
                            return true;
                        case "_ERR":
                            m_eState = eState.Done;
                            m_wtr.p_sInfo = "Error : " + m_wtr.GetErrorString(sRead);
                            return true;
                    }
                }
                else
                {
                    //forget
                }
                return false; 
            }

            public string SendCmd()
            {
                if (m_eState != eState.Queue) return "Protocol State != Queue";
                m_wtr.SendCmd(m_sCmd);
                m_eState = eState.Send;
                return "OK";
            }

            public bool m_bValid = true;
            public string WaitDone(int secWait)
            {
                if (EQ.IsStop()) return "EQ Stop";
                if (m_wtr.m_rs232.p_bConnect == false) return "RS232 Connection Lost !!";
                int nWait = 100 * secWait;
                while (nWait > 0)
                {
                    if (EQ.IsStop()) return "EQ Stop";
                    Thread.Sleep(10);
                    if (m_eState == eState.Done) return "OK";
                    nWait--;
                }
                m_bValid = false;
                return m_sCmd + " : WaitDone Timeout !!";
            }

            public eCmd m_eCmd;
            WTR_Cymechs m_wtr;
            public Protocol(eCmd eCmd, WTR_Cymechs loadport, params string[] asParam)
            {
                m_eType = eType.Command; 
                m_eCmd = eCmd;
                m_wtr = loadport;
                m_sCmd = eCmd.ToString();
                foreach (string sParam in asParam) m_sCmd += " " + sParam;
            }
        }
        Protocol m_protocolSend = null;
        #endregion

        #region RS232
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
            if (m_protocolSend != null)
            {
                bool bDone = m_protocolSend.OnReceive(sRead);
                if (bDone) m_protocolSend = null; 
            }
        }

        string SendCmd(string sCmd)
        {
            if (EQ.IsStop()) return "EQ Stop";
            m_log.Info(" [ Send --> " + sCmd);
            return m_rs232.Send(sCmd);
        }
        #endregion

        public WTR_Cymechs(string id, IEngineer engineer)
        {
            InitErrorString(); 
            InitArms(id);
            InitBase(id, engineer);
            InitThread(); 
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        //forget
        public void AddChild(params IWTRChild[] childs)
        {
            throw new NotImplementedException();
        }

        public void ReadInfoReticle_Registry()
        {
            throw new NotImplementedException();
        }
    }
}
