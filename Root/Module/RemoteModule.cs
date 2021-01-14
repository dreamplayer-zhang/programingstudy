using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Text;
using System.Threading;

namespace Root.Module
{
    public class RemoteModule : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_remote.GetTools(bInit);
        }
        #endregion

        public RemoteModule(string id, IEngineer engineer, eRemote eRemote)
        {
            InitBase(id, engineer, eRemote);
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
        }

        public override void Reset()
        {
            base.Reset();
            //
        }

        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Run(this), true, "Desc Run");
        }

        public class Run_Run : ModuleRunBase
        {
            RemoteModule m_module;
            public Run_Run(RemoteModule module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_nTry = 3;
            string m_sTest = "Test";
            CPoint m_cp = new CPoint(); 
            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                run.m_nTry = m_nTry;
                run.m_sTest = m_sTest;
                run.m_cp = new CPoint(m_cp); 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nTry = tree.Set(m_nTry, 3, "Try", "Try Count", bVisible);
                m_sTest = tree.Set(m_sTest, m_sTest, "Test", "Test", bVisible);
                m_cp = tree.Set(m_cp, m_cp, "Pos", "Pos", bVisible); 
            }

            public override string Run()
            {
                Thread.Sleep(1000 * m_nTry); 
                return "OK";
            }
        }
    }
}
