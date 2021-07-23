using Root_EFEM.Module;
using Root_EFEM.Module.FrontsideVision;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Lens.LinearTurret;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision.Utility;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Root_EFEM
{
    public class Vision_Frontside : ModuleBase, IWTRChild
    {
        ALID m_alid_WaferExist;
        public void SetAlarm()
        {
            m_alid_WaferExist.Run(true, "Vision Stage Wafer Exist Error");
        }
        #region ToolBox
        Axis m_axisRotate;
        Axis m_axisZ;
        AxisXY m_axisXY;
        DIO_O m_doVac;
        DIO_O m_doBlow;
        DIO_I m_diReadyX;
        DIO_I m_diReadyY;
        DIO_I m_diWaferExistLoad;
        DIO_I m_diWaferExistHome;
        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryMain;
        MemoryData m_memoryLayer;
        LightSet m_lightSet;

        Camera_Dalsa m_CamMain;
        Camera_Basler m_CamAlign;
        Camera_Basler m_CamAutoFocus;
        Camera_Basler m_CamVRS;

        public Camera_Basler p_CamAutoFocus
        {
            get
            {
                return m_CamAutoFocus;
            }
            set
            {
                m_CamAutoFocus = value;
            }
        }

        public Camera_Basler p_CamVRS
        {
            get
            {
                return m_CamVRS;
            }
            set
            {
                m_CamVRS = value;
            }
        }

        public Camera_Basler p_CamAlign
        {
            get
            {
                return m_CamAlign;
            }
            set
            {
                m_CamAlign = value;
            }
        }

        KlarfData_Lot m_KlarfData_Lot;
        LensLinearTurret m_LensLinearTurret;
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
        public Camera_Basler CamVRS { get => m_CamVRS; private set => m_CamVRS = value; }
        public Camera_Basler CamAutoFocus { get => m_CamAutoFocus; private set => m_CamAutoFocus = value; }

        public KlarfData_Lot KlarfData_Lot { get => m_KlarfData_Lot; private set => m_KlarfData_Lot = value; }
        #endregion

        public override void GetTools(bool bInit)
        {
            if (p_eRemote != eRemote.Client)
            {
                p_sInfo = m_toolBox.GetAxis(ref m_axisRotate, this, "Axis Rotate");
                p_sInfo = m_toolBox.GetAxis(ref m_axisZ, this, "Axis Z");
                p_sInfo = m_toolBox.GetAxis(ref m_axisXY, this, "Axis XY");
                p_sInfo = m_toolBox.GetDIO(ref m_doVac, this, "Stage Vacuum");
                p_sInfo = m_toolBox.GetDIO(ref m_doBlow, this, "Stage Blow");
                p_sInfo = m_toolBox.GetDIO(ref m_diReadyX, this, "Stage Ready X");
                p_sInfo = m_toolBox.GetDIO(ref m_diReadyY, this, "Stage Ready Y");
                p_sInfo = m_toolBox.GetDIO(ref m_diWaferExistHome, this, "Wafer Exist On Home Position");
                p_sInfo = m_toolBox.GetDIO(ref m_diWaferExistLoad, this, "Wafer Exist On Load Position");
                p_sInfo = m_toolBox.Get(ref m_lightSet, this);
                p_sInfo = m_toolBox.GetCamera(ref m_CamMain, this, "MainCam");
                p_sInfo = m_toolBox.GetCamera(ref m_CamAlign, this, "AlignCam");
                p_sInfo = m_toolBox.GetCamera(ref m_CamVRS, this, "VRSCam");
                p_sInfo = m_toolBox.GetCamera(ref m_CamAutoFocus, this, "AutoFocusCam");
                p_sInfo = m_toolBox.Get(ref m_LensLinearTurret, this, "LensTurret");
            }
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
            m_alid_WaferExist = m_gaf.GetALID(this, "Vision Wafer Exist", "Vision Wafer Exist");
            m_remote.GetTools(bInit);
        }
        #endregion

        #region Grab Mode
        int m_lGrabMode = 0;
        public ObservableCollection<GrabModeFront> m_aGrabMode = new ObservableCollection<GrabModeFront>();
        public List<string> p_asGrabMode
        {
            get
            {
                List<string> asGrabMode = new List<string>();
                foreach (GrabModeBase grabMode in m_aGrabMode) asGrabMode.Add(grabMode.p_sName);
                return asGrabMode;
            }
        }

        public GrabModeFront GetGrabMode(string sGrabMode)
        {
            foreach (GrabModeFront grabMode in m_aGrabMode)
            {
                if (sGrabMode == grabMode.p_sName) return grabMode;
            }
            return null;
        }
        public GrabModeFront GetGrabMode(int index)
        {
            if (m_aGrabMode?.Count > 0)
            {
                return m_aGrabMode[index];
            }
            return null;
        }

        public void ClearData()
        {
            foreach (GrabModeFront grabMode in m_aGrabMode)
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
                GrabModeFront grabMode = new GrabModeFront(id, m_cameraSet, m_lightSet, m_memoryPool, m_LensLinearTurret);
                m_aGrabMode.Add(grabMode);
            }
            while (m_aGrabMode.Count > m_lGrabMode) m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
            foreach (GrabModeFront grabMode in m_aGrabMode) grabMode.RunTreeName(tree.GetTree("Name", false));
            foreach (GrabModeFront grabMode in m_aGrabMode) grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
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
            m_memoryMain = m_memoryGroup.CreateMemory("Layer", 1, 4, 30000, 30000); // Chip 크기 최대 30,000 * 30,000 고정 Origin ROI 메모리 할당 20.11.02 JTL 
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
            //if (p_infoWafer == null)
            //    return p_id + " IsGetOK - InfoWafer not Exist";
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
            //if (p_infoWafer != null)
            //    return p_id + " IsPutOK - InfoWafer Exist";
            //if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false)
            //    return p_id + " not Enable Wafer Size";
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
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforeGet, eRemote.Client, nID);
            else
            {
                Thread.Sleep(1000);
                m_axisXY.StartMove("Position_2");
                m_axisRotate.StartMove("Position_2");
                m_axisZ.StartMove("Position_2");

                Thread.Sleep(1000);
                m_axisXY.WaitReady();
                m_axisRotate.WaitReady();
                m_axisZ.WaitReady();
                DoVac.Write(false);
                if (!m_diReadyX.p_bIn || !m_diReadyY.p_bIn)
                    return "Ready Fail";
                ClearData();
                return "OK";
            }
        }

        public string BeforePut(int nID)
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforePut, eRemote.Client, nID);
            else
            {
                Thread.Sleep(1000);
                m_axisXY.StartMove("Position_2");
                m_axisRotate.StartMove("Position_2");
                m_axisZ.StartMove("Position_2");

                Thread.Sleep(1000);
                m_axisXY.WaitReady();
                m_axisRotate.WaitReady();
                m_axisZ.WaitReady();
                DoVac.Write(false);
                if (!m_diReadyX.p_bIn || !m_diReadyY.p_bIn)
                    return "Ready Fail";
                return "OK";
            }
        }

        public string AfterGet(int nID)
        {
        
                DoVac.Write(false);
                if (m_diWaferExistLoad.p_bIn)
                {
                    m_alid_WaferExist.Run(true, "WTR Get WaferExist Error In Stage");
                    return "WTR Get WaferExist Error In Stage";
                }
                else
                {
                    return "OK";
                }
          
        }

        public string AfterPut(int nID)
        {

            DoVac.Write(true);
            if (!m_diWaferExistLoad.p_bIn)
            {
                m_alid_WaferExist.Run(true, "WTR Get WaferExist Error In Stage");
                return "WTR Get WaferExist Error In Stage";
            }
            else
            {
                return "OK";
            }
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
            get { return _infoWafer; }
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

        public Vision_Frontside(string id, IEngineer engineer, eRemote eRemote)
        {
            //            InitLineScan();+
            //            InitAreaScan();
            base.InitBase(id, engineer, eRemote);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            //            InitMemory();
            OnChangeState += Vision_OnChangeState;
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
            AfterGet,
            AfterPut,
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
                case eRemoteRun.AfterGet: run.m_nID = value; break;
                case eRemoteRun.AfterPut: run.m_nID = value; break;
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
            Vision_Frontside m_module;
            public Run_Remote(Vision_Frontside module)
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

                    case eRemoteRun.AfterGet:
                    case eRemoteRun.AfterPut:
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
                    case eRemoteRun.AfterGet: return m_module.AfterGet(m_nID);
                    case eRemoteRun.AfterPut: return m_module.AfterPut(m_nID);
                }
                return "OK";
            }
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Remote(this), true, "Remote Run");
            //AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            //AddModuleRunList(new Run_Rotate(this), false, "Rotate Axis");
            AddModuleRunList(new Run_GrabLineScan(this), true, "Run Grab LineScan Camera");
            AddModuleRunList(new Run_Inspect(this), true, "Run Inspect");
            AddModuleRunList(new Run_VisionAlign(this), true, "Run VisionAlign");
            AddModuleRunList(new Run_VRSAlign(this), true, "Run VRSAlign");
            //AddModuleRunList(new Run_AutoFocus(this), false, "Run AutoFocus");
        }
        #endregion
    }
}
