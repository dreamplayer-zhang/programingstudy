using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_VEGA_P.Module
{
    /*
    public class RTR : ModuleBase
    {
        #region ToolBox
        TCPAsyncClient m_tcpip;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_tcpip, this, "TCPIP");
            m_dicArm[eArm.Upper].GetTools(m_toolBox);
            m_dicArm[eArm.Lower].GetTools(m_toolBox);
            if (bInit)
            {
                m_tcpip.EventReciveData += M_tcpip_EventReciveData;
                InitALID();
            }
        }
        #endregion

        #region GAF
        ALID alid_RND;
        void InitALID()
        {
            alid_RND = m_gaf.GetALID(this, "RND", "WTR RND ERROR");
        }
        #endregion

        #region Arm
        public enum eArm
        {
            Lower,
            Upper,
        }
        public Dictionary<eArm, Arm> m_dicArm = new Dictionary<eArm, Arm>();
        protected virtual void InitArms(string id, IEngineer engineer)
        {
            m_dicArm.Add(eArm.Lower, new Arm(id, eArm.Lower, this, engineer, true, false));
            m_dicArm.Add(eArm.Upper, new Arm(id, eArm.Upper, this, engineer, true, false));
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
            if (p_asArm.Count < 2)
            {
                foreach (Arm arm in m_dicArm.Values)
                {
                    if (arm.p_bEnable) eArm = arm.m_eArm;
                }
                return;
            }
            string sArm = eArm.ToString();
            sArm = tree.Set(sArm, sArm, p_asArm, "Arm", "Select WTR Arm", bVisible);
            foreach (Arm arm in m_dicArm.Values)
            {
                if (arm.m_eArm.ToString() == sArm) eArm = arm.m_eArm;
            }
        }

        public class Arm
        {
            public eArm m_eArm;

            RTR m_module;
            public Arm(string id, eArm arm, RTR module, IEngineer engineer)
            {
                m_eArm = arm;
                m_module = module;
                Init(id + "." + arm.ToString(), engineer, bEnableWaferSize, bEnableWaferCount);
            }

            public DIO_I m_diCheckVac;
            public DIO_I m_diArmClose;
            public void GetTools(ToolBox toolBox)
            {
                m_module.p_sInfo = toolBox.Get(ref m_diCheckVac, m_module, m_eArm.ToString() + ".CheckVac");
                m_module.p_sInfo = toolBox.Get(ref m_diArmClose, m_module, m_eArm.ToString() + ".ArmClose");
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
                    case eCheckWafer.Sensor: return m_diCheckVac.p_bIn;
                    default: return (p_infoWafer != null);
                }
            }

            public void RunTree(Tree tree)
            {
                m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "Wafer Check", "Wafer Check Option");
                base.RunTree(tree);
            }
        }
        #endregion

        #region ErrorMsg
        string[,] m_sErrorMsgs = new string[,]
        {
            { "E01", "동작 중 실행 불가능 명령을 수신  명령 일람표 확인  "},
            { "E02", "에러 중에 구동 명령을 수신  DRT 명령 후 사용  "},
            { "E03", "이 상태에서는 명령은 수신할 수 없음  명령 일람표 확인"},
            { "E04", "정지 요구 중에 다시 정지 명령을 수신  정지 시 까지 대기"},
            { "E05", "데이터범위에러  데이터 설정 범위 확인"},
            { "E06", "포맷에러  미사용"},
            { "E07", "이동 범위가 소프트 리미트를 넘음  동작 영역 확인 및 소프트 리미트 확인"},
            { "E08", "POINT 넘버에러"},
            { "E09", "지정한 POINT 데이터 없음  지정 Point Data가 존재 하는지 확인"},
            { "E10", "이 명령은 실행할 수 없음  C-HOST 명령어 확인"},
            { "E11", "ＰＯＳ의 요청 범위가 너무 큼  수신 데이터 범위를 줄여서 사용"},
            { "E12", "지정한 명령어가 존재 하지 않음  명령어 확인"},
            { "E13", "매크로 명령어가 중복 정의되어 있음  매크로 명령어 확인"},
            { "E99", "통신 이상  통신 케이블 확인"},
            { "-786", "PROJECT_CUSTOM_ERROR   "},
            { "-1000", "Invalid Robot Number"},
            { "-1001", "Undefined Robot"},
            { "-1002", "Invalid axis number"},
            { "-1003", "Undefined axis"},
            { "-1004", "Invalid motor number"},
            { "-1005", "Undefined motor"},
            { "-1006", "Robot already attached"},
            { "-1007", "Robot not ready to be attached"},
            { "-1008", "Cant detached a moving robot"},
            { "-1009", "Command is not completed"},
            { "-1010", "No robot selected"},
            { "-1011", "Illegal during special Cartesian mode"},
            { "-1012", "Joint out-of-range"},
            { "-1013", "Motor out-of-range"},
            { "-1014", "Time out during nulling"},
            { "-1015", "Invalid roll over spec"},
            { "-1016", "Torque control mode incorrect"},
            { "-1017", "Not in position control mode"},
            { "-1018", "Not in velocity control mode"},
            { "-1019", "Timeout sending servo setpoint"},
            { "-1020", "Timeout reading servo status"},
            { "-1021", "Robot not homed"},
            { "-1022", "Invalid homing parameter"},
            { "-1023", "Missed signal during homing"},
            { "-1024", "Encoder index disabled"},
            { "-1025", "Timeout enabling power"},
            { "-1026", "Timeout enabling amp"},
            { "-1027", "Timout starting commutation"},
            { "-1028", "Hard E-STOP"},
            { "-1029", "Asynchronous error"},
            { "-1030", "Fatal asynchronous error"},
            { "-1031", "Analog input value too small"},
            { "-1032", "Analog input value too big"},
            { "-1033", "Invalid Cartesian value"},
            { "-1034", "Negative Overtravel"},
            { "-1035", "Positive Overtravel"},
            { "-1036", "Kinematics not installed"},
            { "-1037", "Motors not commutated"},
            { "-1038", "Project generated robot error"},
            { "-1039", "Position too close"},
            { "-1040", "Position too far"},
            { "-1041", "Invalid Base transform"},
            { "-1042", "Can’t change robot config"},
            { "-1043", "Asynchronous soft error"},
            { "-1044", "Auto mode disabled"},
            { "-1045", "Soft E-STOP"},
            { "-1046", "Power not enabled"},
            { "-1047", "Virtual MCP in Jog mode"},
            { "-1048", "Hardware MCP in Jog mode"},
            { "-1049", "Timeout on homing DIN"},
            { "-1050", "Illegal during joint motion"},
            { "-1051", "Incorrect Cartesian trajectory mode"},
            { "-1052", "Beyond conveyor limits"},
            { "-1053", "Beyond conveyor limits while tracking"},
            { "-1054", "Can’t attach Encoder Only robot"},
            { "-1055", "Cartesian motion not configured"},
            { "-1522", "Can not save when power on"},
            { "-1600", "Power off requested"},
            { "-1601", "Software Reset using default settings"},
            { "-1602", "External E-STOP"},
            { "-1603", "Watchdog timer expired"},
            { "-1604", "Power light failure"},
            { "-1605", "Unknown power off request"},
            { "-1606", "E-STOP stuck off"},
            { "-1607", "Trajectory task overrun"},
            { "-1609", "E-STOP timer failed"},
            { "-1610", "Controller overheating"},
            { "-1611", "Auto/Manual switch set to Manual"},
            { "-1612", "Power supply relay stuck"},
            { "-1613", "Power supply shorted"},
            { "-1614", "Power supply overloaded"},
            { "-1615", "No 3-phase power"},
            { "-1616", "Shutdown due to overheating"},
            { "-1617", "CPU overheating"},
            { "-3000", "NULL pointer detected"},
            { "-3001", "Too many arguments"},
            { "-3002", "Too few arguments"},
            { "-3003", "Illegal value"},
            { "-3004", "Servo not initialized"},
            { "-3005", "Servo mode transition failed"},
            { "-3006", "Servo mode locked"},
            { "-3007", "Servo hash table not found"},
            { "-3008", "Servo hash entry collision"},
            { "-3009", "No hash entry found"},
            { "-3010", "Servo hash table full"},
            { "-3011", "Illegal parameter access"},
            { "-3012", "Servo high power failed"},
            { "-3013", "Servo task submission failed"},
            { "-3014", "Cal parameters not set correctly"},
            { "-3015", "Cal position not ready"},
            { "-3016", "Illegal cal seek command"},
            { "-3017", "No axis selected"},
            { "-3100", "Hard envelope error"},
            { "-3102", "Illegal zero index"},
            { "-3103", "Missing zero index"},
            { "-3104", "Motor duty cycle exceeded"},
            { "-3105", "Motor stalled"},
            { "-3106", "Axis over-speed"},
            { "-3107", "Amplifier over-current"},
            { "-3108", "Amplifier over-voltage"},
            { "-3109", "Amplifier under-voltage"},
            { "-3110", "Amplifier fault"},
            { "-3111", "Brake fault"},
            { "-3112", "Excessive dual encoder slippage"},
            { "-3113", "Motor commutation setup failed"},
            { "-3114", "Servo tasks overrun"},
            { "-3115", "Encoder quadrature error"},
            { "-3116", "Precise encoder index error"},
            { "-3117", "Amplifier RMS current exceeded"},
            { "-3118", "Dedicated DINs not config’ed for Hall"},
            { "-3119", "Illegal 6-step number"},
            { "-3120", "Illegal commutation angle"},
            { "-3121", "Encoder fault"},
            { "-3122", "Soft envelope error"},
            { "-3123", "Can’t switch serial encoder mode"},
            { "-3124", "Serial encoder busy"},
            { "-3125", "Illegal encoder command"},
            { "-3126", "Encoder operation error"},
            { "-3127", "Encoder battery low"},
            { "-3128", "Encoder battery down"},
            { "-3129", "Invalid encoder multi-turn data"},
            { "-3130", "Illegal encoder operation mode"},
            { "-3131", "Encoder not supported or mismatched"},
            { "-3132", "Trajectory extrapolation limit exceeded"},
            { "-3133", "Amplifier fault, DC bus stuck"},
            { "-3134", "Encoder data or accel/decel limit error"},
            { "-3135", "Phase offset too large"},
            { "-3136", "Cannot configure to adjust phase offset"},
            { "-3137", "Amplifier hardware failure or invalid"},
            { "-3138", "Encoder position not ready"},
            { "-3139", "Encoder not ready"},
            { "-3140", "Encoder communication error"},
            { "-3141", "Encoder overheated"},
            { "-3142", "Encoder hall sensor error"},
            { "-3143", "General serial bus encoder error"},
            { "-3144", "Amplifier overheating"},
            { "-3145", "Motor overheating"},
            { "2", "ERR_NOT_HOMED "},
            { "4", "ERR_EMERGENCY"},
            { "12", "ERR_MOTOR_ERROR"},
            { "194", "ERR_INTERLOCK"},
            { "202", "ERR_WAFER_BEFORE_GET "},
            { "203", "ERR_NO_WAFER_BEFORE_PUT"},
            { "204", "ERR_NO_WAFER_AFTER_GET"},
            { "205", "ERR_WAFER_AFTER_PUT"},
            { "206", "ERR_NO_WAFER_DURING_GET"},
            { "207", "ERR_WAFER_DURING_PUT"},
            { "208", "ERR_NOT_HOMED"},
            { "209", "ERR_NOT_SUPPORTED_FUNC "},
            { "251", "ERR_MAPPING_IS_NOT_PERFORMED"},
            { "252", "ERR_NO_MAPPING_DATA"},
            { "1001", "ERR_INVALID_COMMAND"},
            { "1011", "ERR_INVALID_DATA"},
            { "1012", "ERR_INVALID_STATION"},
            { "1013", "ERR_INVALID_HAND"},
            { "1014", "ERR_INVALID_SLOT"},
            { "1015", "ERR_INVALID_TEACHING_INDEX"},
            { "1016", "ERR_INVALID_PD_INDEX"},
            { "1017", "ERR_WAFER_DOUBLE_ERORR"},
            { "1018", "ERR_WAFER_NOEXIT_ERORR"},
            { "1021", "ERR_INVALID_COORDINATE_TYPE"},
            { "1031", "ERR_INVALID_ARGUMENT"},
            { "1033", "ERR_INVALID_FORMAT"},
            { "1034", "ERR_INVALID_LOCATION_FORMAT"},
            { "1035", "ERR_INVALID_PROFILE_FORMAT"},
            { "1041", "ERR_WRONG_PD_COMMAND"},
            { "1042", "ERR_WRONG_AWC_DATA"},
            { "1043", "ERR_NO_AWC_STATION"},
            { "1051", "ERR_NO_DATA"},
            { "1052", "ERR_NOT_HOME"},
            { "1053", "ERR_CANNOT_RETRACT_ARM"},
            { "1054", "ERR_VACUUM_DETECTING_ERORR"},
            { "1055", "ERR_NO_WAFER"},
            { "1056", "ERR_UPGRIP"},
            { "1057", "ERR_DOUBLEWAFERCHECH"},
            { "1060", "ERR_NOTSUPPLY_AIR"},
            { "1999", "USER_STOP_REQUEST"},
            { "2000", "ERR_RECEIVEBUF_FULL"},
            { "2001", "ERR_SENDBUF_FULL"},
        };
        string GetErrorString(string sCode)
        {
            for (int n = 0; n < m_sErrorMsgs.Length / 2; n++)
            {
                if (m_sErrorMsgs[n, 0] == sCode)
                {
                    alid_RND.Run(true, m_sErrorMsgs[n, 1]);
                    //alid 0207
                    return m_sErrorMsgs[n, 1];
                }
            }
            return "Can't Find Error Massage !!";
        }
        #endregion

        #region TCPIP
        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            string sRead = Encoding.Default.GetString(aBuf);
            string[] sReads = sRead.Split(' ');
            if (sReads[0] == "ERR") m_log.Error(GetErrorString(sReads[1]));
            else m_log.Info(sRead + " <-- Recv] ");
            Run(ReplyCmd(sReads));
            if (p_sInfo != "OK") p_eState = eState.Error;
            m_eSendCmd = eCmd.None;
        }

        eCmd m_eSendCmd = eCmd.None;
        protected string WriteCmd(eCmd cmd, params object[] objs)
        {
            if (EQ.IsStop()) return "EQ Stop";
            Thread.Sleep(10);
            if (m_eSendCmd != eCmd.None)
            {
                if (EQ.IsStop()) return "EQ Stop";
                Thread.Sleep(200);
                if (m_eSendCmd != eCmd.None) return " Communication Error !!";
            }
            if (EQ.IsStop()) return "EQ Stop";
            string str = m_dicCmd[cmd];
            if (objs.Length > 0)
            {
                str += " " + objs[0].ToString();
                for (int n = 1; n < objs.Length; n++) str += "," + objs[n].ToString();
            }
            switch (cmd)
            {
                case eCmd.GetReady:
                case eCmd.PutReady:
                    if (m_eSendCmd == cmd) return "OK";
                    break;
            }
            m_log.Info(" [ Send --> " + str);
            m_eSendCmd = cmd;
            m_tcpip.Send(str);
            return "OK";
        }

        protected string WriteCmdSetSpeed(eCmd cmd, string sSpeed)
        {
            if (EQ.IsStop()) return "EQ Stop";
            Thread.Sleep(10);
            if (m_eSendCmd != eCmd.None)
            {
                if (EQ.IsStop()) return "EQ Stop";
                Thread.Sleep(200);
                if (m_eSendCmd != eCmd.None) return " Communication Error !!";
            }
            if (EQ.IsStop()) return "EQ Stop";

            string str = m_dicCmd[cmd];
            str += " " + sSpeed;
            m_log.Info(" [ Send --> " + str);
            m_eSendCmd = cmd;
            m_tcpip.Send(str);
            return "OK";
        }

        protected string WriteCmdManualMove(eCmd cmd, string sRMove, string sTMove, string sFMove, string sZMove, string sVMove)
        {
            if (EQ.IsStop()) return "EQ Stop";
            Thread.Sleep(10);
            if (m_eSendCmd != eCmd.None)
            {
                if (EQ.IsStop()) return "EQ Stop";
                Thread.Sleep(200);
                if (m_eSendCmd != eCmd.None) return " Communication Error !!";
            }
            if (EQ.IsStop()) return "EQ Stop";

            string str = m_dicCmd[cmd];
            string sMove = String.Format("{0},{1},{2},{3},{4}", sRMove, sTMove, sFMove, sZMove, sVMove);
            str += " " + sMove;
            m_log.Info(" [ Send --> " + str);
            m_eSendCmd = cmd;
            m_tcpip.Send(str);
            return "OK";
        }


        protected string WaitReply(int secTimeout)
        {
            try
            {
                // if (EQ.IsStop()) return "EQ Stop";
                int msDelay = 1000 * secTimeout;
                int ms10 = 0;
                if (m_tcpip.p_bConnect == false) return m_eSendCmd.ToString() + " Communication not Connect !!";
                while (m_eSendCmd != eCmd.None)
                {
                    //if (EQ.IsStop()) return "EQ Stop";
                    Thread.Sleep(10);
                    ms10 += 10;
                    if (ms10 > msDelay) return m_eSendCmd.ToString() + " Has no Answer !!";
                }
                return "OK";
            }
            finally { m_eSendCmd = eCmd.None; }
        }
        #endregion

        #region Protocol
        public enum eCmd
        {
            None,
            ReadState,  // State confirmation
            ResetCPU,   // error시 명령어가 듣지 않음. 리셋.
            FindHome,   // Initialize (얼라이너는 이걸로 홈)
            MoveHome,   // 로봇 origin 위치로 이동
            Grip,       // Gripper on off
            Get,        // Pick up the wafer with the present finger
            GetReady,
            Put,        // Place the wafer with the present finger
            PutReady,
            Extend,
            Retraction,
            SetSpeed,
            ManualMove,
        };
        Dictionary<eCmd, string> m_dicCmd = new Dictionary<eCmd, string>();
        void InitCmd()
        {
            m_dicCmd.Add(eCmd.ReadState, "PRAR VR1,VR2,VR3,VR4");
            m_dicCmd.Add(eCmd.ResetCPU, "DRT");
            m_dicCmd.Add(eCmd.FindHome, "ORG");
            m_dicCmd.Add(eCmd.MoveHome, "HOME");
            m_dicCmd.Add(eCmd.Grip, "GRIP");
            m_dicCmd.Add(eCmd.Get, "GET");
            m_dicCmd.Add(eCmd.GetReady, "GRDY");
            m_dicCmd.Add(eCmd.Put, "PUT");
            m_dicCmd.Add(eCmd.PutReady, "PRDY");
            m_dicCmd.Add(eCmd.Extend, "EXTA");
            m_dicCmd.Add(eCmd.Retraction, "RETA");
            m_dicCmd.Add(eCmd.SetSpeed, "TSPD");
            m_dicCmd.Add(eCmd.ManualMove, "MMI");
        }

        string ReplyCmd(string[] sMsgs)
        {
            string sLastCmd = m_dicCmd[m_eSendCmd];
            try
            {
                if (sMsgs.Length > 1) return GetErrorString(sMsgs[1]);
                else if (sMsgs[0] == sLastCmd && sMsgs.Length == 1) return "OK"; //return "Received Successfully : " + sLastCmd;
                else return "Cannot Receive Status Command : " + sLastCmd;
            }
            catch (Exception)
            {
                if (sMsgs[0].Length != sLastCmd.Length) return "Received error : " + sMsgs[0];
            }
            return "OK";
        }

        bool m_bNeedHome = true;
        bool IsFailResetCPU()
        {
            int msComm = (int)(1000 * m_secDelayComm);
            int ms10 = 0;
            if (m_tcpip.p_bConnect == false) m_tcpip.p_bConnect = true;

            if (Run(WriteCmd(eCmd.ResetCPU))) return true;
            Thread.Sleep(100);

            while (m_eSendCmd != eCmd.None)
            {
                Thread.Sleep(10);
                ms10 += 10;
                if (ms10 > msComm)
                {
                    m_tcpip.p_bConnect = false;
                    m_bNeedHome = true;
                    m_eSendCmd = eCmd.None;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Timeout
        public int m_secDelayComm = 2;
        public int m_secHome = 60;
        public int m_secMotion = 20;
        void RunTimeoutTree(Tree tree)
        {
            m_secDelayComm = tree.Set(m_secDelayComm, m_secDelayComm, "Comm", "Communication Delay (sec)");
            m_secHome = tree.Set(m_secHome, m_secHome, "Home", "Timeout (sec)");
            m_secMotion = tree.Set(m_secMotion, m_secMotion, "Motion", "Timeout (sec)");
        }
        #endregion

        public RTR(string id, IEngineer engineer)
        {
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
    */
}
