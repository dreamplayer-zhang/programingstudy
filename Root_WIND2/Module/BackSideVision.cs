using RootTools;
using RootTools.Camera.Dalsa;
using RootTools.Camera.Silicon;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Root_EFEM.Module;

namespace Root_WIND2.Module
{
    static class Strings
    {
        public const string BackSideMem = "BackSide";
        public const string LADSMem = "LADS";
    }
    public class BackSideVision : ModuleBase, IWTRChild
    {
        #region ToolBox
        Axis axisZ;
        AxisXY axisXY;
        DIO_O doVac;
        DIO_O doBlow;
        DIO_I diWaferExist;
        MemoryPool memoryPool;
        MemoryGroup memoryGroup;
        MemoryData memoryMain;
        MemoryData memoryLADS;
        LightSet lightSet;
        Camera_Dalsa camMain;
        Camera_Silicon camLADS;

        public class LADSInfo//한 줄에 대한 정보
        {
            public double[] m_Heightinfo;
            public RPoint axisPos;//시작점의 x,y
            public double endYPos;//끝점의 y 정보

            LADSInfo() { }
            public LADSInfo(RPoint _axisPos, double _endYPos, int arrcap/*heightinfo capacity*/)
            {
                axisPos = _axisPos;
                endYPos = _endYPos;
                m_Heightinfo = new double[arrcap];
            }
        }

        public List<LADSInfo> ladsinfos;


        #region Getter/Setter
        public Axis AxisZ { get => axisZ; private set => axisZ = value; }
        public AxisXY AxisXY { get => axisXY; private set => axisXY = value; }
        #endregion

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref axisZ, this, "Axis Z");
            p_sInfo = m_toolBox.Get(ref axisXY, this, "Axis XY");
            p_sInfo = m_toolBox.Get(ref doVac, this, "Stage Vacuum");
            p_sInfo = m_toolBox.Get(ref doBlow, this, "Stage Blow");
            p_sInfo = m_toolBox.Get(ref diWaferExist, this, "Wafer Exist");
            p_sInfo = m_toolBox.Get(ref memoryPool, this, "BackSide Memory", 1);
            p_sInfo = m_toolBox.Get(ref lightSet, this);
            p_sInfo = m_toolBox.Get(ref camMain, this, "MainCam");
            p_sInfo = m_toolBox.Get(ref camLADS, this, "LADSCam");
            memoryGroup = memoryPool.GetGroup(p_id);
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

        void RunTreeGrabMode(Tree tree)
        {
            m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
            while (m_aGrabMode.Count < m_lGrabMode)
            {
                string id = "Mode." + m_aGrabMode.Count.ToString("00");
                GrabMode grabMode = new GrabMode(id, m_cameraSet, lightSet, memoryPool);
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
                return doVac.p_bOut;
            }
            set
            {
                if (doVac.p_bOut == value)
                    return;
                doVac.Write(value);
            }
        }

        public bool p_bStageBlow
        {
            get
            {
                return doBlow.p_bOut;
            }
            set
            {
                if (doBlow.p_bOut == value)
                    return;
                doBlow.Write(value);
            }
        }

        public void RunBlow(int msDelay)
        {
            doBlow.DelayOff(msDelay);
        }
        #endregion

        #region override
        public override void InitMemorys()
        {
            memoryGroup = memoryPool.GetGroup(p_id);
            memoryMain = memoryGroup.CreateMemory(Strings.BackSideMem, 1, 1, 1000, 1000);
            memoryLADS = memoryGroup.CreateMemory(Strings.LADSMem, 1, 1, 1000, 1000);
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
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            //if (p_infoWafer == null)
            //    return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
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
            //            string info = MoveReadyPos();
            //            if (info != "OK") return info;
            return "OK";
        }

        public string BeforePut(int nID)
        {
            //            string info = MoveReadyPos();
            //            if (info != "OK") return info;
            return "OK";
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
            //            p_bStageBlow = false;
            //            p_bStageVac = true;
            Thread.Sleep(200);

            if (camMain != null && camMain.p_CamInfo.p_eState == eCamState.Init)
                camMain.Connect();
            if (camLADS != null)
                camLADS.Connect();
                
            base.StateHome();

            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;

            if (diWaferExist.p_bIn == false)
                p_bStageVac = false;

            return p_sInfo;
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
            RunTreeGrabMode(tree.GetTree("Grab Mode", false));
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_GrabBackside(this), true, "Run Grab Backside");
            AddModuleRunList(new Run_LADS(this), true, "Run LADS");
        }
        public ImageData[] GetMemoryData()
        {
            ImageData[] res = new ImageData[2];

            res[0] = new ImageData(memoryPool.GetMemory(p_id, Strings.BackSideMem));
            res[1] = new ImageData(memoryPool.GetMemory(p_id, Strings.LADSMem));

            return res;
        }
        #endregion

        public BackSideVision(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            ladsinfos = new List<LADSInfo>();
            //InitMemorys();
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
