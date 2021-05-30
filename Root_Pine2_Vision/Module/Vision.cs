using RootTools;
using RootTools.Camera.Dalsa;
using RootTools.Light;
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
            public bool m_bOn = false; 
            public List<double> m_aPower = new List<double>(); 

            public void RunTree(Tree tree, bool bVisible)
            {
                m_bOn = tree.Set(m_bOn, m_bOn, "Light On", "Light On", bVisible);
                tree = tree.GetTree("Power");
                for (int n = 0; n < m_aPower.Count; n++)
                {
                    m_aPower[n] = tree.Set(m_aPower[n], m_aPower[n], n.ToString("00"), "Light Power (0 ~ 100)", bVisible);
                }
            }

            public LightPower(int nPower)
            {
                while (m_aPower.Count < nPower) m_aPower.Add(0);
            }
        }

        public int p_nLight
        {
            get
            {
                switch (m_eVision)
                {
                    case eVision.Top3D: return 6;
                    default: return 6; 
                }
            }
        }

        public void RunLight(LightPower lightPower)
        {
            for (int n = 0; n < lightPower.m_aPower.Count; n++)
            {
                Light light = m_lightSet.m_aLight[n];
                if (light.m_light != null) light.m_light.p_fSetPower = lightPower.m_bOn ? lightPower.m_aPower[n] : 0;
            }
        }
        #endregion

        #region ScanData
        public class ScanData
        {
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

            public void RunTree(Tree tree, bool bVisible)
            {
                m_eDirection = (eDirection)tree.Set(m_eDirection, m_eDirection, "Direction", "Scan Direction", bVisible);
                m_eEXT = (eEXT)tree.Set(m_eEXT, m_eEXT, "EXT", "Select EXT", bVisible);
                m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Memory Offset", "Memory Offset Address (pixel)", bVisible);
                m_nOverlap = tree.Set(m_nOverlap, m_nOverlap, "Mmeory Overlap", "Memory Overlap Size (pixel)", bVisible);
                m_lightPower.RunTree(tree.GetTree("Light", bVisible), bVisible); 
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

        #region override
        public override void Reset()
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.Reset, eRemote.Client, null);
            else
            {
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
            if (p_eRemote == eRemote.Client) return;
            m_aWorks[eWorks.A].RunTree(tree.GetTree(m_aWorks[eWorks.A].p_id));
            m_aWorks[eWorks.B].RunTree(tree.GetTree(m_aWorks[eWorks.B].p_id));
        }
        #endregion

        public eVision m_eVision = eVision.Top3D; 
        public Vision(eVision eVision, IEngineer engineer, eRemote eRemote)
        {
            m_eVision = eVision; 
            InitVisionWorks(); 
            InitBase("Vision " + eVision.ToString(), engineer, eRemote);
        }

        public Vision(string id, IEngineer engineer, eRemote eRemote)
        {
            InitBase(id, engineer, eRemote);
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
                m_lightPower = new LightPower(m_module.p_nLight); 
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public LightPower m_lightPower; 
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eRemoteRun = (eRemoteRun)tree.Set(m_eRemoteRun, m_eRemoteRun, "RemoteRun", "Select Remote Run", bVisible);
                m_eRemote = (eRemote)tree.Set(m_eRemote, m_eRemote, "Remote", "Remote", false);
                switch (m_eRemoteRun)
                {
                    default: break; 
                }
            }

            public override string Run()
            {
                switch (m_eRemoteRun)
                {
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
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Light(this), true, "Change Light Power");
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

        public class Run_Light : ModuleRunBase
        {
            Vision m_module;
            public Run_Light(Vision module)
            {
                m_module = module;
                m_lightPower = new LightPower(module.p_nLight); 
                InitModuleRun(module);
            }

            LightPower m_lightPower; 
            public override ModuleRunBase Clone()
            {
                Run_Light run = new Run_Light(m_module);
                run.m_lightPower = m_lightPower; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_lightPower.RunTree(tree, bVisible); 
            }

            public override string Run()
            {
                m_module.RunLight(m_lightPower); 
                return "OK";
            }
        }
        #endregion

    }
}
