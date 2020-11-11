using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root.Module
{
    public class TestRepeat : ModuleBase
    {
        #region ToolBox
        public MemoryPool m_memoryPool;
        CameraBasler m_cam;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_cam, this, "Camera");
            if (bInit)
            {
                if (m_memoryPool.p_gbPool < 1) m_memoryPool.p_gbPool = 1;
                m_cam.OnConnect += M_cam_OnConnect;
            }
        }

        private void M_cam_OnConnect()
        {
            m_memoryBasler.p_nByte = m_cam.p_nByte;
            m_memoryBasler.p_sz = m_cam.p_sz;
            m_cam.SetMemoryData(m_memoryBasler);
        }
        #endregion

        #region Memory
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryBasler;
        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryBasler = m_memoryGroup.CreateMemory("Grab Basler", 1, m_cam.p_nByte, m_cam.p_sz);
            m_cam.SetMemoryData(m_memoryBasler);
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            //
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            //
            p_eState = eState.Ready;
            return "OK";
        }
        #endregion

        public TestRepeat(string id, IEngineer engineer)
        {
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_AxisMove(this), false, "Axis Move");
            AddModuleRunList(new Run_GrabBasler(this), false, "Run Grab Basler Camera");
        }

        public class Run_Delay : ModuleRunBase
        {
            TestRepeat m_module;
            public Run_Delay(TestRepeat module)
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
                return "OK";
            }
        }

        public class Run_AxisMove : ModuleRunBase
        {
            TestRepeat m_module;
            public Run_AxisMove(TestRepeat module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public double m_pos = 0; 
            public override ModuleRunBase Clone()
            {
                Run_AxisMove run = new Run_AxisMove(m_module);
                run.m_pos = m_pos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_pos = tree.Set(m_pos, m_pos, "Position", "Axis Position (pulse)", bVisible);
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                //
                return "OK";
            }
        }

        public class Run_GrabBasler : ModuleRunBase
        {
            TestRepeat m_module;
            public Run_GrabBasler(TestRepeat module)
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
                if (m_module.Run(m_module.m_cam.GrabOne(0))) return p_sInfo;
                return "OK";
            }
        }
        #endregion
    }
}
