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
        public string SetLight()
        {
            List<double> aPower = new List<double>();
            return SetLight(aPower); 
        }

        public string SetLight(List<double> aPower)
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.SetLight, eRemote.Client, aPower);
            else
            {
                while (aPower.Count < m_lightSet.m_aLight.Count) aPower.Add(0);
                for (int n = 0; n < aPower.Count; n++)
                {
                    LightBase light = m_lightSet.m_aLight[n].m_light;
                    if (light != null) light.p_fSetPower = aPower[n];
                }
            }
            return "OK";
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
            SetLight,
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
                case eRemoteRun.SetLight: run.m_aPower = value; break; 
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
            public List<double> m_aPower = new List<double>(); 
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                foreach (double fPower in m_aPower) run.m_aPower.Add(fPower); 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eRemoteRun = (eRemoteRun)tree.Set(m_eRemoteRun, m_eRemoteRun, "RemoteRun", "Select Remote Run", bVisible);
                m_eRemote = (eRemote)tree.Set(m_eRemote, m_eRemote, "Remote", "Remote", false);
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.SetLight: break;
                    default: break; 
                }
            }

            public override string Run()
            {
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.StateHome: return m_module.StateHome();
                    case eRemoteRun.Reset: m_module.Reset(); break;
                    case eRemoteRun.SetLight: m_module.SetLight(m_aPower); break;
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
            AddModuleRunList(new Run_Grab(this), true, "Time Delay");
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

        public class Run_Grab : ModuleRunBase
        {
            Vision m_module;
            public Run_Grab(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            enum eBoat
            {
                Boat1,
                Boat2
            }
            eBoat m_eBoat = eBoat.Boat1; 
            public override ModuleRunBase Clone()
            {
                Run_Grab run = new Run_Grab(m_module);
                run.m_eBoat = m_eBoat;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eBoat = (eBoat)tree.Set(m_eBoat, m_eBoat, "Boat", "Boat ID", bVisible);
            }

            public override string Run()
            {
                //forget
                return "OK";
            }
        }
        #endregion

    }
}
