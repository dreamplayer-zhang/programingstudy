using Root_JEDI_Sorter.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_JEDI.Module
{
    public class TrayOut : ModuleBase
    {
        public enum eOut
        {
            TrayOutL,
            TrayOutR
        }

        #region ToolBox
        Axis m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Stage");
            m_unloadEV.GetTools(m_toolBox, this, bInit);
            m_stage.GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition();
        }
        #endregion

        #region Stage Axis
        public enum ePos
        {
            Elevator,
            Flipper,
            Vision,
        }

        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        public string MoveStage(ePos ePos, bool bWait = true)
        {
            m_axis.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        string AvoidElevator()
        {
            if (m_axis.p_posCommand >= m_axis.GetPosValue(ePos.Flipper)) return "OK";
            return MoveStage(ePos.Flipper, true);
        }

        public bool IsInPos(ePos ePos)
        {
            return (Math.Abs(m_axis.p_posCommand - m_axis.GetPosValue(ePos)) < 1);
        }
        #endregion

        #region RunUnload
        public string StartUnload()
        {
            Run_Unload run = (Run_Unload)m_runUnload.Clone();
            return StartRun(run);
        }

        public string RunUnload()
        {
            try
            {
                if (m_stage.p_infoTray == null) return "RunUnload : p_infoTray == null";
                if (m_stage.IsCheck(true) == false) return "Check Tray Sensor in Stage";
                if (m_unloadEV.IsFull()) return "Unload Elevator is Full";
                if (Run(AvoidElevator())) return p_sInfo;
                if (Run(m_stage.RunAlign(true, false))) return p_sInfo;
                if (Run(m_unloadEV.RunMove(UnloadEV.ePos.Down))) return p_sInfo;
                if (Run(m_stage.RunAlign(true))) return p_sInfo;
                if (Run(MoveStage(ePos.Elevator, true))) return p_sInfo;
                if (Run(m_stage.RunAlign(false))) return p_sInfo;
                if (Run(m_unloadEV.RunMove(UnloadEV.ePos.Stage))) return p_sInfo;
                if (Run(m_unloadEV.RunMove(UnloadEV.ePos.Elevator))) return p_sInfo;
                if (Run(MoveStage(ePos.Flipper, false))) return p_sInfo;
                if (Run(m_unloadEV.RunUnload())) return p_sInfo;
                if (Run(MoveStage(ePos.Flipper, true))) return p_sInfo;
                m_stage.p_infoTray = null;
            }
            finally { m_unloadEV.RunMove(UnloadEV.ePos.Elevator); }
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
            if (m_unloadEV.IsCheck(false) == false) return "Check Tray";
            string sHome = StateHome(m_unloadEV.m_axis);
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
            m_unloadEV.RunTree(tree.GetTree("Elevator"));
        }
        #endregion


        eOut m_eOut;
        public UnloadEV m_unloadEV;
        public Stage m_stage;
        public TrayOut(eOut eOut, IEngineer engineer)
        {
            m_eOut = eOut;
            string id = eOut.ToString();
            m_unloadEV = new UnloadEV(id + ".UnloadEV");
            m_stage = new Stage(id + ".Stage");
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runMove;
        ModuleRunBase m_runUnload;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Align(this), true, "Run Stage Align");
            m_runMove = AddModuleRunList(new Run_Move(this), true, "Run Move Stage");
            m_runUnload = AddModuleRunList(new Run_Unload(this), true, "Run Unload Tray");
        }

        public class Run_Delay : ModuleRunBase
        {
            TrayOut m_module;
            public Run_Delay(TrayOut module)
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

        public class Run_Align : ModuleRunBase
        {
            TrayOut m_module;
            public Run_Align(TrayOut module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bAlign = true;
            public override ModuleRunBase Clone()
            {
                Run_Align run = new Run_Align(m_module);
                run.m_bAlign = m_bAlign;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bAlign = tree.Set(m_bAlign, m_bAlign, "Align", "Run Grip", bVisible);
            }

            public override string Run()
            {
                return m_module.m_stage.RunAlign(m_bAlign);
            }
        }

        public class Run_Move : ModuleRunBase
        {
            TrayOut m_module;
            public Run_Move(TrayOut module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            ePos m_ePos = ePos.Flipper; 
            public override ModuleRunBase Clone()
            {
                Run_Move run = new Run_Move(m_module);
                run.m_ePos = m_ePos; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePos)tree.Set(m_ePos, m_ePos, "Pos", "Axis Move Pos", bVisible); 
            }

            public override string Run()
            {
                return m_module.MoveStage(m_ePos);
            }
        }

        public class Run_Unload : ModuleRunBase
        {
            TrayOut m_module;
            public Run_Unload(TrayOut module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnload();
            }
        }
        #endregion
    }
}
