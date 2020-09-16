using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_ASIS.Module
{
    public class ASIS : ModuleBase
    {
        #region ToolBox
        DIO_Os m_doBuzzer; 

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_doBuzzer, this, "Buzzer", Enum.GetNames(typeof(eBuzzer))); 
            if (bInit) InitTools();
        }

        void InitTools()
        {
        }
        #endregion

        #region Buzzer
        public enum eBuzzer
        {
            Error,
            Finish,
            Warning,
            Home
        }

        public void RunBuzzer(eBuzzer buzzer, bool bOn)
        {
            m_doBuzzer.Write(buzzer, bOn); 
        }
        #endregion

        public ASIS(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Just Time Delay");
            AddModuleRunList(new Run_Buzzer(this), false, "Run Buzzer");
        }

        public class Run_Delay : ModuleRunBase
        {
            ASIS m_module;
            public Run_Delay(ASIS module)
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
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }

        public class Run_Buzzer : ModuleRunBase
        {
            ASIS m_module;
            public Run_Buzzer(ASIS module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eBuzzer m_eBuzzer = eBuzzer.Home; 
            double m_secBuzzer = 2;
            public override ModuleRunBase Clone()
            {
                Run_Buzzer run = new Run_Buzzer(m_module);
                run.m_eBuzzer = m_eBuzzer; 
                run.m_secBuzzer = m_secBuzzer;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eBuzzer = (eBuzzer)tree.Set(m_eBuzzer, m_eBuzzer, "Buzzer", "Select Buzzer", bVisible); 
                m_secBuzzer = tree.Set(m_secBuzzer, m_secBuzzer, "Time", "Buzzer Run Time (sec)", bVisible);
            }

            public override string Run()
            {
                m_module.m_doBuzzer.Write(m_eBuzzer, true);
                Thread.Sleep((int)(1000 * m_secBuzzer));
                m_module.m_doBuzzer.Write(m_eBuzzer, false);
                return "OK";
            }
        }
        #endregion
    }
}
