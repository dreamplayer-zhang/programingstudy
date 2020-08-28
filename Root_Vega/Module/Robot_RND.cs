using Root_Vega.ManualJob;
using RootTools;
using RootTools.Camera.CognexOCR;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.GAFs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Root_Vega.Module
{
    public class Robot_RND : ModuleBase
    {
        #region ToolBox
        public DIO_I m_diCheckVac;
        public DIO_I m_diArmClose;
        RS232 m_rs232;
        Camera_CognexOCR m_camOCR;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_diCheckVac, this, "CheckVac");
            p_sInfo = m_toolBox.Get(ref m_diArmClose, this, "ArmClose");
            p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            p_sInfo = m_toolBox.Get(ref m_camOCR, this, "CamOCR");
            if (bInit)
            {
                InitALID();
                m_rs232.OnRecieve += M_rs232_OnRecieve;
                m_rs232.p_bConnect = true;
            }
        }
        #endregion

        ALID m_alidRTRWarningPos;

        void InitALID()
		{
            m_alidRTRWarningPos = m_gaf.GetALID(this, "RTR WarningPos", "RTR WarningPos Error");
		}

        #region DIO Function
        public string RunGrip(bool bGrip)
        {
            int nGrip = bGrip ? 1 : 0;
            if (Run(WriteCmd(eCmd.Grip, 1, nGrip))) return p_sInfo;
            if (Run(WaitReply(m_secMotion))) return p_sInfo;
            return "OK";
        }

        public bool IsReticleExist(bool bIgnoreExistSensor = false)
        {
            bool bExist = false; 
            if (bIgnoreExistSensor) bExist = (p_infoReticle != null); 
            else bExist = m_diCheckVac.p_bIn;
            p_brushReticleExist = bExist ? Brushes.Yellow : Brushes.Green;
            return bExist; 
        }

        Brush _brushReticleExist = Brushes.Green; 
        public Brush p_brushReticleExist
        {
            get { return _brushReticleExist; }
            set
            {
                if (_brushReticleExist == value) return;
                _brushReticleExist = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region InfoReticle
        string m_sInfoReticle = "";
        InfoReticle _infoReticle = null;
        public InfoReticle p_infoReticle
        {
            get { return _infoReticle; }
            set
            {
                m_sInfoReticle = (value == null) ? "" : value.p_id;
                _infoReticle = value;
                if (m_reg != null) m_reg.Write("sInfoReticle", m_sInfoReticle);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoReticle_Registry()
        {
            m_reg = new Registry(p_id + ".InfoReticle");
            p_bDisableHomeWhenArmOpen = m_reg.Read("p_bDisableHomeWhenArmOpen", false);
            m_sInfoReticle = m_reg.Read("sInfoReticle", m_sInfoReticle);
            p_infoReticle = m_engineer.ClassHandler().GetGemSlot(m_sInfoReticle);
            foreach (IRobotChild child in m_aChild) child.ReadInfoReticle_Registry();
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
            GetExt,     // Robot GET 세부동작1 팔 뻗고 Z축 Get 1단계 UP 동작 까지 수행
            GetRet,     // Robot GET 세부동작2 Grip On 이후 Get 2단계 Up 후 팔 접는 동작까지 수행
            GetReady,
            Put,        // Place the wafer with the present finger
            PutExt,     // Robot PUT 세부동작1 팔 뻗고 z축 PUT 1단계 Down 동작 까지 수행
            PutRet,     // Robot PUT 세부동작2 Grip off 이후 Z축 PUT 2단계 Down 후 팔 접는 동작까지 수행
            PutReady,
            Extend,
            Retraction,
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
            m_dicCmd.Add(eCmd.GetExt, "GAEXTA");
            m_dicCmd.Add(eCmd.GetRet, "GARETA");
            m_dicCmd.Add(eCmd.GetReady, "GRDT");
            m_dicCmd.Add(eCmd.Put, "PUT");
            m_dicCmd.Add(eCmd.PutExt, "PAEXTA");
            m_dicCmd.Add(eCmd.PutRet, "PARETA");
            m_dicCmd.Add(eCmd.PutReady, "PRDY");
            m_dicCmd.Add(eCmd.Extend, "EXTA");
            m_dicCmd.Add(eCmd.Retraction, "RETA");
        }
        public enum eMotion
        {
            Get,        // Pick up the wafer with the present finger
            GetExt,     // Robot GET 세부동작1 팔 뻗고 Z축 Get 1단계 UP 동작 까지 수행
            GetRet,     // Robot GET 세부동작2 Grip On 이후 Get 2단계 Up 후 팔 접는 동작까지 수행
            GetReady,
            Put,        // Place the wafer with the present finger
            PutExt,     // Robot PUT 세부동작1 팔 뻗고 z축 PUT 1단계 Down 동작 까지 수행
            PutRet,     // Robot PUT 세부동작2 Grip off 이후 Z축 PUT 2단계 Down 후 팔 접는 동작까지 수행
            PutReady,
            Extend,
            Retraction,
        }
        Dictionary<eMotion, eCmd> m_dicMotion = new Dictionary<eMotion, eCmd>();
        void InitMotion()
        {
            m_dicMotion.Add(eMotion.Get, eCmd.Get);
            m_dicMotion.Add(eMotion.GetExt, eCmd.GetExt);
            m_dicMotion.Add(eMotion.GetRet, eCmd.GetRet);
            m_dicMotion.Add(eMotion.GetReady, eCmd.GetReady);
            m_dicMotion.Add(eMotion.Put, eCmd.Put);
            m_dicMotion.Add(eMotion.PutExt, eCmd.PutExt);
            m_dicMotion.Add(eMotion.PutRet, eCmd.PutRet);
            m_dicMotion.Add(eMotion.PutReady, eCmd.PutReady);
            m_dicMotion.Add(eMotion.Extend, eCmd.Extend);
            m_dicMotion.Add(eMotion.Retraction, eCmd.Retraction);
        }

        string ReplyCmd(string[] sMsgs)
        {
            string sLastCmd = m_dicCmd[m_eSendCmd]; 
            try
            {
                if (sMsgs.Length > 1) return GetErrorString(sMsgs[1]);
                else if (sMsgs[0] == sLastCmd && sMsgs.Length == 1) return "OK";
                else return "Cannot Recieve Status Command : " + sLastCmd;
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
            int msRS232 = (int)(1000 * m_secRS232);
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
        private void M_rs232_OnRecieve(string sRead)
        {
            string[] sReads = sRead.Split(' ');
            if (sReads[0] == "ERR") m_log.Error(GetErrorString(sReads[1]));
            else m_log.Info(sRead + " <-- Recv] ");
            Run(ReplyCmd(sReads));
            if (p_sInfo != "OK") p_eState = eState.Error;
            m_eSendCmd = eCmd.None;
        }

        eCmd m_eSendCmd = eCmd.None;
        string WriteCmd(eCmd cmd, params object[] objs)
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

        string WaitReply(int secTimeout)
        {
            try
            {
                if (EQ.IsStop()) return "EQ Stop";
                int msDelay = (int)(1000 * secTimeout);
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

        #region Home
        bool _bDisableHomeWhenArmOpen = false;
        public bool p_bDisableHomeWhenArmOpen
		{
            get { return _bDisableHomeWhenArmOpen; }
			set
			{
                if (_bDisableHomeWhenArmOpen == value) return;
                _bDisableHomeWhenArmOpen = value;
                m_reg.Write("p_bDisableHomeWhenArmOpen", value); 
			}
		}

        const int c_nReset = 3;
        public override string StateHome()//CHECK
        {
            if (EQ.p_bSimulate) return "OK";
            if (p_bDisableHomeWhenArmOpen && !m_diArmClose.p_bIn)
            {
                m_alidRTRWarningPos.Run(!m_diArmClose.p_bIn, "Please Check State of RTR Arm. if Arm is opened, Operate it manually.");
                return "RTR's Arm opened";
            }
            int nReset = 0;
            while (IsFailResetCPU())
            {
                nReset++;
                if (nReset > c_nReset) return "Reset CPU Error";
                Thread.Sleep(100);
            }
            foreach (IRobotChild child in m_aChild) child.p_bLock = true;
            if (m_bNeedHome ? Run(WriteCmd(eCmd.FindHome)) : Run(WriteCmd(eCmd.MoveHome))) return p_sInfo;
            m_bNeedHome = false;
            if (Run(WaitReply(m_secHome))) return p_sInfo;
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            foreach (IRobotChild child in m_aChild) child.p_bLock = false;
            p_bDisableHomeWhenArmOpen = false;
            return p_sInfo;
        }
        #endregion

        #region IRobotChild
        public List<string> m_asChild = new List<string>();
        public List<IRobotChild> m_aChild = new List<IRobotChild>();
        public void AddChild(params IRobotChild[] childs)
        {
            foreach (IRobotChild child in childs)
            {
                if (child != null)
                {
                    child.p_bLock = true;
                    m_aChild.Add(child);
                    m_asChild.Add(child.p_id);
                }
            }
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
        }

        IRobotChild GetChild(string sChild)
        {
            foreach (IRobotChild child in m_aChild)
            {
                if (child.p_id == sChild) return child;
            }
            return null;
        }
        #endregion

        #region Timeout
        int m_secRS232 = 2;
        int m_secHome = 20;
        int m_secMotion = 15;
        void RunTimeoutTree(Tree tree)
        {
            m_secRS232 = tree.Set(m_secRS232, m_secRS232, "RS232", "Timeout (sec)");
            m_secHome = tree.Set(m_secHome, m_secHome, "Home", "Timeout (sec)");
            m_secMotion = tree.Set(m_secMotion, m_secMotion, "Motion", "Timeout (sec)");
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
            foreach (IRobotChild child in m_aChild) child.RunTeachTree(tree.GetTree("Teach", false));
            RunTimeoutTree(tree.GetTree("Timeout", false));
            RunTreeTeach(tree.GetTree("Teach", false));
        }

        public int m_teachOCR = 0; 
        void RunTreeTeach(Tree tree)
        {
            m_teachOCR = tree.Set(m_teachOCR, m_teachOCR, "OCR", "Robot Teach Position"); 
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

        public ModuleRunBase GetRunMotion(eMotion motion, string sChild)
        {
            switch (motion)
            {
                case eMotion.Get:
                    Run_Get runGet = (Run_Get)CloneModuleRun("Get");
                    runGet.m_eMotion = motion;
                    runGet.m_sChild = sChild;
                    return runGet;
                case eMotion.Put:
                    Run_Put runPut = (Run_Put)CloneModuleRun("Put");
                    runPut.m_eMotion = motion;
                    runPut.m_sChild = sChild;
                    return runPut;
                default:
                    Run_Motion runMotion = (Run_Motion)CloneModuleRun("Motion");
                    runMotion.m_eMotion = motion;
                    runMotion.m_sChild = sChild;
                    return runMotion;
            }
        }

        public Robot_RND(string id, IEngineer engineer)
        {
            InitCmd();
            InitMotion();
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_ResetCPU(this), false, "Reset Robot CPU");
            AddModuleRunList(new Run_Grip(this), false, "Run Grip Robot Arm");
            AddModuleRunList(new Run_Motion(this), false, "Robot Run Motion");
            AddModuleRunList(new Run_Get(this), false, "Robot Run Get Motion");
            AddModuleRunList(new Run_Put(this), false, "Robot Run Put Motion");
            AddModuleRunList(new Run_ReadOCR(this), true, "Read CamOCR");
        }

        public class Run_ResetCPU : ModuleRunBase
        {
            Robot_RND m_module;
            public Run_ResetCPU(Robot_RND module)
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
            Robot_RND m_module;
            public Run_Grip(Robot_RND module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bGrip = false;
            public override ModuleRunBase Clone()
            {
                Run_Grip run = new Run_Grip(m_module);
                run.m_bGrip = m_bGrip;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bGrip = tree.Set(m_bGrip, m_bGrip, "Grip", "Grip Arm Grip", bVisible);
            }

            public override string Run()
            {
                return m_module.RunGrip(m_bGrip);
            }
        }

        public class Run_Motion : ModuleRunBase
        {
            Robot_RND m_module;
            public Run_Motion(Robot_RND module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eMotion m_eMotion = eMotion.Get;
            public string m_sChild = "";
            public override ModuleRunBase Clone()
            {
                Run_Motion run = new Run_Motion(m_module);
                run.m_eMotion = m_eMotion;
                run.m_sChild = m_sChild;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eMotion = (eMotion)tree.Set(m_eMotion, m_eMotion, "Motion", "Select Robot Motion", bVisible);
                m_sChild = tree.Set(m_sChild, m_sChild, m_module.m_asChild, "Child", "Robot Child Name", bVisible);
            }

            public override string Run()
            {
                IRobotChild child = m_module.GetChild(m_sChild);
                if (child == null) return "Robot Child not Found : " + m_sChild;
                eCmd cmd = m_module.m_dicMotion[m_eMotion];
                int posRobot = -1;
                switch (m_eMotion)
                {
                    case eMotion.Get:
                    case eMotion.GetExt:
                    case eMotion.GetRet:
                    case eMotion.GetReady:
                        if (m_module.Run(child.IsGetOK(ref posRobot))) return p_sInfo; 
                        if (posRobot < 0) return "Robot Teach Position Not Defined";
                        if (m_module.Run(child.BeforeGet())) return p_sInfo; 
                        child.p_bLock = true;
                        if (m_module.Run(m_module.WriteCmd(cmd, posRobot, 1, 1))) return p_sInfo;
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo; 
                        child.p_bLock = false;
                        return child.AfterGet();
                    case eMotion.Put:
                    case eMotion.PutExt:
                    case eMotion.PutRet:
                    case eMotion.PutReady:
                        if (m_module.Run(child.IsPutOK(ref posRobot, m_module.p_infoReticle))) return p_sInfo;
                        if (posRobot < 0) return "Robot Teach Position Not Defined";
                        if (m_module.Run(child.BeforePut())) return p_sInfo; 
                        child.p_bLock = true;
                        if (m_module.Run(m_module.WriteCmd(cmd, posRobot, 1, 1))) return p_sInfo;
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo; 
                        child.p_bLock = false;
                        return child.AfterPut();
                }
                return "OK";
            }
        }

        public class Run_Get : ModuleRunBase
        {
            Robot_RND m_module;
            public Run_Get(Robot_RND module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eMotion m_eMotion = eMotion.Get;
            public string m_sChild = "";
            public override ModuleRunBase Clone()
            {
                Run_Get run = new Run_Get(m_module);
                run.m_eMotion = m_eMotion;
                run.m_sChild = m_sChild;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sChild = tree.Set(m_sChild, m_sChild, m_module.m_asChild, "Child", "Robot Child Name", bVisible);
            }

            public override string Run()
            {
                IRobotChild child = m_module.GetChild(m_sChild);
                if (child == null) return "Robot Child not Found : " + m_sChild;
                if (EQ.p_bSimulate)
                {
                    m_module.p_infoReticle = child.p_infoReticle;
                    child.p_infoReticle = null;
                    return "OK";
                }
                m_module.p_infoReticle = child.p_infoReticle;
                int posRobot = -1;
                if (m_module.Run(child.IsGetOK(ref posRobot))) return p_sInfo;
                if (posRobot < 0) return "Robot Teach Position Not Defined";
                if (m_module.Run(child.BeforeGet())) return p_sInfo;
                child.p_bLock = true;
                if (m_module.Run(m_module.WriteCmd(eCmd.Get, posRobot, 1, 1))) return p_sInfo;
                if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                child.p_bLock = false;
                if (m_module.IsReticleExist())
                {
                    child.p_infoReticle = null;
                    if (child.IsReticleExist(true)) return "Robot Get Error : Reticle Check Sensor Detected at Child = " + child.p_id;
                    //child.p_infoReticle = null;
                }
                else
                {
                    m_module.p_infoReticle = null;
                    if (child.IsReticleExist() == false) child.p_infoReticle = null;
                    return "Robot Get Error : Reticle Check Sensor Detected at Arm";
                }
                return child.AfterGet();
            }
        }

        public class Run_Put : ModuleRunBase
        {
            Robot_RND m_module;
            public Run_Put(Robot_RND module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eMotion m_eMotion = eMotion.Put;
            public string m_sChild = "";
            public override ModuleRunBase Clone()
            {
                Run_Put run = new Run_Put(m_module);
                run.m_eMotion = m_eMotion;
                run.m_sChild = m_sChild;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sChild = tree.Set(m_sChild, m_sChild, m_module.m_asChild, "Child", "Robot Child Name", bVisible);
            }

            public override string Run()
            {
                IRobotChild child = m_module.GetChild(m_sChild);
                if (child == null) return "Robot Child not Found : " + m_sChild;
                if (EQ.p_bSimulate)
                {
                    child.p_infoReticle = m_module.p_infoReticle;
                    m_module.p_infoReticle = null; 
                }
                else
                {
                    //child.p_infoReticle = m_module.p_infoReticle;
                    int posRobot = -1;
                    if (m_module.Run(child.IsPutOK(ref posRobot, m_module.p_infoReticle))) return p_sInfo;
                    if (posRobot < 0) return "Robot Teach Position Not Defined";
                    if (m_module.Run(child.BeforePut())) return p_sInfo;
                    child.p_infoReticle = m_module.p_infoReticle;
                    child.p_bLock = true;
                    if (m_module.Run(m_module.WriteCmd(eCmd.Put, posRobot, 1, 1))) return p_sInfo;
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    child.p_bLock = false;
                    if (child.IsReticleExist(true))
                    {
                        m_module.p_infoReticle = null;
                        if (m_module.IsReticleExist()) return "Robot Put Error : Reticle Check Sensor Detected at Arm";
                        //m_module.p_infoReticle = null;
                    }
                    else
                    {
                        child.p_infoReticle = null;
                        if (m_module.IsReticleExist() == false) m_module.p_infoReticle = null; 
                        return "Robot Put Error : Reticle Check Sensor not Detected at Child = " + child.p_id;
                    }
                    if (m_module.Run(child.AfterPut())) return p_sInfo;
                }
                child.p_infoReticle.STSSetTransport(child.p_id, child.p_id.Contains("Loadport")); 
                return "OK"; 
            }
        }

        public class Run_ReadOCR : ModuleRunBase
        {
            Robot_RND m_module;
            public Run_ReadOCR(Robot_RND module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_nRetry = 3;
            double m_fMinScore = 70; 
            public override ModuleRunBase Clone()
            {
                Run_ReadOCR run = new Run_ReadOCR(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nRetry = tree.Set(m_nRetry, m_nRetry, "Retry", "Retry Count of ReadOCR", bVisible);
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK"; 
                int posRobot = m_module.m_teachOCR;
                if (m_module.Run(m_module.WriteCmd(eCmd.PutReady, posRobot, 1, 1))) return p_sInfo;
                if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                if (m_module.Run(m_module.WriteCmd(eCmd.Extend, posRobot, 1))) return p_sInfo;
                if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                if (m_module.Run(ReadOCR(m_nRetry))) return p_sInfo;
                //if (m_module.Run(m_module.WriteCmd(eCmd.GetReady, posRobot, 1, 1))) return p_sInfo;
                if (m_module.Run(m_module.WriteCmd(eCmd.Retraction))) return p_sInfo;
                if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                return "OK";
            }

            string ReadOCR(int nRetry)
            {
                if (m_module.p_infoReticle == null) return "InfoReticle = null";
                string sOCRMax = ""; 
                string sOCR = "";
                double fScoreMax = 0; 
                double fScore = 0;
                for (int n = 0; n < nRetry; n++)
                {
                    m_module.m_camOCR.ReadOCR(ref sOCR, ref fScore);
                    if (fScore >= m_fMinScore)
                    {
                        m_module.p_infoReticle.p_sSlotID = sOCR;
                        return "OK"; 
                    }
                    if (fScoreMax < fScore)
                    {
                        fScoreMax = fScore;
                        sOCRMax = sOCR; 
                    }
                }
                m_module.p_sInfo = "ReadOCR Score Error : " + fScoreMax.ToString() + "%, " + sOCRMax;
                EQ.p_eState = EQ.eState.Ready; 
                BitmapImage image = m_module.m_camOCR.ReadImage();
                SaveBMP(image, "c:\\Log\\Vega\\OCR.bmp"); 
                ManualOCR memualOCR = new ManualOCR(m_module.p_infoReticle, image);
                memualOCR.ShowOCR();
                return "OK"; 
            }

            void SaveBMP(BitmapImage image, string filePath)
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));

                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }
        #endregion
    }
}
