using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;

namespace Root.Module
{
    public class ACS : ModuleBase
    {
        #region ToolBox
        AxisXY m_axisXY;
        Axis m_axisZ;
        MemoryPool m_memoryPool;
        LightSet m_lightSet;
        CameraBasler m_camBasler;
        CameraDalsa m_camDalsa;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "Stage XY");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "Camera Z");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.Get(ref m_camBasler, this, "Camera Basler");
            p_sInfo = m_toolBox.Get(ref m_camDalsa, this, "Camera Dalsa");
        }
        #endregion

        #region Memory
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryDalsa;
        MemoryData m_memoryBasler;
        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryBasler = m_memoryGroup.CreateMemory("Grab Basler", 1, m_camBasler.p_nByte, m_camBasler.p_sz);
            m_camBasler.SetMemoryData(m_memoryBasler);
            m_memoryDalsa = m_memoryGroup.CreateMemory("Grab Dalsa", 1, m_camDalsa.p_nByte, m_szGrabDalsa);
            m_camDalsa.SetMemoryData(m_memoryDalsa);
        }

        CPoint m_szGrabDalsa = new CPoint(1024, 1024);
        void RunTreeDalsa(Tree tree)
        {
            m_szGrabDalsa = tree.Set(m_szGrabDalsa, m_szGrabDalsa, "Grab Size", "Dalsa Grab Size (pixel)");
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeDalsa(tree.GetTree("Dalsa", false));
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return "OK";
        }
        #endregion

        public ACS(string id, IEngineer engineer)
        {
            InitBase(id, engineer);
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
            AddModuleRunList(new Run_AxisMove(this), false, "Axis Move");
            AddModuleRunList(new Run_GrabBasler(this), false, "Run Grab Basler Camera");
            AddModuleRunList(new Run_GrabDalsa(this), false, "Run Grab Dalsa Camera");
        }

        public class Run_Delay : ModuleRunBase
        {
            ACS m_module;
            public Run_Delay(ACS module)
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

        public class Run_AxisMove : ModuleRunBase
        {
            ACS m_module;
            public Run_AxisMove(ACS module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_z = 0;
            public RPoint m_rp = new RPoint(); 
            public override ModuleRunBase Clone()
            {
                Run_AxisMove run = new Run_AxisMove(m_module);
                run.m_z = m_z;
                run.m_rp = new RPoint(m_rp); 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rp = tree.Set(m_rp, m_rp, "XY", "Axis XY Position (pulse)", bVisible);
                m_z = tree.Set(m_z, m_z, "Z", "Axis Z Position (pulse)", bVisible);
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                if (m_module.Run(m_module.m_axisXY.StartMove(m_rp))) return p_sInfo;
                if (m_module.Run(m_module.m_axisZ.StartMove(m_z))) return p_sInfo;
                if (m_module.Run(m_module.m_axisXY.WaitReady())) return p_sInfo;
                if (m_module.Run(m_module.m_axisZ.WaitReady())) return p_sInfo;
                return "OK";
            }
        }

        public class Run_GrabBasler : ModuleRunBase
        {
            ACS m_module;
            public Run_GrabBasler(ACS module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_GrabBasler run = new Run_GrabBasler(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                if (m_module.Run(m_module.m_camBasler.GrabOne(0))) return p_sInfo;
                return "OK";
            }
        }

        public class Run_GrabDalsa : ModuleRunBase
        {
            ACS m_module;
            public Run_GrabDalsa(ACS module)
            {
                m_module = module;
                m_runAxisMove = new Run_AxisMove(module); 
                InitModuleRun(module);
            }

            Run_AxisMove m_runAxisMove; 
            public override ModuleRunBase Clone()
            {
                Run_GrabDalsa run = new Run_GrabDalsa(m_module);
                run.m_runAxisMove = (Run_AxisMove)m_runAxisMove.Clone();
                run.m_yStart = m_yStart;
                run.m_yLength = m_yLength;
                run.m_dTrigger = m_dTrigger; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_runAxisMove.RunTree(tree.GetTree("Axis Position", true, bVisible), bVisible);
                RunTreeTrigger(tree.GetTree("Trigger", true, bVisible), bVisible); 
            }

            double m_yStart = 0;
            double m_yLength = 1000;
            double m_dTrigger = 10;
            bool m_bTriggerCmd = true; 
            void RunTreeTrigger(Tree tree, bool bVisible)
            {
                m_yStart = tree.Set(m_yStart, m_yStart, "Start", "Start Offset (pulse)", bVisible);
                m_yLength = tree.Set(m_yLength, m_yLength, "Length", "Grab Length (pulse)", bVisible);
                m_dTrigger = tree.Set(m_dTrigger, m_dTrigger, "Interval", "Trigger Interval (pulse)", bVisible);
                m_bTriggerCmd = tree.Set(m_bTriggerCmd, m_bTriggerCmd, "Command", "Use Command Encoder", bVisible); 
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                if (m_module.Run(m_runAxisMove.Run())) return p_sInfo;
                double y0 = m_runAxisMove.m_rp.Y; 
                Axis axisY = m_module.m_axisXY.p_axisY; 
                axisY.SetTrigger(y0 + m_yStart, y0 + m_yLength, m_dTrigger, m_bTriggerCmd);
                axisY.RunTrigger(true);
                if (m_module.Run(m_module.m_camDalsa.StartGrab())) return p_sInfo;
                if (m_module.Run(axisY.StartMove(y0 + m_yLength))) return p_sInfo;
                axisY.RunTrigger(false);
                if (m_module.Run(axisY.WaitReady())) return p_sInfo;
                return "OK";
            }
        }
        #endregion
    }
}
