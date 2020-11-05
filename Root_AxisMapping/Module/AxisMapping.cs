using RootTools;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_AxisMapping.Module
{
    public class AxisMapping : ModuleBase
    {
        #region ToolBox
        Axis m_axisRotate;
        AxisXY m_axisXY;
        Axis m_axisZ;
        DIO_O m_doVacuum;
        public MemoryPool m_memoryPool;
        CameraDalsa m_cam;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "Stage");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "Camera Z"); 
            p_sInfo = m_toolBox.Get(ref m_doVacuum, this, "Vacuum");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_cam, this, "Camera");
            if (bInit) InitTools();
        }

        void InitTools()
        {
            if (m_memoryPool.p_gbPool < 4) m_memoryPool.p_gbPool = 4; 
        }
        #endregion

        #region Memory
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryGrab;
        CPoint m_szGrab = new CPoint(16000, 250000); 
        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryGrab = m_memoryGroup.CreateMemory("AlignGrab", 1, m_cam.p_nByte, m_szGrab);
            m_cam.SetMemoryData(m_memoryGrab);
            m_memoryPool.m_viewer.p_memoryData = m_memoryGrab;
        }

        void RunTreeMemory(Tree tree)
        {
            m_szGrab = tree.Set(m_szGrab, m_szGrab, "Grab Size", "Dalsa Grab Size (pixel)");
            if (m_szGrab != m_memoryGrab.p_sz) m_memoryGrab.p_sz = m_szGrab; 
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeMemory(tree.GetTree("Memory", false)); 
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }
        #endregion

        public AxisMapping(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        public ModuleRunBase m_runGrab;
        public ModuleRunBase m_runRotate;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            m_runGrab = AddModuleRunList(new Run_Grab(this), false, "Grab LineScan");
            m_runRotate = AddModuleRunList(new Run_Rotate(this), false, "Rotate");
        }

        public class Run_Delay : ModuleRunBase
        {
            AxisMapping m_module;
            public Run_Delay(AxisMapping module)
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
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }

        public class Run_Grab : ModuleRunBase
        {
            AxisMapping m_module;
            public Run_Grab(AxisMapping module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            CameraDalsa p_cam { get { return m_module.m_cam; } }
            AxisXY p_axisXY { get { return m_module.m_axisXY; } }
            Axis p_axisY { get { return m_module.m_axisXY.p_axisY; } }
            Axis p_axisZ { get { return m_module.m_axisZ; } }

            Axis.Trigger _trigger = null;
            Axis.Trigger p_trigger
            {
                get
                {
                    if (_trigger == null) _trigger = p_axisY.m_trigger.Clone();
                    return _trigger; 
                }
                set { _trigger = value; }
            }
            double m_xStart = 0;
            double m_zStart = 0;
            double m_dyAcc = 3;
            double m_vGrab = 100; 
            public override ModuleRunBase Clone()
            {
                Run_Grab run = new Run_Grab(m_module);
                run.p_trigger = p_trigger.Clone();
                run.m_xStart = m_xStart;
                run.m_zStart = m_zStart;
                run.m_vGrab = m_vGrab; 
                run.m_dyAcc = m_dyAcc; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_xStart = tree.Set(m_xStart, m_xStart, "X", "Axis X Position (Unit)", bVisible);
                m_zStart = tree.Set(m_zStart, m_zStart, "Z", "Axis X Position (Unit)", bVisible);
                m_vGrab = tree.Set(m_vGrab, m_vGrab, "V", "Axis X Position (Unit)", bVisible);
                m_dyAcc = tree.Set(m_dyAcc, m_dyAcc, "dY Acc", "Axis Y Acc Length (Unit)", bVisible); 
                p_trigger.RunTree(tree.GetTree("Trigger", true, bVisible), p_axisY.m_sUnit, bVisible);
            }

            public override string Run()
            {
                if (m_module.Run(p_axisZ.StartMove(m_zStart))) return p_sInfo;
                if (m_module.Run(p_axisZ.WaitReady())) return p_sInfo;
                double y0 = p_trigger.m_aPos[0] - m_dyAcc;
                if (m_module.Run(p_axisXY.StartMove(m_xStart, y0))) return p_sInfo;
                if (m_module.Run(p_axisXY.WaitReady())) return p_sInfo;
                p_axisY.RunTrigger(true, p_trigger);
                int nLine = (int)Math.Round((p_trigger.m_aPos[1] - p_trigger.m_aPos[0]) / p_trigger.m_dPos);
                if (m_module.Run(p_cam.StartGrab(new CPoint(), nLine))) return p_sInfo; 
                double y1 = p_trigger.m_aPos[1] + m_dyAcc;
                if (m_module.Run(p_axisY.StartMove(y1, m_vGrab))) return p_sInfo;
                if (m_module.Run(p_axisY.WaitReady())) return p_sInfo;
                p_axisY.RunTrigger(false); 
                if (p_cam.p_bOnGrab) return "Dalsa Camera Grab not Finished"; 
                return "OK";
            }
        }

        public class Run_Rotate : ModuleRunBase
        {
            AxisMapping m_module;
            public Run_Rotate(AxisMapping module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            Axis p_axis { get { return m_module.m_axisRotate; } }
            public double m_degRotate = 0;
            int m_nPpR = 10000;
            bool m_bInverse = false; 
            public override ModuleRunBase Clone()
            {
                Run_Rotate run = new Run_Rotate(m_module);
                run.m_degRotate = m_degRotate;
                run.m_nPpR = m_nPpR;
                run.m_bInverse = m_bInverse; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_degRotate = tree.Set(m_degRotate, m_degRotate, "Degree", "Axis Rotate (degree)", bVisible);
                m_nPpR = tree.Set(m_nPpR, m_nPpR, "PnR", "Pulse / Round", bVisible);
                m_bInverse = tree.Set(m_bInverse, m_bInverse, "Inverse", "Rotate Direction", bVisible); 
            }

            public override string Run()
            {
                double pulseRotate = m_nPpR * m_degRotate / 360;
                if (m_bInverse) pulseRotate = -pulseRotate; 
                p_axis.StartShift(pulseRotate);
                return p_axis.WaitReady(); 
            }
        }
        #endregion

    }
}
