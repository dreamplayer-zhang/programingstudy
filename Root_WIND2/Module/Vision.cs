using Root_EFEM;
using Root_EFEM.Module;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.RADS;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Root_WIND2.Module
{
    public class Vision : ModuleBase, IWTRChild
    {
        #region ToolBox
        Axis m_axisRotate;
        Axis m_axisZ;
        AxisXY m_axisXY;
        DIO_O m_doVac;
        DIO_O m_doBlow;
        MemoryPool m_memoryPool;
        MemoryPool m_memoryPool2;
        MemoryGroup m_memoryGroup;
        MemoryGroup m_memoryGroup2;
        MemoryData m_memoryMain;
        LightSet m_lightSet;
        RADSControl m_RADSControl;

        Camera_Dalsa m_CamMain;
        Camera_Basler m_CamAlign;
        Camera_Basler m_CamAutoFocus;

        #region [Getter Setter]
        public Axis AxisRotate { get => m_axisRotate; private set => m_axisRotate = value; }
        public Axis AxisZ { get => m_axisZ; private set => m_axisZ = value; }
        public AxisXY AxisXY { get => m_axisXY; private set => m_axisXY = value; }
        public DIO_O DoVac { get => m_doVac; private set => m_doVac = value; }
        public DIO_O DoBlow { get => m_doBlow; private set => m_doBlow = value; }
        public MemoryPool MemoryPool { get => m_memoryPool; private set => m_memoryPool = value; }
        public MemoryPool MemoryPool2 { get => m_memoryPool2; private set => m_memoryPool2 = value; }
        public MemoryGroup MemoryGroup { get => m_memoryGroup; private set => m_memoryGroup = value; }
        public MemoryGroup MemoryGroup2 { get => m_memoryGroup2; private set => m_memoryGroup2 = value; }
        public MemoryData MemoryMain { get => m_memoryMain; private set => m_memoryMain = value; }
        public LightSet LightSet { get => m_lightSet; private set => m_lightSet = value; }
        public RADSControl RADSControl { get => m_RADSControl; private set => m_RADSControl = value; }
        public Camera_Dalsa CamMain { get => m_CamMain; private set => m_CamMain = value; }
        public Camera_Basler CamAlign { get => m_CamAlign; private set => m_CamAlign = value; }
        public Camera_Basler CamAutoFocus { get => m_CamAutoFocus; private set => m_CamAutoFocus = value; }

        #endregion

        public override void GetTools(bool bInit)
        {
            if (p_eRemote != eRemote.Client)
            {
                p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Axis Rotate");
                p_sInfo = m_toolBox.Get(ref m_axisZ, this, "Axis Z");
                p_sInfo = m_toolBox.Get(ref m_axisXY, this, "Axis XY");
                p_sInfo = m_toolBox.Get(ref m_doVac, this, "Stage Vacuum");
                p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Stage Blow");
                p_sInfo = m_toolBox.Get(ref m_lightSet, this);
                p_sInfo = m_toolBox.Get(ref m_RADSControl, this, "RADSControl", false);
                p_sInfo = m_toolBox.Get(ref m_CamMain, this, "MainCam");
                p_sInfo = m_toolBox.Get(ref m_CamAlign, this, "AlignCam");
                p_sInfo = m_toolBox.Get(ref m_CamAutoFocus, this, "AutoFocusCam");
            }
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
            p_sInfo = m_toolBox.Get(ref m_memoryPool2, this, "pool", 1, true);
            m_remote.GetTools(bInit);
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
                GrabMode grabMode = new GrabMode(id, m_cameraSet, m_lightSet, m_memoryPool, m_RADSControl);
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
            get
            {
                return m_doVac.p_bOut;
            }
            set
            {
                if (m_doVac.p_bOut == value)
                    return;
                m_doVac.Write(value);
            }
        }

        public bool p_bStageBlow
        {
            get
            {
                return m_doBlow.p_bOut;
            }
            set
            {
                if (m_doBlow.p_bOut == value)
                    return;
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
            base.Reset();
        }

        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryMain = m_memoryGroup.CreateMemory("Main", 3, 1, 40000, 40000);
            m_memoryGroup2 = m_memoryPool2.GetGroup("group");
            m_memoryGroup2.CreateMemory("ROI", 1, 4, 30000, 30000); // Chip 크기 최대 30,000 * 30,000 고정 Origin ROI 메모리 할당 20.11.02 JTL 
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
        #endregion

        #region IWTRChild
        bool _bLock = false;
        public bool p_bLock
        {
            get
            {
                return _bLock;
            }
            set
            {
                if (_bLock == value)
                    return;
                _bLock = value;
            }
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false)
                    return false;
                Thread.Sleep(100);
            }
            return true;
        }

        public List<string> p_asChildSlot
        {
            get
            {
                return null;
            }
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
            if (p_eRemote == eRemote.Client)
            {   
                return "OK";
            }
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer == null)
                return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eRemote == eRemote.Client)
            {
                return "OK";
            }
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer != null)
                return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false)
                return p_id + " not Enable Wafer Size";
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null)
                infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            //            string info = MoveReadyPos();
            //            if (info != "OK") return info;
            if (p_eRemote == eRemote.Client)
            {
                return m_remote.RemoteSend(Remote.eProtocol.BeforeGet, "Get", "Get");
            }
            else if (p_eRemote == eRemote.Server)
            {
                m_axisXY.StartMove("Position_0");
                m_axisRotate.StartMove("Position_0");
                m_axisZ.StartMove("Position_0");

                m_axisXY.WaitReady();
                m_axisRotate.WaitReady();
                m_axisZ.WaitReady();

                ClearData();
            }
            return "OK";
        }

        public override string ServerBeforeGet()
        {
            return BeforeGet(0);
        }

        public string BeforePut(int nID)
        {
            //            string info = MoveReadyPos();
            //            if (info != "OK") return info;
            if (p_eRemote == eRemote.Client)
            {
                return m_remote.RemoteSend(Remote.eProtocol.BeforePut, "Put", "Put");
            }
            else if(p_eRemote == eRemote.Server)
            {
                m_axisXY.StartMove("Position_0");
                m_axisRotate.StartMove("Position_0");
                m_axisZ.StartMove("Position_0");

                m_axisXY.WaitReady();
                m_axisRotate.WaitReady();
                m_axisZ.WaitReady();
            }
            return "OK";
        }
        public override string ServerBeforePut()
        {
            return BeforePut(0);
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
            get
            {
                return _infoWafer;
            }
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
            if (EQ.p_bSimulate)
                return "OK";

            if (p_eRemote == eRemote.Client)
            {
                m_remote.RemoteSend(Remote.eProtocol.Initial, "INIT", "INIT");
                ClearData();
                return "OK";
            }
            else
            {
                //            p_bStageBlow = false;
                //            p_bStageVac = true;
                Thread.Sleep(200);
                if (m_CamMain != null && m_CamMain.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init)
                    m_CamMain.Connect();

                //if (m_CamAlign != null)
                //    m_CamAlign.Connect();

                //if (m_CamAutoFocus != null)
                //    m_CamAutoFocus.Connect();

                p_sInfo = base.StateHome();
                p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
                //p_bStageVac = false;
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

        public Vision(string id, IEngineer engineer, eRemote eRemote)
        {
            //            InitLineScan();+
            //            InitAreaScan();
            base.InitBase(id, engineer, eRemote);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            //            InitMemory();
        }

        public override void ThreadStop()
        {
            //
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Rotate(this), false, "Rotate Axis");
            AddModuleRunList(new Run_GrabLineScan(this), false, "Run Grab LineScan Camera");
            AddModuleRunList(new Run_Inspect(this), false, "Run Inspect");
            AddModuleRunList(new Run_VisionAlign(this), false, "Run VisionAlign");
            AddModuleRunList(new Run_AutoFocus(this), false, "Run AutoFocus");
            
        }
        #endregion
    }
}
