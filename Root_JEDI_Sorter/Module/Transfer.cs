using Root_JEDI_Sorter.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

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
                Unload,
                Stage,
                Load
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
            int m_maxStack = 2; 
            public Stack<InfoTray> m_stackTray = new Stack<InfoTray>(); 

            public void RunTree(Tree tree)
            {
                m_maxStack = tree.Set(m_maxStack, m_maxStack, "Max Stack", "Max Stack Count"); 
            }
            #endregion

            public string RunLoad()
            {
                try
                {
                    if (IsCheck(false) == false) return "Picker Load Error : Check Sensor";
                    if (m_stackTray.Count >= m_maxStack) return "Picker Stack Count >= MaxStack";
                    if (Run(RunGrip(true))) return m_sInfo;
                    if (Run(RunMove(ePos.Up))) return m_sInfo;
                    if (Run(RunMove(ePos.Stage))) return m_sInfo;
                    if (Run(RunGrip(false))) return m_sInfo;
                    if (Run(RunMove(ePos.Load))) return m_sInfo;
                    if (Run(RunGrip(true))) return m_sInfo;
                    if (IsCheck(true))
                    {
                        if (Run(RunMove(ePos.Up))) return m_sInfo;
                        return "OK";
                    }
                    else
                    {
                        if (Run(RunGrip(false))) return m_sInfo;
                        if (Run(RunMove(ePos.Stage))) return m_sInfo;
                        if (Run(RunGrip(true))) return m_sInfo;
                        if (Run(RunMove(ePos.Up))) return m_sInfo;
                        return "Check Sensor not Checked";
                    }
                }
                finally { RunMove(ePos.Up); }
            }

            public string RunUnload()
            {
                try
                {
                    if (IsCheck(true) == false) return "Picker Unload Error : Check Sensor";
                    if (m_stackTray.Count <= 0) return "Picker Stack Count == 0";
                    if (Run(RunGrip(true))) return m_sInfo;
                    if (Run(RunMove(ePos.Up))) return m_sInfo;
                    if (Run(RunMove(ePos.Stage))) return m_sInfo;
                    if (Run(RunGrip(false))) return m_sInfo;
                    if (Run(RunMove(ePos.Load))) return m_sInfo;
                    if (Run(RunGrip(true))) return m_sInfo;
                    if (Run(RunMove(ePos.Up))) return m_sInfo;
                }
                finally { RunMove(ePos.Up); }
                return "OK";
            }

            string m_sInfo = "OK";
            bool Run(string sInfo)
            {
                m_sInfo = sInfo;
                return sInfo == "OK"; 
            }

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
            run.m_eIn = eIn; 
            return StartRun(run);
        }

        public string RunLoadIn(In.eIn eIn)
        {
            In In = m_handler.m_in[eIn];
            Stage stage = In.m_stage;
            if (stage.p_infoTray == null) return "InfoTray == null at " + eIn.ToString(); 
            if (stage.IsCheck(true) == false) return "Tray Check Sensor Error at " + eIn.ToString();
            if (In.IsLoadPosition() == false) return eIn.ToString() + " Stage Position not Ready"; 
            ePosIn ePosIn = (eIn == In.eIn.InA) ? ePosIn.InA : ePosIn.InB;
            if (Run(stage.RunAlign(false, false))) return p_sInfo;
            if (Run(MoveIn(ePosIn))) return p_sInfo;
            if (Run(stage.RunAlign(false))) return p_sInfo; 
            if (Run(m_picker[eZ.Z1].RunLoad())) return p_sInfo;
            if (stage.IsCheck(false) == false) return "Tray Check Sensor Error after Load at " + eIn.ToString();
            m_picker[eZ.Z1].m_stackTray.Push(stage.p_infoTray);
            stage.p_infoTray = null; 
            return "OK";
        }

        public string StartLoadGood(eZ eZ, Good.eGood eGood)
        {
            Run_LoadGood run = (Run_LoadGood)m_runLoadGood.Clone();
            run.m_eZ = eZ;
            run.m_eGood = eGood;
            return StartRun(run);
        }

        public string RunLoadGood(eZ eZ, Good.eGood eGood)
        {
            Good good = m_handler.m_good[eGood];
            Stage stage = good.m_stage[Good.eStage.Giver];
            Picker picker = m_picker[eZ];
            if (stage.p_infoTray == null) return "InfoTray == null at " + eGood.ToString();
            if (stage.IsCheck(true) == false) return "Tray Check Sensor Error at " + eGood.ToString();
            if (good.IsInPosition(Good.eStage.Giver) == false) return eGood.ToString() + " Stage Position not Ready";
            ePosGood ePosGood = (eGood == Good.eGood.GoodA) ? ePosGood.GoodA : ePosGood.GoodB;
            if (Run(stage.RunAlign(false, false))) return p_sInfo;
            if (Run(MoveGood(ePosGood, eZ))) return p_sInfo;
            if (Run(stage.RunAlign(false))) return p_sInfo;
            if (Run(picker.RunLoad())) return p_sInfo;
            if (stage.IsCheck(false) == false) return "Tray Check Sensor Error after Load at " + eGood.ToString();
            picker.m_stackTray.Push(stage.p_infoTray);
            stage.p_infoTray = null;
            return "OK";
        }
        #endregion

        #region RunUnload
        public string StartUnloadGood(Good.eGood eGood, Good.eStage eStage)
        {
            Run_UnloadGood run = (Run_UnloadGood)m_runUnloadGood.Clone();
            run.m_eGood = eGood;
            run.m_eStage = eStage; 
            return StartRun(run); 
        }

        public string RunUnloadGood(Good.eGood eGood, Good.eStage eStage)
        {
            Good good = m_handler.m_good[eGood];
            Stage stage = good.m_stage[eStage];
            string sStage = eGood.ToString() + "." + eStage.ToString(); 
            if (stage.p_infoTray != null) return "InfoTRay != null at " + sStage;
            if (stage.IsCheck(false) == false) return "Tray Check Sensor Error at " + sStage; 
            if (good.IsInPosition(eStage) == false) return sStage + " Stage Position not Ready";
            ePosGood ePosGood = (eGood == Good.eGood.GoodA) ? ePosGood.GoodA : ePosGood.GoodB;
            if (Run(stage.RunAlign(false, false))) return p_sInfo;
            if (Run(MoveGood(ePosGood, eZ.Z1))) return p_sInfo;
            if (Run(stage.RunAlign(false))) return p_sInfo;
            if (Run(m_picker[eZ.Z1].RunUnload())) return p_sInfo;
            if (stage.IsCheck(true) == false) return "Tray Check Sensor Error after Unload at " + sStage;
            if (Run(stage.RunAlign(true))) return p_sInfo;
            stage.p_infoTray = m_picker[eZ.Z1].m_stackTray.Pop(); 
            return "OK"; 
        }

        public string StartUnloadBad(Bad.eBad eBad)
        {
            Run_UnloadBad run = (Run_UnloadBad)m_runUnloadBad.Clone();
            run.m_eBad = eBad;
            return StartRun(run);
        }

        public string RunUnloadBad(Bad.eBad eBad)
        {
            Bad bad = m_handler.m_bad[eBad];
            Stage stage = bad.m_stage;
            string sBad = eBad.ToString();
            if (stage.p_infoTray != null) return "InfoTRay != null at " + sBad;
            if (stage.IsCheck(false) == false) return "Tray Check Sensor Error at " + sBad;
            if (bad.IsInPosition() == false) return sBad + " Stage Position not Ready";
            ePosBad ePos = (eBad == Bad.eBad.Reject) ? ePosBad.Reject : ePosBad.Rework; 
            if (Run(stage.RunAlign(false, false))) return p_sInfo;
            if (Run(MoveBad(ePos))) return p_sInfo;
            if (Run(stage.RunAlign(false))) return p_sInfo;
            if (Run(m_picker[eZ.Z2].RunUnload())) return p_sInfo;
            if (stage.IsCheck(true) == false) return "Tray Check Sensor Error after Unload at " + sBad;
            if (Run(stage.RunAlign(true))) return p_sInfo;
            stage.p_infoTray = m_picker[eZ.Z2].m_stackTray.Pop();
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
            m_picker[eZ.Z1].RunTree(tree.GetTree("Picker Z1"));
            m_picker[eZ.Z2].RunTree(tree.GetTree("Picker Z2"));
        }
        #endregion

        JEDI_Sorter_Handler m_handler; 
        public Transfer(string id, IEngineer engineer)
        {
            m_handler = (JEDI_Sorter_Handler)engineer.ClassHandler(); 
            InitPicker();
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoadIn;
        ModuleRunBase m_runLoadGood;
        ModuleRunBase m_runUnloadGood;
        ModuleRunBase m_runUnloadBad;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Grip(this), true, "Run Elevator Grip");
            m_runLoadIn = AddModuleRunList(new Run_LoadIn(this), true, "Run Load at In");
            m_runLoadGood = AddModuleRunList(new Run_LoadGood(this), true, "Run Load at Good");
            m_runUnloadGood = AddModuleRunList(new Run_UnloadGood(this), true, "Run Unload to Good");
            m_runUnloadBad = AddModuleRunList(new Run_UnloadBad(this), true, "Run Unload to Bad");
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

            public In.eIn m_eIn = In.eIn.InA; 
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

        public class Run_LoadGood : ModuleRunBase
        {
            Transfer m_module;
            public Run_LoadGood(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eZ m_eZ;
            public Good.eGood m_eGood;
            public override ModuleRunBase Clone()
            {
                Run_LoadGood run = new Run_LoadGood(m_module);
                run.m_eZ = m_eZ; 
                run.m_eGood = m_eGood;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eZ = (eZ)tree.Set(m_eZ, m_eZ, "Picker", "Load at", bVisible);
                m_eGood = (Good.eGood)tree.Set(m_eGood, m_eGood, "Good", "Load at", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoadGood(m_eZ, m_eGood);
            }
        }

        public class Run_UnloadGood : ModuleRunBase
        {
            Transfer m_module;
            public Run_UnloadGood(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public Good.eGood m_eGood;
            public Good.eStage m_eStage; 
            public override ModuleRunBase Clone()
            {
                Run_UnloadGood run = new Run_UnloadGood(m_module);
                run.m_eGood = m_eGood;
                run.m_eStage = m_eStage;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eGood = (Good.eGood)tree.Set(m_eGood, m_eGood, "Good", "Unload to", bVisible);
                m_eStage = (Good.eStage)tree.Set(m_eStage, m_eStage, "Stage", "Unload to", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnloadGood(m_eGood, m_eStage);
            }
        }

        public class Run_UnloadBad : ModuleRunBase
        {
            Transfer m_module;
            public Run_UnloadBad(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public Bad.eBad m_eBad;
            public override ModuleRunBase Clone()
            {
                Run_UnloadBad run = new Run_UnloadBad(m_module);
                run.m_eBad = m_eBad;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eBad = (Bad.eBad)tree.Set(m_eBad, m_eBad, "Bad", "Unload to", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnloadBad(m_eBad);
            }
        }
        #endregion
    }
}
