﻿using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_EFEM.Module
{
    public class RTR_RND : ModuleBase, IWTR
    {
        #region ToolBox
        protected RS232 m_rs232;
        public int m_teachReticleFlip = -1;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            m_dicArm[eArm.Upper].GetTools(m_toolBox);
            m_dicArm[eArm.Lower].GetTools(m_toolBox);
            if (bInit)
            {
                m_rs232.OnReceive += M_rs232_OnReceive;
                m_rs232.p_bConnect = true;
            }
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

        public class Arm : WTRArm
        {
            public eArm m_eArm;

            RTR_RND m_module;
            public Arm(string id, eArm arm, RTR_RND module, IEngineer engineer, bool bEnableWaferSize, bool bEnableWaferCount)
            {
                m_eArm = arm;
                m_module = module;
                Init(id + "." + arm.ToString(), engineer, bEnableWaferSize, bEnableWaferCount);
            }

            public string RunGrip(bool bGrip)
            {
                int nGrip = bGrip ? 1 : 0;
                if (m_module.Run(m_module.WriteCmd(eCmd.Grip, (int)m_eArm + 1, nGrip))) return m_module.p_sInfo;
                if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return m_module.p_sInfo;
                return "OK";
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

            public override void RunTree(Tree tree)
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
                if (m_sErrorMsgs[n, 0] == sCode) return m_sErrorMsgs[n, 1];
            }
            return "Can't Find Error Massage !!";
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
            int msRS232 = (int)(1000 * m_secDelayRS232);
            int ms10 = 0;
            if (m_rs232.p_bConnect == false) m_rs232.p_bConnect = true;

            if (Run(WriteCmd(eCmd.ResetCPU))) return true;
            Thread.Sleep(100);

            while (m_eSendCmd != eCmd.None)
            {
                Thread.Sleep(10);
                ms10 += 10;
                if (ms10 > msRS232)
                {
                    m_rs232.p_bConnect = false;
                    m_bNeedHome = true;
                    m_eSendCmd = eCmd.None;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region RS232
        protected void M_rs232_OnReceive(string sRead)
        {
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
                if (m_eSendCmd != eCmd.None) return "RS232 Communication Error !!";
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
            m_rs232.Send(str);
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
                if (m_eSendCmd != eCmd.None) return "RS232 Communication Error !!";
            }
            if (EQ.IsStop()) return "EQ Stop";

            string str = m_dicCmd[cmd];
            str += " " + sSpeed;
            m_log.Info(" [ Send --> " + str);
            m_eSendCmd = cmd;
            m_rs232.Send(str);
            return "OK";
        }

        protected string WriteCmdManualMove(eCmd cmd, string sRMove, string sTMove, string sZMove, string sFMove, string sVMove)
        {
            if (EQ.IsStop()) return "EQ Stop";
            Thread.Sleep(10);
            if (m_eSendCmd != eCmd.None)
            {
                if (EQ.IsStop()) return "EQ Stop";
                Thread.Sleep(200);
                if (m_eSendCmd != eCmd.None) return "RS232 Communication Error !!";
            }
            if (EQ.IsStop()) return "EQ Stop";

            string str = m_dicCmd[cmd];
            string sMove = String.Format("{0},{1},{2},{3},{4}", sRMove, sTMove, sZMove, sFMove, sVMove);
            str += " " + sMove;
            m_log.Info(" [ Send --> " + str);
            m_eSendCmd = cmd;
            m_rs232.Send(str);
            return "OK";
        }


        protected string WaitReply(int secTimeout)
        {
            try
            {
                if (EQ.IsStop()) return "EQ Stop";
                int msDelay = 1000 * secTimeout;
                int ms10 = 0;
                if (m_rs232.p_bConnect == false) return m_eSendCmd.ToString() + " RS232 not Connect !!";
                while (m_eSendCmd != eCmd.None)
                {
                    if (EQ.IsStop()) return "EQ Stop";
                    Thread.Sleep(10);
                    ms10 += 10;
                    if (ms10 > msDelay) return m_eSendCmd.ToString() + " Has no Answer !!";
                }
                return "OK";
            }
            finally { m_eSendCmd = eCmd.None; }
        }
        #endregion

        #region Home
        const int c_nReset = 3;
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            try
            {
                int nReset = 0;
                while (IsFailResetCPU())
                {
                    nReset++;
                    if (nReset > c_nReset) return "Reset CPU Error";
                    Thread.Sleep(100);
                }
                foreach (IWTRChild child in p_aChild) child.p_bLock = true;
                if (m_bNeedHome)
                {
                    if (Run(WriteCmd(eCmd.FindHome))) return p_sInfo;
                }
                else
                {
                    if (Run(WriteCmd(eCmd.FindHome))) return p_sInfo;
                }
                m_bNeedHome = false;
                if (Run(WaitReply(m_secHome))) return p_sInfo;
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
        }

        protected IWTRChild GetChild(string sChild)
        {
            foreach (IWTRChild child in p_aChild)
            {
                if (child.p_id == sChild) return child;
            }
            return null;
        }
        protected ModuleBase GetChild_Module(string sChild)
        {
            foreach (ModuleBase child in p_aChild)
            {
                if (child.p_id == sChild) return child;
            }
            return null;
        }

        protected List<string> GetChildSlotNames(string sChild)
        {
            IWTRChild child = GetChild(sChild);
            if (child == null) return null;
            return child.p_asChildSlot;
        }

        protected int GetChildSlotID(string sChild, string sChildSlot)
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
            if (m_dicArm[eArm.Lower].p_infoWafer != null) return true;
            if (m_dicArm[eArm.Upper].p_infoWafer != null) return true;
            return false;
        }

        public void ReadInfoReticle_Registry()
        {
            m_dicArm[eArm.Lower].ReadInfoWafer_Registry();
            m_dicArm[eArm.Upper].ReadInfoWafer_Registry();
            foreach (IWTRChild child in p_aChild) child.ReadInfoWafer_Registry();
        }
        #endregion

        #region Timeout
        public int m_secDelayRS232 = 2;
        public int m_secHome = 60;
        public int m_secMotion = 20;
        void RunTimeoutTree(Tree tree)
        {
            m_secDelayRS232 = tree.Set(m_secDelayRS232, m_secDelayRS232, "RS232", "Timeout (sec)");
            m_secHome = tree.Set(m_secHome, m_secHome, "Home", "Timeout (sec)");
            m_secMotion = tree.Set(m_secMotion, m_secMotion, "Motion", "Timeout (sec)");
        }
        #endregion

        #region InfoWafer UI
        InfoWaferWTR_UI m_ui;
        public List<WTRArm> aArm;

        void InitInfoWaferUI()
        {
            m_ui = new InfoWaferWTR_UI();
            aArm = new List<WTRArm>();
            aArm.Add(m_dicArm[eArm.Upper]);
            aArm.Add(m_dicArm[eArm.Lower]);
            m_ui.Init(p_id + ".InfoWafer", aArm, m_engineer);
            m_aTool.Add(m_ui);
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
            m_dicArm[eArm.Upper].RunTree(tree.GetTree("Upper Arm", false));
            m_dicArm[eArm.Lower].RunTree(tree.GetTree("Lower Arm", false));
            foreach (IWTRChild child in p_aChild) child.RunTreeTeach(tree.GetTree("Teach", false));
            RunTimeoutTree(tree.GetTree("Timeout", false));
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override void ButtonHome()
        {
            m_bNeedHome = true;
            base.ButtonHome();
        }
        #endregion

        public RTR_RND(string id, IEngineer engineer)
        {
            InitCmd();
            InitArms(id, engineer);
            InitBase(id, engineer);
            InitInfoWaferUI();
        }

        public override void ThreadStop()
        {
            m_ui.ThreadStop();
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
            AddModuleRunList(new Run_ResetCPU(this), false, "Reset WTR CPU");
            AddModuleRunList(new Run_Grip(this), false, "Run Grip WTR Arm");
            m_runGet = AddModuleRunList(new Run_Get(this), false, "WTR Run Get Motion");
            m_runPut = AddModuleRunList(new Run_Put(this), false, "WTR Run Put Motion");
        }

        public class Run_ResetCPU : ModuleRunBase
        {
            RTR_RND m_module;
            public Run_ResetCPU(RTR_RND module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bReset = true;
            public override ModuleRunBase Clone()
            {
                Run_ResetCPU run = new Run_ResetCPU(m_module);
                run.m_bReset = m_bReset;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bReset = tree.Set(m_bReset, m_bReset, "Reset", "Reset CPU", bVisible, true);
            }

            public override string Run()
            {
                return m_module.WriteCmd(eCmd.ResetCPU);
            }

        }

        public class Run_Grip : ModuleRunBase
        {
            RTR_RND m_module;
            public Run_Grip(RTR_RND module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eArm m_eArm = eArm.Upper;
            bool m_bGrip = false;
            public override ModuleRunBase Clone()
            {
                Run_Grip run = new Run_Grip(m_module);
                run.m_eArm = m_eArm;
                run.m_bGrip = m_bGrip;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_module.RunTreeArm(tree, ref m_eArm, bVisible);
                m_bGrip = tree.Set(m_bGrip, m_bGrip, "Grip", "Grip Arm Grip", bVisible);
            }

            public override string Run()
            {
                return m_module.m_dicArm[m_eArm].RunGrip(m_bGrip);
            }
        }
        public static bool m_bDoflip = false;

        public class Run_Get : ModuleRunBase, IWTRRun
        {
            RTR_RND m_module;
            public Run_Get(RTR_RND module)
            {
                p_sChild = "";
                m_module = module;
                InitModuleRun(module);
            }

            public string p_sChild { get; set; }
            public eArm m_eArm = eArm.Upper;
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
                ModuleBase child_module = m_module.GetChild_Module(p_sChild);
                if (child == null) return "WTR Child not Found : " + p_sChild;
                if (EQ.p_bSimulate)
                {
                    m_module.m_dicArm[m_eArm].p_infoWafer = child.GetInfoWafer(m_nChildID);
                    child.SetInfoWafer(m_nChildID, null);
                    return "OK";
                }
                int posWTR = 0;
                if (m_bDoflip == true && child.p_id == "MainVision")
                {
                    posWTR = m_module.m_teachReticleFlip;
                    m_bDoflip = false;
                }
                else
                {
                    posWTR = child.GetTeachWTR(child.GetInfoWafer(m_nChildID));
                }
                if (posWTR < 0) return "WTR Teach Position Not Defined";
                if (child.p_eState != eState.Ready)
                {
                    if (m_module.Run(m_module.WriteCmd(eCmd.GetReady, posWTR, m_nChildID + 1, (int)m_eArm + 1))) return p_sInfo;
                }
                while (child.p_eState != eState.Ready)
                {
                    if (EQ.IsStop()) return "Stop";
                    Thread.Sleep(100);
                }

                if (p_sChild == "MainVision")
                {
                    while (child_module.IsBusy()) Thread.Sleep(10);
                }

                if (m_module.Run(child.BeforeGet(m_nChildID))) return p_sInfo;
                if (p_sChild == "MainVision")
                {
                    while (child_module.IsBusy()) Thread.Sleep(10);
                }
                if (m_module.Run(child.IsGetOK(m_nChildID))) return p_sInfo;
                m_module.m_dicArm[m_eArm].p_infoWafer = child.GetInfoWafer(m_nChildID);
                try
                {
                    child.p_bLock = true;
                    if (m_module.Run(m_module.WriteCmd(eCmd.Get, posWTR, m_nChildID + 1, (int)m_eArm + 1))) return p_sInfo;
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    child.p_bLock = false;
                    child.AfterGet(m_nChildID);
                }
                finally
                {
                    if (m_module.m_dicArm[m_eArm].IsWaferExist()) child.SetInfoWafer(m_nChildID, null);
                    else m_module.m_dicArm[m_eArm].p_infoWafer = null;
                }
                if (m_module.m_dicArm[m_eArm].IsWaferExist()) return "OK";
                return "WTR Get Error : Wafer Check Sensor not Detected at Arm = " + m_eArm.ToString();
            }
        }

        public class Run_Put : ModuleRunBase, IWTRRun
        {
            RTR_RND m_module;
            public Run_Put(RTR_RND module)
            {
                p_sChild = "";
                m_module = module;
                InitModuleRun(module);
            }

            public string p_sChild { get; set; }
            public eArm m_eArm = eArm.Upper;
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
                ModuleBase child_module = m_module.GetChild_Module(p_sChild);
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
                if (p_sChild == "MainVision")
                {
                    while (child_module.IsBusy()) Thread.Sleep(10);
                }
                if (m_module.Run(child.BeforePut(m_nChildID))) return p_sInfo;
                if (p_sChild == "MainVision")
                {
                    while (child_module.IsBusy()) Thread.Sleep(10);
                }
                if (m_module.Run(child.IsPutOK(m_module.m_dicArm[m_eArm].p_infoWafer, m_nChildID))) return p_sInfo;
                child.SetInfoWafer(m_nChildID, m_module.m_dicArm[m_eArm].p_infoWafer);
                try
                {
                    child.p_bLock = true;
                    if (m_module.Run(m_module.WriteCmd(eCmd.Put, posWTR, m_nChildID + 1, (int)m_eArm + 1))) return p_sInfo;
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    child.p_bLock = false;
                    child.AfterPut(m_nChildID);
                }
                finally
                {                
                    m_module.m_dicArm[m_eArm].p_infoWafer = null;
                }
                if (m_module.m_dicArm[m_eArm].IsWaferExist() == false) return "OK";
                return "WTR Put Error : Wafer Check Sensor not Detected at Child = " + child.p_id;
            }
        }
        #endregion

    }
}
