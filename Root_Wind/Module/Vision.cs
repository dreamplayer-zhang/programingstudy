using RootTools;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_WIND2
{
    public class Vision : ModuleBase
    {
        #region ToolBox
        Axis m_axisRotate;
        AxisXY m_axisXZ;
        DIO_O m_doVac;
        DIO_O m_doBlow;
        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup;
        LightSet m_lightSet;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.Get(ref m_axisXZ, this, "Camera XZ");
            p_sInfo = m_toolBox.Get(ref m_doVac, this, "Stage Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Stage Blow");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
        }
        #endregion

        #region DIO
        public bool p_bStageVac
        {
            get
            {
                return m_doVac.p_bOut;
            }
            set
            {
                if (m_doVac.p_bOut == value)
                    return;
                m_doVac.Write(value);
            }
        }

        public bool p_bStageBlow
        {
            get
            {
                return m_doBlow.p_bOut;
            }
            set
            {
                if (m_doBlow.p_bOut == value)
                    return;
                m_doBlow.Write(value);
            }
        }

        public void RunBlow(int msDelay)
        {
            m_doBlow.DelayOff(msDelay);
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
        }

        #endregion

        #region Axis
        public int m_pulseRound = 1000;
        void RunTreeAxis(Tree tree)
        {
            m_pulseRound = tree.Set(m_pulseRound, m_pulseRound, "Rotate Pulse / Round", "Rotate Axis Pulse / 1 Round (pulse)");
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
                return "OK";
            //            p_bStageBlow = false;
            //            p_bStageVac = true;
            Thread.Sleep(200);
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            //p_bStageVac = false;
            return "OK";
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeAxis(tree.GetTree("Axis", false));
        }
        #endregion

        public Vision(string id, IEngineer engineer)
        {
            //            InitLineScan();
            //            InitAreaScan();
            base.InitBase(id, engineer);
            //            InitMemory();
        }

        public override void ThreadStop()
        {
            //
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_Rotate(this), false, "Rotate Axis");
            AddModuleRunList(new Run_GrabLineScan(this), false, "Run Grab LineScan Camera");
        }

        public class Run_Delay : ModuleRunBase
        {
            Vision m_module;
            public Run_Delay(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return "OK";
            }
        }

        public class Run_Rotate : ModuleRunBase
        {
            Vision m_module;
            public Run_Rotate(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_fDeg = 0;
            public override ModuleRunBase Clone()
            {
                Run_Rotate run = new Run_Rotate(m_module);
                run.m_fDeg = m_fDeg;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fDeg = tree.Set(m_fDeg, m_fDeg, "Degree", "Rotation Degree (0 ~ 360)", bVisible);
            }

            public override string Run()
            {
                int pulseRound = m_module.m_pulseRound;
                Axis axis = m_module.m_axisRotate;
                int pulse = (int)Math.Round(m_module.m_pulseRound * m_fDeg / 360);
                while (pulse < axis.p_posCommand)
                    pulse += pulseRound;
                {
                    axis.p_posCommand -= pulseRound;
                    axis.p_posActual -= pulseRound;
                }
                if (m_module.Run(axis.StartMove(pulse)))
                    return p_sInfo;
                return axis.WaitReady();
            }
        }

        public class Run_GrabLineScan : ModuleRunBase
        {
            Vision m_module;
            public Run_GrabLineScan(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_GrabLineScan run = new Run_GrabLineScan(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return "OK";
            }
        }
        #endregion
    }
}
