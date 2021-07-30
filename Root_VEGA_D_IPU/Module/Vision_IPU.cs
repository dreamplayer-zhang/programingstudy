using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Comm;
using RootTools.Lens.LinearTurret;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Threading;

namespace Root_VEGA_D_IPU.Module
{
    public class Vision_IPU : ModuleBase
    {
        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryMain;
        LightSet m_lightSet;

        Camera_Dalsa m_CamMain;

        LensLinearTurret m_LensLinearTurret;

        TCPIPComm_VEGA_D m_tcpipCommClient;

        public bool m_bImgInspectCompleted = true;
        
        Registry m_regTCPIP;

        #region [Getter Setter]
        public MemoryPool MemoryPool { get => m_memoryPool; private set => m_memoryPool = value; }
        public LightSet LightSet { get => m_lightSet; private set => m_lightSet = value; }
        public Camera_Dalsa CamMain { get => m_CamMain; private set => m_CamMain = value; }
        #endregion

        #region ToolBox

        public override void GetTools(bool bInit)
        {
            switch (p_eRemote)
            {
                case eRemote.Client: GetClientTools(bInit); break;
                case eRemote.Server: GetServerTools(bInit); break; 
            }

            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.GetCamera(ref m_CamMain, this, "MainCam");

            p_sInfo = m_toolBox.Get(ref m_LensLinearTurret, this, "LensTurret");

            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);

            //m_remote.GetTools(bInit);
        }

        void GetClientTools(bool bInit)
        {
        }

        void GetServerTools(bool bInit)
        {
        }
        #endregion

        #region Grab Mode
        int m_lGrabMode = 0;
        public ObservableCollection<GrabMode> m_aGrabMode = new ObservableCollection<GrabMode>();
        public List<string> p_asGrabMode
        {
            get
            {
                List<string> asGrabMode = new List<string>();
                foreach (GrabMode grabMode in m_aGrabMode) asGrabMode.Add(grabMode.p_sName);
                return asGrabMode;
            }
        }

        public GrabMode GetGrabMode(string sGrabMode)
        {
            foreach (GrabMode grabMode in m_aGrabMode)
            {
                if (sGrabMode == grabMode.p_sName) return grabMode;
            }
            return null;
        }

        public void ClearData()
        {
            foreach (GrabMode grabMode in m_aGrabMode)
            {
                grabMode.m_ptXYAlignData = new RPoint(0, 0);
                //grabMode.m_dVRSFocusPos = 0;
            }
            this.RunTree(Tree.eMode.RegWrite);
            this.RunTree(Tree.eMode.Init);
        }

        void RunTreeGrabMode(Tree tree)
        {
            m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
            while (m_aGrabMode.Count < m_lGrabMode)
            {
                string id = "Mode." + m_aGrabMode.Count.ToString("00");
                GrabMode grabMode = new GrabMode(id, m_cameraSet, m_lightSet, m_memoryPool, m_LensLinearTurret);
                m_aGrabMode.Add(grabMode);
            }
            while (m_aGrabMode.Count > m_lGrabMode) m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
            foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTreeName(tree.GetTree("Name", false));
            foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
        }
        #endregion

        #region override
        public override void Reset()
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.Reset, eRemote.Client, null);
            else
            {
                base.Reset();
            }
        }

        public override void InitMemorys()
        {
            switch (p_eRemote)
            {
                case eRemote.Client:
                    break;
                case eRemote.Server:
                    break; 
            }

            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryMain = m_memoryGroup.CreateMemory("OtherPC", 3, 1, 40000, 40000);
        }
        #endregion

        #region InfoWafer ??
        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }

        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get { return _infoWafer; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_regInfoWafer != null) m_regInfoWafer.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_regInfoWafer = null;
        public void ReadInfoWafer_Registry()
        {
            m_regInfoWafer = new Registry(p_id + ".InfoWafer");
            m_sInfoWafer = m_regInfoWafer.Read("sInfoWafer", m_sInfoWafer);
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.StateHome, eRemote.Client, null);
            else
            {
                Thread.Sleep(200);
                //if (m_CamMain != null && m_CamMain.p_CamInfo.p_eState == eCamState.Init) m_CamMain.Connect();
                p_sInfo = base.StateHome();
                p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
                //p_bStageVac = false;
                //ClearData();
                return "OK";
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
//            RunTreeAxis(tree.GetTree("Axis", false));
//            RunTreeGrabMode(tree.GetTree("Grab Mode", false));
        }
        #endregion

        public Vision_IPU(string id, IEngineer engineer, eRemote eRemote = eRemote.Local)
        {
            //            InitLineScan();+
            //            InitAreaScan();
            base.InitBase(id, engineer, eRemote);
            m_waferSize = new InfoWafer.WaferSize(id, false, false); //forget delete ?
            //            InitMemory();

            OnChangeState += Vision_IPU_OnChangeState;

            // Main PC와 연결될 Client Socket 생성
            TCPIPClient client = null;
            m_toolBox.GetComm(ref client, this, "TCPIP");

            m_tcpipCommClient = new TCPIPComm_VEGA_D(client);
            m_tcpipCommClient.EventReceiveData += EventReceiveData;
            m_tcpipCommClient.EventConnect += EventConnect;

            m_regTCPIP = new Registry(p_id + ".TCPIP");
        }

        private void Vision_IPU_OnChangeState(eState eState)
        {
            switch (p_eState)
            {
                case eState.Init:
                case eState.Error:
                    RemoteRun(eRemoteRun.ServerState, eRemote.Server, eState);
                    break;
            }
        }

        public override void ThreadStop()
        {
            //
            base.ThreadStop();
        }

        #region RemoteRun
        public enum eRemoteRun
        {
            ServerState,
            StateHome,
            Reset,
        }

        Run_Remote GetRemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = new Run_Remote(this);
            run.m_eRemoteRun = eRemoteRun;
            run.m_eRemote = eRemote;
            switch (eRemoteRun)
            {
                case eRemoteRun.ServerState: run.m_eState = value; break;
                case eRemoteRun.StateHome: break;
                case eRemoteRun.Reset: break;
            }
            return run;
        }

        string RemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = GetRemoteRun(eRemoteRun, eRemote, value);
            StartRun(run);
            while (run.p_eRunState != ModuleRunBase.eRunState.Done)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            return p_sInfo;
        }

        public class Run_Remote : ModuleRunBase
        {
            Vision_IPU m_module;
            public Run_Remote(Vision_IPU module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public eState m_eState = eState.Init;
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_eState = m_eState;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eRemoteRun = (eRemoteRun)tree.Set(m_eRemoteRun, m_eRemoteRun, "RemoteRun", "Select Remote Run", bVisible);
                m_eRemote = (eRemote)tree.Set(m_eRemote, m_eRemote, "Remote", "Remote", false);
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.ServerState:
                        m_eState = (eState)tree.Set(m_eState, m_eState, "State", "Module State", bVisible);
                        break;
                }
            }

            public override string Run()
            {
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.ServerState: m_module.p_eState = m_eState; break;
                    case eRemoteRun.StateHome: return m_module.StateHome();
                    case eRemoteRun.Reset: m_module.Reset(); break;
                }
                return "OK";
            }
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Remote(this), true, "Remote Run");
            //            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            //            AddModuleRunList(new Run_Rotate(this), false, "Rotate Axis");
            //            AddModuleRunList(new Run_GrabLineScan(this), true, "Run Grab LineScan Camera");
            //            AddModuleRunList(new Run_Inspect(this), true, "Run Inspect");
            //            AddModuleRunList(new Run_VisionAlign(this), true, "Run VisionAlign");
            //            AddModuleRunList(new Run_AutoFocus(this), false, "Run AutoFocus");
        }
        #endregion

        #region Communication to VEGA-D

        const string REGSUBKEYNAME_GRABLINESCANSTATE = "GrabLineScanState";
        const string REGSUBKEYNAME_TOTALSCANLINE = "TotalScanLine";
        const string REGSUBKEYNAME_CURRENTSCANLINE = "CurrentScanLine";
        const string REGSUBKEYNAME_STARTSCANLINE = "StartScanLine";


        private void EventReceiveData(byte[] aBuf, int nSize, Socket socket)
        {
            if (nSize <= 0)
                return;

            int nStartIdx = 0;
            TCPIPComm_VEGA_D.Command cmd = TCPIPComm_VEGA_D.Command.None;
            Dictionary<string, string> mapParam = new Dictionary<string, string>();

            while (m_tcpipCommClient.ParseMessage(aBuf, nSize, ref nStartIdx, ref cmd, mapParam))
            {
                switch (cmd)
                {
                    case TCPIPComm_VEGA_D.Command.LineStart:
                        {
                            if (m_CamMain.p_CamInfo.p_eState == eCamState.Init)
                                m_CamMain.ConnectAsSlave();

                            // 메세지에서 데이터 얻어오기
                            int offsetX = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_OFFSETX]);
                            int offsetY = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_OFFSETY]);
                            bool bScanDir = bool.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_SCANDIR]);

                            CPoint ptOffset = new CPoint(offsetX, offsetY);                                             // Memory offset
                            eGrabDirection dir = bScanDir == false ? eGrabDirection.Forward : eGrabDirection.BackWard;  // Scan dir
                            int fov = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_FOV]);                             // fov
                            int overlap = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_OVERLAP]);                     // Overlap
                            int nLine = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_LINE]);                          // Line
                            int nScanLineNum = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_TOTALSCANLINECOUNT]);     // Total scan line count
                            int nScanLine = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_CURRENTSCANLINE]);           // Current scan line
                            int nStartScanLine = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_STARTSCANLINE]);        // Start scan line

                            // 이미지 데이터 복사 스레드 실행
                            string strPool = m_memoryPool.p_id;
                            string strGroup = m_memoryGroup.p_id;
                            string strMemory = m_memoryMain.p_id;
                            MemoryData mem = m_engineer.GetMemory(strPool, strGroup, strMemory);
                            m_CamMain.GrabLineScan(mem, ptOffset, (int)(nLine * 0.98));


                            // 작업 중인 이미지 그랩 데이터 저장
                            m_regTCPIP.Write(REGSUBKEYNAME_GRABLINESCANSTATE, cmd.ToString());
                            m_regTCPIP.Write(REGSUBKEYNAME_TOTALSCANLINE, nScanLineNum);
                            m_regTCPIP.Write(REGSUBKEYNAME_CURRENTSCANLINE, nScanLine);
                            m_regTCPIP.Write(REGSUBKEYNAME_STARTSCANLINE, nStartScanLine);
                        }
                        break;
                    case TCPIPComm_VEGA_D.Command.LineEnd:
                        {
                            // Grab 스레드 작업 정지
                            while(m_CamMain.p_CamInfo.p_eState != eCamState.Ready)
                            {
                                Thread.Sleep(10);
                            }

                            m_CamMain.StopGrab();

                            // LineEndAck 메세지 메인으로 전달
                            m_tcpipCommClient.SendMessage(TCPIPComm_VEGA_D.Command.LineEndAck);

                            // 작업 중인 이미지 그랩 데이터 얻어오기
                            int totalScanLine = m_regTCPIP.Read(REGSUBKEYNAME_TOTALSCANLINE, -1);
                            int curScanLine = m_regTCPIP.Read(REGSUBKEYNAME_CURRENTSCANLINE, -1);
                            string grabLineScanState = cmd.ToString();

                            if (totalScanLine != -1 && curScanLine != -1)
                            {
                                // 마지막 라인 스캔일 때
                                if (totalScanLine == curScanLine + 1)
                                {
                                    grabLineScanState = "";
                                }
                            }

                            // 작업 중인 이미지 그랩 데이터 저장
                            m_regTCPIP.Write(REGSUBKEYNAME_GRABLINESCANSTATE, grabLineScanState);

                            // 이미지 검사 시작
                            // (임시처리 - 검사 거치지 않고 마지막 라인일경우 바로 Result 메세지 전달)
                            if(grabLineScanState == "")
                                m_tcpipCommClient.SendMessage(TCPIPComm_VEGA_D.Command.Result);
                            

                            EQ.p_bStop = true;
                        }
                        break;
                    case TCPIPComm_VEGA_D.Command.RcpName:
                        string rcpname = mapParam[TCPIPComm_VEGA_D.PARAM_NAME_RCPNAME];
                        break;
                    default:
                        break;
                }
            }
        }

        private void EventConnect(Socket socket)
        {
            // 이전에 작업하던 이미지 그랩 데이터 확인
            string strState = m_regTCPIP.Read(REGSUBKEYNAME_GRABLINESCANSTATE, "");
            if(strState != "")
            {
                int totalScanLine = m_regTCPIP.Read(REGSUBKEYNAME_TOTALSCANLINE, -1);
                int curScanLine = m_regTCPIP.Read(REGSUBKEYNAME_CURRENTSCANLINE, -1);
                int startScanLine = m_regTCPIP.Read(REGSUBKEYNAME_STARTSCANLINE, -1);
                if(totalScanLine != -1 && curScanLine != -1 && startScanLine != -1)
                {
                    Dictionary<string, string> mapParam = new Dictionary<string, string>();

                    mapParam[TCPIPComm_VEGA_D.PARAM_NAME_TOTALSCANLINECOUNT] = totalScanLine.ToString();
                    mapParam[TCPIPComm_VEGA_D.PARAM_NAME_CURRENTSCANLINE] = curScanLine.ToString();
                    mapParam[TCPIPComm_VEGA_D.PARAM_NAME_STARTSCANLINE] = startScanLine.ToString();

                    m_tcpipCommClient.SendMessage(TCPIPComm_VEGA_D.Command.resume, mapParam);
                }
            }
        }
        #endregion
    }
}
