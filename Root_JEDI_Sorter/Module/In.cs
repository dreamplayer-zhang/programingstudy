using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_JEDI_Sorter.Module
{
    public class In : ModuleBase
    {
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
            Transfer
        }
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        public string MoveStage(ePos ePos, bool bWait)
        {
            m_axis.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }
        #endregion

        #region RunLoad
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
                if (Run(MoveStage(ePos.Transfer, true))) return p_sInfo;
                if (Run(m_loadEV.RunMove(LoadEV.ePos.Grip, false))) return p_sInfo;
                if (Run(m_stage.RunAlign(false))) return p_sInfo;
                m_stage.p_infoTray = new InfoTray("Test"); //forget
                if (Run(m_loadEV.RunMove(LoadEV.ePos.Grip))) return p_sInfo;
            }
            finally { m_loadEV.RunMove(LoadEV.ePos.Grip); }
            return "OK";
        }
        #endregion

        public LoadEV m_loadEV;
        public Stage m_stage; 
        public In(string id, IEngineer engineer)
        {
            m_loadEV = new LoadEV(id + ".LoadEV");
            m_stage = new Stage(id + ".Stage");
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop(); 
        }

        #region ModuleRun
        ModuleRunBase m_runLoad;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Grip(this), true, "Run Elevator Grip");
            AddModuleRunList(new Run_Align(this), true, "Run Stage Align");
            m_runLoad = AddModuleRunList(new Run_Load(this), true, "Run Load Tray");
        }

        public class Run_Delay : ModuleRunBase
        {
            In m_module;
            public Run_Delay(In module)
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
            In m_module;
            public Run_Grip(In module)
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
            In m_module;
            public Run_Align(In module)
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

        public class Run_Load : ModuleRunBase
        {
            In m_module;
            public Run_Load(In module)
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
