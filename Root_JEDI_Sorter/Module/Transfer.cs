using Root_JEDI_Sorter.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_JEDI_Sorter.Module
{
    public class Transfer : ModuleBase
    {
        #region Picker
        public enum eZ
        {
            Z1,
            Z2
        }

        public class Picker : NotifyProperty
        {
            public Axis m_axis; 
            DIO_Is m_diCheck;
            DIO_I8O2 m_dioGrip;
            public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
            {
                toolBox.GetAxis(ref m_axis, module, p_id); 
                toolBox.GetDIO(ref m_diCheck, module, p_id + ".Check", new string[2] { "0", "1" });
                toolBox.GetDIO(ref m_dioGrip, module, p_id + ".Grip", "Off", "Grip");
                if (bInit) InitPosition(); 
            }

            #region Axis
            public enum ePos
            {
                Up,
                Down,
            }
            void InitPosition()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos)));
            }

            public string RunMove(ePos ePos, bool bWait = true)
            {
                m_axis.StartMove(ePos);
                return bWait ? m_axis.WaitReady() : "OK";
            }
            #endregion

            #region DIO
            public bool IsCheck(bool bCheck)
            {
                if (m_diCheck.ReadDI(0) != bCheck) return false;
                if (m_diCheck.ReadDI(1) != bCheck) return false;
                return true;
            }

            public string RunGrip(bool bGrip, bool bWait = true)
            {
                m_dioGrip.Write(bGrip);
                return bWait ? m_dioGrip.WaitDone() : "OK";
            }
            #endregion

            #region InfoTray
            InfoTray _infoTray = null;
            public InfoTray p_infoTray
            {
                get { return _infoTray; }
                set
                {
                    _infoTray = value;
                    OnPropertyChanged();
                }
            }
            #endregion

            string p_id { get; set; }
            public Picker(string id)
            {
                p_id = id; 
            }
        }
        public Dictionary<eZ, Picker> m_picker = new Dictionary<eZ, Picker>(); 
        void InitPicker()
        {
            m_picker.Add(eZ.Z1, new Picker("Z1"));
            m_picker.Add(eZ.Z2, new Picker("Z2"));
        }
        #endregion

        #region ToolBox
        Axis m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Transfer");
            m_picker[eZ.Z1].GetTools(m_toolBox, this, bInit);
            m_picker[eZ.Z2].GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition();
        }
        #endregion

        #region Transfer Axis
        public enum ePosIn
        {
            InA,
            InB,
        }
        public enum ePosGood
        {
            GoodA,
            GoodB,
        }
        public enum ePosBad
        {
            Reject,
            Rework
        }

        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePosIn)));
            m_axis.AddPos(Enum.GetNames(typeof(ePosGood)));
            m_axis.AddPos(Enum.GetNames(typeof(ePosBad)));
        }

        public string MoveIn(ePosIn ePos, bool bWait = true)
        {
            m_axis.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        double m_dZ = 0;
        public string MoveGood(ePosGood ePos, eZ eZ, bool bWait = true)
        {
            double fOffset = (eZ == eZ.Z1) ? 0 : -m_dZ;
            m_axis.StartMove(ePos, fOffset);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string MoveBad(ePosBad ePos, bool bWait = true)
        {
            m_axis.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        void RunTreeAxis(Tree tree)
        {
            m_dZ = tree.Set(m_dZ, m_dZ, "dZ", "Distance between Z1, Z2 (pulse)"); 
        }
        #endregion

        #region RunLoad
        public string StartLoadIn(In.eIn eIn)
        {
            Run_LoadIn run = (Run_LoadIn)m_runLoadIn.Clone();
            return StartRun(run);
        }

        public string RunLoadIn(In.eIn eIn)
        {
            return "OK";
        }
        #endregion

        #region override
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            string sHome = StateHome(m_picker[eZ.Z1].m_axis, m_picker[eZ.Z2].m_axis);
            if (sHome != "OK") return sHome;
            sHome = StateHome(m_axis);
            if (sHome == "OK") p_eState = eState.Ready;
            return sHome;
        }

        public override string StateReady()
        {
            return base.StateReady();
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override void RunTree(Tree tree)
        {
            RunTreeAxis(tree.GetTree("Transfer"));
        }
        #endregion

        JEDI_Sorter_Handler m_hadler; 
        public Transfer(string id, IEngineer engineer)
        {
            m_hadler = (JEDI_Sorter_Handler)engineer.ClassHandler(); 
            InitPicker();
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoadIn;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Grip(this), true, "Run Elevator Grip");
            m_runLoadIn = AddModuleRunList(new Run_LoadIn(this), true, "Run Load at In");
        }

        public class Run_Delay : ModuleRunBase
        {
            Transfer m_module;
            public Run_Delay(Transfer module)
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

        public class Run_Grip : ModuleRunBase
        {
            Transfer m_module;
            public Run_Grip(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eZ m_eZ = eZ.Z1; 
            bool m_bGrip = true;
            public override ModuleRunBase Clone()
            {
                Run_Grip run = new Run_Grip(m_module);
                run.m_eZ = m_eZ; 
                run.m_bGrip = m_bGrip;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eZ = (eZ)tree.Set(m_eZ, m_eZ, "Picker", "Select Picker", bVisible); 
                m_bGrip = tree.Set(m_bGrip, m_bGrip, "Grip", "Run Grip", bVisible);
            }

            public override string Run()
            {
                return m_module.m_picker[m_eZ].RunGrip(m_bGrip);
            }
        }

        public class Run_LoadIn : ModuleRunBase
        {
            Transfer m_module;
            public Run_LoadIn(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            In.eIn m_eIn = In.eIn.InA; 
            public override ModuleRunBase Clone()
            {
                Run_LoadIn run = new Run_LoadIn(m_module);
                run.m_eIn = m_eIn; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eIn = (In.eIn)tree.Set(m_eIn, m_eIn, "In", "Load In", bVisible); 
            }

            public override string Run()
            {
                return m_module.RunLoadIn(m_eIn);
            }
        }
        #endregion
    }
}
