using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace Root_Pine2_Vision.Module
{
    public class Vision : ModuleBase
    {
        public enum eVision
        {
            Top3D,
            Top2D,
            Bottom
        }

        #region ToolBox
        Camera_Dalsa m_camera;
        public LightSet m_lightSet;
        public override void GetTools(bool bInit)
        {
            if (p_eRemote == eRemote.Server)
            {
                p_sInfo = m_toolBox.GetCamera(ref m_camera, this, "Camera");
                p_sInfo = m_toolBox.Get(ref m_lightSet, this);
                m_aWorks[eWorks.A].GetTools(m_toolBox, bInit);
                m_aWorks[eWorks.B].GetTools(m_toolBox, bInit);
            }
            m_remote.GetTools(bInit);
        }
        #endregion

        #region Light
        public class LightPower
        {
            public List<double> m_aPower = new List<double>();

            public LightPower Clone()
            {
                LightPower power = new LightPower(m_vision);
                for (int n = 0; n < m_vision.p_lLight; n++) power.m_aPower[n] = m_aPower[n];
                return power;
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                while (m_aPower.Count < m_vision.p_lLight) m_aPower.Add(0);
                for (int n = 0; n < m_aPower.Count; n++)
                {
                    m_aPower[n] = tree.Set(m_aPower[n], m_aPower[n], n.ToString("00"), "Light Power (0 ~ 100)", bVisible);
                }
            }

            Vision m_vision;
            public LightPower(Vision vision)
            {
                m_vision = vision;
            }
        }

        int _lLight = 6;
        public int p_lLight
        {
            get
            {
                if (p_eRemote == eRemote.Client) return _lLight;
                return m_lightSet.m_aLight.Count;
            }
            set
            {
                if (p_eRemote == eRemote.Client) _lLight = value;
            }
        }

        public void RunLight(LightPower lightPower)
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.RunLight, eRemote.Client, lightPower);
            else
            {
                for (int n = 0; n < p_lLight; n++)
                {
                    Light light = m_lightSet.m_aLight[n];
                    if (light.m_light != null) light.m_light.p_fSetPower = lightPower.m_aPower[n];
                }
            }
        }

        public void RunLightOff()
        {
            for (int n = 0; n < p_lLight; n++)
            {
                Light light = m_lightSet.m_aLight[n];
                if (light.m_light != null) light.m_light.p_fSetPower = 0;
            }
        }
        #endregion

        #region Recipe
        string _sRecipe = ""; 
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                if (_sRecipe == value) return;
                _sRecipe = value;
                m_aWorks[eWorks.A].SendRecipe(value);
                m_aWorks[eWorks.B].SendRecipe(value);
            }
        }

        public void SetRecipe(string sRecipe)
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.Recipe, eRemote.Client, sRecipe);
            p_sRecipe = sRecipe; 
        }
        #endregion

        #region GrabData
        public int m_nLine = 30000;
        public class Grab
        {
            public int m_nFovStart = 0;
            public int m_nFovSize = 8000;
            public double[] m_dScale = new double[3] { 1, 1, 1 };
            public double[] m_dShift = new double[3] { 0, 0, 0 };
            public int[] m_yShift = new int[3] { 0, 0, 0 };

            public void SetData(GrabData data)
            {
                data.m_nFovStart = m_nFovStart;
                data.m_nFovSize = m_nFovSize;
                data.m_dScaleR = m_dScale[0];
                data.m_dScaleG = m_dScale[1];
                data.m_dScaleB = m_dScale[2];
                data.m_dShiftR = m_dShift[0];
                data.m_dShiftG = m_dShift[1];
                data.m_dShiftB = m_dShift[2];
                data.m_nYShiftR = m_yShift[0];
                data.m_nYShiftG = m_yShift[1];
                data.m_nYShiftB = m_yShift[2];
            }

            public void RunTree(Tree tree)
            {
                RunTreeFOV(tree.GetTree("FOV"));
                RunTreeColor(tree.GetTree("Red"), 0);
                RunTreeColor(tree.GetTree("Green"), 1);
                RunTreeColor(tree.GetTree("Blue"), 2);
            }

            void RunTreeFOV(Tree tree)
            {
                m_nFovStart = tree.Set(m_nFovStart, m_nFovStart, "Start", "FOV Start (pixel)");
                m_nFovSize = tree.Set(m_nFovSize, m_nFovSize, "Size", "FOV Size (pixel)");
            }

            void RunTreeColor(Tree tree, int i)
            {
                m_dScale[i] = tree.Set(m_dScale[i], m_dScale[i], "Scale", "Color Scale (ratio)");
                m_dShift[i] = tree.Set(m_dShift[i], m_dShift[i], "Shift", "Color Shift");
                m_yShift[i] = tree.Set(m_yShift[i], m_yShift[i], "Y Shift", "Color Shift");
            }
        }
        public Dictionary<eWorks, Grab> m_aGrabData = new Dictionary<eWorks, Grab>();
        void InitGrabData()
        {
            m_aGrabData.Add(eWorks.A, new Grab());
            m_aGrabData.Add(eWorks.B, new Grab());
        }
        #endregion

        #region SnapData
        public class SnapData
        {
            public class Snap
            {
                public RPoint m_dpAxis = new RPoint();

                public enum eDirection
                {
                    Forward,
                    Backward
                }
                public eDirection m_eDirection = eDirection.Forward;
                public enum eEXT
                {
                    EXT1,
                    EXT2,
                }
                public eEXT m_eEXT = eEXT.EXT1;
                public CPoint m_cpMemory = new CPoint();
                public int m_nOverlap = 0;
                public LightPower m_lightPower;

                public Snap Clone()
                {
                    Snap snap = new Snap(m_vision);
                    snap.m_dpAxis = new RPoint(m_dpAxis);
                    snap.m_eDirection = m_eDirection;
                    snap.m_eEXT = m_eEXT;
                    snap.m_cpMemory = new CPoint(m_cpMemory);
                    snap.m_nOverlap = m_nOverlap;
                    snap.m_lightPower = m_lightPower.Clone();
                    return snap;
                }

                public GrabData GetGrabData(eWorks eWorks)
                {
                    GrabData data = new GrabData();
                    data.bInvY = (m_eDirection == eDirection.Backward);
                    data.m_nOverlap = m_nOverlap;
                    data.nScanOffsetY = m_cpMemory.Y;
                    data.ReverseOffsetY = m_cpMemory.Y + m_vision.m_nLine;
                    m_vision.m_aGrabData[eWorks].SetData(data); 
                    return data;
                }

                public CPoint GetMemoryOffset()
                {
                    CPoint cp = new CPoint(m_cpMemory);
                    if (m_eDirection == eDirection.Backward) cp.Y += m_vision.m_nLine;
                    return cp;
                }

                public void RunTree(Tree tree, bool bVisible)
                {
                    RunTreeStage(tree.GetTree("Stage", true, bVisible), bVisible);
                    RunTreeMemory(tree.GetTree("Memory", true, bVisible), bVisible);
                    m_lightPower.RunTree(tree.GetTree("Light", true, bVisible), bVisible);
                }

                void RunTreeStage(Tree tree, bool bVisible)
                {
                    m_dpAxis = tree.Set(m_dpAxis, m_dpAxis, "Offset", "Axis Offset (pulse)", bVisible);
                    m_eDirection = (eDirection)tree.Set(m_eDirection, m_eDirection, "Direction", "Scan Direction", bVisible);
                }

                void RunTreeMemory(Tree tree, bool bVisible)
                {
                    m_eEXT = (eEXT)tree.Set(m_eEXT, m_eEXT, "EXT", "Select EXT", bVisible);
                    m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Offset", "Memory Offset Address (pixel)", bVisible);
                    m_nOverlap = tree.Set(m_nOverlap, m_nOverlap, "Overlap", "Memory Overlap Size (pixel)", bVisible);
                }

                Vision m_vision;
                public Snap(Vision vision)
                {
                    m_vision = vision;
                    m_lightPower = new LightPower(vision);
                }
            }
            public List<Snap> m_aSnap = new List<Snap>();

            public int p_lSnap
            {
                get { return m_aSnap.Count; }
                set
                {
                    if (m_aSnap.Count == value) return;
                    while (m_aSnap.Count > value) m_aSnap.RemoveAt(m_aSnap.Count - 1);
                    while (m_aSnap.Count < value) m_aSnap.Add(new Snap(m_vision));
                }
            }
            public eWorks m_eWorks = eWorks.A;

            public SnapData Clone()
            {
                SnapData snapData = new SnapData(m_vision);
                snapData.m_eWorks = m_eWorks;
                foreach (Snap snap in m_aSnap) snapData.m_aSnap.Add(snap.Clone());
                return snapData; 
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "Works", "Vision eWorks", bVisible);
                p_lSnap = tree.Set(p_lSnap, p_lSnap, "Count", "SnapData Count");
                for (int n = 0; n < m_aSnap.Count; n++) m_aSnap[n].RunTree(tree.GetTree("Snap" + n.ToString("00"), true, bVisible), bVisible);
            }

            Vision m_vision; 
            public SnapData(Vision vision)
            {
                m_vision = vision; 
            }
        }
        #endregion

        #region Works
        public enum eWorks
        {
            A,
            B,
        }
        public Dictionary<eWorks, IWorks> m_aWorks = new Dictionary<eWorks, IWorks>(); 
        void InitVisionWorks()
        {
            switch (m_eVision)
            {
                case eVision.Top2D:
                case eVision.Bottom:
                    m_aWorks.Add(eWorks.A, new Works2D(eWorks.A, this));
                    m_aWorks.Add(eWorks.B, new Works2D(eWorks.B, this));
                    break;
                case eVision.Top3D:
                    m_aWorks.Add(eWorks.A, new Works3D(eWorks.A, this));
                    m_aWorks.Add(eWorks.B, new Works3D(eWorks.B, this));
                    break;
            }
        }

        IWorks GetVisionWorks(string sVisionWorks)
        {
            foreach (IWorks vision in m_aWorks.Values)
            {
                if (vision.p_id == sVisionWorks) return vision; 
            }
            return null; 
        }
        #endregion

        #region RunSnap
        public string StartSnap(SnapData.Snap snapData, eWorks eWorks, string sRecipe, int iSnap)
        {
            Run_Snap run = (Run_Snap)m_runSnap.Clone();
            run.m_eWorks = eWorks; 
            run.m_snapData = snapData;
            run.m_sRecipe = sRecipe;
            run.m_iSnap = iSnap; 
            return StartRun(run); 
        }

        public string RunSnap(SnapData.Snap snapData, eWorks eWorks, string sRecipe, int iSnap)
        {
            MemoryData memory = m_aWorks[eWorks].p_memSnap[(int)snapData.m_eEXT];
            CPoint cpOffset = snapData.GetMemoryOffset();
            GrabData grabData = snapData.GetGrabData(eWorks);
            try
            {
                m_camera.GrabLineScan(memory, cpOffset, m_nLine, grabData);
                Thread.Sleep(200);
                while (m_camera.p_CamInfo.p_eState != eCamState.Ready)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                m_aWorks[eWorks].SendSnapDone(sRecipe, iSnap); 
            }
            finally 
            {
                m_camera.StopGrab(); 
                RunLightOff(); 
            }
            return "OK";
        }
        #endregion

        #region Request
        public string ReqSnap(eWorks eWorks, string sRecipe)
        {
            return "OK"; 
        }
        #endregion

        #region override
        public override void Reset()
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.Reset, eRemote.Client, null);
            else
            {
                m_aWorks[eWorks.A].Reset();
                m_aWorks[eWorks.B].Reset();
                base.Reset();
            }
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.StateHome, eRemote.Client, null);
            else p_eState = eState.Ready;
            return "OK";
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            if (p_eRemote == eRemote.Client)
            {
                p_lLight = tree.GetTree("Light").Set(p_lLight, p_lLight, "Channel", "Light Channel Count");
            }
            else
            {
                m_nLine = tree.GetTree("Camera").Set(m_nLine, m_nLine, "Line", "Memory Snap Lines (pixel)");
                m_aWorks[eWorks.A].RunTree(tree.GetTree("Works " + m_aWorks[eWorks.A].p_id));
                m_aWorks[eWorks.B].RunTree(tree.GetTree("Works " + m_aWorks[eWorks.B].p_id));
                m_aGrabData[eWorks.A].RunTree(tree.GetTree("GrabData A"));
                m_aGrabData[eWorks.B].RunTree(tree.GetTree("GrabData B"));
            }
        }
        #endregion

        #region Vision_Snap_UI
        Vision_Snap_UI m_ui;
        void InitVision_Snap_UI()
        {
            m_ui = new Vision_Snap_UI();
            m_ui.Init(this);
            m_aTool.Add(m_ui);
        }
        #endregion

        public eVision m_eVision = eVision.Top2D; 
        public Vision(eVision eVision, IEngineer engineer, eRemote eRemote)
        {
            m_eVision = eVision;
            InitVisionWorks();
            InitBase("Vision " + eVision.ToString(), engineer, eRemote);
        }

        public Vision(string id, IEngineer engineer, eRemote eRemote)
        {
            InitGrabData();
            InitVisionWorks();
            InitBase(id, engineer, eRemote);
            InitVision_Snap_UI();
        }

        public override void ThreadStop()
        {
            foreach (IWorks visionWorks in m_aWorks.Values) visionWorks.ThreadStop(); 
            base.ThreadStop();
        }

        #region RemoteRun
        public enum eRemoteRun
        {
            StateHome,
            Reset,
            RunLight,
            Recipe
        }

        Run_Remote GetRemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = new Run_Remote(this);
            run.m_eRemoteRun = eRemoteRun;
            run.m_eRemote = eRemote;
            switch (eRemoteRun)
            {
                case eRemoteRun.StateHome: break;
                case eRemoteRun.Reset: break;
                case eRemoteRun.RunLight: run.m_lightPower = value; break;
                case eRemoteRun.Recipe: run.m_sRecipe = value; break; 
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
                m_lightPower = new LightPower(m_module); 
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public LightPower m_lightPower;
            public string m_sRecipe = ""; 
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_lightPower = m_lightPower.Clone();
                run.m_sRecipe = m_sRecipe; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eRemoteRun = (eRemoteRun)tree.Set(m_eRemoteRun, m_eRemoteRun, "RemoteRun", "Select Remote Run", bVisible);
                m_eRemote = (eRemote)tree.Set(m_eRemote, m_eRemote, "Remote", "Remote", false);
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.RunLight: m_lightPower.RunTree(tree.GetTree("Light Power", true, bVisible), bVisible); break;
                    case eRemoteRun.Recipe: m_sRecipe = tree.Set(m_sRecipe, m_sRecipe, "Recipe", "Recipe", bVisible); break; 
                    default: break; 
                }
            }

            public override string Run()
            {
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.StateHome: return m_module.StateHome();
                    case eRemoteRun.Reset: m_module.Reset(); break;
                    case eRemoteRun.RunLight: m_module.RunLight(m_lightPower); break;
                    case eRemoteRun.Recipe: m_module.SetRecipe(m_sRecipe); break; 
                }
                return "OK";
            }
        }
        #endregion

        #region ModuleRun
        ModuleRunBase m_runSnap; 
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Remote(this), true, "Remote Run");
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            m_runSnap = AddModuleRunList(new Run_Snap(this), true, "Snap");
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

        public class Run_Snap : ModuleRunBase
        {
            Vision m_module;
            public Run_Snap(Vision module)
            {
                m_module = module;
                m_snapData = new SnapData.Snap(module); 
                InitModuleRun(module);
            }

            public eWorks m_eWorks = eWorks.A;
            public string m_sRecipe = "";
            public int m_iSnap = 0; 
            public SnapData.Snap m_snapData; 
            public override ModuleRunBase Clone()
            {
                Run_Snap run = new Run_Snap(m_module);
                run.m_eWorks = m_eWorks; 
                run.m_snapData = m_snapData.Clone();
                run.m_sRecipe = m_sRecipe;
                run.m_iSnap = m_iSnap; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "Works", "Vision eWorks", bVisible);
                m_sRecipe = tree.Set(m_sRecipe, m_sRecipe, "Recipe", "Recipe", bVisible);
                m_iSnap = tree.Set(m_iSnap, m_iSnap, "Snap Index", "Snap Index", bVisible); 
                m_snapData.RunTree(tree, bVisible); 
            }

            public override string Run()
            {
                return m_module.RunSnap(m_snapData, m_eWorks, m_sRecipe, m_iSnap);
            }
        }
        #endregion

    }
}
