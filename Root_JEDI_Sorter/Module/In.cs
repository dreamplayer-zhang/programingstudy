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
            if (m_stage.p_infoTray != null) return "RunLoad : p_infoTray != null";
            if (Run(m_loadEV.RunMove(LoadEV.ePos.Up, false))) return p_sInfo; 
            if (Run(m_stage.RunAlign(false, false))) return p_sInfo;
            if (Run(MoveStage(ePos.Elevator, true))) return p_sInfo; 
            if (Run(m_stage.RunAlign(false, true))) return p_sInfo;
            //forget
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
