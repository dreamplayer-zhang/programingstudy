using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_JEDI_Sorter.Module
{
    public class Good : ModuleBase
    {
        #region ToolBox
        Axis m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Stage");
            m_unloadEV.GetTools(m_toolBox, this, bInit);
            m_stage[eStage.Taker].GetTools(m_toolBox, this, bInit);
            m_stage[eStage.Giver].GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition();
        }
        #endregion

        #region Stage Axis
        public enum eStage
        {
            Taker,
            Giver,
        }
        public enum ePos
        {
            Elevator,
            TakerTransfer,
            GiverTransfer,
            TakerPicker,
            GiverPicker,
        }
        public enum ePicker
        {
            PickerA,
            PickerB
        }
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        public string MoveStage(ePos ePos, bool bWait, double fOffset = 0)
        {
            m_axis.StartMove(ePos, fOffset);
            return bWait ? m_axis.WaitReady() : "OK";
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
            Stage stage = m_stage[(int)eStage.Taker]; 
            if (stage.p_infoTray == null) return "RunUnload : p_infoTray == null";
            //forget
            return "OK";
        }
        #endregion

        public UnloadEV m_unloadEV;
        public Dictionary<eStage, Stage> m_stage = new Dictionary<eStage, Stage>(); 
        public Good(string id, IEngineer engineer)
        {
            m_unloadEV = new UnloadEV(id + ".UnloadEV");
            m_stage.Add(eStage.Taker, new Stage(id + ".Taker", "Taker."));
            m_stage.Add(eStage.Giver, new Stage(id + ".Giver", "Giver.")); 
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop(); 
        }

        #region ModuleRun
        ModuleRunBase m_runUnload;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            m_runUnload = AddModuleRunList(new Run_Unload(this), true, "Run Unload Tray");
        }

        public class Run_Delay : ModuleRunBase
        {
            Good m_module;
            public Run_Delay(Good module)
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

        public class Run_Unload : ModuleRunBase
        {
            Good m_module;
            public Run_Unload(Good module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eStage m_eStage = eStage.Taker; 
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
