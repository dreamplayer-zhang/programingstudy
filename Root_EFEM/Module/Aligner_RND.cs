using RootTools;
using RootTools.Camera.CognexOCR;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using RootTools.GAFs;

namespace Root_EFEM.Module
{
    public class Aligner_RND : ModuleBase, IWTRChild
    {
        #region ToolBox
        DIO_I m_diWaferExist;
        RS232 m_rs232;
        MemoryPool m_memoryPool;
        Camera_CognexOCR m_camOCR;
        public ALID m_alid_AlignFail;
        ALID m_alid_WaferExist;

        public void SetAlarm()
        {
            m_alid_WaferExist.Run(true, "Aligner Wafer Exist Error");
        }

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_diWaferExist, this, "WaferExist");
            p_sInfo = m_toolBox.GetComm(ref m_rs232, this, "RS232");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
            p_sInfo = m_toolBox.GetCamera(ref m_camOCR, this, "OCR");
            if (bInit)
            {
                InitMemory();
                m_rs232.OnReceive += M_rs232_OnReceive;
                m_rs232.p_bConnect = true;
            }
            m_alid_WaferExist = m_gaf.GetALID(this, "Aligner Wafer Exist", "Aligner Wafer Exist");
            m_alid_AlignFail = m_gaf.GetALID(this, "Align Fail", "Aligner Align Fail");
        }

        private void M_rs232_OnReceive(string sRead)
        {
            if (m_protocolSend != null)
            {
                p_sInfo = m_protocolSend.OnReceive(sRead);
                m_protocolSend.m_bDone = true;
            }
        }
        #endregion

        #region Memory
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryOCR;
        void InitMemory()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryOCR = m_memoryGroup.CreateMemory("OCR", 1, 1, new CPoint(2048, 2048));
        }
        #endregion

        #region Aligner Property
        int _pulseRotate = 0;
        public int p_pulseRotate
        {
            get { return _pulseRotate; }
            set
            {
                if (_pulseRotate == value) return;
                _pulseRotate = value;
                m_log.Info("Aligner Angle = " + value.ToString());
                OnPropertyChanged();
            }
        }
        RPoint _rpAxis = new RPoint();
        public RPoint p_rpAxis
        {
            get { return _rpAxis; }
            set
            {
                if (_rpAxis == value) return;
                _rpAxis = value;
                m_log.Info("Aligner Position = " + value.ToString());
                OnPropertyChanged();
            }
        }

        bool _bCheckWaferVac = false;
        public bool p_bCheckWaferVac
        {
            get { return _bCheckWaferVac; }
            set
            {
                if (_bCheckWaferVac == value) return;
                _bCheckWaferVac = value;
                m_log.Info("Aligner Check Wafer Vacuum = " + value.ToString());
                OnPropertyChanged();
            }
        }

        bool _bCheckWaferCCD = false;
        public bool p_bCheckWaferCCD
        {
            get { return _bCheckWaferCCD; }
            set
            {
                if (_bCheckWaferCCD == value) return;
                _bCheckWaferCCD = value;
                m_log.Info("Aligner Check Wafer CCD = " + value.ToString());
                OnPropertyChanged();
            }
        }

        string _sAlignerError = "";
        public string p_sAlignerError
        {
            get { return _sAlignerError; }
            set
            {
                if (_sAlignerError == value) return;
                _sAlignerError = value;
                m_log.Info("Aligner Error = " + value);
                OnPropertyChanged();
            }
        }

        string _sAlignerState = "";
        public string p_sAlignerState
        {
            get { return _sAlignerState; }
            set
            {
                if (_sAlignerState == value) return;
                _sAlignerState = value;
                m_log.Info("Aligner State = " + value);
                OnPropertyChanged();
            }
        }

        string _sAlignerControl = "";
        public string p_sAlignerControl
        {
            get { return _sAlignerControl; }
            set
            {
                if (_sAlignerControl == value) return;
                _sAlignerControl = value;
                m_log.Info("Aligner Control State = " + value);
                OnPropertyChanged();
            }
        }
        #endregion

        #region Aligner Command
        public enum eCmd
        {
            CheckVersion,   // 펌웨어 버전 읽기
            SearchHome,     // 원점 복귀
            ResetPos,       // 초기위치 이동
            GetPos,         // 현재위치 읽기
            SlowStop,       // 감속정지
            EmgStop,        // 비상정지
            ReadError,      // 에러상태 문의
            ClearError,     // 에러 해제
            WriteValue,     // 내부변수 쓰기
            ReadValue,      // 내부변수 읽기
            GetControlState,   // 제어기 상태 읽기
            GetState,       // 상태 요구
            VacuumOn,       // 진공 ON
            VacuumOff,      // 진공 OFF
            CheckWaferVac,  // 웨이퍼 유무 확인 (진공센서 사용)
            CheckWaferCCD,  // 웨이퍼 유무 확인 (CCD 사용)
            Align,          // 웨이퍼 정렬(ALIGN) 수행
            AlignRotate,    // 웨이퍼 정렬(ALIGN) 수행 후 파라미터에서 지정한 값 만큼 이동 및 회전. 소수점 2자리 지원
            SetWaferType,   // 웨이퍼 타입 설정
            SetWaferSize,   // 웨이퍼 크기 설정
            None,
        }
        class Command
        {
            public eCmd m_eCmd;
            public string m_sCmd;
            public double m_secWait = 6;

            public Command(eCmd eCmd, string sCmd)
            {
                m_eCmd = eCmd;
                m_sCmd = sCmd;
            }
        }
        List<Command> m_aCmd = new List<Command>();
        void InitCmd()
        {
            m_aCmd.Add(new Command(eCmd.CheckVersion, "VER"));
            m_aCmd.Add(new Command(eCmd.SearchHome, "ORG"));
            m_aCmd.Add(new Command(eCmd.ResetPos, "RST"));
            m_aCmd.Add(new Command(eCmd.GetPos, "ASP"));
            m_aCmd.Add(new Command(eCmd.SlowStop, "ASS"));
            m_aCmd.Add(new Command(eCmd.EmgStop, "AES"));
            m_aCmd.Add(new Command(eCmd.ReadError, "ERR"));
            m_aCmd.Add(new Command(eCmd.ClearError, "DRT"));
            m_aCmd.Add(new Command(eCmd.WriteValue, "DWL"));
            m_aCmd.Add(new Command(eCmd.ReadValue, "UWL"));
            m_aCmd.Add(new Command(eCmd.GetControlState, "IDO 1"));
            m_aCmd.Add(new Command(eCmd.GetState, "STS"));
            m_aCmd.Add(new Command(eCmd.VacuumOn, "VVN"));
            m_aCmd.Add(new Command(eCmd.VacuumOff, "VVF"));
            //m_aCmd.Add(new Command(eCmd.CheckWaferVac, "WCH VR1"));
            m_aCmd.Add(new Command(eCmd.CheckWaferVac, "WCH"));
            //m_aCmd.Add(new Command(eCmd.CheckWaferCCD, "WCHC VR1"));
            m_aCmd.Add(new Command(eCmd.CheckWaferCCD, "WCHC"));
            m_aCmd.Add(new Command(eCmd.Align, "ALG"));
            m_aCmd.Add(new Command(eCmd.AlignRotate, "ALX"));
            m_aCmd.Add(new Command(eCmd.SetWaferType, "WFT"));
            m_aCmd.Add(new Command(eCmd.SetWaferSize, "WFS"));
            m_aCmd.Add(new Command(eCmd.None, ""));
            if (Enum.GetNames(typeof(eCmd)).Length != m_aCmd.Count) p_sInfo = "Init Command Error";
        }

        Command GetCommand(string sCmd)
        {
            foreach (Command command in m_aCmd)
            {
                if (command.m_sCmd == sCmd) return command;
            }
            return null;
        }

        Command GetCommand(eCmd eCmd)
        {
            foreach (Command command in m_aCmd)
            {
                if (command.m_eCmd == eCmd) return command;
            }
            return null;
        }

        void RunTreeCommand(Tree tree)
        {
            foreach (Command command in m_aCmd)
            {
                command.m_secWait = tree.Set(command.m_secWait, command.m_secWait, command.m_eCmd.ToString(), "Wait Timeout (sec)");
            }
        }
        #endregion

        #region Aligner Error Code
        string[,] _asErrorEng = new string[,]
        {
            { "E01", "Command Received During Motion" },
            { "E02", "Command Received During Error. Send DRT Command First" },
            { "E05", "Data Arrange Error. Check the Data Arrange" },
            { "E06", "Format Error" },
            { "E08", "Not exist" },
            { "E12", "The Command don't exist. Check The Command" },
            { "000", "Not Exist Error" },
            { "002", "Incomplete Return to Zero. Try Orgin" },
            { "003", "Not Detected a Vacuum Sensor. Check The Sensor" },
            { "004", "Emergency. Check User IO" },
            { "008", "The Current State Is Over Run. Check the Range of Operating" },
            { "012", "The Driver State Is Alarm (Code 99). Check The Driver Error Code" },
            { "215", "The Align Retry Count Is Exceeded. Check The Wafer Position" },
            { "216", "Out Of CCD Operating Range. Check CCD Operating Range" },
            { "217", "CCD Data Is Wrong. Check CCD Operating Range" },
            { "218", "CCD Data Is Wrong. Check CCD Operating Range" },
            { "221", "The Notch Data Is Wrong. Check The Wafer Type" },
            { "223", "No Wafer. Check Wafer Exist" },
            { "250", "The Range of Align Is Exceeded. Check CCD Data" },
            { "252", "Notch or Flat Shape Is Wrong." },
            { "254", "Detected Doubble Notchs. Check The Wafer" },
        };
        string[,] _asErrorKor = new string[,]
        {
            { "E01", "동작 중 실행 불가능 명령을 수신" },
            { "E02", "에러 중에 구동 명령을 수신. DRT 명령 후 사용" },
            { "E05", "데이터 범위 에러. 데이터 설정 범위 확인" },
            { "E06", "포멧 에러. 미사용" },
            { "E08", "Not exist" },
            { "E12", "지정한 명령어가 존재하지 않음. 명령어 확인" },
            { "000", "에러없음" },
            { "002", "원점 복위 미완료. 원점 복귀" },
            { "003", "Vacuum 센서 미감지. 센서 확인" },
            { "004", "비상 정지가 입력. User IO 확인" },
            { "008", "오버런 상태. 동작 범위 확인" },
            { "012", "드라이버가 알람 상태(코드는 99). 드라이버 상태에러 확인(ACD의 드라이버 에러코드 확인)" },
            { "215", "얼라인 리트라이 횟수 초과. 웨이퍼 위치 확인" },
            { "216", "CCD 작업 영역 벗어남. CCD 범위 확인" },
            { "217", "CCD 데이터 이상. CCD 범위 확인" },
            { "218", "CCD 데이터 이상. CCD 범위 확인" },
            { "221", "노치 데이터 이상. 웨이퍼 타입 설정 확인" },
            { "223", "웨이퍼 없음. 웨이퍼 확인" },
            { "250", "얼라인 범위 초과. CCD 데이터 확인" },
            { "252", "노치 또는 플랫 형상 이상" },
            { "254", "더블 노치 감지. 웨이퍼 확인" },
        };
        string[,] p_asError
        {
            get { return _asErrorEng; }
        }

        string GetError(string sErrorCode)
        {
            for (int n = 0; n < p_asError.Length / 2; n++)
            {
                if (p_asError[n, 0] == sErrorCode) return p_asError[n, 1];
            }
            return sErrorCode + " : Unkown Error Code";
        }
        #endregion

        #region Aligner State
        string[,] _asState = new string[,]
        {
            { "100", "It is in stand by (no error)" },                           // 실제 에러는 아님               
            { "101", "Alignment is executed normally (no error)" },              // 실제 에러는 아님
            { "102", "Aligner is operating (no error)" },                        // 실제 에러는 아님
            { "106", "Initialization is in progress (no error)" },               // 실제 에러는 아님
            { "110", "Stand by as initialization is not processed(no error)" },  // 실제 에러는 아님
            { "111", "Wafer is disappeared while alignment was executed" },
            { "113", "Wafer is not set on the aligner" },
            { "115", "It exceeded the range of the line sensor" },
            { "116", "The sampling operation is not processing" },
            { "117", "Alignment cannot be executed" },
            { "118", "Aligner has not been initialized" },
            { "120", "It is a command error" },
            { "121", "The input value of the line sensor is abnormal" },
            { "122", "Initialization cannot be normally done" },
        };
        string GetState(string sStateCode)
        {
            for (int n = 0; n < _asState.Length / 2; n++)
            {
                if (p_asError[n, 0] == sStateCode) return p_asError[n, 1];
            }
            return sStateCode + " : Unkown State Code";
        }
        #endregion

        #region Aligner Control State
        string[] m_asControlState =
        {
            "Working",
            "Stopping",
            "Search Home Complete",
            "Over Run",
            "Error",
            " ",
            "Vacuum On"
        };

        string GetControlState(int nControlState)
        {
            if ((nControlState & 0x10) != 0) m_qSend.Enqueue(new Protocol(this, eCmd.ReadError));
            string sControlState = "";
            for (int n = 0; n < 6; n++)
            {
                if ((nControlState % 2) == 1)
                {
                    if (sControlState != "") sControlState += ", ";
                    sControlState += m_asControlState[n];
                }
                nControlState /= 2;
            }
            return sControlState;
        }
        #endregion

        #region Protocol
        class Protocol
        {
            public eCmd m_eCmd = eCmd.None;
            public string m_sCmd = null;
            public bool m_bDone = false;

            bool m_bSend = false; 
            public void SendCmd()
            {
                string sSend = m_aligner.GetCommand(m_eCmd).m_sCmd;
                if (m_sCmd != null) sSend += " " + m_sCmd;
                m_aligner.m_rs232.Send(sSend);
                m_aligner.m_log.Info(" [ Send --> " + sSend);
                m_swWait.Start();
                m_bSend = true;
            }

            public string OnReceive(string sRead)
            {
                string[] sReads = sRead.Split(' ');
                m_aligner.m_log.Info(" <-- Recv] " + sRead);
                if (sReads.Length < 1) return "OnReceive String too Short : " + sRead;
                eCmd eCmdReceive = m_aligner.GetCommand(sReads[0]).m_eCmd;
                if (m_eCmd != eCmdReceive) return "OnReceive Protocol MissMatch !!"; 
                if ((sReads.Length > 2) && (sReads[1] == "E12")) m_aligner.m_qSend.Enqueue(new Protocol(m_aligner, eCmd.ClearError));
                try
                {
                    switch (eCmdReceive)
                    {
                        case eCmd.GetPos:
                            if (sReads.Length < 3) return "OnReceive Get Position Massage too Short";
                            m_aligner.p_pulseRotate = Convert.ToInt32(sReads[1]);
                            m_aligner.p_rpAxis = new RPoint(Convert.ToDouble(sReads[2]), Convert.ToDouble(sReads[3]));
                            break;
                        case eCmd.CheckWaferVac:
                            m_aligner.p_bCheckWaferVac = (sReads.Length > 1) && (sReads[1][5] == '1');
                            break;
                        case eCmd.CheckWaferCCD:
                            m_aligner.p_bCheckWaferCCD = (sReads.Length > 1) && (sReads[1][5] == '1');
                            break;
                        case eCmd.ReadError:
                            m_aligner.p_sAlignerError = m_aligner.GetError(sReads[1]);
                            break;
                        case eCmd.GetState:
                            m_aligner.p_sAlignerState = m_aligner.GetState(sReads[1].Replace("V01=", ""));
                            break;
                        case eCmd.GetControlState:
                            m_aligner.p_sAlignerControl = m_aligner.GetControlState(Convert.ToInt32(sReads[1]));
                            break;
                        case eCmd.EmgStop:
                            //? 20210313 임시 추가
                            m_aligner.p_sInfo = m_aligner.GetError(sReads[1]);
                            break;
                        default:
                            if (sReads.Length > 1)
                            {
                                m_aligner.p_sInfo = m_aligner.GetError(sReads[1]);
                                m_aligner.m_alid_AlignFail.Run(true, "Aligner Align Fail");
                            }
                            break;
                    }
                }
                catch (Exception e) { m_aligner.p_sInfo = "OnReceive Exception : " + e.Message; }
                return "OK"; 
            }

            StopWatch m_swWait = new StopWatch(); 
            public string WaitReceive()
            {
                int msWait = (int)(1000 * m_aligner.GetCommand(m_eCmd).m_secWait); 
                while (true)
                {
                    Thread.Sleep(10);
                    if (m_bDone) return "OK";
                    if (!m_bSend && (m_swWait.ElapsedMilliseconds > msWait)) 
                        return "Wait Receive Timeout : " + m_eCmd.ToString();
                    if (EQ.IsStop()) return "EQ Stop"; 
                }
            }

            Aligner_RND m_aligner; 
            public Protocol(Aligner_RND aligner, eCmd eCmd, string sCmd = null)
            {
                m_aligner = aligner; 
                m_eCmd = eCmd;
                m_sCmd = sCmd; 
            }
        }
        Queue<Protocol> m_qSend = new Queue<Protocol>();
        public string SendCmd(eCmd eCmd)
        {
            Protocol protocol = new Protocol(this, eCmd);
            m_qSend.Enqueue(protocol);
            if (Run(protocol.WaitReceive())) return p_sInfo;
            return "OK";
        }

        public string RunAlign(double fDeg)
        {
            MarsLogManager marsLogManager = MarsLogManager.Instance;
            if (!m_diWaferExist.p_bIn)
            {
                m_alid_WaferExist.Run(true, "Aligner Wafer Exist Error");
                return "Fail";
            }
            int nDeg = (int)Math.Round(100 * fDeg); 
            string sCmd = "0,0," + nDeg.ToString(); 
            Protocol protocol = new Protocol(this, eCmd.AlignRotate, sCmd);
            m_qSend.Enqueue(protocol);
            if (Run(protocol.WaitReceive()))
            {
               
                return p_sInfo;
            }
            marsLogManager.WriteFNC(EQ.p_nRunLP, this.p_id, "Align", SSLNet.STATUS.END, type:SSLNet.MATERIAL_TYPE.WAFER);
            return "OK";
        }

        DispatcherTimer m_timer = new DispatcherTimer(); 
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(10);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        Protocol m_protocolSend = null; 
        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (m_protocolSend != null)
            {
                if (m_protocolSend.m_bDone) m_protocolSend = null;
                return;
            }
            if (m_qSend.Count > 0)
            {
                m_protocolSend = m_qSend.Dequeue();
                m_protocolSend.SendCmd(); 
            }
        }
        #endregion

        #region OCR
        public string RunOCR()
        {
            if (Run(m_camOCR.SendReadOCR())) return p_sInfo;
            return "OK";
        }
        #endregion

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get { return _infoWafer; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null) m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(p_id + ".InfoWafer");
            m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region InfoWafer UI
        InfoWaferChild_UI m_ui;
        void InitInfoWaferUI()
        {
            m_ui = new InfoWaferChild_UI();
            m_ui.Init(this);
            m_aTool.Add(m_ui);
        }
        #endregion

        #region IWTRChild
        bool _bLock = false;
        public bool p_bLock
        {
            get { return _bLock; }
            set
            {
                if (_bLock == value) return;
                _bLock = value;
            }
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false) return false;
                Thread.Sleep(100);
            }
            return true;
        }

        public List<string> p_asChildSlot { get { return null; } }

        public InfoWafer GetInfoWafer(int nID)
        {
            return p_infoWafer;
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            p_infoWafer = infoWafer;
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            //if (p_infoWafer == null) return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            //if (p_infoWafer != null) return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false) return p_id + " not Enable Wafer Size";
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null) infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            return SendCmd(eCmd.VacuumOff);
        }

        public string BeforePut(int nID)
        {
            //if (p_infoWafer != null) return p_id + " BeforePut : InfoWafer != null";
            return SendCmd(eCmd.VacuumOn); 
        }

        public string AfterGet(int nID)
        {
            ////m_bgwWaferExist.RunWorkerAsync(true);
            //m_bgwWaferExist.RunWorkerAsync(false);
            
            return SendCmd(eCmd.ResetPos);
        }

        public string AfterPut(int nID)
        {
            //string sPut = SendCmd(eCmd.VacuumOn);
            //m_bgwWaferExist.RunWorkerAsync(true);
            MarsLogManager marsLogManager = MarsLogManager.Instance;
            marsLogManager.ChangeMaterialSlot(EQ.p_nRunLP, p_infoWafer.m_nSlot + 1);
            if (!m_diWaferExist.p_bIn)
            {
                m_alid_WaferExist.Run(true, "Aligner Wafer Exist Fail");
                return "ExistFail";
            }
            else
                return "OK"; 
        }

        enum eCheckWafer
        {
            InfoWafer,
            Sensor
        }
        eCheckWafer m_eCheckWafer = eCheckWafer.Sensor;
        public bool IsWaferExist(int nID)
        {
            switch (m_eCheckWafer)
            {
                case eCheckWafer.Sensor: return m_diWaferExist.p_bIn;
                default: return (p_infoWafer != null);
            }
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }
        #endregion

        #region CheckWaferExist
        BackgroundWorker m_bgwWaferExist = new BackgroundWorker();
        void InitCheckWaferExist()
        {
            m_bgwWaferExist.DoWork += M_bgwWaferExist_DoWork;
        }

        private void M_bgwWaferExist_DoWork(object sender, DoWorkEventArgs e)
        {
            string sCheck = CheckWaferExist((bool)e.Argument);
            if (sCheck != "OK")
            {
                p_eState = eState.Error;
                p_sInfo = sCheck;
            }
        }

        bool m_bCheckWaferCCD = true;
        bool m_bCheckWaferVac = false;
        string CheckWaferExist(bool bExist)
        {
            if (m_diWaferExist.p_bIn != bExist) return "Check Wafer DI Error";
            if (m_bCheckWaferCCD)
            {
                SendCmd(eCmd.CheckWaferCCD);
                if (p_bCheckWaferCCD != bExist) return "Check Wafer CCD Error";
            }
            if (m_bCheckWaferVac)
            {
                if (bExist == false) SendCmd(eCmd.VacuumOn); 
                Thread.Sleep(300);
                SendCmd(eCmd.CheckWaferVac); 
                if (bExist == false) SendCmd(eCmd.VacuumOff);
                if (p_bCheckWaferVac != bExist) return "Check Wafer Vacuum Error"; 
            }
            return "OK"; 
        }

        void RunTreeCheckWaferExist(Tree tree)
        {
            m_bCheckWaferCCD = tree.Set(m_bCheckWaferCCD, m_bCheckWaferCCD, "CCD", "Check Wafer Exist Enable");
            m_bCheckWaferVac = tree.Set(m_bCheckWaferVac, m_bCheckWaferVac, "Vacuum", "Check Wafer Exist Enable");
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
            m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
            RunTreeCommand(tree.GetTree("Command Timeout", false));
            RunTreeCheckWaferExist(tree.GetTree("Check Wafer Exist", false));
        }

        public override void Reset()
        {
            //Run(SendCmd(eCmd.SlowStop));
            //Run(SendCmd(eCmd.ClearError));
            //Run(SendCmd(eCmd.ResetPos));
            base.Reset();
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            string sHome = RunCmdHome(); 
            p_eState = (sHome == "OK") ? eState.Ready : eState.Error;
            return sHome;
        }

        string RunCmdHome()
        {
            if (Run(SendCmd(eCmd.ClearError)))
            {
                if (Run(SendCmd(eCmd.ClearError))) return p_sInfo;
            }
            if (Run(SendCmd(eCmd.CheckWaferCCD))) return p_sInfo;
            if (Run(SendCmd(eCmd.VacuumOn))) return p_sInfo;
            if (Run(SendCmd(eCmd.CheckWaferVac))) return p_sInfo;
            if (Run(SendCmd(eCmd.SearchHome))) return p_sInfo;
            if (Run(SendCmd(eCmd.VacuumOff))) return p_sInfo;
            return "OK";
        }
        #endregion

        public Aligner_RND(string id, IEngineer engineer)
        {
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            InitCmd(); 
            base.InitBase(id, engineer);
            InitTimer();
            InitCheckWaferExist();
            InitInfoWaferUI(); 
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Command(this), false, "Aligner Run Command");
            AddModuleRunList(new Run_Align(this), true, "Run Align");
        }

        public class Run_Delay : ModuleRunBase
        {
            Aligner_RND m_module;
            public Run_Delay(Aligner_RND module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDelay = 2;
            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }

            public override string Run()
            {
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }

        public class Run_Command : ModuleRunBase
        {
            Aligner_RND m_module;
            public Run_Command(Aligner_RND module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eCmd m_eCmd = eCmd.ClearError; 
            public override ModuleRunBase Clone()
            {
                Run_Command run = new Run_Command(m_module);
                run.m_eCmd = m_eCmd;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eCmd = (eCmd)tree.Set(m_eCmd, m_eCmd, "Command", "Select Command", bVisible);
            }

            public override string Run()
            {
                return m_module.SendCmd(m_eCmd); 
            }
        }

        public class Run_Align : ModuleRunBase
        {
            Aligner_RND m_module;
            public Run_Align(Aligner_RND module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_fDeg = 0; 
            double m_fDegOCR = 0; 
            bool m_bOCR = false; 
            public override ModuleRunBase Clone()
            {
                Run_Align run = new Run_Align(m_module);
                run.m_fDeg = m_fDeg;
                run.m_bOCR = m_bOCR;
                run.m_fDegOCR = m_fDegOCR;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fDeg = tree.Set(m_fDeg, m_fDeg, "Rotate", "Rotate Angle after Align (deg)", bVisible);
                RunTreeOCR(tree.GetTree("OCR", true, bVisible), bVisible); 
            }

            void RunTreeOCR(Tree tree, bool bVisible)
            {
                m_bOCR = tree.Set(m_bOCR, m_bOCR, "Use", "Run OCR", bVisible);
                m_fDegOCR = tree.Set(m_fDegOCR, m_fDegOCR, "Rotate", "Rotate Angle for OCR (deg)", bVisible);
            }

            public override string Run()
            {
                MarsLogManager marsLogManager = MarsLogManager.Instance;

                marsLogManager.WriteFNC(EQ.p_nRunLP, m_module.p_id, "Align", SSLNet.STATUS.START, type:SSLNet.MATERIAL_TYPE.WAFER);
                if (m_bOCR)
                {
                    if (m_module.Run(m_module.RunAlign(m_fDegOCR))) return p_sInfo;
                    if (m_module.Run(m_module.RunOCR())) return p_sInfo; 
                }
                return m_module.RunAlign(m_fDeg); 
            }
        }
        #endregion
    }
}
