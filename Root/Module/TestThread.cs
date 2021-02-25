using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Root.Module
{
    public class TestThread : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {

        }
        #endregion

        #region Thread Write
        Thread m_threadSave; 
        void InitThread()
        {
            m_threadSave = new Thread(new ThreadStart(RunThreadSave));
            m_threadSave.Start();
        }

        void RunThreadSave()
        {
            for (int n = 0; n < 10; n++)
            {
                Thread.Sleep(300);
                FileStream fs = new FileStream("c:\\Log\\Thread.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(n.ToString());
                sw.Close();
                fs.Close();
            }
        }
        #endregion

        #region RunRun
        string RunRun()
        {
            StackTrace st = new StackTrace(true);
            for (int n = 0; n < st.FrameCount; n++)
            {
                StackFrame sf = st.GetFrame(n);
                System.Reflection.MethodBase mb = sf.GetMethod();
                string sMethod = mb.Name;
                string sClass = mb.DeclaringType.Name; 
            }
            InitThread();
            int[] aBuf = new int[5]; 
            for (int n = 0; n < 10; n++)
            {
                aBuf[n] = n;
                int f = 100 / (3 - n); 
            }
            return "OK"; 
        }
        #endregion

        #region override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override string StateHome()
        {
            return base.StateHome();
        }
        #endregion

        public TestThread(string id, IEngineer engineer)
        {
            InitBase(id, engineer);
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Run(this), true, "Desc Run");
        }

        public class Run_Run : ModuleRunBase
        {
            TestThread m_module;
            public Run_Run(TestThread module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunRun();
            }
        }
        #endregion

    }
}
