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
        DIO_I m_diPickerSet; 

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_doBuzzer, this, "Buzzer", Enum.GetNames(typeof(eBuzzer)));
            p_sInfo = m_toolBox.GetDIO(ref m_diPickerSet, this, "PickerSet"); 
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

        #region PickerSet
        public enum ePickerSet
        {
            UpDown,
            Vacuum,
            End
        }
        StopWatch m_swPickerSet = new StopWatch();
        public ePickerSet CheckPickerSet()
        {
            while (m_diPickerSet.p_bIn == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return ePickerSet.End;
            }
            m_swPickerSet.Start();
            while (m_diPickerSet.p_bIn)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return ePickerSet.End;
                if (m_swPickerSet.ElapsedMilliseconds > 3000) return ePickerSet.End;
            }
            return (m_swPickerSet.ElapsedMilliseconds < 600) ? ePickerSet.UpDown : ePickerSet.Vacuum;
        }

        public string m_sFilePickerSet = "";
        void RunTreePickerSet(Tree tree)
        {
            m_sFilePickerSet = tree.SetFile(m_sFilePickerSet, m_sFilePickerSet, "RunRinse_Loader", "ModuleRun", "PickerSet ModuleRun File");
        }
        #endregion

        #region Run Mode
        void RunTreeMode(Tree tree) //forget
        {
            Strip.p_bUseMGZ = tree.Set(Strip.p_bUseMGZ, Strip.p_bUseMGZ, "Use MGZ", "Use MGZ or LoadEV");
            Strip.p_szStrip = tree.Set(Strip.p_szStrip, Strip.p_szStrip, "Strip Size", "Strip Size");
            Strip.m_szStripTeach = tree.Set(Strip.m_szStripTeach, Strip.m_szStripTeach, "Teach Strip Size", "Teach Strip Size");
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeMode(tree.GetTree("Mode"));
            RunTreePickerSet(tree.GetTree("PickerSet"));
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        ASIS_Handler m_handler; 
        public ASIS(string id, IEngineer engineer)
        {
            m_handler = (ASIS_Handler)engineer.ClassHandler(); 
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
