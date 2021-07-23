using Root_EFEM.Module.BacksideVision;
using RootTools;
using RootTools.Camera.Dalsa;
using RootTools.Camera.Silicon;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Root_EFEM.Module
{
    public class Vision_Backside : ModuleBase, IWTRChild
    {
        public enum ScanMemory
        {
            BackSide, LADS
        }
        public void SetAlarm()
        {
            alid_WaferExist.Run(true, "BackSideVision Wafer Exist Error");
        }
        #region ToolBox
        Axis axisZ;
        AxisXY axisXY;
        DIO_O doVac;
        DIO_O doBlow;
        DIO_I diWaferExist;
        DIO_I diWaferExistVac;
        DIO_I di_test1;
        DIO_I di_test2;
        DIO_I di_test3;
        DIO_I di_test4;

        MemoryPool memoryPool;
        MemoryGroup memoryGroup;
        MemoryData memoryMain;
        MemoryData memoryLADS;
        LightSet lightSet;
        Camera_Dalsa camMain;
        Camera_Silicon camLADS;
        List<List<double>> ladsinfos;

        ALID alid_WaferExist;

        #region Getter/Setter
        public Axis AxisZ { get => axisZ; private set => axisZ = value; }
        public AxisXY AxisXY { get => axisXY; private set => axisXY = value; }
        public List<List<double>> LadsInfos
        { get => ladsinfos; private set => ladsinfos = value; }
        #endregion

        public override void GetTools(bool bInit)
        {
            if (p_eRemote != eRemote.Client)
            {
                p_sInfo = m_toolBox.GetAxis(ref axisZ, this, "Axis Z");
                p_sInfo = m_toolBox.GetAxis(ref axisXY, this, "Axis XY");
                p_sInfo = m_toolBox.GetDIO(ref doVac, this, "Stage Vacuum");
                p_sInfo = m_toolBox.GetDIO(ref doBlow, this, "Stage Blow");
                p_sInfo = m_toolBox.GetDIO(ref diWaferExist, this, "Wafer Exist");
                p_sInfo = m_toolBox.GetDIO(ref diWaferExistVac, this, "Wafer Exist Vac Check");
                p_sInfo = m_toolBox.GetDIO(ref di_test1, this, "ESide Elec Panel Top door");
                p_sInfo = m_toolBox.GetDIO(ref di_test2, this, "ESide Elec Panel Bottom door");
                p_sInfo = m_toolBox.GetDIO(ref di_test3, this, "Eside PC Door Sensor");
                p_sInfo = m_toolBox.GetDIO(ref di_test4, this, "Eside Top Loof Door Sensor");

                p_sInfo = m_toolBox.Get(ref lightSet, this);
                p_sInfo = m_toolBox.GetCamera(ref camMain, this, "MainCam");
                p_sInfo = m_toolBox.GetCamera(ref camLADS, this, "LADSCam");
                if (camLADS != null)
                    camLADS.Connect();
            }
            p_sInfo = m_toolBox.Get(ref memoryPool, this, "Memory", 1);
            memoryGroup = memoryPool.GetGroup(p_id);
            alid_WaferExist = m_gaf.GetALID(this, "Wafer Exist", "Wafer Exist");
            m_remote.GetTools(bInit);
        }
        #endregion

        #region Grab Mode
        int m_lGrabMode = 0;
        public ObservableCollection<GrabModeBack> m_aGrabMode = new ObservableCollection<GrabModeBack>();
        public List<string> p_asGrabMode
        {
            get
            {
                List<string> asGrabMode = new List<string>();
                foreach (GrabModeBase grabMode in m_aGrabMode) asGrabMode.Add(grabMode.p_sName);
                return asGrabMode;
            }
        }

        public GrabModeBack GetGrabMode(string sGrabMode)
        {
            foreach (GrabModeBack grabMode in m_aGrabMode)
            {
                if (sGrabMode == grabMode.p_sName) return grabMode;
            }
            return null;
        }

        public GrabModeBack GetGrabMode(int index)
        {
            if (m_aGrabMode?.Count > 0)
            {
                return m_aGrabMode[index];
            }
            return null;
        }

        void RunTreeGrabMode(Tree tree)
        {
            m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
            while (m_aGrabMode.Count < m_lGrabMode)
            {
                string id = "Mode." + m_aGrabMode.Count.ToString("00");
                GrabModeBack grabMode = new GrabModeBack(id, m_cameraSet, lightSet, memoryPool);
                m_aGrabMode.Add(grabMode);
            }
            while (m_aGrabMode.Count > m_lGrabMode) m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
            foreach (GrabModeBack grabMode in m_aGrabMode) grabMode.RunTreeName(tree.GetTree("Name", false));
            foreach (GrabModeBack grabMode in m_aGrabMode)
            {
                grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
                if (!grabMode.p_sName.Contains("LADS"))
                    grabMode.RunTreeLADS(tree.GetTree(grabMode.p_sName, false));
            }
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
            memoryMain = memoryGroup.CreateMemory(ScanMemory.BackSide.ToString(), 1, 1, 1000, 1000);
            memoryLADS = memoryGroup.CreateMemory(ScanMemory.LADS.ToString(), 1, 1, 1000, 1000);
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
            if (p_eRemote == eRemote.Client)
                return RemoteRun(eRemoteRun.BeforeGet, eRemote.Client, nID);
            else
            {
                return "OK";
                //if (p_infoWafer == null) return m_id + " BeforeGet : InfoWafer = null";
                //return CheckGetPut();
            }
        }

        public string BeforePut(int nID)
        {
            if (p_eRemote == eRemote.Client)
                return RemoteRun(eRemoteRun.BeforePut, eRemote.Client, nID);
            else
            {
                // Move to Ready Pos ?
                // Vacuum Off ?
                return "OK";
            }
        }

        public string AfterGet(int nID)
        {
            doVac.Write(false);
            Thread.Sleep(500);
            if (!diWaferExist.p_bIn )
            {
                return "OK";
            }
            else
            {
                alid_WaferExist.Run(true, "Back Side Wafer Exist Error");
                return "OK";
            }
            return "OK";
        }

        public string AfterPut(int nID)
        {
            doVac.Write(true);
            Thread.Sleep(500);
            if (diWaferExist.p_bIn && diWaferExistVac.p_bIn)
            {
                return "OK";
            }
            else
            {
                alid_WaferExist.Run(true, "Back Side Wafer Exist Error");
                return "OK";
            }
            //if (!diWaferExist.p_bIn || !diWaferExistVac.p_bIn)
            //    alid_WaferExist.Run(true, "Wafer Check Error");
           // return "OK";
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
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.StateHome, eRemote.Client, null);
            else
            {
                //            p_bStageBlow = false;
                //            p_bStageVac = true;
                Thread.Sleep(200);

                if (camMain != null && camMain.p_CamInfo.p_eState == eCamState.Init)
                    camMain.Connect();


                base.StateHome();

                p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;

                if (diWaferExist.p_bIn == false)
                    p_bStageVac = false;

                return p_sInfo;
            }
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
            AddModuleRunList(new Run_Remote(this), true, "Remote Run");
            AddModuleRunList(new Run_GrabBackside(this), true, "Run Grab Backside");
            //AddModuleRunList(new Run_LADS(this), true, "Run LADS");
            AddModuleRunList(new Run_InspectBackside(this), true, "Run BackInspection");

        }
        public ImageData GetMemoryData(ScanMemory mem)
        {
            return new ImageData(memoryPool.GetMemory(p_id, mem.ToString()));
        }
        #endregion

        public Vision_Backside(string id, IEngineer engineer, eRemote remote)
        {
            base.InitBase(id, engineer, remote);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            ladsinfos = new List<List<double>>();
            OnChangeState += Backside_OnChangeState;
            InitMemorys();
            testthread = new Thread(new ThreadStart(RunTest));
            testthread.Start();
        }

        public void RunTest()
        {
            //while(true)
            //{
            //    Thread.Sleep(500);
            //    if (!di_test1.p_bIn)
            //        GeneralFunction.WriteINIFile("1","2",di_test1.m_id + "Door Open Detected", @"D:\Share\1.ini");
            //    else if (!di_test2.p_bIn)
            //        GeneralFunction.WriteINIFile("1", "2", di_test2.m_id + "Door Open Detected", @"D:\Share\1.ini");
            //    else if (!di_test3.p_bIn)
            //        GeneralFunction.WriteINIFile("1", "2", di_test3.m_id + "Door Open Detected", @"D:\Share\1.ini");
            //    else if (!di_test4.p_bIn)
            //        GeneralFunction.WriteINIFile("1", "2", di_test4.m_id + "Door Open Detected", @"D:\Share\1.ini");
            //    else
            //        GeneralFunction.WriteINIFile("1", "2", "", @"D:\Share\1.ini");
            //}
        }

        Thread testthread;

        private void Backside_OnChangeState(eState eState)
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
                case eRemoteRun.ServerState:
                    run.m_eState = value;
                    break;
                case eRemoteRun.StateHome:
                    break;
                case eRemoteRun.Reset:
                    break;
                case eRemoteRun.BeforeGet:
                    run.m_nSlotID = value;
                    break;
                case eRemoteRun.BeforePut:
                    run.m_nSlotID = value;
                    break;
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
                if (EQ.IsStop())
                    return "EQ Stop";
            }
            return p_sInfo;
        }

        public class Run_Remote : ModuleRunBase
        {
            Vision_Backside m_module;
            public Run_Remote(Vision_Backside module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public eState m_eState = eState.Init;
            public int m_nSlotID = 0;
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_eState = m_eState;
                run.m_nSlotID = m_nSlotID;
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
                        m_nSlotID = tree.Set(m_nSlotID, m_nSlotID, "SlotID", "WTRChild SlotID", bVisible);
                        break;
                }
            }

            public override string Run()
            {
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.ServerState:
                        m_module.p_eState = m_eState;
                        break;
                    case eRemoteRun.StateHome:
                        return m_module.StateHome();
                    case eRemoteRun.Reset:
                        m_module.Reset();
                        break;
                    case eRemoteRun.BeforeGet:
                        return m_module.BeforeGet(m_nSlotID);
                    case eRemoteRun.BeforePut:
                        return m_module.BeforePut(m_nSlotID);
                }
                return "OK";
            }
        }
        #endregion


        // 210502 이전 
        //ALID m_alid_WaferExist;
        //public void SetAlarm()
        //{
        //    m_alid_WaferExist.Run(true, "Backside Wafer Exist Error");
        //}
        //#region ToolBox
        //MemoryPool m_memoryPool;
        //public override void GetTools(bool bInit)
        //{
        //    p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
        //    m_alid_WaferExist = m_gaf.GetALID(this, "Backside Wafer Exist", "Backside Wafer Exist");
        //    if (bInit)
        //    {
        //        InitMemory();
        //    }
        //}
        //#endregion

        //#region Memory
        //void InitMemory()
        //{
        //}
        //#endregion

        //#region InfoWafer
        //string m_sInfoWafer = "";
        //InfoWafer _infoWafer = null;
        //public InfoWafer p_infoWafer
        //{
        //    get { return _infoWafer; }
        //    set
        //    {
        //        m_sInfoWafer = (value == null) ? "" : value.p_id;
        //        _infoWafer = value;
        //        if (m_reg != null) m_reg.Write("sInfoWafer", m_sInfoWafer);
        //        OnPropertyChanged();
        //    }
        //}

        //Registry m_reg = null;
        //public void ReadInfoWafer_Registry()
        //{
        //    m_reg = new Registry(p_id + ".InfoWafer");
        //    m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
        //    p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        //}
        //#endregion

        //#region InfoWafer UI
        //InfoWaferChild_UI m_ui;
        //void InitInfoWaferUI()
        //{
        //    m_ui = new InfoWaferChild_UI();
        //    m_ui.Init(this);
        //    m_aTool.Add(m_ui);
        //}
        //#endregion

        //#region IWTRChild
        //bool _bLock = false;
        //public bool p_bLock
        //{
        //    get { return _bLock; }
        //    set
        //    {
        //        if (_bLock == value) return;
        //        _bLock = value;
        //    }
        //}

        //bool IsLock()
        //{
        //    for (int n = 0; n < 10; n++)
        //    {
        //        if (p_bLock == false) return false;
        //        Thread.Sleep(100);
        //    }
        //    return true;
        //}

        //public List<string> p_asChildSlot { get { return null; } }

        //public InfoWafer GetInfoWafer(int nID)
        //{
        //    return p_infoWafer;
        //}

        //public void SetInfoWafer(int nID, InfoWafer infoWafer)
        //{
        //    p_infoWafer = infoWafer;
        //}

        //public string IsGetOK(int nID)
        //{
        //    if (p_eState != eState.Ready) return p_id + " eState not Ready";
        //    if (p_infoWafer == null) return p_id + " IsGetOK - InfoWafer not Exist";
        //    return "OK";
        //}

        //public string IsPutOK(InfoWafer infoWafer, int nID)
        //{
        //    if (p_eState != eState.Ready) return p_id + " eState not Ready";
        //    if (p_infoWafer != null) return p_id + " IsPutOK - InfoWafer Exist";
        //    if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false) return p_id + " not Enable Wafer Size";
        //    return "OK";
        //}

        //public int GetTeachWTR(InfoWafer infoWafer = null)
        //{
        //    if (infoWafer == null) infoWafer = p_infoWafer;
        //    return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        //}

        //public string BeforeGet(int nID)
        //{
        //    return "OK";
        //}

        //public string BeforePut(int nID)
        //{
        //    if (p_infoWafer != null) return p_id + " BeforePut : InfoWafer != null";
        //    return "OK";
        //}

        //public string AfterGet(int nID)
        //{
        //    return "OK";
        //}

        //public string AfterPut(int nID)
        //{
        //    return "OK";
        //}

        //enum eCheckWafer
        //{
        //    InfoWafer,
        //    Sensor
        //}
        //eCheckWafer m_eCheckWafer = eCheckWafer.InfoWafer;
        //public bool IsWaferExist(int nID)
        //{
        //    switch (m_eCheckWafer)
        //    {
        //        case eCheckWafer.Sensor: return false; // m_diWaferExist.p_bIn;
        //        default: return (p_infoWafer != null);
        //    }
        //}

        //InfoWafer.WaferSize m_waferSize;
        //public void RunTreeTeach(Tree tree)
        //{
        //    m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        //}
        //#endregion

        //#region Override
        //public override void RunTree(Tree tree)
        //{
        //    base.RunTree(tree);
        //    RunTreeSetup(tree.GetTree("Setup", false));
        //}

        //void RunTreeSetup(Tree tree)
        //{
        //    m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
        //    m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
        //}

        //public override void Reset()
        //{
        //    base.Reset();
        //}
        //#endregion

        //public Vision_Backside(string id, IEngineer engineer)
        //{
        //    m_waferSize = new InfoWafer.WaferSize(id, false, false);
        //    base.InitBase(id, engineer);
        //    InitInfoWaferUI(); 
        //}

        //public override void ThreadStop()
        //{
        //    base.ThreadStop();
        //}

        //#region ModuleRun
        //protected override void InitModuleRuns()
        //{
        //    AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
        //}

        //public class Run_Delay : ModuleRunBase
        //{
        //    Vision_Backside m_module;
        //    public Run_Delay(Vision_Backside module)
        //    {
        //        m_module = module;
        //        InitModuleRun(module);
        //    }

        //    double m_secDelay = 2;
        //    public override ModuleRunBase Clone()
        //    {
        //        Run_Delay run = new Run_Delay(m_module);
        //        run.m_secDelay = m_secDelay;
        //        return run;
        //    }

        //    public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        //    {
        //        m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
        //    }

        //    public override string Run()
        //    {
        //        Thread.Sleep((int)(1000 * m_secDelay));
        //        return "OK";
        //    }
        //}
        //#endregion
    }
}
