using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_AUP01.Module
{
    public class Packer : ModuleBase
    {
        #region ToolBox
        DIO_I2O m_dioHead;
        DIO_I2O m_dioPad;
        DIO_I2O m_dioLidGuide;
        DIO_I m_diPressCheck;
        DIO_I m_diTopCoverCheck;
        DIO_IO m_dioVacuum;
        DIO_O m_doBlow;

        Axis m_axisRotate;
        DIO_I2O m_dioAlign;
        DIO_I m_diAlignCheck;
        DIO_I m_diCaseCheck;

        DIO_I2O m_dioCartrige;
        DIO_I2O m_dioCutter;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_dioHead, this, "Head", "Up", "Down");
            p_sInfo = m_toolBox.Get(ref m_dioPad, this, "PadUp", "Up", "Down");
            p_sInfo = m_toolBox.Get(ref m_dioLidGuide, this, "LidGuide", "Release", "Hold");
            p_sInfo = m_toolBox.Get(ref m_diPressCheck, this, "PressCheck");
            p_sInfo = m_toolBox.Get(ref m_diTopCoverCheck, this, "TopCoverCheck");
            p_sInfo = m_toolBox.Get(ref m_dioVacuum, this, "Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Blow");

            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.Get(ref m_dioAlign, this, "Align", "Backward", "Align");
            p_sInfo = m_toolBox.Get(ref m_diAlignCheck, this, "AlignCheck");
            p_sInfo = m_toolBox.Get(ref m_diCaseCheck, this, "CaseCheck");

            p_sInfo = m_toolBox.Get(ref m_dioCartrige, this, "Cartrige", "Backward", "Forward");
            p_sInfo = m_toolBox.Get(ref m_dioCutter, this, "Cutter", "Backward", "Forward");

            if (bInit) InitTools(); 
        }

        void InitTools()
        {
            m_dioHead.Write(false);
            m_dioPad.Write(false);
            m_dioLidGuide.Write(false);
            m_dioVacuum.Write(false);
            m_doBlow.Write(false);
            m_dioAlign.Write(false);
            m_dioCartrige.Write(false);
            m_dioCutter.Write(false); 
        }
        #endregion

        #region DIO Function
        string RunHead(bool bOn)
        {
            m_dioHead.Write(bOn);
            int msHead = (int)(1000 * m_sSolHead);
            while (m_dioHead.p_bIn != bOn)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (m_dioHead.m_swWrite.ElapsedMilliseconds > msHead) return "Head Sol Valve Move Timeout";
            }
            return "OK"; 
        }

        public string CoverOpen()
        {
            m_dioHead.Write(false); 
            m_dioPad.Write(false);
            m_dioLidGuide.Write(false);
            int msPad = (int)(1000 * m_sSolPad);
            int msLidGuise = (int)(1000 * m_sSolLidGuide);
            while (m_dioLidGuide.p_bIn || m_dioPad.p_bIn)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (m_dioPad.m_swWrite.ElapsedMilliseconds > msPad) return "Pad Sol Valve Move Timeout";
                if (m_dioLidGuide.m_swWrite.ElapsedMilliseconds > msLidGuise) return "Lig Guide Sol Valve Move Timeout"; 
            }

            if (Run(RunHead(true))) return p_sInfo; 

            m_dioVacuum.Write(true);
            int msVac = (int)(1000 * m_sVac);
            while (m_dioVacuum.p_bIn == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (m_dioVacuum.m_swWrite.ElapsedMilliseconds > msVac) return "Vacuum Sensor Timeout";
            }
            return "OK"; 
        }

        int m_lRotate = 2621440;

        double m_sSolHead = 5;
        double m_sSolPad = 5;
        double m_sSolLidGuide = 5;
        double m_sVac = 2;
        void RunTreeDIOWait(Tree tree)
        {
            m_sSolHead = tree.Set(m_sSolHead, m_sSolHead, "Head", "Sol Value Move Wait (sec)");
            m_sSolPad = tree.Set(m_sSolPad, m_sSolPad, "Pad", "Sol Value Move Wait (sec)");
            m_sSolLidGuide = tree.Set(m_sSolLidGuide, m_sSolLidGuide, "Lid Guide", "Sol Value Move Wait (sec)");
            m_sVac = tree.Set(m_sVac, m_sVac, "Vac", "Vacuum Sensor Wait (sec)");
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
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }
        #endregion

        public Packer(string id, IEngineer engineer)
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
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
        }

        public class Run_Delay : ModuleRunBase
        {
            Packer m_module;
            public Run_Delay(Packer module)
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

        public class Run_Cover : ModuleRunBase
        {
            Packer m_module;
            public Run_Cover(Packer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bOpen = true;
            public override ModuleRunBase Clone()
            {
                Run_Cover run = new Run_Cover(m_module);
                run.m_bOpen = m_bOpen;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bOpen = tree.Set(m_bOpen, m_bOpen, "Open", "Cover Open", bVisible);
            }
            public override string Run()
            {
                if (m_bOpen)
                {
                }
                return "OK";
            }
        }
        #endregion
    }
}
