using Root_JEDI_Sorter.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_JEDI.Module
{
    public class BufferTray : ModuleBase
    {
        #region ToolBox
        Axis m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Stage");
            m_stage.GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition();
        }
        #endregion

        #region Stage Axis
        public enum ePos
        {
            Handler,
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

        #region override
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            string sHome = StateHome(m_axis);
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

        public Stage m_stage;
        public BufferTray(string id, IEngineer engineer)
        {
            m_stage = new Stage(id + ".Stage");
            m_stage.p_infoTray = new InfoTray(id); 
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runMove;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Align(this), true, "Run Stage Align");
            m_runMove = AddModuleRunList(new Run_Move(this), true, "Run Move Stage");
        }

        public class Run_Delay : ModuleRunBase
        {
            BufferTray m_module;
            public Run_Delay(BufferTray module)
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
            BufferTray m_module;
            public Run_Align(BufferTray module)
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
            BufferTray m_module;
            public Run_Move(BufferTray module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePos m_ePos = ePos.Handler;
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
        #endregion
    }
}
