﻿using RootTools;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_Pine2_Vision.Module
{
    public class Vision : ModuleBase
    {
        #region ToolBox
        LightSet lightSet;
        MemoryPool memoryPool;
        MemoryGroup memoryGroup;
        public override void GetTools(bool bInit)
        {
            if (p_eRemote == eRemote.Server)
            {
                p_sInfo = m_toolBox.Get(ref memoryPool, this, "Memory", 1);
                p_sInfo = m_toolBox.Get(ref lightSet, this);
            }
            m_remote.GetTools(bInit);
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
        }
        #endregion

        public Vision(string id, IEngineer engineer, eRemote eRemote)
        {
            InitBase(id, engineer, eRemote);
        }

        public override void ThreadStop()
        {
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
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
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
