using Root_EFEM.Module;
using Root_VEGA_D_IPU.Module;
using RootTools;
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
using RootTools_Vision.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Threading;

namespace Root_VEGA_D.Module
{
    public class Vision : ModuleBase, IWTRChild
    {
        #region GAF
        public ALID m_visionHomeError;
        public ALID m_visionInspectError;
        public ALID m_alidShutterDownError;
        public ALID m_alidShutterUpError;
        ALID m_alid_WaferExist;
        void InitGAF()
        {
            m_visionHomeError = m_gaf.GetALID(this, "Vision Home Error", "Vision Home Error");
            m_visionInspectError = m_gaf.GetALID(this, "Vision Inspect Error", "Vision Inspect Error");
            m_alidShutterDownError = m_gaf.GetALID(this, "VS Shutter Error", "Shutter is not down");
            m_alidShutterUpError = m_gaf.GetALID(this, "VS Shutter Error", "Shutter is not up");

        }
        public void SetAlarm()
        {
            m_alid_WaferExist.Run(true, "Vision Wafer Exist Error");
        }
        #endregion

        #region ToolBox
        Axis m_axisRotate;
        Axis m_axisZ;
        AxisXY m_axisXY;
        DIO_O m_doVac;
        DIO_O m_doBlow;
        DIO_O m_doShutterDown;
        DIO_O m_doShutterUp;
        DIO_I m_diShutterDownCheck;
        DIO_I m_diShutterUpCheck;
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
        RADSControl m_RADSControl;

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
            p_sInfo = m_toolBox.GetDIO(ref m_doShutterUp, this, "Shutter Up");
            p_sInfo = m_toolBox.GetDIO(ref m_doShutterDown, this, "Shutter Down");
            p_sInfo = m_toolBox.GetDIO(ref m_diShutterUpCheck, this, "Shutter Up Check");
            p_sInfo = m_toolBox.GetDIO(ref m_diShutterDownCheck, this, "Shutter Down Check");
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
            //if (m_CamRADS.p_CamInfo != null) bUseRADS = true;
            //p_sInfo = m_toolBox.Get(ref m_RADSControl, this, "RADSControl", bUseRADS);
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
            //shutter
            //m_doShutterUp.Write(false);
            //Thread.Sleep(100);
            //m_doShutterDown.Write(true);
            //Thread.Sleep(3000);
            //StopWatch sw = new StopWatch();
            //sw.Start();
            //while (!m_diShutterDownCheck.p_bIn || m_diShutterUpCheck.p_bIn)
            //{
            //    if (sw.ElapsedMilliseconds > 5000)
            //    {
            //        m_alidShutterDownError.Run(true, "Shutter error in Beforeget");
            //    }
            //}
            //
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
            //shutter
            //m_doShutterUp.Write(false);
            //Thread.Sleep(100);
            //m_doShutterDown.Write(true);
            //Thread.Sleep(3000);
            //StopWatch sw = new StopWatch();
            //sw.Start();
            //while (!m_diShutterDownCheck.p_bIn || m_diShutterUpCheck.p_bIn)
            //{
            //    if (sw.ElapsedMilliseconds > 5000)
            //    {
            //        m_alidShutterDownError.Run(true, "Shutter error in Beforeput");
            //    }
            //}
            //
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
            //shutter
            //m_doShutterDown.Write(false);
            //Thread.Sleep(100);
            //m_doShutterUp.Write(true);
            //Thread.Sleep(3000);
            //StopWatch sw = new StopWatch();
            //sw.Start();
            //while (m_diShutterDownCheck.p_bIn || !m_diShutterUpCheck.p_bIn)
            //{
            //    if (sw.ElapsedMilliseconds > 5000)
            //    {
            //        m_alidShutterDownError.Run(true, "Shutter error in Afterget");
            //    }
            //}
            //
            return "OK";
        }

        public string AfterPut(int nID)
        {
            //shutter
            //m_doShutterDown.Write(false);
            //Thread.Sleep(100);
            //m_doShutterUp.Write(true);
            //Thread.Sleep(3000);
            //StopWatch sw = new StopWatch();
            //sw.Start();
            //while (m_diShutterDownCheck.p_bIn || !m_diShutterUpCheck.p_bIn)
            //{
            //    if (sw.ElapsedMilliseconds > 5000)
            //    {
            //        m_alidShutterDownError.Run(true, "Shutter error in Afterput");
            //    }
            //}
            //
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
                m_visionHomeError.Run(p_sInfo != "OK", "Vision Home Error");
                p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;

                ClearData();

                return "OK";
            }
        }

        public int m_secHome = 60;
        void RunTreeTimeout(Tree tree)
        {
            m_secHome = tree.Set(m_secHome, m_secHome, "Home", "Timeout (sec)");
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
            m_tcpipCommServer.EventReciveData += EventReceiveData;
            InitGAF();
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

        private void EventReceiveData(byte[] aBuf, int nSize, Socket socket)
        {
            if (nSize <= 0)
                return;

            int nStartIdx = 0;
            TCPIPComm_VEGA_D.Command cmd = TCPIPComm_VEGA_D.Command.none;
            Dictionary<string, string> mapParam = new Dictionary<string, string>();

            while (m_tcpipCommServer.ParseMessage(aBuf, nSize, ref nStartIdx, ref cmd, mapParam))
            {
                switch (cmd)
                {
                    case TCPIPComm_VEGA_D.Command.result:
                        {
                            ModuleRunBase moduleRunBase = m_aModuleRun.Find(x => (x.GetType() == typeof(Run_GrabLineScan)));
                            if (moduleRunBase != null)
                                moduleRunBase.Run();
                        }
                        break;
                    case TCPIPComm_VEGA_D.Command.alive:
                        break;
                    case TCPIPComm_VEGA_D.Command.ready:
                        break;
                    default:
                        break;
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
        }
        #endregion
    }
}
