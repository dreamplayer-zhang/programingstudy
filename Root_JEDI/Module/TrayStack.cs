using Root_JEDI_Sorter.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace Root_JEDI.Module
{
    public class TrayStack : ModuleBase
    {
        #region ToolBox
        public DIO_I4O m_dioAlignY;
        public DIO_I2O m_dioAlignX;
        public DIO_Is[] m_diCheck = new DIO_Is[4] { null, null, null, null };
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetDIO(ref m_dioAlignX, this, "AlignX", "Off", "Align");
            m_toolBox.GetDIO(ref m_dioAlignY, this, "AlignY", "Off", "Align");
            m_toolBox.GetDIO(ref m_diCheck[0], this, "Check 1F", new string[2] { "A", "B" });
            m_toolBox.GetDIO(ref m_diCheck[1], this, "Check 2F", new string[2] { "A", "B" });
            m_toolBox.GetDIO(ref m_diCheck[2], this, "Check 3F", new string[2] { "A", "B" });
            m_toolBox.GetDIO(ref m_diCheck[3], this, "Check 4F", new string[2] { "A", "B" });
        }
        #endregion

        #region Align
        public string RunAlign(bool bAlign, bool bWait = true)
        {
            m_dioAlignX.Write(bAlign);
            m_dioAlignY.Write(bAlign);
            if (bWait == false) return "OK";
            string sX = m_dioAlignX.WaitDone();
            string sY = m_dioAlignY.WaitDone();
            if ((sX == "OK") && (sY == "OK")) return "OK";
            return sX + ", " + sY;
        }

        public bool IsCheck(int nFloor, bool bCheck)
        {
            if (m_diCheck[nFloor].ReadDI(0) != bCheck) return false;
            if (m_diCheck[nFloor].ReadDI(1) != bCheck) return false;
            return true;
        }

        public string CheckTrayCount(ref int nTray)
        {
            nTray = 0; 
            for (int n = 0; n < m_maxStack; n++)
            {
                bool bOn0 = m_diCheck[n].ReadDI(0);
                bool bOn1 = m_diCheck[n].ReadDI(1); 
                if (bOn0 != bOn1) return "Check Tray Sensor";
                if (bOn0) nTray++;
            }
            return "OK";
        }
        #endregion

        #region InfoTray
        int m_maxStack = 4;
        public Stack<InfoTray> m_stackTray = new Stack<InfoTray>();

        public string InitMetalTray()
        {
            int nTray = 0;
            string sCheck = CheckTrayCount(ref nTray);
            if (sCheck != "OK") return sCheck;
            while (m_stackTray.Count < nTray) m_stackTray.Push(new InfoTray("Metal"));
            while (m_stackTray.Count > nTray) m_stackTray.Pop(); 
            return "OK";
        }
        #endregion

        #region override
        public override void StartHome()
        {
            p_eState = eState.Ready; 
        }

        public override void Reset()
        {
            base.Reset();
            InitMetalTray(); 
        }

        public override void RunTree(Tree tree)
        {
        }
        #endregion

        public TrayStack(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            InitMetalTray(); 
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_InitInfoTray(this), true, "Init InfoTray");
        }

        public class Run_Delay : ModuleRunBase
        {
            TrayStack m_module;
            public Run_Delay(TrayStack module)
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

        public class Run_InitInfoTray : ModuleRunBase
        {
            TrayStack m_module;
            public Run_InitInfoTray(TrayStack module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_InitInfoTray run = new Run_InitInfoTray(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.InitMetalTray();
            }
        }
        #endregion
    }
}
