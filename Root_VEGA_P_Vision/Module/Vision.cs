using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Camera.Matrox;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Root_VEGA_P_Vision.Module
{
    public class Vision : ModuleBase, IRTRChild
    {
        #region ToolBox
        LightSet lightSet;
        MemoryPool memoryPool;
        MemoryGroup memoryGroup;
        public int sideGrabCnt = 0;
        public MainOptic m_mainOptic;
        public SideOptic m_sideOptic;

        public enum eParts
        {
            EIP_Cover, EIP_Plate
        }
        public enum eUpDown
        {
            Front, Back
        }
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref memoryPool, this, "Memory", 1);
            p_sInfo = m_toolBox.Get(ref lightSet, this);
            m_stage.GetTools(m_toolBox, bInit); 
            m_mainOptic.GetTools(m_toolBox, bInit); //TDI, Stain, ZStack
            m_sideOptic.GetTools(m_toolBox, bInit);  //Side
            m_remote.GetTools(bInit);
        }
        #endregion

        #region[Move]
        public string Move(Axis axis, double pos, bool bWait = true)
        {
            string sRun = axis.StartMove(pos);
            if (sRun.Equals("OK")) return sRun;
            return bWait ? axis.WaitReady() : "OK";
        }
        public string Move(Axis axis, double pos,double v,bool bWait = true)
        {
            string sRun = axis.StartMove(pos,v);
            if (sRun.Equals("OK")) return sRun;
            return bWait ? axis.WaitReady() : "OK";
        }
        public string MoveXY(RPoint posmm, bool bWait = true)
        {
            string sRun = m_stage.m_axisXY.StartMove(new RPoint(posmm));
            if (sRun.Equals("OK")) return sRun;
            return bWait ? m_stage.m_axisXY.WaitReady() : "OK";
        }
        #endregion

        #region Stage
        public class Stage : NotifyProperty
        {
            public AxisXY m_axisXY;
            public Axis m_axisR;
            public enum eLoading
            {
                StageLoadX,StageLoadY
            }
            DIO_I[] m_diStageLoad = new DIO_I[2] { null, null };
            DIO_I[] m_diStageVac = new DIO_I[2] { null, null };
            DIO_O[] m_doStageVac = new DIO_O[2] { null, null };
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.GetAxis(ref m_axisXY, m_vision, "Stage");
                m_vision.p_sInfo = toolBox.GetAxis(ref m_axisR, m_vision, "Stage Rotate");
                m_vision.p_sInfo = toolBox.GetDIO(ref m_diStageLoad[0], m_vision, "Stage Load X");
                m_vision.p_sInfo = toolBox.GetDIO(ref m_diStageLoad[1], m_vision, "Stage Load Y");
                m_vision.p_sInfo = toolBox.GetDIO(ref m_diStageVac[0], m_vision, "Stage Vac Cover Check");
                m_vision.p_sInfo = toolBox.GetDIO(ref m_diStageVac[1], m_vision, "Stage Vac Base Plate Check"); 
                m_vision.p_sInfo = toolBox.GetDIO(ref m_doStageVac[0], m_vision, "Stage Vac Cover");
                m_vision.p_sInfo = toolBox.GetDIO(ref m_doStageVac[1], m_vision, "Stage Vac Base Plate");
                if (bInit)
                {
                    m_axisXY.p_axisX.AddPos(Enum.GetName(typeof(eLoading), eLoading.StageLoadX));
                    m_axisXY.p_axisY.AddPos(Enum.GetName(typeof(eLoading), eLoading.StageLoadY));
                }
            }

            public bool IsCoverVac()
            {
                if (m_diStageVac[0] == null) return false;

                return m_diStageVac[0].p_bIn;
            }
            public bool IsPlateVac()
            {
                if (m_diStageVac[1] == null) return false;

                return m_diStageVac[1].p_bIn;
            }
            public string RunCoverStageVac(bool bCover)
            {
                m_doStageVac[0].Write(bCover);
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < 3000)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                    if (bCover == IsCoverVac()) return "OK";
                }
                return "Run Cover Stage Vac Timeout";
            }
            public string RunPlateStageVac(bool bPlate)
            {
                m_doStageVac[1].Write(bPlate);
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < 3000)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                    if (bPlate == IsPlateVac()) return "OK";
                }
                return "Run Plate Stage Vac Timeout";
            }
            public bool IsLoad()
            {
                if (m_diStageLoad[0] == null) return false;
                if (m_diStageLoad[1] == null) return false;
                return m_diStageLoad[0].p_bIn && m_diStageLoad[1].p_bIn; 
            }

            double _pulsePerRound = 360000;
            public double p_pulsePerRound
            {
                get { return _pulsePerRound; }
                set
                {
                    _pulsePerRound = value;
                    OnPropertyChanged(); 
                }
            }

            public string Rotate(double degree, bool bWait = true)
            {
                double rotate = m_axisR.p_posActual + degree * p_pulsePerRound / 360;
                string sRun = m_axisR.StartMove(rotate);
                if (sRun != "OK") return sRun;
                return bWait ? m_axisR.WaitReady() : "OK";
            }

            double m_pulsePermm = 10000;
            public double m_thetaOffset = 0;
            public void RunTree(Tree tree)
            {
                m_pulsePermm = tree.Set(m_pulsePermm, m_pulsePermm, "Pulse / mm", "Stage XY Pulse per 1mm (pulse)"); 
                p_pulsePerRound = tree.Set(p_pulsePerRound, p_pulsePerRound, "Pulse / Round", "Stage Rotate Pulse per Round (pulse)");
                m_thetaOffset = tree.Set(m_thetaOffset, m_thetaOffset, "Theta Offset / Pulse", "Stage Theta Offset (pulse)");
            }

            Vision m_vision; 
            public Stage(Vision vision)
            {
                m_vision = vision; 
            }
        }
        public Stage m_stage;
        #endregion

        #region MainOptic
        public class MainOptic : NotifyProperty
        {
            Vision m_vision;

            public Axis m_axisZ;
            public Camera_Dalsa camTDI;
            public Camera_Basler camStain;
            public Camera_Matrox camZStack;
            public double m_pulsePermm;

            public enum eInsp
            {
                Stain,Main,Stack
            }

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.GetAxis(ref m_axisZ, m_vision, "Main Optic AxisZ");
                m_vision.p_sInfo = toolBox.GetCamera(ref camTDI, m_vision, "TDI Cam");
                m_vision.p_sInfo = toolBox.GetCamera(ref camStain, m_vision, "Stain Cam");
                m_vision.p_sInfo = toolBox.GetCamera(ref camZStack, m_vision, "Z-Stacking Cam");

                if (bInit)
                {

                }
            }

            public void InitMemorys()
            {
                foreach(var v in Enum.GetValues(typeof(eParts)))
                    foreach(var v2 in Enum.GetValues(typeof(eInsp)))
                        foreach(var v3 in Enum.GetValues(typeof(eUpDown)))
                            m_vision.memoryGroup.CreateMemory(v.ToString() + "." + v2.ToString()+"."+v3.ToString(), 1, 1, 1000,1000);
            }
            public MemoryData GetMemoryData(InfoPod.ePod parts,eInsp insp,eUpDown updown)
            {
                return m_vision.memoryPool.GetMemory(m_vision.p_id, parts.ToString()+"."+insp.ToString()+"."+updown.ToString());
            }

            public void RunTree(Tree tree)
            {
                m_pulsePermm = tree.Set(m_pulsePermm, m_pulsePermm, "Pulse / mm", "Stage XY Pulse per 1mm (pulse)");
            }

            public MainOptic(Vision vision)
            {
                m_vision = vision;
                m_pulsePermm = 10000;
            }
            public string CameraInit()
            {
                if (camStain == null) return "Camera Stain is NULL";
                camStain.Connect();

                if (camTDI == null) return "Main Camera is NULL";
                if (!camTDI.p_CamInfo.p_eState.Equals(RootTools.Camera.Dalsa.eCamState.Init)) return "Main Camera state is not init";
                camTDI.Connect();

                if (camZStack == null) return "ZStacking Camera is NULL";
                if (!camZStack.p_CamInfo.p_eState.Equals(RootTools.Camera.Matrox.eCamState.Init)) return "Zstacking Camera state is not init";
                camZStack.Connect();

                return "OK";
            }
        }
        #endregion

        #region SideOptic
        public class SideOptic : NotifyProperty
        {
            Vision m_vision;
            public Axis axisZ;
            public Camera_Basler camSide;
            public double m_pulsePermm;

            public enum eSide
            {
                Left,Bottom,Right,Top,All
            }

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.GetCamera(ref camSide, m_vision, "Side Cam");
                m_vision.p_sInfo = toolBox.GetAxis(ref axisZ, m_vision, "Side Optic AxisZ");
                if (bInit)
                {

                }
            }

            public void InitMemorys()
            {
                foreach (var v in Enum.GetValues(typeof(eParts)))
                    foreach (var v2 in Enum.GetValues(typeof(eSide)))
                        m_vision.memoryGroup.CreateMemory(v.ToString() + "." + v2.ToString(), 1, 1, 1000, 1000);
            }

            public MemoryData GetMemoryData(InfoPod.ePod parts,eSide side)
            {
                return m_vision.memoryPool.GetMemory(m_vision.p_id, parts.ToString()+"."+side.ToString());
            }
            public MemoryData GetMemoryData(string str)
            {
                return m_vision.memoryPool.GetMemory(m_vision.p_id, str);
            }

            public void RunTree(Tree tree)
            {
                m_pulsePermm = tree.Set(m_pulsePermm, m_pulsePermm, "Pulse / mm", "Stage XY Pulse per 1mm (pulse)");
            }

            public SideOptic(Vision vision)
            {
                m_vision = vision;
                m_pulsePermm = 10000;
            }

            public string CameraInit()
            {
                if (camSide == null) return "Camera Stain is NULL";
                camSide.Connect();

                return "OK";
            }
        }
        #endregion

        #region InfoPod
        InfoPod _infoPod = null;
        public InfoPod p_infoPod
        {
            get { return _infoPod; }
            set
            {
                int nPod = (value != null) ? (int)value.p_ePod : -1; 
                _infoPod = value;
                m_reg.Write("InfoPod", nPod);
                value?.WriteReg(); 
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadPod_Registry()
        {
            m_reg = new Registry("InfoPod");
            int nPod = m_reg.Read(p_id, -1);
            if (nPod < 0) return;  
            p_infoPod = new InfoPod((InfoPod.ePod)nPod);
            p_infoPod.ReadReg();
        }
        #endregion

        #region IRTRChild
        public bool p_bLock { get; set; }

        public string IsGetOK()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready"; 
            return (p_infoPod != null) ? "OK" : p_id + " IsGetOK - Pod not Exist";
        }

        public string IsPutOK(InfoPod infoPod)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            switch (infoPod.p_ePod)
            {
                case InfoPod.ePod.EOP_Dome:
                case InfoPod.ePod.EOP_Door:
                    return p_id + " Invalid Pod Type"; 
            }
            return (p_infoPod == null) ? "OK" : p_id + " IsPutOK - Pod Exist";
        }

        public string BeforeGet()
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforeGet, eRemote.Client, null);
            else
            {
                if (p_infoPod == null)
                    return "Pod Doesn't exist";
                if (Run(m_stage.m_axisXY.p_axisX.StartMove(Enum.GetName(typeof(Stage.eLoading), Stage.eLoading.StageLoadX))))
                    return p_sInfo;
                if (Run(m_stage.m_axisXY.p_axisY.StartMove(Enum.GetName(typeof(Stage.eLoading), Stage.eLoading.StageLoadY))))
                    return p_sInfo;

                if (Run(m_stage.m_axisXY.p_axisX.WaitReady()))
                    return p_sInfo;
                if (Run(m_stage.m_axisXY.p_axisY.WaitReady()))
                    return p_sInfo;

                switch (p_infoPod.p_ePod)
                {
                    case InfoPod.ePod.EIP_Cover:
                        if (Run(m_stage.RunCoverStageVac(false)))
                            return p_sInfo;
                        break;
                    case InfoPod.ePod.EIP_Plate:
                        if (Run(m_stage.RunPlateStageVac(false)))
                            return p_sInfo;
                        break;
                }

                p_infoPod = null;
                return "OK";
            }
        }

        public string BeforePut(InfoPod infoPod)
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforePut, eRemote.Client, infoPod);
            else
            {
                if (Run(m_stage.m_axisXY.p_axisX.StartMove(Enum.GetName(typeof(Stage.eLoading), Stage.eLoading.StageLoadX))))
                    return p_sInfo;
                if (Run(m_stage.m_axisXY.p_axisY.StartMove(Enum.GetName(typeof(Stage.eLoading), Stage.eLoading.StageLoadY))))
                    return p_sInfo;

                if (Run(m_stage.m_axisXY.p_axisX.WaitReady()))
                    return p_sInfo;
                if (Run(m_stage.m_axisXY.p_axisY.WaitReady()))
                    return p_sInfo;

                switch (infoPod.p_ePod)
                {
                    case InfoPod.ePod.EIP_Cover:
                        if (Run(m_stage.RunCoverStageVac(false)))
                            return p_sInfo;
                        break;
                    case InfoPod.ePod.EIP_Plate:
                        if (Run(m_stage.RunPlateStageVac(false)))
                            return p_sInfo;
                        break;
                }

                p_infoPod = infoPod;
                return "OK";
            }
        }

        public string AfterGet()
        {
            // ??
            return "OK";
        }

        public string AfterPut()
        {
            // ??
            return "OK";
        }

        public bool IsPodExist(InfoPod.ePod ePod)
        {
            return (p_infoPod != null);
        }

        public bool IsEnableRecovery()
        {
            return p_infoPod != null;
        }
        #endregion

        #region Teach RTR
        Holder.TeachRTR m_teach; 
        public int GetTeachRTR(InfoPod infoPod)
        {
            return m_teach.GetTeach(infoPod);
        }

        public void RunTreeTeach(Tree tree)
        {
            m_teach.RunTree(tree.GetTree(p_id));
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
            memoryGroup = memoryPool.GetGroup(p_id);
            memoryGroup.CreateMemory(App.mMaskLayer, 1, 1, 1000, 1000);
            m_mainOptic.InitMemorys();
            m_sideOptic.InitMemorys(); 
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.StateHome, eRemote.Client, null);
            else
            {
                Thread.Sleep(100);

                m_mainOptic.CameraInit();
                m_sideOptic.CameraInit();

                p_sInfo = m_sideOptic.axisZ.StartHome();
                p_sInfo = m_mainOptic.m_axisZ.StartHome();

                m_sideOptic.axisZ.WaitReady();
                m_mainOptic.m_axisZ.WaitReady();

                p_sInfo = base.StateHome();
                m_stage.m_axisR.StartMove(m_stage.m_thetaOffset);
                m_stage.m_axisR.WaitReady();
                p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;

                return "OK";
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_stage.RunTree(tree.GetTree("Stage"));
            m_mainOptic.RunTree(tree.GetTree("Main Optic"));
            m_sideOptic.RunTree(tree.GetTree("Side Optic"));
            RunTreeGrabMode(tree.GetTree("Grab Mode"));
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
                foreach (GrabMode grabMode in m_aGrabMode) 
                    asGrabMode.Add(grabMode.p_sName);
                return asGrabMode;
            }
        }

        public GrabMode GetGrabMode(string sGrabMode)
        {
            foreach (GrabMode grabMode in m_aGrabMode)
                if (sGrabMode == grabMode.p_sName) return grabMode;
          
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
            foreach (GrabMode grabMode in m_aGrabMode)
                grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
        }

        #endregion

        public Vision(string id, IEngineer engineer, eRemote eRemote)
        {
            m_reg = new Registry(id + "_InfoPod");
            m_teach = new Holder.TeachRTR(); 
            m_stage = new Stage(this);
            m_mainOptic = new MainOptic(this);
            m_sideOptic = new SideOptic(this); 
            InitBase(id, engineer, eRemote);
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
            base.ThreadStop();
        }

        #region Test Result
        public class TestResult
        {
            public int m_nResult = 0;
            public string m_sResult = "OK";

            public void RunTree(Tree tree, bool bVisible)
            {
                m_nResult = tree.Set(m_nResult, m_nResult, "Int", "Int Result", bVisible);
                m_sResult = tree.Set(m_sResult, m_sResult, "String", "String Result", bVisible);
            }
        }
        TestResult _testResult = new TestResult();
        public TestResult p_testResult
        {
            get { return _testResult; }
            set
            {
                _testResult = value;
                if (p_eRemote == eRemote.Server) RemoteRun(eRemoteRun.TestResult, eRemote.Server, value);
            }
        }
        #endregion

        #region RemoteRun
        public enum eRemoteRun
        {
            ServerState,
            StateHome,
            Reset,
            BeforeGet,
            BeforePut,
            TestResult,
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
                case eRemoteRun.BeforeGet: break;
                case eRemoteRun.BeforePut: run.m_infoPod = value; break;
                case eRemoteRun.TestResult: run.m_testResult = value; break;
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
            public InfoPod m_infoPod = new InfoPod(InfoPod.ePod.EIP_Cover);
            public TestResult m_testResult = new TestResult();
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_eState = m_eState;
                run.m_infoPod = m_infoPod;
                run.m_testResult = m_testResult;
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
                    case eRemoteRun.BeforePut:
                        m_infoPod.RunTree(tree.GetTree("InfoPod", true, bVisible), bVisible);
                        break;
                    case eRemoteRun.TestResult:
                        m_testResult.RunTree(tree.GetTree("TestResult", true, bVisible), bVisible);
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
                    case eRemoteRun.BeforeGet: return m_module.BeforeGet();
                    case eRemoteRun.BeforePut: return m_module.BeforePut(m_infoPod);
                    case eRemoteRun.TestResult: m_module.p_testResult = m_testResult; break;
                }
                return "OK";
            }
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Rotate(this), true, App.mRotate);
            AddModuleRunList(new Run_MainGrab(this), true, App.mMainGrab);
            AddModuleRunList(new Run_SideGrab(this), true, App.mSideGrab);
            AddModuleRunList(new Run_StainGrab(this), true, App.mStainGrab);
            AddModuleRunList(new Run_ZStack(this), true, App.mZStack);
            AddModuleRunList(new Run_Remote(this), false, "Remote Run");
            AddModuleRunList(new Run_Align(this), false, App.mVisionAlign);
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Inspection(this), true, App.mInspection);
        }        
            
        public class Run_Delay : ModuleRunBase
        {
            Vision m_module;
            public Run_Delay(Vision module)
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
                Thread.Sleep((int)(1000 * m_secDelay / 2));
                return "OK";
            }
        }
        #endregion
    }
}
