using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Root_EFEM.Module;
using Root_VEGA_D_IPU.Module;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Control.Ajin;
using RootTools.GAFs;
using RootTools.Lens.LinearTurret;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.RADS;
using RootTools.Trees;
using RootTools_Vision;
using RootTools_Vision.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Root_VEGA_D.Module
{
    public class Vision : ModuleBase, IWTRChild
    {
        ALID m_alid_WaferExist;
        public void SetAlarm()
        {
            m_alid_WaferExist.Run(true, "Vision Wafer Exist Error");
        }
        #region ToolBox
        Axis m_axisRotate;
        Axis m_axisZ;
        AxisXY m_axisXY;
        DIO_O m_doVac;
        DIO_O m_doBlow;
        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryMain;
        MemoryData m_memoryLayer;
        LightSet m_lightSet;

        Camera_Dalsa m_CamMain;
        Camera_Basler m_CamAlign;
        Camera_Basler m_CamAutoFocus;
        Camera_Basler m_CamRADS;

        KlarfData_Lot m_KlarfData_Lot;
        LensLinearTurret m_LensLinearTurret;

        TCPIPComm_VEGA_D m_tcpipCommServer;
        public RADSControl m_RADSControl;

        #region [Getter Setter]
        public Axis AxisRotate { get => m_axisRotate; private set => m_axisRotate = value; }
        public Axis AxisZ { get => m_axisZ; private set => m_axisZ = value; }
        public AxisXY AxisXY { get => m_axisXY; private set => m_axisXY = value; }
        public DIO_O DoVac { get => m_doVac; private set => m_doVac = value; }
        public DIO_O DoBlow { get => m_doBlow; private set => m_doBlow = value; }
        public MemoryPool MemoryPool { get => m_memoryPool; private set => m_memoryPool = value; }
        public MemoryGroup MemoryGroup { get => m_memoryGroup; private set => m_memoryGroup = value; }
        public MemoryData MemoryMain { get => m_memoryMain; private set => m_memoryMain = value; }
        public MemoryData MemoryLayer { get => m_memoryLayer; private set => m_memoryLayer = value; }
        public LightSet LightSet { get => m_lightSet; private set => m_lightSet = value; }
        public Camera_Dalsa CamMain { get => m_CamMain; private set => m_CamMain = value; }
        public Camera_Basler CamAlign { get => m_CamAlign; private set => m_CamAlign = value; }
        public Camera_Basler CamAutoFocus { get => m_CamAutoFocus; private set => m_CamAutoFocus = value; }
        public Camera_Basler CamRADS { get => m_CamRADS; private set => m_CamRADS = value; }
        public KlarfData_Lot KlarfData_Lot { get => m_KlarfData_Lot; private set => m_KlarfData_Lot = value; }
        public TCPIPComm_VEGA_D TcpipCommServer { get => m_tcpipCommServer; private set => m_tcpipCommServer = value; }
        #endregion

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetAxis(ref m_axisRotate, this, "Axis Rotate");
            p_sInfo = m_toolBox.GetAxis(ref m_axisZ, this, "Axis Z");
            p_sInfo = m_toolBox.GetAxis(ref m_axisXY, this, "Axis XY");
            p_sInfo = m_toolBox.GetDIO(ref m_doVac, this, "Stage Vacuum");
            p_sInfo = m_toolBox.GetDIO(ref m_doBlow, this, "Stage Blow");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.GetCamera(ref m_CamMain, this, "MainCam");
            p_sInfo = m_toolBox.GetCamera(ref m_CamAlign, this, "AlignCam");
            p_sInfo = m_toolBox.GetCamera(ref m_CamAutoFocus, this, "AutoFocusCam");
            p_sInfo = m_toolBox.GetCamera(ref m_CamRADS, this, "RADS");
            p_sInfo = m_toolBox.Get(ref m_LensLinearTurret, this, "LensTurret");

            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
            m_alid_WaferExist = m_gaf.GetALID(this, "Vision Wafer Exist", "Vision Wafer Exist");
            //m_remote.GetTools(bInit);

            bool bUseRADS = false;
            if (m_CamRADS.p_CamInfo != null) bUseRADS = true;
            p_sInfo = m_toolBox.Get(ref m_RADSControl, this, "RADSControl", bUseRADS);
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
                grabMode.m_dVRSFocusPos = 0;
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

        #region DIO
        public bool p_bStageVac
        {
            get { return m_doVac.p_bOut; }
            set
            {
                if (m_doVac.p_bOut == value) return;
                m_doVac.Write(value);
            }
        }

        public bool p_bStageBlow
        {
            get { return m_doBlow.p_bOut; }
            set
            {
                if (m_doBlow.p_bOut == value) return;
                m_doBlow.Write(value);
            }
        }

        public void RunBlow(int msDelay)
        {
            m_doBlow.DelayOff(msDelay);
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
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryMain = m_memoryGroup.CreateMemory("Main", 3, 1, 40000, 40000);
            //m_memoryGroup2 = m_memoryPool2.GetGroup("group");
            //m_memoryGroup2.CreateMemory("ROI", 1, 4, 30000, 30000); // Chip 크기 최대 30,000 * 30,000 고정 Origin ROI 메모리 할당 20.11.02 JTL 
        }
        #endregion

        #region Axis
        private int m_pulseRound = 1000;
        public int PulseRound { get => this.m_pulseRound; set => this.m_pulseRound = value; }
        void RunTreeAxis(Tree tree)
        {
            m_pulseRound = tree.Set(m_pulseRound, m_pulseRound, "Rotate Pulse / Round", "Rotate" +
                " Axis Pulse / 1 Round (pulse)");
        }

        public enum eAxisPosX
        { 
            Ready,
        }
        public enum eAxisPosY
        {
            Ready,
        }
        public enum eAxisPosZ
        {
            Ready,
        }
        public enum eAxisPosRotate
        {
            Ready,
        }
        void InitPosAlign()
        {
            m_axisZ.AddPos(Enum.GetNames(typeof(eAxisPosZ)));
            m_axisRotate.AddPos(Enum.GetNames(typeof(eAxisPosRotate)));
            if (m_axisXY.p_axisX != null)
            {
                (m_axisXY.p_axisX).AddPos(Enum.GetNames(typeof(eAxisPosX)));
            }
            if (m_axisXY.p_axisY != null)
            {
                (m_axisXY.p_axisY).AddPos(Enum.GetNames(typeof(eAxisPosY)));
            }
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

        public List<string> p_asChildSlot
        {
            get { return null; }
        }

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
            if (p_infoWafer == null) return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer != null) return p_id + " IsPutOK - InfoWafer Exist";
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
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforeGet, eRemote.Client, nID);
            else
            {
                if (Run(m_axisZ.StartMove(eAxisPosZ.Ready))) return p_sInfo;
                if (Run(m_axisRotate.StartMove(eAxisPosRotate.Ready))) return p_sInfo;
                if (Run(m_axisXY.StartMove(eAxisPosX.Ready))) return p_sInfo;

                if (Run(m_axisXY.WaitReady()))
                    return p_sInfo;
                if (Run(m_axisRotate.WaitReady()))
                    return p_sInfo;
                if (Run(m_axisZ.WaitReady()))
                    return p_sInfo;
                //m_axisXY.StartMove("Position_0");
                //m_axisRotate.StartMove("Position_0");
                //m_axisZ.StartMove("Position_0");

                //m_axisXY.WaitReady();
                //m_axisRotate.WaitReady();
                //m_axisZ.WaitReady();

                ClearData();

                return "OK";
            }
        }

        public string BeforePut(int nID)
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforePut, eRemote.Client, nID);
            else
            {
                if (Run(m_axisZ.StartMove(eAxisPosZ.Ready))) return p_sInfo;
                if (Run(m_axisRotate.StartMove(eAxisPosRotate.Ready))) return p_sInfo;
                if (Run(m_axisXY.StartMove(eAxisPosX.Ready))) return p_sInfo;

                if (Run(m_axisXY.WaitReady()))
                    return p_sInfo;
                if (Run(m_axisRotate.WaitReady()))
                    return p_sInfo;
                if (Run(m_axisZ.WaitReady()))
                    return p_sInfo;
                //m_axisXY.StartMove("Position_0");
                //m_axisRotate.StartMove("Position_0");
                //m_axisZ.StartMove("Position_0");

                //m_axisXY.WaitReady();
                //m_axisRotate.WaitReady();
                //m_axisZ.WaitReady();

                return "OK";
            }
        }

        public string AfterGet(int nID)
        {
            return "OK";
        }

        public string AfterPut(int nID)
        {
            return "OK";
        }

        enum eCheckWafer
        {
            InfoWafer,
            Sensor
        }
        eCheckWafer m_eCheckWafer = eCheckWafer.InfoWafer;
        public bool IsWaferExist(int nID)
        {
            switch (m_eCheckWafer)
            {
                case eCheckWafer.Sensor: return false; // m_diWaferExist.p_bIn;
                default: return (p_infoWafer != null);
            }
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }

        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get{ return _infoWafer; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null)
                    m_reg.Write("sInfoWafer", m_sInfoWafer);
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

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.StateHome, eRemote.Client, null);
            else
            {
                Thread.Sleep(200);
                if (m_CamMain != null && m_CamMain.p_CamInfo.p_eState == eCamState.Init) m_CamMain.Connect();

                p_sInfo = base.StateHome();
                p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;

                ClearData();

                return "OK";
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeAxis(tree.GetTree("Axis", false));
            RunTreeGrabMode(tree.GetTree("Grab Mode", false));
        }
        #endregion

        #region Vision Algorithm
        Image<Gray, byte> GetGrayByteImageFromMemory(MemoryData mem, CRect crtROI)
        {
            if (crtROI.Width < 1 || crtROI.Height < 1) return null;
            if (crtROI.Left < 0 || crtROI.Top < 0) return null;
            if (crtROI.Width % 4 != 0)
            {
                int nSpare = 0;
                while (((crtROI.Width + nSpare) % 4) != 0) nSpare++;
                crtROI = new CRect(crtROI.Left, crtROI.Top, crtROI.Right + nSpare, crtROI.Bottom);
            }
            ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
            IntPtr p = mem.GetPtr();
            img.SetData(p, crtROI, (int)mem.W);

            byte[] barr = img.GetByteArray();
            System.Drawing.Bitmap bmp = img.GetBitmapToArray(crtROI.Width, crtROI.Height, barr);
            Image<Gray, byte> imgReturn = new Image<Gray, byte>(bmp);

            return imgReturn;
        }

        Image<Gray, byte> GetGrayByteImageFromMemory_12bit(MemoryData mem, CRect crtROI)
        {
            if (crtROI.Width < 1 || crtROI.Height < 1) return null;
            if (crtROI.Left < 0 || crtROI.Top < 0) return null;
            if (crtROI.Width % 4 != 0)
            {
                int nSpare = 0;
                while (((crtROI.Width + nSpare) % 4) != 0) nSpare++;
                crtROI = new CRect(crtROI.Left, crtROI.Top, crtROI.Right + nSpare, crtROI.Bottom);
            }
            ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
            IntPtr p = mem.GetPtr();
            img.SetData_12bit(p, crtROI, (int)(mem.W / mem.p_nByte));

            byte[] barr = img.GetByteArray();
            System.Drawing.Bitmap bmp = img.GetBitmapToArray(crtROI.Width, crtROI.Height, barr);
            Image<Gray, byte> imgReturn = new Image<Gray, byte>(bmp);

            return imgReturn;
        }

        bool TemplateMatching(MemoryData mem, CRect crtSearchArea, Image<Gray, byte> imgSrc, Image<Gray, byte> imgTemplate, out CPoint cptCenter, double dMatchScore)
        {
            // variable
            int nWidthDiff = 0;
            int nHeightDiff = 0;
            Point ptMaxRelative = new Point();
            float fMaxScore = float.MinValue;
            bool bFoundTemplate = false;

            // implement
            if (imgTemplate.Width > imgSrc.Width || imgTemplate.Height > imgSrc.Height)
            {
                cptCenter = new CPoint();
                cptCenter.X = 0;
                cptCenter.Y = 0;
                return false;
            }
            Image<Gray, float> imgResult = imgSrc.MatchTemplate(imgTemplate, TemplateMatchingType.CcorrNormed);
            nWidthDiff = imgSrc.Width - imgResult.Width;
            nHeightDiff = imgSrc.Height - imgResult.Height;
            float[,,] matches = imgResult.Data;

            for (int x = 0; x < matches.GetLength(1); x++)
            {
                for (int y = 0; y < matches.GetLength(0); y++)
                {
                    if (fMaxScore < matches[y, x, 0] && dMatchScore <= matches[y, x, 0])
                    {
                        fMaxScore = matches[y, x, 0];
                        ptMaxRelative.X = x;
                        ptMaxRelative.Y = y;
                        bFoundTemplate = true;
                    }
                }
            }
            cptCenter = new CPoint();
            cptCenter.X = (int)(crtSearchArea.Left + ptMaxRelative.X) + (int)(nWidthDiff / 2);
            cptCenter.Y = (int)(crtSearchArea.Top + ptMaxRelative.Y) + (int)(nHeightDiff / 2);

            return bFoundTemplate;
        }
        #endregion

        public Vision(string id, IEngineer engineer, eRemote eRemote = eRemote.Local)
        {
            base.InitBase(id, engineer);
            InitPosAlign();
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            OnChangeState += Vision_OnChangeState;

            // IPU PC와 연결될 Server Socket 생성
            TCPIPServer server = null;
            m_toolBox.GetComm(ref server, this, "TCPIP");

            m_tcpipCommServer = new TCPIPComm_VEGA_D(server);
            m_tcpipCommServer.EventReceiveData += EventReceiveData;
            m_tcpipCommServer.EventAccept += EventAccept;
        }

        private void Vision_OnChangeState(eState eState)
        {
            switch (p_eState)
            {
                case eState.Init:
                case eState.Error:
                    RemoteRun(eRemoteRun.ServerState, eRemote.Server, eState);
                    break;
            }
        }
        ModuleRunBase PeekModuleRun()
        {
            if (m_qModuleRun.Count > 0)
            {
                return m_qModuleRun.Peek();
            }
            else
                return null;
        }

        bool m_bDisconnectedGrabLineScanning = false;    // 이미지 스캔 도중 소켓통신 연결이 끊어졌을 때의 상태값
        int m_nDisconnectedStateResetTime = 20000;              // IPU와 연결이 끊어지고 재개될때까지 대기하는 시간
        private void ResetDisconnectedStateTimerTick(object sender, EventArgs e)
        {
            // 소켓 연결 해제 이후 m_nDisconnectedStateResetTime (ms) 만큼 시간 뒤 호출되는 콜백함수
            DispatcherTimer timer = sender as DispatcherTimer;
            if (timer != null)
                timer.Stop();

            // 이미지 스캔을 재개하지 않을 것이므로
            // 이미지 스캔 중 연결이 끊겼다는 상태값을 다시 리셋
            m_bDisconnectedGrabLineScanning = false;

            // 재 연결까지 Run_GrabLineScan이 대기 중이라면 상태 변경
            Run_GrabLineScan runGrabLineScan = PeekModuleRun() as Run_GrabLineScan;
            if (runGrabLineScan != null)
            {
                // 현재 GrabLineScan 진행중이었다면
                if (runGrabLineScan.p_eRunState == ModuleRunBase.eRunState.Run)
                {
                    // EQ Stop 상태로 변경하고 이미지그랩 중에 중단되었다는 상태값 설정
                    runGrabLineScan.m_bWaitRun = false;
                }
            }
        }
        private void EventAccept(Socket socket)
        {
        }
        private void EventReceiveData(byte[] aBuf, int nSize, Socket socket)
        {
            if (nSize <= 0)
            {
                // 연결이 종료되었을 때
                if (socket.Connected)
                    return;

                Run_GrabLineScan runGrabLineScan = PeekModuleRun() as Run_GrabLineScan;
                if (runGrabLineScan != null)
                {
                    // 현재 GrabLineScan 진행중이었다면
                    if (runGrabLineScan.p_eRunState == ModuleRunBase.eRunState.Run)
                    {
                        // EQ Stop 상태로 변경하고 이미지그랩 중에 중단되었다는 상태값 설정
                        m_bDisconnectedGrabLineScanning = true;

                        // 재연결 시까지 RunGrabLineScan은 대기
                        runGrabLineScan.m_bWaitRun = true;

                        // IPU와 소켓통신 재 연결 될때까지의 상태 리셋 타이머 실행
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromMilliseconds(m_nDisconnectedStateResetTime);
                        timer.Tick += new EventHandler(ResetDisconnectedStateTimerTick);
                        timer.Start();
                    }
                }
            }
            else
            {
                int nStartIdx = 0;
                TCPIPComm_VEGA_D.Command cmd = TCPIPComm_VEGA_D.Command.None;
                Dictionary<string, string> mapParam = new Dictionary<string, string>();

                // 도착한 메세지를 파싱하여 메세지 단위마다 처리
                while (m_tcpipCommServer.ParseMessage(aBuf, nSize, ref nStartIdx, ref cmd, mapParam))
                {
                    switch (cmd)
                    {
                        case TCPIPComm_VEGA_D.Command.resume:
                            {
                                if (m_bDisconnectedGrabLineScanning)
                                {
                                    // 이전에 이미지 그랩 작업을 이어서 할 수 있도록 처리
                                    int totalScanLine = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_TOTALSCANLINECOUNT]);
                                    int curScanLine = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_CURRENTSCANLINE]);
                                    int startScanLine = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_STARTSCANLINE]);

                                    Run_GrabLineScan runGrabLineScan = PeekModuleRun() as Run_GrabLineScan;
                                    if (runGrabLineScan != null)
                                    {
                                        runGrabLineScan.m_nCurScanLine = curScanLine;
                                        runGrabLineScan.m_grabMode.m_ScanLineNum = totalScanLine;
                                        runGrabLineScan.m_grabMode.m_ScanStartLine = startScanLine;

                                        lock (runGrabLineScan.m_lockWaitRun)
                                        {
                                            runGrabLineScan.m_bWaitRun = false;
                                        }   
                                    }
                                }
                                else
                                {
                                    // EQ Stop 상태를 변경하여 이미지 스캔을 재개하지 않을 것이므로
                                    // 이미지 스캔 중 연결이 끊겼다는 상태값을 다시 리셋
                                    m_bDisconnectedGrabLineScanning = false;
                                }
                            }
                            break;
                        case TCPIPComm_VEGA_D.Command.Result:
                            {
                                Run_GrabLineScan runGrabLineScan = PeekModuleRun() as Run_GrabLineScan;
                                if (runGrabLineScan != null)
                                {
                                    // 현재 GrabLineScan 진행중이었다면
                                    if (runGrabLineScan.p_eRunState == ModuleRunBase.eRunState.Run)
                                    {
                                        // IPU에서 이미지 검사 완료되었기 때문에 해당 상태변수 true로 변경
                                        runGrabLineScan.m_bIPUCompleted = true;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
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
            BeforeGet,
            BeforePut,
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
                case eRemoteRun.BeforeGet: run.m_nID = value; break;
                case eRemoteRun.BeforePut: run.m_nID = value; break;
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
            Vision m_module;
            public Run_Remote(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public eState m_eState = eState.Init;
            public int m_nID = 0;
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_eState = m_eState;
                run.m_nID = m_nID;
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
                    case eRemoteRun.BeforeGet:
                    case eRemoteRun.BeforePut:
                        m_nID = tree.Set(m_nID, m_nID, "SlotID", "Slot ID", false);
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
                    case eRemoteRun.BeforeGet: return m_module.BeforeGet(m_nID);
                    case eRemoteRun.BeforePut: return m_module.BeforePut(m_nID);
                }
                return "OK";
            }
        }
        #endregion
        #region Test_Run
        public class Run_Test : ModuleRunBase
        {
            Vision m_module;
            public RPoint m_rpAxisCenter = new RPoint();
            public Run_Test(Vision module)
            {
                m_module = module;
                InitModuleRun(module);

            }
            public override ModuleRunBase Clone()
            {
                Run_Test run = new Run_Test(m_module);
                run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
                return run;
            }
            string m_sFlip = "Test";
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
                m_sFlip = tree.Set(m_sFlip, m_sFlip, "Test", "Bottom", bVisible, true);
            }

            public override string Run()
            {
                Thread.Sleep(1000);
                AxisXY axisXY = m_module.m_axisXY;
                double dStartPosY = m_rpAxisCenter.Y;



                double dPosX = m_rpAxisCenter.X;


                if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
                Thread.Sleep(2000);
                //m_module.p_eState = eState.Ready;
                return "OK";
            }
        }
        #endregion

        #region RADSTEST
        public class Run_RADS : ModuleRunBase
        {
            Vision m_module;

            public Run_RADS(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_RADS run = new Run_RADS(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }
            public override string Run()
            {
                // variabgle
                Camera_Basler camRADS = (Camera_Basler)m_module.CamRADS;

                // implement
                try
                {
                    // RADS 연결
                    if (m_module.m_RADSControl.p_IsRun == false)
                    {
                        m_module.m_RADSControl.m_timer.Start();
                        m_module.m_RADSControl.p_IsRun = true;
                        m_module.m_RADSControl.StartRADS();
                        StopWatch sw = new StopWatch();
                        if (camRADS.p_CamInfo._OpenStatus == false) camRADS.Connect();
                        while (camRADS.p_CamInfo._OpenStatus == false)
                        {
                            if (sw.ElapsedMilliseconds > 15000)
                            {
                                sw.Stop();
                                return "RADS Camera Not Connected";
                            }
                        }
                        sw.Stop();
                        camRADS.SetMulticast();
                        camRADS.GrabContinuousShot();
                    }

                    StopWatch swDelay = new StopWatch();
                    while(true)
                    {
                        if (swDelay.ElapsedMilliseconds > 10000)
                        {
                            swDelay.Stop();
                            break;
                        }
                    }
                }
                finally
                {
                    if (m_module.m_RADSControl.p_IsRun == true)
                    {
                        m_module.m_RADSControl.m_timer.Stop();
                        m_module.m_RADSControl.p_IsRun = false;
                        m_module.m_RADSControl.StopRADS();
                        if (camRADS.p_CamInfo._IsGrabbing == true) camRADS.StopGrab();
                    }
                }
                return "OK";
            }
        }
        #endregion

        #region Run_MakeTemplateImage
        public class Run_MakeTemplateImage : ModuleRunBase
        {
            Vision m_module;
            public CPoint m_cptTopAlignMarkCenterPos = new CPoint();
            public int m_nTopWidth = 500;
            public int m_nTopHeight = 500;
            public CPoint m_cptBottomAlignMarkCenterPos = new CPoint();
            public int m_nBottomWidth = 500;
            public int m_nBottomHeight = 500;

            public Run_MakeTemplateImage(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_MakeTemplateImage run = new Run_MakeTemplateImage(m_module);

                run.m_cptTopAlignMarkCenterPos = m_cptTopAlignMarkCenterPos;
                run.m_nTopWidth = m_nTopWidth;
                run.m_nTopHeight = m_nTopHeight;
                run.m_cptBottomAlignMarkCenterPos = m_cptBottomAlignMarkCenterPos;
                run.m_nBottomWidth = m_nBottomWidth;
                run.m_nBottomHeight = m_nBottomHeight;

                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_cptTopAlignMarkCenterPos = tree.Set(m_cptTopAlignMarkCenterPos, m_cptTopAlignMarkCenterPos, "Top Align Mark Center Position", "Top Align Mark Center Position", bVisible);
                m_nTopWidth = tree.Set(m_nTopWidth, m_nTopWidth, "Top Align Mark Width", "Top Align Mark Width", bVisible);
                m_nTopHeight = tree.Set(m_nTopHeight, m_nTopHeight, "Top Align Mark Height", "Top Align Mark Height", bVisible);
                m_cptBottomAlignMarkCenterPos = tree.Set(m_cptBottomAlignMarkCenterPos, m_cptBottomAlignMarkCenterPos, "Bottom Align Mark Center Position", "Bottom Align Mark Center Position", bVisible);
                m_nBottomWidth = tree.Set(m_nBottomWidth, m_nBottomWidth, "Bottom Align Mark Width", "Bottom Align Mark Width", bVisible);
                m_nBottomHeight = tree.Set(m_nBottomHeight, m_nBottomHeight, "Bottom Align Mark Height", "Bottom Align Mark Height", bVisible);
                m_nBottomHeight = tree.Set(m_nBottomHeight, m_nBottomHeight, "Bottom Align Mark Height", "Bottom Align Mark Height", bVisible);
            }

            public override string Run()
            {
                // variable
                string strAlignMarkTemplateImagePath = "D:\\AlignMarkTemplateImage";
                string strPool = "Vision.Memory";
                string strGroup = "Vision";
                string strMemory = "Main";
                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

                // implement
                if (!Directory.Exists(strAlignMarkTemplateImagePath))
                    Directory.CreateDirectory(strAlignMarkTemplateImagePath);

                CRect crtTopAlignMarkROI = new CRect(m_cptTopAlignMarkCenterPos, m_nTopWidth, m_nTopHeight);
                CRect crtBottomAlignMarkROI = new CRect(m_cptBottomAlignMarkCenterPos, m_nBottomWidth, m_nBottomHeight);

                m_module.GetGrayByteImageFromMemory_12bit(mem, crtTopAlignMarkROI).Save(Path.Combine(strAlignMarkTemplateImagePath, "TopTemplateImage.bmp"));
                m_module.GetGrayByteImageFromMemory_12bit(mem, crtBottomAlignMarkROI).Save(Path.Combine(strAlignMarkTemplateImagePath, "BottomTemplateImage.bmp"));

                return "OK";
            }
        }
        #endregion

        #region Run_PatternAlign
        public class Run_PatternAlign : ModuleRunBase
        {
            Vision m_module;

            // Scan Parameter
            public GrabMode m_grabMode = null;
            public int m_nCurScanLine = 0;
            string m_sGrabMode = "";
            public string p_sGrabMode
            {
                get { return m_sGrabMode; }
                set
                {
                    m_sGrabMode = value;
                    m_grabMode = m_module.GetGrabMode(value);
                }
            }

            // Template Matching Parameter
            public int m_nSearchAreaSize = 1000;
            public double m_dMatchScore = 0.9;
            public Run_PatternAlign(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_PatternAlign run = new Run_PatternAlign(m_module);
                run.p_sGrabMode = p_sGrabMode;
                run.m_nSearchAreaSize = m_nSearchAreaSize;
                run.m_dMatchScore = m_dMatchScore;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nSearchAreaSize = tree.Set(m_nSearchAreaSize, m_nSearchAreaSize, "Template Matching Search Area Size", "Template Matching Search Area Size", bVisible);
                m_dMatchScore = tree.Set(m_dMatchScore, m_dMatchScore, "Template Matching Score", "Template Matching Score", bVisible);
            }

            public override string Run()
            {
                // Align Mark 스캔
                if (m_grabMode == null) return "Grab Mode == null";

                // StopWatch 설정
                StopWatch snapTimeWatcher = new StopWatch();
                snapTimeWatcher.Start();

                // 상수 값
                const int TIMEOUT_10MS = 10000;     // ms
                const int TIMEOUT_50MS = 50000;     // ms
                const int TIMEOUT_INTERVAL = 10;    // ms
                const int RESCAN_MAX = 3;
                int MM_PER_UM = 1000;

                // 축 가져오기
                AxisXY axisXY = m_module.AxisXY;
                Axis axisZ = m_module.AxisZ;
                Axis axisRotate = m_module.AxisRotate;

                Camera_Dalsa camMain = (Camera_Dalsa)m_grabMode.m_camera;
                Camera_Basler camRADS = (Camera_Basler)m_module.CamRADS;
                GrabData grabData = m_grabMode.m_GD;

                // variable
                CPoint cptTopCenter;
                CPoint cptBottomCenter;
                CPoint cptTopResultCenter;
                CPoint cptBottomResultCenter;
                Run_MakeTemplateImage moduleRun = (Run_MakeTemplateImage)m_module.CloneModuleRun("MakeTemplateImage");
                string strPool   = m_grabMode.m_memoryPool.p_id;
                string strGroup  = m_grabMode.m_memoryGroup.p_id;
                string strMemory = m_grabMode.m_memoryData.p_id;
                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                bool bFoundTop = false;
                bool bFoundBottom = false;
                Image<Gray, byte> imgTop = new Image<Gray, byte>("D:\\AlignMarkTemplateImage\\TopTemplateImage.bmp");
                Image<Gray, byte> imgBottom = new Image<Gray, byte>("D:\\AlignMarkTemplateImage\\BottomTemplateImage.bmp");

                try
                {
                    // RADS 연결
                    if (m_grabMode.pUseRADS && m_module.m_RADSControl.p_IsRun == false)
                    {
                        m_module.m_RADSControl.m_timer.Start();
                        m_module.m_RADSControl.p_IsRun = true;
                        m_module.m_RADSControl.StartRADS();
                        StopWatch sw = new StopWatch();
                        if (camRADS.p_CamInfo._OpenStatus == false) camRADS.Connect();
                        while (camRADS.p_CamInfo._OpenStatus == false)
                        {
                            if (sw.ElapsedMilliseconds > 15000)
                            {
                                sw.Stop();
                                return "RADS Camera Not Connected";
                            }
                        }
                        sw.Stop();
                        camRADS.SetMulticast();
                        camRADS.GrabContinuousShot();
                    }

                    // 카메라 연결 시도
                    camMain.Connect();

                    int nTimeOut = TIMEOUT_50MS / TIMEOUT_INTERVAL;
                    while (camMain.p_CamInfo.p_eState != eCamState.Ready)
                    {
                        if (nTimeOut-- == 0)
                        {
                            throw new Exception("Camera Connect Error");
                        }
                        Thread.Sleep(TIMEOUT_INTERVAL);
                    }

                    // 스캔 라인 초기화
                    m_nCurScanLine = 0;

                    // 기계 장치 설정
                    //m_grabMode.SetLens();
                    m_grabMode.SetLight(true);

                    // 메모리 오프셋
                    CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                    cpMemoryOffset.X += m_grabMode.m_ScanStartLine * grabData.m_nFovSize;

                    m_grabMode.m_dTrigger = Math.Round(m_grabMode.m_dResY_um * m_grabMode.m_dCamTriggerRatio, 1);     // 트리거 (1 pulse = 3.0 mm)

                    int nWaferSizeY_px = (int)Math.Round(m_grabMode.m_nWaferSize_mm * MM_PER_UM / m_grabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                    int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수

                    while (m_grabMode.m_ScanLineNum > m_nCurScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        // 이동 위치 계산
                        int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * (double)m_grabMode.m_nScanRate / 100);
                        int nScanOffset_pulse = (int)((double)nScanSpeed * axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move).m_acc * 0.5) * 2;

                        int nLineIndex = m_grabMode.m_ScanStartLine + m_nCurScanLine;

                        double dfov_mm = grabData.m_nFovSize * m_grabMode.m_dResX_um * 0.001;
                        double dOverlap_mm = grabData.m_nOverlap * m_grabMode.m_dResX_um * 0.001;
                        double dPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_nWaferSize_mm * 0.5 - nLineIndex * (dfov_mm - dOverlap_mm);
                        double dNextPosX = dPosX - (dfov_mm - dOverlap_mm);
                        double dPosZ = m_grabMode.m_dFocusPosZ;

                        double dMarginY = m_grabMode.m_nWaferSize_mm * 0.1;
                        double dTriggerStartPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y - m_grabMode.m_nWaferSize_mm * 0.5 - dMarginY;
                        double dTriggerEndPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y + m_grabMode.m_nWaferSize_mm * 0.5 + dMarginY;
                        double dStartPosY = dTriggerStartPosY - nScanOffset_pulse;
                        double dEndPosY = dTriggerEndPosY + nScanOffset_pulse;

                        // Grab 방향 및 시작, 종료 위치 설정
                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                        if (m_grabMode.m_bUseBiDirectionScan && m_nCurScanLine % 2 == 1)
                        {
                            // dStartPosY <--> dEndPosY 바꿈.
                            (dStartPosY, dEndPosY) = (dEndPosY, dStartPosY);
                            (dTriggerStartPosY, dTriggerEndPosY) = (dTriggerEndPosY, dTriggerStartPosY);

                            m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }

                        //포커스 높이로 Z축 이동
                        if (m_module.Run(axisZ.StartMove(dPosZ)))
                            return p_sInfo;

                        // 시작 위치로 X, Y축 이동
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(dPosX, dStartPosY)))
                            return p_sInfo;

                        // 이동 대기
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        grabData.bInvY = m_grabMode.m_eGrabDirection == eGrabDirection.BackWard;
                        grabData.nScanOffsetY = nLineIndex * m_grabMode.m_nYOffset;

                        // 카메라 그랩 시작
                        CPoint tmpMemOffset = new CPoint(cpMemoryOffset);
                        // IPU PC와 연결된 상태에서는 이미지 데이터가 복사될 Main PC의 Memory 위치가
                        // Memory Width를 넘어가게 되면 다시 0부터 이미지를 얻어오도록 Memory Offset을 계산
                        long div = tmpMemOffset.X / mem.W;
                        long remain = tmpMemOffset.X - mem.W * div;
                        long offset = remain % grabData.m_nFovSize;
                        tmpMemOffset.X = (int)(remain - offset);
                        m_grabMode.StartGrab(mem, tmpMemOffset, nWaferSizeY_px, grabData);

                        // Y축 트리거 발생
                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, 0.001, true);

                        // 라인스캔 완료 대기
                        if (m_module.Run(axisXY.p_axisY.WaitReady()))
                            return p_sInfo;

                        // 다음 이미지 획득을 위해 변수 값 변경
                        m_nCurScanLine++;
                        cpMemoryOffset.X += grabData.m_nFovSize;
                    }
                    m_grabMode.m_camera.StopGrab();

                    snapTimeWatcher.Stop();

                    // Log
                    TempLogger.Write("Snap", string.Format("{0:F3}", (double)snapTimeWatcher.ElapsedMilliseconds / (double)1000));

                    return "OK";
                }
                catch (Exception e)
                {
                    m_log.Info(e.Message);
                    return "OK";
                }
                finally
                {
                    m_grabMode.SetLight(false);
                    if (m_grabMode.pUseRADS && m_module.m_RADSControl.p_IsRun == true)
                    {
                        m_module.m_RADSControl.m_timer.Stop();
                        m_module.m_RADSControl.p_IsRun = false;
                        m_module.m_RADSControl.StopRADS();
                        if (camRADS.p_CamInfo._IsGrabbing == true) camRADS.StopGrab();
                    }
                }

                // implement
                cptTopCenter = moduleRun.m_cptTopAlignMarkCenterPos;
                cptBottomCenter = moduleRun.m_cptBottomAlignMarkCenterPos;

                // Top Template Image Processing
                Point ptStart = new Point(cptTopCenter.X - (m_nSearchAreaSize / 2), cptTopCenter.Y - (m_nSearchAreaSize / 2));
                Point ptEnd = new Point(cptTopCenter.X + (m_nSearchAreaSize / 2), cptTopCenter.Y + (m_nSearchAreaSize / 2));
                CRect crtSearchArea = new CRect(ptStart, ptEnd);
                Image<Gray, byte> imgSrc = m_module.GetGrayByteImageFromMemory_12bit(mem, crtSearchArea);
                bFoundTop = m_module.TemplateMatching(mem, crtSearchArea, imgSrc, imgTop, out cptTopResultCenter, m_dMatchScore);

                // Bottom Template Image Processing
                ptStart = new Point(cptBottomCenter.X - (m_nSearchAreaSize / 2), cptBottomCenter.Y - (m_nSearchAreaSize / 2));
                ptEnd = new Point(cptBottomCenter.X + (m_nSearchAreaSize / 2), cptBottomCenter.Y + (m_nSearchAreaSize / 2));
                crtSearchArea = new CRect(ptStart, ptEnd);
                imgSrc = m_module.GetGrayByteImageFromMemory_12bit(mem, crtSearchArea);
                bFoundBottom = m_module.TemplateMatching(mem, crtSearchArea, imgSrc, imgBottom, out cptBottomResultCenter, m_dMatchScore);

                // Calculate Theta
                if (bFoundTop && bFoundBottom) // Top & Bottom 모두 Template Matching 성공했을 경우
                {
                    double dThetaRadian = Math.Atan2((double)(cptBottomResultCenter.Y - cptTopResultCenter.Y), (double)(cptBottomResultCenter.X - cptTopResultCenter.X));
                    double dThetaDegree = dThetaRadian * (180 / Math.PI);
                    dThetaDegree -= 90;

                    // Rotate 축 Theta만큼 회전
                    if (m_module.Run(axisRotate.StartMove(dThetaDegree * -1)))
                        return p_sInfo;
                    if (m_module.Run(axisRotate.WaitReady()))
                        return p_sInfo;
                }

                return "OK";
            }
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Test(this), true, "Test");
            AddModuleRunList(new Run_Remote(this), true, "Remote Run");
            //AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            //AddModuleRunList(new Run_Rotate(this), false, "Rotate Axis");
            AddModuleRunList(new Run_GrabLineScan(this), true, "Run Grab LineScan Camera");
            //AddModuleRunList(new Run_Inspect(this), true, "Run Inspect");
            //AddModuleRunList(new Run_VisionAlign(this), true, "Run VisionAlign");
            //AddModuleRunList(new Run_AutoFocus(this), false, "Run AutoFocus");
            AddModuleRunList(new Run_MakeTemplateImage(this), true, "Run Make TemplateImage");
            AddModuleRunList(new Run_PatternAlign(this), true, "Run Pattern Align");
            AddModuleRunList(new Run_RADS(this), true, "Run RADS");
        }
        #endregion
    }
}
