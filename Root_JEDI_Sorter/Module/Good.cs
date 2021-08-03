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
            GiverTransfer,
            TakerTransfer,
            GiverPicker,
            TakerPicker,
        }
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        string MoveStage(ePos ePos, double fOffset, bool bWait)
        {
            m_axis.StartMove(ePos, fOffset);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string MoveElevator(bool bWait = true)
        {
            return MoveStage(ePos.Elevator, 0, bWait); 
        }

        public string MoveTransfer(eStage eStage, bool bWait = true)
        {
            switch (eStage)
            {
                case eStage.Giver: return MoveStage(ePos.GiverTransfer, 0, bWait);
                case eStage.Taker: return MoveStage(ePos.TakerTransfer, 0, bWait); 
            }
            return "Unknown Stage"; 
        }

        public string MovePicker(eStage eStage, double fOffset, bool bWait = true)
        {
            switch (eStage)
            {
                case eStage.Giver: return MoveStage(ePos.GiverPicker, fOffset, bWait);
                case eStage.Taker: return MoveStage(ePos.TakerPicker, fOffset, bWait);
            }
            return "Unknown Stage";
        }

        string AvoidElevator()
        {
            if (m_axis.p_posCommand >= m_axis.GetPosValue(ePos.TakerTransfer)) return "OK";
            return MoveStage(ePos.TakerTransfer, 0, true);
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
            try
            {
                if (stage.p_infoTray == null) return "RunUnload : p_infoTray == null";
                if (stage.IsCheck(true) == false) return "Check Tray Sensor in Stage";
                if (m_unloadEV.IsFull()) return "Unload Elevator is Full";
                if (Run(AvoidElevator())) return p_sInfo;
                if (Run(stage.RunAlign(true, false))) return p_sInfo;
                if (Run(m_unloadEV.RunMove(UnloadEV.ePos.Down))) return p_sInfo;
                if (Run(stage.RunAlign(true))) return p_sInfo;
                if (Run(MoveStage(ePos.Elevator, 0, true))) return p_sInfo;
                if (Run(stage.RunAlign(false))) return p_sInfo;
                if (Run(m_unloadEV.RunMove(UnloadEV.ePos.Stage))) return p_sInfo;
                if (Run(m_unloadEV.RunMove(UnloadEV.ePos.Elevator))) return p_sInfo;
                if (Run(MoveStage(ePos.TakerTransfer, 0, false))) return p_sInfo;
                if (Run(m_unloadEV.RunUnload())) return p_sInfo;
                if (Run(MoveStage(ePos.TakerTransfer, 0, true))) return p_sInfo;
                stage.p_infoTray = null;
            }
            finally { m_unloadEV.RunMove(UnloadEV.ePos.Elevator); }
            return "OK";
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            m_unloadEV.RunTree(tree.GetTree("Elevator"));
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
            AddModuleRunList(new Run_Align(this), true, "Run Stage Align");
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

        public class Run_Align : ModuleRunBase
        {
            Good m_module;
            public Run_Align(Good module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eStage m_eState = eStage.Giver; 
            bool m_bAlign = true;
            public override ModuleRunBase Clone()
            {
                Run_Align run = new Run_Align(m_module);
                run.m_bAlign = m_bAlign;
                run.m_eState = m_eState; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eState = (eStage)tree.Set(m_eState, m_eState, "Stage", "Select Stage", bVisible); 
                m_bAlign = tree.Set(m_bAlign, m_bAlign, "Align", "Run Grip", bVisible);
            }

            public override string Run()
            {
                return m_module.m_stage[m_eState].RunAlign(m_bAlign);
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
