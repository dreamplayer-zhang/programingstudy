using RootTools;
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
            if (p_eRemote == eRemote.Client) return;
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
            AddModuleRunList(new Run_Grab(this), true, "Time Delay");
            AddModuleRunList(new Run_StartVision(this), true, "Start Vision");
            AddModuleRunList(new Run_KillVision(this), true, "Start Vision");
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


        System.Diagnostics.Process VisionWorks2_A = null;
        System.Diagnostics.Process VisionWorks2_B = null;

        public class Run_StartVision : ModuleRunBase
        {
            Vision m_module;

            public Run_StartVision(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            string m_strVisionWorks2Path_A = "C:\\WisVision\\VisionWorks2.exe";
            string m_strVisionWorks2Path_B = "C:\\WisVision\\VisionWorks2.exe";
            public override ModuleRunBase Clone()
            {
                Run_StartVision run = new Run_StartVision(m_module);
                run.m_strVisionWorks2Path_A = m_strVisionWorks2Path_A;
                run.m_strVisionWorks2Path_B = m_strVisionWorks2Path_B;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_strVisionWorks2Path_A = tree.Set(m_strVisionWorks2Path_A, m_strVisionWorks2Path_A, "VisionWorks2_A Path", "VisionWorks2_A Path(Full Path)", bVisible);
                m_strVisionWorks2Path_B = tree.Set(m_strVisionWorks2Path_B, m_strVisionWorks2Path_B, "VisionWorks2_B Path", "VisionWorks2_B Path(Full Path)", bVisible);
            }

            public override string Run()
            {
                // 1. VisionWorks2_A
                if (!System.IO.File.Exists(m_strVisionWorks2Path_A))
                {
                    System.Windows.MessageBox.Show("VisionWorks2_A 파일이 해당 경로에 없습니다.");
                }
                else
                {
                    if (m_module.VisionWorks2_A == null)
                    {
                        m_module.VisionWorks2_A = System.Diagnostics.Process.Start(m_strVisionWorks2Path_A);
                        System.Windows.MessageBox.Show("VisionWorks2_A 실행 - Process ID : " + m_module.VisionWorks2_A.Id);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("VisionWorks2_A 이미 실행 중 - Process ID : " + m_module.VisionWorks2_A.Id);
                    }
                }

                // 2. VisionWorks2_B
                if (!System.IO.File.Exists(m_strVisionWorks2Path_B))
                {
                    System.Windows.MessageBox.Show("VisionWorks2_B 파일이 해당 경로에 없습니다.");
                }
                else
                {
                    if (m_module.VisionWorks2_B == null)
                    {
                        m_module.VisionWorks2_B = System.Diagnostics.Process.Start(m_strVisionWorks2Path_B);
                        System.Windows.MessageBox.Show("VisionWorks2_B 실행 - Process ID : " + m_module.VisionWorks2_B.Id);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("VisionWorks2_B 이미 실행 중 - Process ID : " + m_module.VisionWorks2_B.Id);
                    }
                }

                return "OK";
            }
        }

        public class Run_KillVision : ModuleRunBase
        {
            Vision m_module;

            public Run_KillVision(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bKillVisionWorks2_A = true;
            bool m_bKillVisionWorks2_B = true;
            public override ModuleRunBase Clone()
            {
                Run_KillVision run = new Run_KillVision(m_module);
                run.m_bKillVisionWorks2_A = m_bKillVisionWorks2_A;
                run.m_bKillVisionWorks2_B = m_bKillVisionWorks2_B;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bKillVisionWorks2_A = tree.Set(m_bKillVisionWorks2_A, m_bKillVisionWorks2_A, "Kill VisionWorks2_A", "Kill VisionWorks2_A", bVisible);
                m_bKillVisionWorks2_B = tree.Set(m_bKillVisionWorks2_B, m_bKillVisionWorks2_B, "Kill VisionWorks2_B", "Kill VisionWorks2_B", bVisible);
            }

            public override string Run()
            {
                // 1. VisionWorks2_A
                if(m_bKillVisionWorks2_A)
                {
                    if(m_module.VisionWorks2_A != null)
                    {
                        m_module.VisionWorks2_A.Kill();
                        m_module.VisionWorks2_A = null;
                        System.Windows.MessageBox.Show("VisionWorks2_A 프로그램 종료");
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("종료할 VisionWorks2_A 프로그램이 없음");
                    }
                }

                // 2. VisionWorks2_B
                if (m_bKillVisionWorks2_B)
                {
                    if (m_module.VisionWorks2_B != null)
                    {
                        m_module.VisionWorks2_B.Kill();
                        m_module.VisionWorks2_B = null;
                        System.Windows.MessageBox.Show("VisionWorks2_B 프로그램 종료");
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("종료할 VisionWorks2_B 프로그램이 없음");
                    }
                }

                return "OK";
            }
        }

        #endregion

    }
}
