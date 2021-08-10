using Root_JEDI_Sorter.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_JEDI.Module
{
    public class TrayIn : ModuleBase
    {
        public enum eIn
        {
            TrayInL,
            TrayInR
        }

        #region ToolBox
        Axis m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Stage");
            m_loadEV.GetTools(m_toolBox, this, bInit);
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

        public bool IsInPos(ePos ePos)
        {
            return (Math.Abs(m_axis.p_posCommand - m_axis.GetPosValue(ePos)) < 1);
        }
        #endregion

        #region RunLoad
        public string StartMove(ePos ePos)
        {
            Run_Move run = (Run_Move)m_runMove.Clone();
            run.m_ePos = ePos;
            return StartRun(run); 
        }

        public string StartLoad()
        {
            Run_Load run = (Run_Load)m_runLoad.Clone();
            return StartRun(run);
        }

        public string RunLoad()
        {
            try
            {
                if (m_stage.p_infoTray != null) return "RunLoad : p_infoTray != null";
                if (m_stage.IsCheck(false) == false) return "Check Tray in Stage";
                if (m_loadEV.IsCheck(true) == false) return "No Tray in Elevator";
                if (Run(MoveStage(ePos.Elevator, false))) return p_sInfo;
                if (Run(m_stage.RunAlign(false, false))) return p_sInfo;
                if (Run(m_loadEV.RunLoad())) return p_sInfo;
                if (Run(MoveStage(ePos.Elevator, true))) return p_sInfo;
                if (Run(m_loadEV.RunMove(LoadEV.ePos.Stage))) return p_sInfo;
                if (m_stage.IsCheck(true) == false)
                {
                    m_loadEV.RunUnload();
                    return "Stage Check Sensor Error when Elevator put Tray";
                }
                if (Run(m_loadEV.RunMove(LoadEV.ePos.Down))) return p_sInfo;
                if (Run(m_stage.RunAlign(true))) return p_sInfo;
                if (Run(MoveStage(ePos.Flipper, true))) return p_sInfo;
                if (Run(m_loadEV.RunMove(LoadEV.ePos.Grip, false))) return p_sInfo;
                if (Run(m_stage.RunAlign(false))) return p_sInfo;
                m_stage.p_infoTray = new InfoTray("Test"); //forget
                if (Run(m_loadEV.RunMove(LoadEV.ePos.Grip))) return p_sInfo;
            }
            finally { m_loadEV.RunMove(LoadEV.ePos.Grip); }
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
            if (m_loadEV.IsCheck(false) == false) return "Check Tray";
            string sHome = StateHome(m_loadEV.m_axis);
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
        #endregion

        public eIn m_eIn = eIn.TrayInL;
        public LoadEV m_loadEV;
        public Stage m_stage;
        public TrayIn(eIn eIn, IEngineer engineer)
        {
            m_eIn = eIn;
            string id = eIn.ToString();
            m_loadEV = new LoadEV(id + ".LoadEV");
            m_stage = new Stage(id + ".Stage");
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runMove;
        ModuleRunBase m_runLoad;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Grip(this), true, "Run Elevator Grip");
            AddModuleRunList(new Run_Align(this), true, "Run Stage Align");
            m_runMove = AddModuleRunList(new Run_Move(this), true, "Run Move Stage");
            m_runLoad = AddModuleRunList(new Run_Load(this), true, "Run Load Tray");
        }

        public class Run_Delay : ModuleRunBase
        {
            TrayIn m_module;
            public Run_Delay(TrayIn module)
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
            TrayIn m_module;
            public Run_Grip(TrayIn module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bGrip = true;
            public override ModuleRunBase Clone()
            {
                Run_Grip run = new Run_Grip(m_module);
                run.m_bGrip = m_bGrip;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bGrip = tree.Set(m_bGrip, m_bGrip, "Grip", "Run Grip", bVisible);
            }

            public override string Run()
            {
                return m_module.m_loadEV.RunGrip(m_bGrip);
            }
        }

        public class Run_Align : ModuleRunBase
        {
            TrayIn m_module;
            public Run_Align(TrayIn module)
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
            TrayIn m_module;
            public Run_Move(TrayIn module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePos m_ePos = ePos.Flipper; 
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

        public class Run_Load : ModuleRunBase
        {
            TrayIn m_module;
            public Run_Load(TrayIn module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunLoad();
            }
        }
        #endregion
    }
}
