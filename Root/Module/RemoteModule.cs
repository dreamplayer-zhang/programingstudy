using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.IO;
using System.Text;

namespace Root.Module
{
    public class RemoteModule : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {

        }
        #endregion

        #region RemoteRun
        public class RemoteRun
        {
            MemoryStream m_memoryStream = new MemoryStream();
            public void Init(bool bSend)
            {
                if (bSend)
                {
                    m_memoryStream = new MemoryStream();
                    m_treeRoot.m_job = new Job(m_memoryStream, true, m_log);
                    m_treeRoot.p_eMode = Tree.eMode.JobSave; 
                }
            }

            string m_id;
            Log m_log;
            public TreeRoot m_treeRoot;
            public RemoteRun(string id, Log log)
            {
                m_id = id;
                m_log = log; 
                m_treeRoot = new TreeRoot(id, log);
/*                m_memoryStream = new MemoryStream();
                StreamWriter sw = new StreamWriter(m_memoryStream);
                sw.WriteLine("Test");
                sw.Flush();
                byte[] aWrite = m_memoryStream.ToArray();
                string sWrite = Encoding.Default.GetString(aWrite);
                aWrite = Encoding.UTF8.GetBytes(sWrite);
                m_memoryStream = new MemoryStream(aWrite); 
                sw.Close(); */
            }
        }
        RemoteRun m_remote; 
        
        public void Remote(bool bSend)
        {
            m_remote.Init(bSend);
            if (bSend)
            {
                m_run.RunTree(m_remote.m_treeRoot, false);
                m_remote.m_treeRoot.m_job.Close(); 
            }
        }
        #endregion

        public RemoteModule(string id, IEngineer engineer)
        {
            InitBase(id, engineer);
            m_remote = new RemoteRun(id, m_log); 
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

        ModuleRunBase m_run; 
        protected override void InitModuleRuns()
        {
            m_run = AddModuleRunList(new Run_Run(this), true, "Desc Run");
            AddModuleRunList(new Run_Test(this), true, "Desc Run");
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
                return "OK";
            }
        }

        public class Run_Test : ModuleRunBase
        {
            RemoteModule m_module;
            public Run_Test(RemoteModule module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bSend = true; 
            public override ModuleRunBase Clone()
            {
                Run_Test run = new Run_Test(m_module);
                run.m_bSend = m_bSend;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bSend = tree.Set(m_bSend, m_bSend, "Send", "Send", bVisible);
            }

            public override string Run()
            {
                m_module.Remote(m_bSend); 
                return "OK";
            }
        }
    }
}
